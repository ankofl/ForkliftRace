using UnityEngine;

public enum DialType
{
	/// <summary>
	/// Без значения
	/// </summary>
	None,

	/// <summary>
	/// Тахометр
	/// </summary>
	Tachometer,

	/// <summary>
	/// Спидометр
	/// </summary>
	Speedometer,

	/// <summary>
	/// Уровень топлива
	/// </summary>
	Fuel,
}

public class Dial : MonoBehaviour
{
	[SerializeField, Tooltip("Тип прибора")]
	private DialType type = DialType.None;

	[SerializeField, Tooltip("Подпись / название прибора")]
	private string label = "Text";

	[Header("Ссылки на компоненты")]
	[SerializeField]
	private TextMesh textMesh = default;

	[SerializeField]
	private Transform arrow = default;

	private const float MinAngle = 60f;
	private const float MaxAngle = 300f;

	private void OnValidate()
	{
		if (textMesh == null) return;
		textMesh.text = label;
	}

	/// <summary>
	/// Устанавливает значение стрелки прибора.  
	/// Ожидается нормализованное значение в диапазоне от 0 до 1.
	/// </summary>
	/// <param name="normalizedValue">Значение от 0 (минимум) до 1 (максимум)</param>
	public void SetValue(float normalizedValue)
	{
		normalizedValue = Mathf.Clamp01(normalizedValue);

		float angle = Mathf.Lerp(MinAngle, MaxAngle, normalizedValue);

		if (arrow == null) return;
		arrow.localEulerAngles = new Vector3(0f, angle, 0f);
	}
}