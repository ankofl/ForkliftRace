using UnityEngine;


[ExecuteAlways]
public class Tooltip : MonoBehaviour
{
	[SerializeField] 
	private TextMesh textMesh;

	[SerializeField] 
	private Transform quad;

	[Header("Отступы вокруг текста")]
	[SerializeField] 
	private float paddingX = 0.02f;

	[SerializeField]
	private float charWidth = 0.008f;

	public string Text
	{
		get => textMesh.text;
		set
		{
			if (textMesh == null) return;
			textMesh.text = value;
			UpdateQuadSize();
		}
	}

	private void UpdateQuadSize()
	{
		if (quad == null) return;

		quad.localScale = new Vector3(Text.Length * charWidth + paddingX, quad.localScale.y, quad.localScale.z);
	}
}