using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;
using UniRx;

[RequireComponent(typeof(Rigidbody))]
public class ForkliftController : MonoBehaviour
{
	[Header("Камера")]
	[SerializeField] private float sensitivity = 0.3f;
	[SerializeField] private Transform CamTran;

	[Header("Вилы и мачта")]
	[SerializeField] private Transform fork;
	[SerializeField] private Transform mast;
	[SerializeField] private float MaxSpeedLift = 0.3f;

	[Header("Движение")]
	[SerializeField] private float MaxSpeed = 5f;
	private float CurrentMaxSpeedK = 1;
	private float MoveSpeed;
	private float Tachos;
	[SerializeField] private float MaxTachos = 5f;
	[SerializeField] private float MinTachos = 1f;

	[Header("Руль")]
	[SerializeField] private Transform Stear;
	private float StearAngle;

	[Header("Колеса")]
	[SerializeField] private Wheel WheelForwardLeft;
	[SerializeField] private Wheel WheelForwardRight;
	[SerializeField] private Wheel WheelBackwardLeft;
	[SerializeField] private Wheel WheelBackwardRight;

	[Header("Топливо")]
	[SerializeField] private float MaxFuel = 60f;
	public ReactiveProperty<float> Fuel { get; } = new ReactiveProperty<float>();

	[Inject] private PalleteLocker Locker;

	[SerializeField] private DialsPanel Dials;

	private Rigidbody Rg;
	private Vector3 StartPosition;
	private Quaternion StartRotation;

	// ================== RX СОБЫТИЯ ==================

	private Subject<bool> palleteLocked = new Subject<bool>();
	public IObservable<bool> PalleteLocked => palleteLocked;


	// Приватный Subject для отслеживания изменения состояния двигателя
	private Subject<bool> _engineStateSubject = new Subject<bool>();
	public IObservable<bool> EngineChangeState => _engineStateSubject;

	public bool EngineState
	{
		get => _engineState;
		set
		{
			_engineState = value;
			_engineStateSubject.OnNext(_engineState); // уведомляем всех подписчиков
		}
	}
	private bool _engineState;


	private Subject<Unit> fuelEnded = new Subject<Unit>();
	public IObservable<Unit> FuelEnded => fuelEnded;

	// =================================================

	private void Awake()
	{
		Fuel.Value = MaxFuel;

		Rg = GetComponent<Rigidbody>();
		StartPosition = transform.position;
		StartRotation = transform.rotation;
	}

	[Inject]
	public void Construct(PalleteLocker locker)
	{
		Locker = locker;

		Locker.LockedStream
			.Subscribe(locked => palleteLocked.OnNext(locked))
			.AddTo(this);
	}

	private void LateUpdate()
	{
		if (Camera.main == null || Mouse.current == null)
			return;

		if (Camera.main.transform.parent == null)
		{
			Camera.main.transform.SetParent(CamTran);
			Camera.main.transform.localPosition = Vector3.zero;
			Camera.main.transform.localRotation = Quaternion.identity;
		}

		Vector2 delta = Mouse.current.delta.ReadValue();

		Vector3 angles = Camera.main.transform.localEulerAngles;
		float pitch = NormalizeAngle(angles.x);
		float yaw = NormalizeAngle(angles.y);

		yaw += delta.x * sensitivity;
		pitch -= delta.y * sensitivity;

		yaw = Mathf.Clamp(yaw, -120f, 120f);
		pitch = Mathf.Clamp(pitch, -70f, 70f);

		Camera.main.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
	}

	private float NormalizeAngle(float angle)
	{
		if (angle > 180f) angle -= 360f;
		return angle;
	}

	public void Restart()
	{
		Fuel.Value = MaxFuel;
		EngineState = false;

		transform.SetPositionAndRotation(StartPosition, StartRotation);
		fork.localPosition = Vector3.zero;
		mast.localPosition = Vector3.zero;
		Locker.State = PalleteLockerState.Stay;
	}

	private void FixedUpdate()
	{
		if (Keyboard.current == null)
			return;

		float currentMaxSpeed = MaxSpeed * CurrentMaxSpeedK;

		if (Keyboard.current.tKey.wasPressedThisFrame && Fuel.Value > 0)
		{
			EngineState = !EngineState;
		}

		HandleForkAndMastMovement();
		HandleDriveMovement(currentMaxSpeed);
		HandleSteering();
		ApplyVelocity();
		UpdateDials();
	}

	private void HandleForkAndMastMovement()
	{
		bool up = Keyboard.current.qKey.isPressed && EngineState;
		bool down = Keyboard.current.eKey.isPressed || !EngineState;

		if (up)
		{
			Locker.State = PalleteLockerState.Up;

			fork.localPosition = Vector3.MoveTowards(
				fork.localPosition,
				new Vector3(0, 3f, 0),
				MaxSpeedLift * Time.fixedDeltaTime);

			if (fork.localPosition.y > 1.5f)
			{
				mast.localPosition = Vector3.MoveTowards(
					mast.localPosition,
					new Vector3(0, 1.5f, 0),
					MaxSpeedLift * Time.fixedDeltaTime);
			}
		}
		else if (down)
		{
			Locker.State = PalleteLockerState.Down;

			fork.localPosition = Vector3.MoveTowards(
				fork.localPosition,
				Vector3.zero,
				MaxSpeedLift * Time.fixedDeltaTime);

			mast.localPosition = Vector3.MoveTowards(
				mast.localPosition,
				Vector3.zero,
				MaxSpeedLift * Time.fixedDeltaTime);
		}
		else
		{
			Locker.State = PalleteLockerState.Stay;
		}
	}

	private void HandleDriveMovement(float currentMaxSpeed)
	{
		bool forward = Keyboard.current.wKey.isPressed && EngineState;
		bool backward = Keyboard.current.sKey.isPressed && EngineState;

		if (forward)
		{
			if (MoveSpeed < 0)
			{
				// Замедление в обратном направлении
				Tachos = Mathf.Clamp(Tachos - 5 * Time.fixedDeltaTime, MinTachos, MaxTachos);
				MoveSpeed = Mathf.Clamp(MoveSpeed + 5 * Time.fixedDeltaTime, -currentMaxSpeed, 0);
			}
			else
			{
				Tachos = Mathf.Clamp(Tachos + 5 * Time.fixedDeltaTime, MinTachos, MaxTachos);
				MoveSpeed = Mathf.Clamp(MoveSpeed + Tachos * Time.fixedDeltaTime, -currentMaxSpeed, currentMaxSpeed);
			}
		}
		else if (backward)
		{
			if (MoveSpeed > 0)
			{
				// Замедление вперед
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
			// Движение на месте
			if (EngineState)
			{
				if (Tachos < MinTachos)
					Tachos = Mathf.Clamp(Tachos + 5 * Time.fixedDeltaTime, 0, MinTachos);
				else
					Tachos = Mathf.Clamp(Tachos - 5 * Time.fixedDeltaTime, MinTachos, MaxTachos);
			}
			else
			{
				Tachos = Mathf.Clamp(Tachos - 5 * Time.fixedDeltaTime, 0, MaxTachos);
			}

			if (MoveSpeed > 0)
				MoveSpeed = Mathf.Clamp(MoveSpeed - 1 * Time.fixedDeltaTime, 0, currentMaxSpeed);
			else if (MoveSpeed < 0)
				MoveSpeed = Mathf.Clamp(MoveSpeed + 1 * Time.fixedDeltaTime, -currentMaxSpeed, 0);
		}

		// Расход топлива
		Fuel.Value = Mathf.Clamp(Fuel.Value - Tachos / MaxTachos * Time.fixedDeltaTime, 0, MaxFuel);

		if (Fuel.Value <= 0f && EngineState)
		{
			EngineState = false;
			fuelEnded.OnNext(Unit.Default);
		}
		else if (Fuel.Value <= MaxFuel / 2f)
		{
			CurrentMaxSpeedK = Mathf.Clamp(CurrentMaxSpeedK - Time.fixedDeltaTime, 0.5f, 1f);
		}
	}

	private void HandleSteering()
	{
		// Горизонтальное управление
		float h = 0f;
		if (Keyboard.current.dKey.isPressed) h += 1f;
		if (Keyboard.current.aKey.isPressed) h -= 1f;

		float steerSpeed = 2f;

		if (h == 0)
		{
			// Возврат руля к центру при отсутствии ввода
			if (StearAngle > 0f)
				StearAngle = Mathf.Clamp(StearAngle - steerSpeed * Time.fixedDeltaTime, 0f, 1f);
			else if (StearAngle < 0f)
				StearAngle = Mathf.Clamp(StearAngle + steerSpeed * Time.fixedDeltaTime, -1f, 0f);
		}
		else if (EngineState)
		{
			// Плавная смена направления
			if (h > 0f && StearAngle < 0f)
				steerSpeed = 1f;
			if (h < 0f && StearAngle > 0f)
				steerSpeed = 1f;

			StearAngle += h * steerSpeed * Time.fixedDeltaTime;
			StearAngle = Mathf.Clamp(StearAngle, -1f, 1f);
		}

		// Визуальный поворот руля
		float steerWheelAngle = StearAngle * 35f;
		float steerAngle = StearAngle * 270f;
		Stear.localRotation = Quaternion.Euler(0f, steerAngle, 0f);

		// Вращение колес
		WheelForwardLeft.SetRotation(MoveSpeed, steerWheelAngle);
		WheelForwardRight.SetRotation(MoveSpeed, steerWheelAngle);
		WheelBackwardLeft.SetRotation(MoveSpeed, 0f);
		WheelBackwardRight.SetRotation(MoveSpeed, 0f);
	}

	private void ApplyVelocity()
	{
		Vector3 globalVelocity = transform.TransformDirection(new Vector3(0f, 0f, MoveSpeed));
		Rg.linearVelocity = new Vector3(globalVelocity.x, Rg.linearVelocity.y, globalVelocity.z);

		float turnRadius = 2f;
		float angularY = 0f;

		float steerWheelAngle = StearAngle * 35f;
		if (Mathf.Abs(steerWheelAngle) > 0.01f)
		{
			float steerRad = steerWheelAngle * Mathf.Deg2Rad;
			angularY = (MoveSpeed / turnRadius) * Mathf.Tan(steerRad) * Mathf.Rad2Deg * Time.fixedDeltaTime;
		}

		transform.Rotate(0f, angularY, 0f, Space.World);
	}

	private void UpdateDials()
	{
		Dials.SetValues(
			Tachos / MaxTachos,
			Mathf.Abs(MoveSpeed) / MaxSpeed,
			Fuel.Value / MaxFuel);
	}

	private void OnDestroy()
	{
		_engineStateSubject.OnCompleted();
		_engineStateSubject.Dispose();
	}
}