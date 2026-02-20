using UnityEngine;

public class ZoneLoading : MonoBehaviour
{
	/// <summary>
	/// Префаб паллеты для спавна
	/// </summary>
	[SerializeField, Tooltip("Префаб паллеты для спавна")]
	private Pallete palletePrefab;

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

		// Создаём новую паллету
		pallete = Instantiate(palletePrefab, transform.position, Quaternion.identity);

		// Запускаем анимацию подъёма для паллеты
		pallete.Anim(PalleteAnimType.LoadingZone);
	}
}