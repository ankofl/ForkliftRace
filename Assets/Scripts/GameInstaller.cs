using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller
{
	[SerializeField]
	private Pallete palletePrefab;

	public override void InstallBindings()
	{
		// Singleton для GameManager
		Container.Bind<GameManager>()
			.FromComponentInHierarchy()
			.AsSingle()
			.NonLazy();

		// Компоненты сцены
		Container.Bind<ForkliftController>()
			.FromComponentInHierarchy()
			.AsSingle()
			.NonLazy();

		Container.Bind<PalleteLocker>()
			.FromComponentInHierarchy()
			.AsSingle()
			.NonLazy();

		Container.Bind<ZoneLoading>()
			.FromComponentInHierarchy()
			.AsSingle()
			.NonLazy();

		Container.Bind<ZoneUnloading>()
			.FromComponentInHierarchy()
			.AsSingle()
			.NonLazy();

		Container.Bind<FadeController>()
			.FromComponentInHierarchy()
			.AsSingle()
			.NonLazy();

		Container.Bind<Tooltip>()
			.FromComponentInHierarchy()
			.AsSingle()
			.NonLazy();

		Container.BindFactory<Pallete, Pallete.Factory>()
		 .FromComponentInNewPrefab(palletePrefab)
		 .AsSingle();
	}
}