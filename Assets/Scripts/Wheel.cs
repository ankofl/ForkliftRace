using UnityEngine;

[RequireComponent(typeof(Transform))]
public class Wheel : MonoBehaviour
{
	/// <summary>
	/// Радиус колеса
	/// </summary>
	[SerializeField, Tooltip("Радиус колеса")]
	private float radius = 1;

	/// <summary>
	/// Текущий угол вращения колеса
	/// </summary>
	private float wheelRotation = 0;

	/// <summary>
	/// Устанавливает вращение колеса
	/// </summary>
	/// <param name="speed">Скорость движения погрузчика</param>
	/// <param name="steerWheelAngle">Угол поворота руля в градусах</param>
	public void SetRotation(float speed, float steerWheelAngle)
	{
		// Вычисляем смещение вращения колеса за кадр
		float deltaRotation = speed / (radius * 2) * Mathf.Rad2Deg * Time.fixedDeltaTime;

		// Обновляем текущий угол вращения
		wheelRotation += deltaRotation;

		// Применяем вращение и угол поворота к трансформу колеса
		transform.localRotation = Quaternion.Euler(wheelRotation, steerWheelAngle, 0f);
	}
}