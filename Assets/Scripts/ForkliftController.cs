using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

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
	private float CurrentMaxSpeedK = 1;
	private float MoveSpeed;

	private float Tachos;

	[SerializeField]
	private float MaxTachos = 5f;
	[SerializeField]
	private float MinTachos = 1f;

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

	[SerializeField]
	private float MaxFuel = 60;
	public float Fuel { get; private set; }


	[SerializeField]
	private PaletteLocker Locker;

	[SerializeField]
	private DialsPanel Dials;

	private void Awake()
	{
		Fuel = MaxFuel;

		Rg = GetComponent<Rigidbody>();
		Locker.Locked += (locked) => PalleteLocked.Invoke(locked);

		StartPosition = transform.position;
		StartRotation = transform.rotation;
	}
	private Rigidbody Rg;

	private Vector3 StartPosition;
	private Quaternion StartRotation;

	public Action<bool> PalleteLocked;

	public Action<bool> EngineChangeState;

	public Action FuelEnded;

	public bool EngineState
	{
		get => _engineState;
		set
		{
			_engineState = value;
			EngineChangeState?.Invoke(EngineState);
		}
	}
	private bool _engineState = false;

	private void LateUpdate()
	{
		if (Camera.main == null || Mouse.current == null) return;

		if (Camera.main.transform.parent == null)
		{
			Camera.main.transform.parent = CamTran;
			Camera.main.transform.localPosition = Vector3.zero;
			Camera.main.transform.localRotation = Quaternion.identity;
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

	public void Restart()
	{
		Fuel = MaxFuel;
		EngineState = false;

		transform.SetPositionAndRotation(StartPosition, StartRotation);
		fork.transform.localPosition = new Vector3(0, 0, 0);
		mast.transform.localPosition = new Vector3(0, 0, 0);
		Locker.State = PalleteLockerState.Stay;
	}

	void FixedUpdate()
	{
		if (Keyboard.current == null) return;

		var currentMaxSpeed = MaxSpeed * CurrentMaxSpeedK;

		if (Keyboard.current.tKey.wasPressedThisFrame && Fuel > 0)
		{
			EngineState = !EngineState;
		}

		if (Keyboard.current.qKey.isPressed && EngineState)
		{
			Locker.State = PalleteLockerState.Up;

			fork.transform.localPosition = Vector3.MoveTowards(fork.transform.localPosition,
					new Vector3(0, 3f, 0), MaxSpeedLift * Time.fixedDeltaTime);


			if (fork.transform.localPosition.y > 1.5f)
			{
				mast.transform.localPosition = Vector3.MoveTowards(mast.transform.localPosition,
				new Vector3(0, 1.5f, 0), MaxSpeedLift * Time.fixedDeltaTime);
			}
		}
		else if (Keyboard.current.eKey.isPressed || !EngineState)
		{
			Locker.State = PalleteLockerState.Down;

			fork.transform.localPosition = Vector3.MoveTowards(fork.transform.localPosition,
				Vector3.zero, MaxSpeedLift * Time.fixedDeltaTime);
			mast.transform.localPosition = Vector3.MoveTowards(mast.transform.localPosition,
				Vector3.zero, MaxSpeedLift * Time.fixedDeltaTime);
		}
		else
		{
			Locker.State = PalleteLockerState.Stay;
		}


		if (Keyboard.current.wKey.isPressed && EngineState)
		{
			if (MoveSpeed < 0)
			{
				Tachos = Mathf.Clamp(Tachos - 5 * Time.fixedDeltaTime, MinTachos, MaxTachos);
				MoveSpeed = Mathf.Clamp(MoveSpeed + 5 * Time.fixedDeltaTime, -currentMaxSpeed, 0);
			}
			else
			{
				Tachos = Mathf.Clamp(Tachos + 5 * Time.fixedDeltaTime, MinTachos, MaxTachos);
				MoveSpeed = Mathf.Clamp(MoveSpeed + Tachos * Time.fixedDeltaTime, -currentMaxSpeed, currentMaxSpeed);
			}
		}
		else if (Keyboard.current.sKey.isPressed && EngineState)
		{
			if(MoveSpeed > 0)
			{
				Tachos = Mathf.Clamp(Tachos - 5 * Time.fixedDeltaTime, MinTachos, MaxTachos);
				MoveSpeed = Mathf.Clamp(MoveSpeed - 5 * Time.fixedDeltaTime, 0, currentMaxSpeed);
			}
			else
			{
				Tachos = Mathf.Clamp(Tachos + 5 * Time.fixedDeltaTime, MinTachos, MaxTachos);
				MoveSpeed = Mathf.Clamp(MoveSpeed - Tachos * Time.fixedDeltaTime, -currentMaxSpeed, currentMaxSpeed);
			}
		}
		else
		{
			if (EngineState)
			{
				if(Tachos < MinTachos)
				{
					Tachos = Mathf.Clamp(Tachos + 5 * Time.fixedDeltaTime, 0, MinTachos);
				}
				else
				{
					Tachos = Mathf.Clamp(Tachos - 5 * Time.fixedDeltaTime, MinTachos, MaxTachos);
				}
			}
			else
			{
				Tachos = Mathf.Clamp(Tachos - 5 * Time.fixedDeltaTime, 0, MaxTachos);
			}

			if (MoveSpeed > 0)
			{
				MoveSpeed = Mathf.Clamp(MoveSpeed - 1 * Time.fixedDeltaTime, 0, currentMaxSpeed);
			}
			else if (MoveSpeed < 0)
			{
				MoveSpeed = Mathf.Clamp(MoveSpeed + 1 * Time.fixedDeltaTime, -currentMaxSpeed, 0);
			}
		}

		Fuel = Mathf.Clamp(Fuel - Tachos / MaxTachos * Time.fixedDeltaTime, 0, MaxFuel);
		if(Fuel == 0 && EngineState)
		{
			EngineState = false;
			FuelEnded?.Invoke();
		}
		else if (Fuel <= MaxFuel / 2)
		{
			CurrentMaxSpeedK = Mathf.Clamp(CurrentMaxSpeedK - Time.fixedDeltaTime, 0.5f, 1);
		}


		Dials.SetValues(Tachos / MaxTachos, MathF.Abs(MoveSpeed) / MaxSpeed, Fuel / MaxFuel);


		float h = 0f;
		if (Keyboard.current.dKey.isPressed) h += 1f;
		if (Keyboard.current.aKey.isPressed) h -= 1f;

		var steerSpeed = 2f;

		if (h == 0)
		{
			// возвращаем руль в исходное положение
			if (StearAngle > 0)
			{
				StearAngle = Mathf.Clamp(StearAngle - steerSpeed * Time.fixedDeltaTime, 0, 1);
			}
			else if (StearAngle < 0)
			{
				StearAngle = Mathf.Clamp(StearAngle + steerSpeed * Time.fixedDeltaTime, -1, 0);
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

				StearAngle = Mathf.Clamp(StearAngle + steerSpeed * Time.fixedDeltaTime, -1, 1);
			}
			else if (h < 0)
			{
				if (StearAngle > 0)
				{
					steerSpeed = 1;
				}

				StearAngle = Mathf.Clamp(StearAngle - steerSpeed * Time.fixedDeltaTime, -1, 1);
			}
		}

		var steerWheelAngle = StearAngle * 35;
		var steerAngle = StearAngle * 270;

		// ─── Применяем визуально к модели руля ─────────────────────────────
		Stear.localRotation = Quaternion.Euler(0, steerAngle, 0);


		// ---------- Передние колёса ----------

		WheelForwardLeft.SetRotation(MoveSpeed, steerWheelAngle);

		WheelForwardRight.SetRotation(MoveSpeed, steerWheelAngle);

		// ---------- Задние колёса ----------

		WheelBackwardLeft.SetRotation(MoveSpeed, 0);

		WheelBackwardRight.SetRotation(MoveSpeed, 0);

		// Преобразуем в глобальные координаты
		Vector3 globalVelocity = transform.TransformDirection(new Vector3(0f, 0f, MoveSpeed));

		// Применяем линейную и угловую скорость к Rigidbody
		Rg.linearVelocity = new Vector3(globalVelocity.x, Rg.linearVelocity.y, globalVelocity.z); // сохраняем y для гравитации

		// ---------- Поворот трансформа погрузчика ----------
		float turnRadius = 2f; // расстояние между передней и задней осью (wheelBase)
		float angularY = 0f;

		if (Mathf.Abs(steerWheelAngle) > 0.01f)
		{
			// Используем формулу Ackermann через угол рулевых колес
			float steerRad = steerWheelAngle * Mathf.Deg2Rad;
			angularY = (MoveSpeed / turnRadius) * Mathf.Tan(steerRad) * Mathf.Rad2Deg * Time.fixedDeltaTime;
		}

		transform.Rotate(0f, angularY, 0f, Space.World);
	}
}
