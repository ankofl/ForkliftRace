using System.Collections;
using UnityEngine;

public class FadeController : MonoBehaviour
{
	[SerializeField]
	private Material FadeMaterial;


	private Coroutine currentFade;

	public void SetFade(float from, float to, float duration, float delay = 0)
	{
		if (currentFade != null)
			StopCoroutine(currentFade);

		currentFade = StartCoroutine(FadeRoutine(from, to, duration, delay));
	}

	private IEnumerator FadeRoutine(float from, float to, float duration, float delay = 0)
	{
		float time = -delay;

		SetAlpha(from);

		while (time < duration)
		{
			time += Time.deltaTime;
			float t = time / duration;

			float alpha = Mathf.Lerp(from, to, t);
			SetAlpha(alpha);

			yield return null;
		}

		SetAlpha(to);
		currentFade = null;
	}

	private void SetAlpha(float value)
	{
		Color c = FadeMaterial.color;
		c.a = Mathf.Clamp01(value);
		FadeMaterial.color = c;
	}
}
