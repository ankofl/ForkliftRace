using System.Collections;
using UnityEngine;


public class GameManager : MonoBehaviour
{
	[SerializeField]
	private Camera MainCamera;

	[SerializeField]
	private FadeController Fade;

	[SerializeField]
	private TextMesh Tooltip;

	[SerializeField]
	private ForkliftController Forklift;

	[SerializeField]
	private ZoneLoading Loading;

    [SerializeField]
	private ZoneUnloading Unloading;

	private Pallete SpawnedPallete = null;

	private void Awake()
	{
		MainCamera = Camera.main;

		Fade = MainCamera.GetComponentInChildren<FadeController>();
		Fade.SetFade(1, 0, 2, 1);

		Tooltip = MainCamera.GetComponentInChildren<TextMesh>();
		Tooltip.text = "Startup Engine [T]";

		Loading.SpawnPallete(ref SpawnedPallete);

		Unloading.Delivered += Delivered;


		Forklift.EngineChangeState += OnEngineChangeState;
		Forklift.PalleteLocked += OnPalleteLocked;
		Forklift.FuelEnded += OnFuelEnded;
	}



	private void OnFuelEnded()
	{
		if (restartRoutine != null)
			StopCoroutine(restartRoutine);

		restartRoutine = StartCoroutine(RestartRoutine(1, 3));
	}
	private Coroutine restartRoutine;

	private IEnumerator RestartRoutine(float duration, float delay = 0)
	{
		Fade.SetFade(0, 1, duration, delay);

		yield return new WaitForSeconds(duration + delay);

		if (SpawnedPallete != null)
		{
			Destroy(SpawnedPallete.gameObject);
			SpawnedPallete = null;
		}

		Forklift.Restart();
		Loading.SpawnPallete(ref SpawnedPallete);

		Fade.SetFade(1, 0, duration);
	}

	private void OnEngineChangeState(bool state)
	{
		if (state)
		{
			Tooltip.text = "Move [WASD] | Up/Down [Q/E]";
		}
		else if(Forklift.Fuel > 0)
		{
			Tooltip.text = "Startup Engine [T]";
		}
		else if (Forklift.Fuel == 0)
		{
			Tooltip.text = "Fuel ended! Restarting...";
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

	private async void Delivered()
	{
		Tooltip.text = "Delivered!";
		await SpawnedPallete.Anim(PalleteAnimType.UnloadingZone);

		Loading.SpawnPallete(ref SpawnedPallete);

		Unloading.SetTriggerState(true);
	}
}
