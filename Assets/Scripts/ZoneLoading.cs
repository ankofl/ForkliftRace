using UnityEngine;
using Zenject;

public class ZoneLoading : MonoBehaviour
{
	private Pallete.Factory palleteFactory;

	// В конструкторе инжектируем только фабрику, а не конкретную паллету
	[Inject]
	public void Construct(Pallete.Factory factory)
	{
		palleteFactory = factory;
	}

	/// <summary>
	/// Создаёт новую паллету на позиции зоны загрузки
	/// </summary>
	/// <param name="pallete">Ссылка на текущую паллету</param>
	public void SpawnPallete(ref Pallete pallete)
	{
		// Удаляем старую паллету, если она существует
		if (pallete != null)
		{
			Destroy(pallete.gameObject);
		}

		// Создаём новую паллету через фабрику
		pallete = palleteFactory.Create();
		pallete.transform.position = transform.position;
		pallete.transform.rotation = Quaternion.identity;

		// Запускаем анимацию подъёма
		pallete.Anim(PalleteAnimType.LoadingZone);
	}
}