using UnityEngine;

public class DialsPanel : MonoBehaviour
{
	[SerializeField]
	private Dial Tañhometer;

	[SerializeField]
	private Dial Speedometer;

	[SerializeField]
	private Dial Fuel;

	public void SetValues(float tachometer, float speedometer, float fuel)
	{
		Tañhometer.SetValue(tachometer);
		Speedometer.SetValue(speedometer);
		Fuel.SetValue(fuel);
	}
}
