using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PalleteAnimType
{
	None,
	LoadingZone,
	UnloadingZone,
}

[RequireComponent(typeof(Rigidbody))]
public class Pallete : MonoBehaviour
{
	[SerializeField]
	private Transform Rotator;


	private List<BoxCollider> Colliders;
	private Rigidbody Rb;

	private Coroutine currentAnim;

	private void Awake()
	{
		Rb = GetComponent<Rigidbody>();

		Colliders = new List<BoxCollider>();
		foreach (var collider in GetComponentsInChildren<BoxCollider>(true))
		{
			Colliders.Add(collider);
		}
	}

	public void Anim(PalleteAnimType type)
	{
		if (currentAnim != null)
		{
			StopCoroutine(currentAnim);
		}

		switch (type)
		{
			case PalleteAnimType.LoadingZone:
				currentAnim = StartCoroutine(LoadingAnim());
				break;

			case PalleteAnimType.UnloadingZone:
				currentAnim = StartCoroutine(UnloadingAnim());
				break;

			default:
				ToKinematic(false);
				break;
		}
	}

	private IEnumerator LoadingAnim()
	{
		ToKinematic(true);

		float duration = 5f;
		float elapsed = 0f;

		Vector3 startPos = transform.position + Vector3.up * 5f;
		Vector3 targetPos = transform.position;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);

			// Подъём
			transform.position = Vector3.Lerp(startPos, targetPos, t);

			// Вращение по всем осям (добавляем динамику)
			float spin = 720f * t; // 2 полных оборота

			Rotator.rotation = Quaternion.Euler(spin, spin, spin);

			yield return null;
		}

		ToKinematic(false);

		currentAnim = null;
	}

	private IEnumerator UnloadingAnim()
	{
		ToKinematic(true);

		float duration = 5f;
		float elapsed = 0f;

		Vector3 startPos = transform.position;
		Vector3 targetPos = startPos + Vector3.up * 5f;

		Quaternion startRot = transform.rotation;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);

			// Подъём
			transform.position = Vector3.Lerp(startPos, targetPos, t);

			// Вращение только по Y
			transform.Rotate(new Vector3(0, 1, 0), 180 * Time.deltaTime);

			yield return null;
		}

		transform.position = targetPos;

		currentAnim = null;

		Destroy(gameObject);
	}

	private void ToKinematic(bool state)
	{
		Rb.isKinematic = state;

		foreach (var collider in Colliders)
		{
			collider.isTrigger = state;
		}
	}

	public void Lock(Transform tran)
	{
		Locked = tran != null;

		ToKinematic(Locked);

		transform.SetParent(tran);
	}

	public bool Locked { get; private set; }
}