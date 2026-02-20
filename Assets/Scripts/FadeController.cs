using System.Collections;
using UnityEngine;

public class FadeController : MonoBehaviour
{
	[Header("Настройки затемнения")]
	[SerializeField, Tooltip("Quad, прозрачность которого будет анимироваться")]
	private Renderer quadRenderer = default;

	private Coroutine currentFadeCoroutine;

	public void SetFade(float from, float to, float duration, float delay = 0f)
	{
		if (currentFadeCoroutine != null)
		{
			StopCoroutine(currentFadeCoroutine);
			currentFadeCoroutine = null;
		}

		currentFadeCoroutine = StartCoroutine(FadeRoutine(from, to, duration, delay));
	}

	private IEnumerator FadeRoutine(float from, float to, float duration, float delay)
	{
		if (quadRenderer == null)
			yield break;

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

		SetAlpha(from);

		float elapsed = 0f;
		while (elapsed < duration)
		{
			elapsed += Time.deltaTime;
			float t = elapsed / duration;
			float currentAlpha = Mathf.Lerp(from, to, t);
			SetAlpha(currentAlpha);
			yield return null;
		}

		SetAlpha(to);
		currentFadeCoroutine = null;
	}

	private void SetAlpha(float value)
	{
		if (quadRenderer == null)
			return;

		Material mat = quadRenderer.material; // инстанс материала для конкретного квадрата
		Color currentColor = mat.color;
		currentColor.a = Mathf.Clamp01(value);
		mat.color = currentColor;
	}
}