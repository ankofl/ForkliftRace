using System;
using UniRx;
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
	private PalleteLockerState _state;

	// UniRx поток изменений фиксации
	private Subject<bool> _lockedSubject = new Subject<bool>();
	public IObservable<bool> LockedStream => _lockedSubject;

	public PalleteLockerState State
	{
		get => _state;
		set
		{
			_state = value;

			if (_state != PalleteLockerState.Down)
				return;

			Pallete pallete = transform.GetComponentInChildren<Pallete>(true);
			if (pallete == null) return;

			pallete.Lock(null);

			// Сообщаем через поток
			_lockedSubject.OnNext(false);
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (_state != PalleteLockerState.Up)
			return;

		if (!other.TryGetComponent(out Pallete pallete))
			return;

		pallete.Lock(transform);

		// Сообщаем через поток
		_lockedSubject.OnNext(true);
	}

	private void OnDestroy()
	{
		_lockedSubject.OnCompleted();
		_lockedSubject.Dispose();
	}
}