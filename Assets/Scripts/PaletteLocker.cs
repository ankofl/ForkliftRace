using System;
using UnityEngine;

public enum PalleteLockerState { Stay, Down, Up }

public class PaletteLocker : MonoBehaviour
{
	public PalleteLockerState State
	{
		get => _state;
		set
		{
			_state = value;

			if (_state == PalleteLockerState.Down)
			{
				var pallete = transform.GetComponentInChildren<Pallete>(true);
				if(pallete != null)
				{
					pallete.Lock(null);
					Locked?.Invoke(false);
				}
			}
		}
	}
	private PalleteLockerState _state;

	public Action<bool> Locked;

	private void OnTriggerStay(Collider other)
	{
		if (_state != PalleteLockerState.Up)
			return;

		// Проверяем родителя коллайдера
		if (!other.TryGetComponent<Pallete>(out var pallete))
			return;

		// Фиксируем
		pallete.Lock(transform);

		Locked?.Invoke(true);
	}
}