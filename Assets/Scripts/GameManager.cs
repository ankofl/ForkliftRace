using System.Collections;
using UnityEngine;
using Zenject;

/// <summary>
/// Главный менеджер игры, управляющий состояниями, UI и перезапуском
/// </summary>
public class GameManager : MonoBehaviour
{
	[Inject] 
	private ForkliftController Forklift = null!;

	[Inject] 
	private ZoneLoading Loading = null!;

	[Inject] 
	private ZoneUnloading Unloading = null!;

	[Inject] 
	private FadeController Fade = null!;

	[Inject] 
	private Tooltip Tooltip = null!;

	private Pallete SpawnedPallete = null;
	private Coroutine restartRoutine;

	private void Start()
	{
		// Плавное появление
		Fade.SetFade(1, 0, 2, 1);
		Tooltip.Text = "Startup Engine [T]";

		// Спавн паллеты
		Loading.SpawnPallete(ref SpawnedPallete);
	}

	[Inject]
	public void Construct()
	{
		// Подписки на события
		Unloading.Delivered += Delivered;
		Forklift.EngineChangeState += OnEngineChangeState;
		Forklift.PalleteLocked += OnPalleteLocked;
		Forklift.FuelEnded += OnFuelEnded;
	}


	/// <summary>
	/// Обработчик окончания топлива
	/// </summary>
	private void OnFuelEnded()
	{
		if (restartRoutine != null)
			StopCoroutine(restartRoutine);

		restartRoutine = StartCoroutine(RestartRoutine(1, 3));
	}

	/// <summary>
	/// Короутина перезапуска сцены
	/// </summary>
	private IEnumerator RestartRoutine(float duration, float delay = 0)
	{
		// Запуск затемнения
		Fade.SetFade(0, 1, duration, delay);

		// Ожидание завершения затемнения
		yield return new WaitForSeconds(duration + delay);

		if (SpawnedPallete != null)
		{
			Destroy(SpawnedPallete.gameObject);
			SpawnedPallete = null;
		}

		// Перезапуск погрузчика
		Forklift.Restart();

		// Спавн новой паллеты
		Loading.SpawnPallete(ref SpawnedPallete);

		// Плавное появление
		Fade.SetFade(1, 0, duration);
	}

	/// <summary>
	/// Обработчик изменения состояния двигателя
	/// </summary>
	private void OnEngineChangeState(bool state)
	{
		if (state)
		{
			Tooltip.Text = "Move [WASD] | Up/Down [Q/E]";
			return;
		}

		if (Forklift.Fuel > 0)
		{
			Tooltip.Text = "Startup Engine [T]";
			return;
		}

		Tooltip.Text = "Fuel ended! Restarting...";
	}

	/// <summary>
	/// Обработчик изменения состояния фиксации паллеты
	/// </summary>
	private void OnPalleteLocked(bool locked)
	{
		if (!Forklift.EngineState)
			return;

		if (locked)
		{
			Tooltip.Text = "Release Pallete [E] on [Unloading Zone]";
			return;
		}

		Tooltip.Text = "Pickup Pallete [Q] on [Loading Zone]";
	}

	/// <summary>
	/// Обработчик успешной доставки паллеты
	/// </summary>
	private async void Delivered()
	{
		// Текст успешной доставки
		Tooltip.Text = "Delivered!";

		// Анимация выгрузки
		await SpawnedPallete.Anim(PalleteAnimType.UnloadingZone);

		// Спавн новой паллеты
		Loading.SpawnPallete(ref SpawnedPallete);

		// Разрешаем повторную активацию зоны
		Unloading.SetTriggerState(true);
	}
}