using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Pallete : MonoBehaviour
{
	private void Awake()
	{
		Rb = GetComponent<Rigidbody>();

		Colliders = new List<BoxCollider>();
		foreach (var collider in GetComponentsInChildren<BoxCollider>(true))
		{
			Colliders.Add(collider);
		}
	}
	List<BoxCollider> Colliders;

	public bool IsKinematic
	{
		get => Rb.isKinematic;
		set
		{
			Rb.isKinematic = value;

			foreach (var collider in Colliders)
			{
				collider.isTrigger = value;
			}
		}
	}

	private Rigidbody Rb;
}
