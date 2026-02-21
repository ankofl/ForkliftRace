using System.Collections;
using System.Threading.Tasks;
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
	/// Запуск анимации затемнения (корутина)
	/// </summary>
	/// <param name="from">Начальная прозрачность</param>
	/// <param name="to">Конечная прозрачность</param>
	/// <param name="duration">Длительность анимации</param>
	/// <param name="delay">Задержка перед началом</param>
	public void SetFade(float from, float to, float duration, float delay = 0f)
	{
		if (currentFadeCoroutine != null)
		{
			StopCoroutine(currentFadeCoroutine);
			currentFadeCoroutine = null;
		}

		currentFadeCoroutine = StartCoroutine(FadeRoutine(from, to, duration, delay));
	}

	/// <summary>
	/// Асинхронная версия анимации прозрачности
	/// </summary>
	/// <param name="from">Начальная прозрачность</param>
	/// <param name="to">Конечная прозрачность</param>
	/// <param name="duration">Длительность</param>
	/// <param name="delay">Задержка перед началом</param>
	/// <returns>Task, который завершается после окончания анимации</returns>
	public Task FadeAsync(float from, float to, float duration, float delay = 0f)
	{
		var tcs = new TaskCompletionSource<bool>();

		if (currentFadeCoroutine != null)
		{
			StopCoroutine(currentFadeCoroutine);
			currentFadeCoroutine = null;
		}

		currentFadeCoroutine = StartCoroutine(FadeRoutine(from, to, duration, delay, tcs));
		return tcs.Task;
	}

	/// <summary>
	/// Корутина плавного изменения прозрачности (для SetFade)
	/// </summary>
	private IEnumerator FadeRoutine(float from, float to, float duration, float delay)
	{
		yield return FadeRoutineInternal(from, to, duration, delay, null);
	}

	/// <summary>
	/// Корутина с TaskCompletionSource для FadeAsync
	/// </summary>
	private IEnumerator FadeRoutine(float from, float to, float duration, float delay, TaskCompletionSource<bool> tcs)
	{
		yield return FadeRoutineInternal(from, to, duration, delay, tcs);
	}

	/// <summary>
	/// Внутренняя корутина для плавного изменения альфы
	/// </summary>
	private IEnumerator FadeRoutineInternal(float from, float to, float duration, float delay, TaskCompletionSource<bool> tcs)
	{
		if (quadRenderer == null)
		{
			tcs?.SetResult(true);
			yield break;
		}

		// Обработка задержки
		if (delay > 0f)
		{
			float elapsedDelay = 0f;
			while (elapsedDelay < delay)
			{
				elapsedDelay += Time.deltaTime;
				yield return null;
			}
		}

		// Начальное значение
		SetAlpha(from);

		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = Mathf.Clamp01(elapsed / duration);
			float currentAlpha = Mathf.Lerp(from, to, t);
			SetAlpha(currentAlpha);
			yield return null;
		}

		// Конечное значение
		SetAlpha(to);

		currentFadeCoroutine = null;
		tcs?.SetResult(true);
	}

	/// <summary>
	/// Установка прозрачности квадрата
	/// </summary>
	private void SetAlpha(float value)
	{
		if (quadRenderer == null)
			return;

		Material mat = quadRenderer.material; // Создаёт инстанс материала
		Color color = mat.color;
		color.a = Mathf.Clamp01(value);
		mat.color = color;
	}
}