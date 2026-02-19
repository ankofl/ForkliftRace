using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class PerInstanceColor : MonoBehaviour
{
	private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

	[SerializeField]
	private Color instanceColor = Color.white;

	private MeshRenderer meshRenderer;
	private MaterialPropertyBlock propertyBlock;

	private void OnEnable()
	{
		EnsureInit();
		ApplyColor(instanceColor);
	}

	private void OnValidate()
	{
		EnsureInit();
		ApplyColor(instanceColor);
	}

	private void EnsureInit()
	{
		if (meshRenderer == null)
			meshRenderer = GetComponent<MeshRenderer>();

		if (propertyBlock == null)
			propertyBlock = new MaterialPropertyBlock();
	}

	public void ApplyColor(Color color)
	{
		meshRenderer.GetPropertyBlock(propertyBlock);
		propertyBlock.SetColor(BaseColorId, color);
		meshRenderer.SetPropertyBlock(propertyBlock);
	}
}