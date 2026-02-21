using UnityEngine;
using UniRx;
using System;

public class ZoneUnloading : MonoBehaviour
{
	/// <summary>
	/// Поток событий доставки паллеты
	/// </summary>
	private Subject<Pallete> deliveredSubject = new Subject<Pallete>();

	/// <summary>
	/// Публичный IObservable для подписки на доставку паллет
	/// </summary>
	public IObservable<Pallete> Delivered => deliveredSubject;

	/// <summary>
	/// Коллайдер зоны выгрузки
	/// </summary>
	private BoxCollider trigger;

	private void Awake()
	{
		trigger = GetComponent<BoxCollider>();
		if (trigger == null)
		{
			Debug.LogError("ZoneUnloading: BoxCollider не найден на объекте!");
		}
		else
		{
			trigger.isTrigger = true;
		}
	}

	private void OnTriggerStay(Collider other)
	{
		// Проверяем наличие компонента Pallete
		if (!other.TryGetComponent(out Pallete pallete))
			return;

		// Игнорируем, если паллета заблокирована или поднята выше зоны
		if (pallete.Locked || pallete.transform.position.y > 0.01f)
			return;

		// Временно отключаем триггер, чтобы событие не сработало повторно
		SetTriggerState(false);

		// Публикуем событие в UniRx
		deliveredSubject.OnNext(pallete);
	}

	/// <summary>
	/// Включение/выключение триггера зоны
	/// </summary>
	/// <param name="enabled">Состояние коллайдера</param>
	public void SetTriggerState(bool enabled)
	{
		if (trigger != null)
			trigger.enabled = enabled;
	}

	private void OnDestroy()
	{
		deliveredSubject.OnCompleted();
		deliveredSubject.Dispose();
	}
}