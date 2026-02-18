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
					// 1. Отцепляем от родителя
					pallete.transform.SetParent(null);

					// 2. Очень важно - возвращаем физику в нормальное состояние
					pallete.IsKinematic = false;
				}
			}
		}
	}
	private PalleteLockerState _state;

	private void OnTriggerStay(Collider other)
	{
		if (_state != PalleteLockerState.Up)
			return;

		if (other == null || other.transform == null || other.transform.parent == null)
			return;

		// Проверяем родителя коллайдера
		if (!other.transform.parent.TryGetComponent<Pallete>(out var pallete))
			return;

		if (Vector3.Distance(transform.position, pallete.transform.position) > 0.2f ||
			Mathf.Abs(transform.position.y - pallete.transform.position.y) > 0.1f)
			return;

		// Фиксируем
		pallete.IsKinematic = true;
		pallete.transform.SetParent(transform);
	}
}