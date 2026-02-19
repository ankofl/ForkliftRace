using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System;

[RequireComponent(typeof(Rigidbody))]
public class ForkliftController : MonoBehaviour
{
	[SerializeField]
	private float sensitivity = 0.3f;

	[SerializeField]
	private Transform CamTran;

	[SerializeField]
	private Transform fork;
	[SerializeField]
	private Transform mast;

	[SerializeField]
	private float MaxSpeedLift = 0.3f;

	[SerializeField]
	private float MaxSpeed = 5f;

	[SerializeField]
	private Transform Stear;

	private float StearAngle;

	[SerializeField]
	private Wheel WheelForwardLeft;
	[SerializeField]
	private Wheel WheelForwardRight;
	[SerializeField]
	private Wheel WheelBackwardLeft;
	[SerializeField]
	private Wheel WheelBackwardRight;

	private float WheelSpeed;
	private float WheelVelocity;

	[SerializeField]
	private PaletteLocker Locker;

	private void Awake()
	{
		Rg = GetComponent<Rigidbody>();
		Locker = GetComponentInChildren<PaletteLocker>();
		Locker.Locked += (locked) => PalleteLocked.Invoke(locked);
	}
	private Rigidbody Rg;

	public Action<bool> PalleteLocked;

	public Action<bool> EngineChangeState;

	public bool EngineState { get; private set; }

	private void LateUpdate()
	{
		if (Camera.main == null || Mouse.current == null) return;

		if (Camera.main.transform.parent == null)
		{
			Camera.main.transform.parent = CamTran;
			Camera.main.transform.localPosition = Vector3.zero;
			Camera.main.transform.localRotation = Quaternion.identity;
		}

		if (Keyboard.current.tKey.wasPressedThisFrame)
		{
			EngineState = !EngineState;

			EngineChangeState?.Invoke(EngineState);
		}

		var delta = Mouse.current.delta.ReadValue();

		// Берём текущие углы
		Vector3 angles = Camera.main.transform.localEulerAngles;

		float pitch = NormalizeAngle(angles.x);
		float yaw = NormalizeAngle(angles.y);

		// Добавляем дельту
		yaw += delta.x * sensitivity;
		pitch -= delta.y * sensitivity;

		// Ограничения
		yaw = Mathf.Clamp(yaw, -120f, 120f);
		pitch = Mathf.Clamp(pitch, -70f, 70f);

		Camera.main.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
	}

	private float NormalizeAngle(float angle)
	{
		if (angle > 180f)
			angle -= 360f;

		return angle;
	}


	void FixedUpdate()
	{
		if (Keyboard.current == null) return;

		if (Keyboard.current.qKey.isPressed && EngineState)
		{
			Locker.State = PalleteLockerState.Up;

			fork.transform.localPosition = Vector3.MoveTowards(fork.transform.localPosition,
					new Vector3(0, 3f, 0), MaxSpeedLift * Time.deltaTime);


			if (fork.transform.localPosition.y > 1.5f)
			{
				mast.transform.localPosition = Vector3.MoveTowards(mast.transform.localPosition,
				new Vector3(0, 1.5f, 0), MaxSpeedLift * Time.deltaTime);
			}
		}
		else if (Keyboard.current.eKey.isPressed || !EngineState)
		{
			Locker.State = PalleteLockerState.Down;

			fork.transform.localPosition = Vector3.MoveTowards(fork.transform.localPosition,
				Vector3.zero, MaxSpeedLift * Time.deltaTime);
			mast.transform.localPosition = Vector3.MoveTowards(mast.transform.localPosition,
				Vector3.zero, MaxSpeedLift * Time.deltaTime);
		}
		else
		{
			Locker.State = PalleteLockerState.Stay;
		}


		if (Keyboard.current.wKey.isPressed && EngineState)
		{
			WheelSpeed = Mathf.Clamp(WheelSpeed + 5 * Time.deltaTime, -MaxSpeed, MaxSpeed);
		}
		else if (Keyboard.current.sKey.isPressed && EngineState)
		{
			WheelSpeed = Mathf.Clamp(WheelSpeed - 5 * Time.deltaTime, -MaxSpeed, MaxSpeed);
		}
		else
		{
			if (WheelSpeed > 0)
			{
				WheelSpeed = Mathf.Clamp(WheelSpeed - 1 * Time.deltaTime, 0, MaxSpeed);
			}
			else if (WheelSpeed < 0)
			{
				WheelSpeed = Mathf.Clamp(WheelSpeed + 1 * Time.deltaTime, -MaxSpeed, 0);
			}
		}



		float h = 0f;
		if (Keyboard.current.dKey.isPressed) h += 1f;
		if (Keyboard.current.aKey.isPressed) h -= 1f;

		var steerSpeed = 2f;

		if (h == 0)
		{
			// возвращаем руль в исходное положение
			if (StearAngle > 0)
			{
				StearAngle = Mathf.Clamp(StearAngle - steerSpeed * Time.deltaTime, 0, 1);
			}
			else if (StearAngle < 0)
			{
				StearAngle = Mathf.Clamp(StearAngle + steerSpeed * Time.deltaTime, -1, 0);
			}
		}
		else if (EngineState)
		{
			if (h > 0)
			{
				if (StearAngle < 0)
				{
					steerSpeed = 1;
				}

				StearAngle = Mathf.Clamp(StearAngle + steerSpeed * Time.deltaTime, -1, 1);
			}
			else if (h < 0)
			{
				if (StearAngle > 0)
				{
					steerSpeed = 1;
				}

				StearAngle = Mathf.Clamp(StearAngle - steerSpeed * Time.deltaTime, -1, 1);
			}
		}

		var steerWheelAngle = StearAngle * 35;
		var steerAngle = StearAngle * 270;

		// ─── Применяем визуально к модели руля ─────────────────────────────
		Stear.localRotation = Quaternion.Euler(0, steerAngle, 0);


		// ---------- Передние колёса ----------

		WheelForwardLeft.SetRotation(WheelSpeed, steerWheelAngle);

		WheelForwardRight.SetRotation(WheelSpeed, steerWheelAngle);

		// ---------- Задние колёса ----------

		WheelBackwardLeft.SetRotation(WheelSpeed, 0);

		WheelBackwardRight.SetRotation(WheelSpeed, 0);

		// Преобразуем в глобальные координаты
		Vector3 globalVelocity = transform.TransformDirection(new Vector3(0f, 0f, WheelSpeed));

		// Применяем линейную и угловую скорость к Rigidbody
		Rg.linearVelocity = new Vector3(globalVelocity.x, Rg.linearVelocity.y, globalVelocity.z); // сохраняем y для гравитации

		// ---------- Поворот трансформа погрузчика ----------
		float turnRadius = 2f; // расстояние между передней и задней осью (wheelBase)
		float angularY = 0f;

		if (Mathf.Abs(steerWheelAngle) > 0.01f)
		{
			// Используем формулу Ackermann через угол рулевых колес
			float steerRad = steerWheelAngle * Mathf.Deg2Rad;
			angularY = (WheelSpeed / turnRadius) * Mathf.Tan(steerRad) * Mathf.Rad2Deg * Time.fixedDeltaTime;
		}

		transform.Rotate(0f, angularY, 0f, Space.World);
	}
}
