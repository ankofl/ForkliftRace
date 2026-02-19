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
		Tooltip.text = "Sturtup Engine [T]";


		Loading.SpawnPallete();

		Unloading.Delivered += Delivered;


		Forklift.EngineChangeState += OnEngineChangeState;
		Forklift.PalleteLocked += OnPalleteLocked; 
	}

	private void OnEngineChangeState(bool state)
	{
		if (state)
		{
			Tooltip.text = "Move [WASD] | Up/Down [Q/E]";
		}
		else
		{
			Tooltip.text = "Sturtup Engine [T]";
		}
	}

	private void OnPalleteLocked(bool locked)
	{
		if (locked && Forklift.EngineState)
		{
			Tooltip.text = "Release Pallete [E] on [Unloading Zone]";
		}
		else if(Forklift.EngineState)
		{
			Tooltip.text = "Pickup Pallete [Q] on [Loading Zone]";
		}
	}

	private void Delivered(Pallete pallete)
	{
		Tooltip.text = "Delivered!";
		Destroy(pallete.gameObject);

		Loading.SpawnPallete();

		Unloading.SetTriggerState(true);
	}
}
