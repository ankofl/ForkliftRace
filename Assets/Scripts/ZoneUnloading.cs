using System;
using UnityEngine;

public class ZoneUnloading : MonoBehaviour
{
	/// <summary>
	/// Событие срабатывает при доставке паллеты в зону
	/// </summary>
	public Action Delivered;

	/// <summary>
	/// Коллайдер зоны выгрузки
	/// </summary>
	private BoxCollider trigger;

	/// <summary>
	/// Инициализация
	/// </summary>
	private void Awake()
	{
		// Получаем коллайдер зоны
		trigger = GetComponent<BoxCollider>();
	}

	/// <summary>
	/// Проверка нахождения паллеты в зоне
	/// </summary>
	/// <param name="other">Коллайдер объекта</param>
	private void OnTriggerStay(Collider other)
	{
		// Проверяем, есть ли паллета
		if (!other.TryGetComponent(out Pallete pallete))
			return;

		// Игнорируем, если паллета заблокирована или поднята
		if (pallete.Locked || pallete.transform.position.y > 0.01f)
			return;

		// Отключаем триггер временно
		SetTriggerState(false);

		// Вызываем событие доставки
		Delivered?.Invoke();
	}

	/// <summary>
	/// Включение/выключение триггера зоны
	/// </summary>
	/// <param name="enabled">Состояние коллайдера</param>
	public void SetTriggerState(bool enabled)
	{
		trigger.enabled = enabled;
	}
}