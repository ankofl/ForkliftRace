using System;
using UnityEngine;

public class ZoneUnloading : MonoBehaviour
{
	private void Awake()
	{
		Trigger = GetComponent<BoxCollider>();
	}
	private BoxCollider Trigger;

	private void OnTriggerStay(Collider other)
	{
		if (!other.TryGetComponent(out Pallete pallete))
			return;

		if (pallete.Locked)
			return;

		SetTriggerState(false);

		Delivered?.Invoke(pallete);
	}

	public void SetTriggerState(bool enabled)
	{
		Trigger.enabled = enabled;
	}

	public Action<Pallete> Delivered;
}
