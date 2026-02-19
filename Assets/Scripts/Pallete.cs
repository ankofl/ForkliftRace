using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

[RequireComponent(typeof(Rigidbody))]
public class Pallete : MonoBehaviour
{
	private void Awake()
	{
		Rb = GetComponent<Rigidbody>();

		Colliders = new List<BoxCollider>();
		foreach (var collider in GetComponentsInChildren<BoxCollider>(true))
		{
			if(collider.name != "Cargo")
			{
				Colliders.Add(collider);
			}
		}
	}
	List<BoxCollider> Colliders;

	public void Lock(Transform tran)
	{
		Locked = tran != null;

		Rb.isKinematic = Locked;

		foreach (var collider in Colliders)
		{
			collider.isTrigger = Locked;
		}

		transform.SetParent(tran);
	}

	public bool Locked { get; private set; }

	private Rigidbody Rb;
}
