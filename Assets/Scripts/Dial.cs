using UnityEngine;

public enum DialType
{
    None,
	Tachometer,
	Speedometer,
    Fuel,
}

public class Dial : MonoBehaviour
{
    [SerializeField]
    private DialType Type;

	[SerializeField]
	private string DialText = "Text";

	[SerializeField]
	private TextMesh Text;

	[SerializeField]
	private Transform Arrow;

	private void OnValidate()
	{
		Text.text = DialText;
	}

	public void SetValue(float value)
	{
		value = Mathf.Clamp01(value);

		var y = Mathf.Lerp(60, 300, value);
		Arrow.localEulerAngles = new Vector3(0, y, 0);
	}
}
