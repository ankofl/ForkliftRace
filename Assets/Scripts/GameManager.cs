using System.Collections;
using UnityEngine;

/// <summary>
/// Главный менеджер игры, управляющий состояниями, UI и перезапуском
/// </summary>
public class GameManager : MonoBehaviour
{
	[Header("Основные ссылки")]
	/// <summary>
	/// Основная камера сцены
	/// </summary>
	[SerializeField]
	private Camera MainCamera;

	/// <summary>
	/// Контроллер затемнения экрана
	/// </summary>
	[SerializeField]
	private FadeController Fade;

	/// <summary>
	/// Текстовая подсказка игроку
	/// </summary>
	[SerializeField]
	private TextMesh Tooltip;

	/// <summary>
	/// Контроллер погрузчика
	/// </summary>
	[SerializeField]
	private ForkliftController Forklift;

	/// <summary>
	/// Зона загрузки паллет
	/// </summary>
	[SerializeField]
	private ZoneLoading Loading;

	/// <summary>
	/// Зона выгрузки паллет
	/// </summary>
	[SerializeField]
	private ZoneUnloading Unloading;

	/// <summary>
	/// Текущая заспавненная паллета
	/// </summary>
	private Pallete SpawnedPallete = null;

	/// <summary>
	/// Короутина перезапуска
	/// </summary>
	private Coroutine restartRoutine;

	/// <summary>
	/// Инициализация менеджера игры
	/// </summary>
	private void Awake()
	{
		// Получаем основную камеру
		MainCamera = Camera.main;

		// Получаем компонент затемнения
		Fade = MainCamera.GetComponentInChildren<FadeController>();

		// Запускаем плавное появление
		Fade.SetFade(1, 0, 2, 1);

		// Получаем текст подсказки
		Tooltip = MainCamera.GetComponentInChildren<TextMesh>();

		// Начальный текст
		Tooltip.text = "Startup Engine [T]";

		// Спавним первую паллету
		Loading.SpawnPallete(ref SpawnedPallete);

		// Подписка на событие доставки
		Unloading.Delivered += Delivered;

		// Подписки на события погрузчика
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
			Tooltip.text = "Move [WASD] | Up/Down [Q/E]";
			return;
		}

		if (Forklift.Fuel > 0)
		{
			Tooltip.text = "Startup Engine [T]";
			return;
		}

		Tooltip.text = "Fuel ended! Restarting...";
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
			Tooltip.text = "Release Pallete [E] on [Unloading Zone]";
			return;
		}

		Tooltip.text = "Pickup Pallete [Q] on [Loading Zone]";
	}

	/// <summary>
	/// Обработчик успешной доставки паллеты
	/// </summary>
	private async void Delivered()
	{
		// Текст успешной доставки
		Tooltip.text = "Delivered!";

		// Анимация выгрузки
		await SpawnedPallete.Anim(PalleteAnimType.UnloadingZone);

		// Спавн новой паллеты
		Loading.SpawnPallete(ref SpawnedPallete);

		// Разрешаем повторную активацию зоны
		Unloading.SetTriggerState(true);
	}
}