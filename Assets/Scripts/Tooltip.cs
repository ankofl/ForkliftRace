using UnityEngine;
using DG.Tweening;

[ExecuteAlways]
public class Tooltip : MonoBehaviour
{
	[SerializeField] private TextMesh textMesh;
	[SerializeField] private Transform quad;

	[Header("Отступы вокруг текста")]
	[SerializeField] private float paddingX = 0.02f;
	[SerializeField] private float charWidth = 0.008f;

	[Header("Анимация текста")]
	[SerializeField] private float fadeDuration = 0.25f;

	private Material textMaterial;
	private Sequence fadeSequence;

	private void Awake()
	{
		if (textMesh != null)
		{
			// Без утечки материалов используем sharedMaterial
			textMaterial = textMesh.GetComponent<Renderer>().sharedMaterial;
		}
	}

	public string Text
	{
		get => textMesh.text;
		set
		{
			if (textMesh == null || textMaterial == null) return;

			// Останавливаем текущую анимацию
			fadeSequence?.Kill();

			// Создаём последовательность DOTween
			fadeSequence = DOTween.Sequence();

			fadeSequence.Append(textMaterial.DOFade(0f, fadeDuration / 2f));
			fadeSequence.AppendCallback(() =>
			{
				textMesh.text = value;
				UpdateQuadSize();
			});
			fadeSequence.Append(textMaterial.DOFade(1f, fadeDuration / 2f));

			fadeSequence.Play();
		}
	}

	private void UpdateQuadSize()
	{
		if (quad == null || textMesh == null) return;

		float width = textMesh.text.Length * charWidth + paddingX;
		quad.localScale = new Vector3(width, quad.localScale.y, quad.localScale.z);
	}
}