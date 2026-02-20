using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

/// <summary>
/// Тип анимации паллеты
/// </summary>
public enum PalleteAnimType
{
	/// <summary>
	/// Без анимации
	/// </summary>
	None,

	/// <summary>
	/// Анимация появления в зоне загрузки
	/// </summary>
	LoadingZone,

	/// <summary>
	/// Анимация выгрузки в зоне разгрузки
	/// </summary>
	UnloadingZone,
}

/// <summary>
/// Компонент паллеты с поддержкой анимации, фиксации и физики
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class Pallete : MonoBehaviour
{
	[Header("Ссылки")]
	/// <summary>
	/// Трансформ для визуального вращения во время анимации
	/// </summary>
	[SerializeField]
	private Transform Rotator;

	/// <summary>
	/// Список всех BoxCollider паллеты
	/// </summary>
	private List<BoxCollider> Colliders;

	/// <summary>
	/// Rigidbody паллеты
	/// </summary>
	private Rigidbody Rb;

	/// <summary>
	/// Текущая запущенная корутина анимации
	/// </summary>
	private Coroutine currentAnim;

	/// <summary>
	/// Источник завершения Task для асинхронной анимации
	/// </summary>
	private TaskCompletionSource<bool> Tcs;

	/// <summary>
	/// Состояние фиксации паллеты
	/// </summary>
	public bool Locked { get; private set; }

	/// <summary>
	/// Инициализация компонентов паллеты
	/// </summary>
	private void Awake()
	{
		// Получаем Rigidbody
		Rb = GetComponent<Rigidbody>();

		// Инициализируем список коллайдеров
		Colliders = new List<BoxCollider>();

		// Заполняем список всеми дочерними BoxCollider
		foreach (BoxCollider collider in GetComponentsInChildren<BoxCollider>(true))
		{
			Colliders.Add(collider);
		}
	}

	/// <summary>
	/// Запуск анимации паллеты
	/// </summary>
	public Task Anim(PalleteAnimType type)
	{
		if (currentAnim != null)
		{
			StopCoroutine(currentAnim);
		}

		Tcs?.TrySetResult(true);
		Tcs = new TaskCompletionSource<bool>();

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
				Tcs.TrySetResult(true);
				break;
		}

		return Tcs.Task;
	}

	/// <summary>
	/// Анимация появления паллеты в зоне загрузки
	/// </summary>
	private IEnumerator LoadingAnim()
	{
		ToKinematic(true);

		// Длительность анимации
		float duration = 5f;

		// Текущее время
		float elapsed = 0f;

		// Начальная позиция (выше на 5 метров)
		Vector3 startPos = transform.position + Vector3.up * 5f;

		// Целевая позиция
		Vector3 targetPos = transform.position;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;

			// Нормализованное время
			float t = Mathf.Clamp01(elapsed / duration);

			// Интерполяция позиции
			transform.position = Vector3.Lerp(startPos, targetPos, t);

			// Угол вращения (2 полных оборота)
			float spin = 720f * t;

			// Вращение по всем осям
			Rotator.rotation = Quaternion.Euler(spin, spin, spin);

			yield return null;
		}

		ToKinematic(false);

		currentAnim = null;

		Tcs?.TrySetResult(true);
	}

	/// <summary>
	/// Анимация выгрузки паллеты
	/// </summary>
	private IEnumerator UnloadingAnim()
	{
		ToKinematic(true);

		// Длительность анимации
		float duration = 5f;

		// Текущее время
		float elapsed = 0f;

		// Начальная позиция
		Vector3 startPos = transform.position;

		// Конечная позиция (подъем вверх)
		Vector3 targetPos = startPos + Vector3.up * 5f;

		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;

			// Нормализованное время
			float t = Mathf.Clamp01(elapsed / duration);

			// Интерполяция позиции
			transform.position = Vector3.Lerp(startPos, targetPos, t);

			// Поворот только вокруг оси Y
			transform.Rotate(Vector3.up, 180f * Time.deltaTime);

			yield return null;
		}

		transform.position = targetPos;

		currentAnim = null;

		Tcs?.TrySetResult(true);
	}

	/// <summary>
	/// Переключение физического режима паллеты
	/// </summary>
	private void ToKinematic(bool state)
	{
		Rb.isKinematic = state;

		foreach (BoxCollider collider in Colliders)
		{
			collider.isTrigger = state;
		}
	}

	/// <summary>
	/// Фиксация или освобождение паллеты
	/// </summary>
	public void Lock(Transform tran)
	{
		// Определяем состояние фиксации
		Locked = tran != null;

		// Переключаем физику
		ToKinematic(Locked);

		// Устанавливаем родителя
		transform.SetParent(tran);
	}
}