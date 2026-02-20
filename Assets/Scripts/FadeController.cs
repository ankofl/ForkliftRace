using System.Collections;
using UnityEngine;

public class FadeController : MonoBehaviour
{
	[Header("Настройки затемнения")]

	/// <summary>
	/// Quad, прозрачность которого будет анимироваться
	/// </summary>
	[SerializeField, Tooltip("Quad, прозрачность которого будет анимироваться")]
	private Renderer quadRenderer = default;

	/// <summary>
	/// Текущая корутина анимации затемнения
	/// </summary>
	private Coroutine currentFadeCoroutine;

	/// <summary>
	/// Запуск анимации затемнения
	/// </summary>
	/// <param name="from">Начальная прозрачность</param>
	/// <param name="to">Конечная прозрачность</param>
	/// <param name="duration">Длительность анимации</param>
	/// <param name="delay">Задержка перед началом</param>
	public void SetFade(float from, float to, float duration, float delay = 0f)
	{
		// Если анимация уже идёт, останавливаем её
		if (currentFadeCoroutine != null)
		{
			StopCoroutine(currentFadeCoroutine);
			currentFadeCoroutine = null;
		}

		// Запускаем новую корутину
		currentFadeCoroutine = StartCoroutine(FadeRoutine(from, to, duration, delay));
	}

	/// <summary>
	/// Корутина плавного изменения прозрачности
	/// </summary>
	private IEnumerator FadeRoutine(float from, float to, float duration, float delay)
	{
		if (quadRenderer == null)
			yield break;

		// Обработка задержки перед началом
		float actualDelay = Mathf.Max(0f, delay);
		if (actualDelay > 0f)
		{
			float waitTime = 0f;
			while (waitTime < actualDelay)
			{
				waitTime += Time.deltaTime;
				yield return null;
			}
		}

		// Устанавливаем начальное значение альфы
		SetAlpha(from);

		// Плавное изменение прозрачности
		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			float currentAlpha = Mathf.Lerp(from, to, t);
			SetAlpha(currentAlpha);
			yield return null;
		}

		// Устанавливаем конечное значение альфы
		SetAlpha(to);
		currentFadeCoroutine = null;
	}

	/// <summary>
	/// Установка прозрачности квадрата
	/// </summary>
	/// <param name="value">Значение альфы</param>
	private void SetAlpha(float value)
	{
		if (quadRenderer == null)
			return;

		// Создаем инстанс материала для конкретного квадрата
		Material mat = quadRenderer.material;
		Color currentColor = mat.color;
		currentColor.a = Mathf.Clamp01(value);
		mat.color = currentColor;
	}
}