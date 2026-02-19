using UnityEngine;


public class GameManager : MonoBehaviour
{
	[SerializeField]
	private Camera MainCamera;

	[SerializeField]
	private TextMesh Tooltip;

	[SerializeField]
	private ForkliftController Forklift;

	[SerializeField]
	private ZoneLoading Loading;

    [SerializeField]
	private ZoneUnloading Unloading;

	private void Awake()
	{
		MainCamera = Camera.main;

		Tooltip = MainCamera.GetComponentInChildren<TextMesh>();

		Loading.SpawnPallete();

		Unloading.Delivered += Delivered;
	}

	private void Delivered(Pallete pallete)
	{
		Tooltip.text = "Delivered";
		Destroy(pallete.gameObject);

		Loading.SpawnPallete();

		Unloading.SetTriggerState(true);
	}
}
