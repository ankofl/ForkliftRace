using UnityEngine;

public class Wheel : MonoBehaviour
{
	[SerializeField]
	private float radius = 1;

	public void SetRotation(float speed, float steerWheelAngle)
	{
		WheelRotation += speed / (radius * 2) * Mathf.Rad2Deg * Time.fixedDeltaTime;

		transform.localRotation = Quaternion.Euler(WheelRotation, steerWheelAngle, 0f);
	}
	private float WheelRotation = 0;
}
