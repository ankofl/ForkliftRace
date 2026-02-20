using UnityEngine;

public class DialsPanel : MonoBehaviour
{
	[Header("Приборы")]
	[SerializeField, Tooltip("Тахометр")]
	private Dial tachometer = default;

	[SerializeField, Tooltip("Спидометр")]
	private Dial speedometer = default;

	[SerializeField, Tooltip("Указатель уровня топлива")]
	private Dial fuel = default;

	/// <summary>
	/// Устанавливает значения всех приборов одновременно.  
	/// Все значения должны быть нормализованы (в диапазоне 0..1).
	/// </summary>
	/// <param name="tachometerValue">Нормализованное значение для тахометра (0..1)</param>
	/// <param name="speedometerValue">Нормализованное значение для спидометра (0..1)</param>
	/// <param name="fuelValue">Нормализованное значение для уровня топлива (0..1)</param>
	public void SetValues(
		float tachometerValue,
		float speedometerValue,
		float fuelValue)
	{
		if (tachometer != null)
			tachometer.SetValue(tachometerValue);

		if (speedometer != null)
			speedometer.SetValue(speedometerValue);

		if (fuel != null)
			fuel.SetValue(fuelValue);
	}
}