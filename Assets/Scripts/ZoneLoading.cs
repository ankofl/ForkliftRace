using System;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class ZoneLoading : MonoBehaviour
{
	private Pallete.Factory palleteFactory;

	[Inject]
	public void Construct(Pallete.Factory factory)
	{
		palleteFactory = factory;
	}

	/// <summary>
	/// Создаёт паллету и ждёт окончания анимации загрузки
	/// </summary>
	public Pallete SpawnPalleteAsync()
	{
		// Создаём паллету
		Pallete pallete = palleteFactory.Create();
		pallete.transform.position = transform.position;
		pallete.transform.rotation = Quaternion.identity;

		return pallete;
	}
}