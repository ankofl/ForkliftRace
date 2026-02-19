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
				// Находим все подцепленные паллеты (на случай, если их несколько)
				var children = transform.GetComponentsInChildren<Pallete>(true);

				foreach (var pallete in children)
				{
					pallete.Lock(null);
				}
			}
		}
	}
	private PalleteLockerState _state;

	private void OnTriggerStay(Collider other)
	{
		if (_state != PalleteLockerState.Up)
			return;

		// Проверяем родителя коллайдера
		if (!other.TryGetComponent<Pallete>(out var pallete))
			return;

		// Фиксируем
		pallete.Lock(transform);
	}
}