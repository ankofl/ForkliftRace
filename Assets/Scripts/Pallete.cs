using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using Zenject;
using Unity.VisualScripting;

/// <summary>
/// Тип анимации паллеты
/// </summary>
public enum PalleteAnimType
{
	None,
	LoadingZone,
	UnloadingZone,
}

[RequireComponent(typeof(Rigidbody))]
public class Pallete : MonoBehaviour
{
	// Фабрика Zenject
	public class Factory : PlaceholderFactory<Pallete> { }

	[Header("Ссылки")]
	[SerializeField] private Transform Rotator;

	private List<BoxCollider> Colliders;
	private Rigidbody Rb;

	private bool isAnimating;

	public bool Locked { get; private set; }

	private void Awake()
	{
		Rb = GetComponent<Rigidbody>();
		Colliders = new List<BoxCollider>(GetComponentsInChildren<BoxCollider>(true));
	}

	/// <summary>
	/// Запуск анимации паллеты через async UniTask
	/// </summary>
	public async UniTask Anim(PalleteAnimType type)
	{
		// Если уже идёт анимация — отменяем её
		if (isAnimating)
			return;

		isAnimating = true;

		switch (type)
		{
			case PalleteAnimType.LoadingZone:
				await LoadingAnim();
				break;

			case PalleteAnimType.UnloadingZone:
				await UnloadingAnim();
				break;

			default:
				ToKinematic(false);
				break;
		}

		isAnimating = false;
	}

	private async UniTask LoadingAnim()
	{
		if (gameObject.IsDestroyed())
			return;

		ToKinematic(true);

		float duration = 5f;
		float elapsed = 0f;
		Vector3 startPos = transform.position + Vector3.up * 5f;
		Vector3 targetPos = transform.position;

		while (elapsed < duration)
		{
			if (gameObject.IsDestroyed())
				return;

			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);

			transform.position = Vector3.Lerp(startPos, targetPos, t);

			float spin = 720f * t;
			Rotator.rotation = Quaternion.Euler(spin, spin, spin);

			await UniTask.Yield();
		}

		ToKinematic(false);
	}

	private async UniTask UnloadingAnim()
	{
		ToKinematic(true);

		if (gameObject.IsDestroyed())
			return;

		float duration = 5f;
		float elapsed = 0f;
		Vector3 startPos = transform.position;
		Vector3 targetPos = startPos + Vector3.up * 5f;

		while (elapsed < duration)
		{
			if (gameObject.IsDestroyed())
				return;

			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);

			transform.position = Vector3.Lerp(startPos, targetPos, t);
			transform.Rotate(Vector3.up, 180f * Time.deltaTime);

			await UniTask.Yield();
		}

		transform.position = targetPos;
	}

	private void ToKinematic(bool state)
	{
		Rb.isKinematic = state;
		foreach (var collider in Colliders)
			collider.isTrigger = state;
	}

	public void Lock(Transform tran)
	{
		Locked = tran != null;
		ToKinematic(Locked);
		transform.SetParent(tran);
	}
}