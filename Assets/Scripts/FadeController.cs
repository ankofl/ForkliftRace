using System.Collections;
using UnityEngine;

public class FadeController : MonoBehaviour
{
	[Header("Настройки затемнения")]
	[SerializeField, Tooltip("Материал, альфа-канал которого будет анимироваться")]
	private Material fadeMaterial = default;

	/// <summary>
	/// Активная в данный момент корутина анимации затемнения/проявления
	/// </summary>
	private Coroutine currentFadeCoroutine;

	/// <summary>
	/// Запускает плавное изменение прозрачности материала от одного значения к другому.
	/// Если в этот момент уже выполняется другая анимация — она немедленно прерывается.
	/// </summary>
	/// <param name="from">Начальная прозрачность (0 = полностью прозрачно, 1 = полностью непрозрачно)</param>
	/// <param name="to">Конечная прозрачность (0 = полностью прозрачно, 1 = полностью непрозрачно)</param>
	/// <param name="duration">Продолжительность перехода в секундах (должна быть больше 0)</param>
	/// <param name="delay">Задержка перед началом анимации в секундах (? 0)</param>
	public void SetFade(float from, float to, float duration, float delay = 0f)
	{
		// Прерываем предыдущую анимацию, если она активна
		if (currentFadeCoroutine != null)
		{
			StopCoroutine(currentFadeCoroutine);
			currentFadeCoroutine = null;
		}

		// Запускаем новую анимацию
		currentFadeCoroutine = StartCoroutine(FadeRoutine(from, to, duration, delay));
	}

	/// <summary>
	/// Корутина, выполняющая плавное изменение альфа-канала материала.
	/// Задержка всегда неотрицательная — если передать отрицательное значение, оно будет приравнено к нулю.
	/// </summary>
	/// <param name="from">Начальное значение альфа</param>
	/// <param name="to">Целевое значение альфа</param>
	/// <param name="duration">Длительность анимации</param>
	/// <param name="delay">Задержка перед стартом (всегда ? 0)</param>
	private IEnumerator FadeRoutine(float from, float to, float duration, float delay)
	{
		// Защита от отсутствия материала
		if (fadeMaterial == null)
		{
			yield break;
		}

		// Задержка не может быть отрицательной
		float actualDelay = Mathf.Max(0f, delay);

		// Ждём задержку, если она есть
		if (actualDelay > 0f)
		{
			float waitTime = 0f;
			while (waitTime < actualDelay)
			{
				waitTime += Time.deltaTime;
				yield return null;
			}
		}

		// Устанавливаем начальное значение сразу после задержки
		SetAlpha(from);

		// Время, прошедшее с начала анимации
		float elapsed = 0f;

		// Основной цикл анимации
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;

			float t = elapsed / duration;
			float currentAlpha = Mathf.Lerp(from, to, t);

			SetAlpha(currentAlpha);

			yield return null;
		}

		// Гарантированно устанавливаем конечное значение
		SetAlpha(to);

		// Сбрасываем ссылку на завершившуюся корутину
		currentFadeCoroutine = null;
	}

	/// <summary>
	/// Устанавливает значение альфа-канала материала в диапазоне [0, 1].
	/// </summary>
	/// <param name="value">Желаемое значение прозрачности (будет обрезано до 0..1)</param>
	private void SetAlpha(float value)
	{
		if (fadeMaterial == null)
		{
			return;
		}

		Color currentColor = fadeMaterial.color;
		currentColor.a = Mathf.Clamp01(value);
		fadeMaterial.color = currentColor;
	}
}