using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshRenderer))]
public class PerInstanceColor : MonoBehaviour
{
	/// <summary>
	/// ID свойства цвета в шейдере
	/// </summary>
	private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

	/// <summary>
	/// Цвет для данного экземпляра
	/// </summary>
	[SerializeField, Tooltip("Цвет для данного экземпляра")]
	private Color instanceColor = Color.white;

	/// <summary>
	/// MeshRenderer объекта
	/// </summary>
	private MeshRenderer meshRenderer;

	/// <summary>
	/// MaterialPropertyBlock для установки цвета на инстанс
	/// </summary>
	private MaterialPropertyBlock propertyBlock;

	/// <summary>
	/// Инициализация компонентов
	/// </summary>
	private void Awake()
	{
		Initialize();
	}

	/// <summary>
	/// Обновление при включении объекта
	/// </summary>
	private void OnEnable()
	{
		Initialize();
		ApplyColor();
	}

	/// <summary>
	/// Обновление в редакторе при изменении значения
	/// </summary>
	private void OnValidate()
	{
		Initialize();
		ApplyColor();
	}

	/// <summary>
	/// Инициализация MeshRenderer и MaterialPropertyBlock
	/// </summary>
	private void Initialize()
	{
		// Получаем MeshRenderer компонента
		if (meshRenderer == null)
			meshRenderer = GetComponent<MeshRenderer>();

		// Создаём MaterialPropertyBlock, если ещё не создан
		if (propertyBlock == null)
			propertyBlock = new MaterialPropertyBlock();
	}

	/// <summary>
	/// Применение цвета к этому экземпляру MeshRenderer
	/// </summary>
	private void ApplyColor()
	{
		// Проверка на null, если объект ещё не инициализирован
		if (meshRenderer == null)
			return;

		// Получаем текущие свойства материала
		meshRenderer.GetPropertyBlock(propertyBlock);

		// Очищаем старые значения
		propertyBlock.Clear();

		// Устанавливаем новый цвет
		propertyBlock.SetColor(BaseColorId, instanceColor);

		// Применяем изменения к MeshRenderer
		meshRenderer.SetPropertyBlock(propertyBlock);
	}
}