using UnityEngine;
using Zenject;
using UniRx;
using Cysharp.Threading.Tasks;

public class GameManager : MonoBehaviour
{
	[Inject] private ForkliftController forklift = null!;
	[Inject] private ZoneLoading zoneLoading = null!;
	[Inject] private ZoneUnloading zoneUnloading = null!;
	[Inject] private FadeController fade = null!;
	[Inject] private Tooltip tooltip = null!;

	private Pallete currentPallete;
	private CompositeDisposable disposables = new CompositeDisposable();

	private void Start()
	{
		// Начальное затемнение
		fade.SetFade(1f, 0f, 2f, 1f);
		tooltip.Text = "Startup Engine [T]";

		SpawnNewPallete();

		BindStreams();
	}

	private void BindStreams()
	{
		// Подписка на состояние двигателя
		forklift.EngineChangeState
			.AsObservable()
			.Subscribe(OnEngineStateChanged)
			.AddTo(disposables);

		// Подписка на фиксацию паллеты
		forklift.PalleteLocked
			.AsObservable()
			.Subscribe(OnPalleteLocked)
			.AddTo(disposables);

		// Подписка на окончание топлива
		forklift.FuelEnded
			.AsObservable()
			.Subscribe(_ => RestartAsync().Forget())
			.AddTo(disposables);

		// Подписка на доставку паллеты
		zoneUnloading.Delivered
			.AsObservable()
			.Subscribe(_ => DeliveredAsync().Forget())
			.AddTo(disposables);
	}

	private async void SpawnNewPallete()
	{
		// Создаём новую паллету и подписываемся на завершение анимации
		currentPallete = zoneLoading.SpawnPalleteAsync();

		// Запускаем анимацию загрузки
		await currentPallete.Anim(PalleteAnimType.LoadingZone);
	}

	private async UniTask RestartAsync()
	{
		tooltip.Text = "Fuel ended! Restarting...";

		// Плавное затемнение
		await fade.FadeAsync(0f, 1f, 1f);

		if (currentPallete != null)
		{
			Destroy(currentPallete.gameObject);
			currentPallete = null;
		}

		forklift.Restart();

		SpawnNewPallete();

		// Плавное появление
		await fade.FadeAsync(1f, 0f, 1f);
	}

	private async UniTask DeliveredAsync()
	{
		tooltip.Text = "Delivered!";

		if (currentPallete != null)
		{
			// Анимация выгрузки паллеты
			await currentPallete.Anim(PalleteAnimType.UnloadingZone);

			Destroy(currentPallete.gameObject);
		}

		SpawnNewPallete();

		zoneUnloading.SetTriggerState(true);
	}

	private void OnEngineStateChanged(bool state)
	{
		if (state)
		{
			tooltip.Text = "Move [WASD] | Up/Down [Q/E]";
			return;
		}

		if (forklift.Fuel.Value > 0)
		{
			tooltip.Text = "Startup Engine [T]";
			return;
		}

		tooltip.Text = "Fuel ended! Restarting...";
	}

	private void OnPalleteLocked(bool locked)
	{
		if (!forklift.EngineState)
			return;

		tooltip.Text = locked
			? "Release Pallete [E] on [Unloading Zone]"
			: "Pickup Pallete [Q] on [Loading Zone]";
	}

	private void OnDestroy()
	{
		disposables.Dispose();
	}
}