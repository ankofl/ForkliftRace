using System;
using UnityEngine;

/// <summary>
/// Возможные состояния механизма фиксации паллеты
/// </summary>
public enum PalleteLockerState
{
	/// <summary>
	/// Ожидание без движения
	/// </summary>
	Stay,

	/// <summary>
	/// Опускание (разблокировка)
	/// </summary>
	Down,

	/// <summary>
	/// Поднятие (готовность к фиксации)
	/// </summary>
	Up
}

/// <summary>
/// Компонент фиксации паллеты на вилах погрузчика
/// </summary>
public class PalleteLocker : MonoBehaviour
{
	/// <summary>
	/// Текущее состояние фиксатора паллеты
	/// </summary>
	public PalleteLockerState State
	{
		get => _state;
		set
		{
			_state = value;

			if (_state != PalleteLockerState.Down)
				return;

			// Ищем паллету среди дочерних объектов
			Pallete pallete = transform.GetComponentInChildren<Pallete>(true);

			if (pallete == null)
				return;

			// Снимаем фиксацию
			pallete.Lock(null);

			// Уведомляем об изменении состояния
			Locked?.Invoke(false);
		}
	}

	/// <summary>
	/// Внутреннее поле хранения состояния фиксатора
	/// </summary>
	private PalleteLockerState _state;

	/// <summary>
	/// Событие изменения состояния фиксации паллеты (true — зафиксирована)
	/// </summary>
	public Action<bool> Locked;

	/// <summary>
	/// Обработка нахождения паллеты в зоне триггера фиксатора
	/// </summary>
	private void OnTriggerStay(Collider other)
	{
		if (_state != PalleteLockerState.Up)
			return;

		// Проверяем наличие компонента паллеты
		if (!other.TryGetComponent(out Pallete pallete))
			return;

		// Фиксируем паллету на текущем трансформе
		pallete.Lock(transform);

		// Уведомляем об успешной фиксации
		Locked?.Invoke(true);
	}
}