using System;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody))]
public class ForkliftController : MonoBehaviour
{
	[Header("Камера")]
	/// <summary>
	/// Чувствительность вращения камеры
	/// </summary>
	[SerializeField, Tooltip("Чувствительность вращения камеры")]
	private float sensitivity = 0.3f;

	/// <summary>
	/// Трансформ камеры
	/// </summary>
	[SerializeField, Tooltip("Трансформ камеры")]
	private Transform CamTran;

	[Header("Вилы и мачта")]
	/// <summary>
	/// Трансформ вил
	/// </summary>
	[SerializeField, Tooltip("Трансформ вил")]
	private Transform fork;

	/// <summary>
	/// Трансформ мачты
	/// </summary>
	[SerializeField, Tooltip("Трансформ мачты")]
	private Transform mast;

	/// <summary>
	/// Максимальная скорость подъема вил
	/// </summary>
	[SerializeField, Tooltip("Максимальная скорость подъема вил")]
	private float MaxSpeedLift = 0.3f;

	[Header("Движение")]
	/// <summary>
	/// Максимальная скорость движения погрузчика
	/// </summary>
	[SerializeField, Tooltip("Максимальная скорость движения")]
	private float MaxSpeed = 5f;

	/// <summary>
	/// Коэффициент текущей максимальной скорости
	/// </summary>
	private float CurrentMaxSpeedK = 1;

	/// <summary>
	/// Текущая скорость движения погрузчика
	/// </summary>
	private float MoveSpeed;

	/// <summary>
	/// Текущие тахометры двигателя
	/// </summary>
	private float Tachos;

	/// <summary>
	/// Максимальное значение тахометра
	/// </summary>
	[SerializeField, Tooltip("Максимальное значение тахометра")]
	private float MaxTachos = 5f;

	/// <summary>
	/// Минимальное значение тахометра
	/// </summary>
	[SerializeField, Tooltip("Минимальное значение тахометра")]
	private float MinTachos = 1f;

	[Header("Руль")]
	/// <summary>
	/// Трансформ руля
	/// </summary>
	[SerializeField, Tooltip("Трансформ руля")]
	private Transform Stear;

	/// <summary>
	/// Текущий угол поворота руля
	/// </summary>
	private float StearAngle;

	[Header("Колеса")]
	/// <summary>
	/// Переднее левое колесо
	/// </summary>
	[SerializeField, Tooltip("Переднее левое колесо")]
	private Wheel WheelForwardLeft;

	/// <summary>
	/// Переднее правое колесо
	/// </summary>
	[SerializeField, Tooltip("Переднее правое колесо")]
	private Wheel WheelForwardRight;

	/// <summary>
	/// Заднее левое колесо
	/// </summary>
	[SerializeField, Tooltip("Заднее левое колесо")]
	private Wheel WheelBackwardLeft;

	/// <summary>
	/// Заднее правое колесо
	/// </summary>
	[SerializeField, Tooltip("Заднее правое колесо")]
	private Wheel WheelBackwardRight;

	[Header("Топливо")]
	/// <summary>
	/// Максимальный запас топлива
	/// </summary>
	[SerializeField, Tooltip("Максимальный запас топлива")]
	private float MaxFuel = 60;

	/// <summary>
	/// Текущий запас топлива
	/// </summary>
	public float Fuel { get; private set; }

	[Header("Системные ссылки")]
	/// <summary>
	/// Ссылка на замок паллеты
	/// </summary>
	[SerializeField, Tooltip("Ссылка на замок паллеты")]
	private PaletteLocker Locker;

	/// <summary>
	/// Панель приборов
	/// </summary>
	[SerializeField, Tooltip("Панель приборов")]
	private DialsPanel Dials;

	/// <summary>
	/// Rigidbody погрузчика
	/// </summary>
	private Rigidbody Rg;

	/// <summary>
	/// Начальная позиция погрузчика
	/// </summary>
	private Vector3 StartPosition;

	/// <summary>
	/// Начальная ротация погрузчика
	/// </summary>
	private Quaternion StartRotation;

	[Header("События")]
	/// <summary>
	/// Событие изменения состояния фиксации паллеты (true — зафиксирована)
	/// </summary>
	public Action<bool> PalleteLocked;

	/// <summary>
	/// Событие изменения состояния двигателя
	/// </summary>
	public Action<bool> EngineChangeState;

	/// <summary>
	/// Событие окончания топлива
	/// </summary>
	public Action FuelEnded;

	[Header("Двигатель")]
	/// <summary>
	/// Состояние двигателя
	/// </summary>
	public bool EngineState
	{
		get => _engineState;
		set
		{
			_engineState = value;
			EngineChangeState?.Invoke(_engineState);
		}
	}
	private bool _engineState = false;
	/// <summary>
	/// Инициализация переменных
	/// </summary>
	private void Awake()
	{
		Fuel = MaxFuel;

		Rg = GetComponent<Rigidbody>();

		Locker.Locked += (locked) => PalleteLocked.Invoke(locked);

		StartPosition = transform.position;
		StartRotation = transform.rotation;
	}

	/// <summary>
	/// Обновление камеры
	/// </summary>
	private void LateUpdate()
	{
		if (Camera.main == null || Mouse.current == null)
			return;

		if (Camera.main.transform.parent == null)
		{
			Camera.main.transform.parent = CamTran;
			Camera.main.transform.localPosition = Vector3.zero;
			Camera.main.transform.localRotation = Quaternion.identity;
		}

		// Дельта мыши
		Vector2 delta = Mouse.current.delta.ReadValue();

		// Текущие углы камеры
		Vector3 angles = Camera.main.transform.localEulerAngles;
		float pitch = NormalizeAngle(angles.x);
		float yaw = NormalizeAngle(angles.y);

		yaw += delta.x * sensitivity;
		pitch -= delta.y * sensitivity;

		yaw = Mathf.Clamp(yaw, -120f, 120f);
		pitch = Mathf.Clamp(pitch, -70f, 70f);

		Camera.main.transform.localRotation = Quaternion.Euler(pitch, yaw, 0f);
	}

	/// <summary>
	/// Нормализует угол к диапазону [-180;180]
	/// </summary>
	private float NormalizeAngle(float angle)
	{
		if (angle > 180f)
			angle -= 360f;
		return angle;
	}

	/// <summary>
	/// Перезапуск погрузчика в начальное состояние
	/// </summary>
	public void Restart()
	{
		Fuel = MaxFuel;
		EngineState = false;

		transform.SetPositionAndRotation(StartPosition, StartRotation);
		fork.transform.localPosition = Vector3.zero;
		mast.transform.localPosition = Vector3.zero;
		Locker.State = PalleteLockerState.Stay;
	}

	/// <summary>
	/// Физическое обновление движения и механики погрузчика
	/// </summary>
	private void FixedUpdate()
	{
		if (Keyboard.current == null)
			return;

		float currentMaxSpeed = MaxSpeed * CurrentMaxSpeedK;

		// Переключение двигателя
		if (Keyboard.current.tKey.wasPressedThisFrame && Fuel > 0)
		{
			EngineState = !EngineState;
		}

		HandleForkAndMastMovement();
		HandleDriveMovement(currentMaxSpeed);
		HandleSteering();
		ApplyVelocity();
		UpdateDials();
	}

	/// <summary>
	/// Управление движением вил и мачты
	/// </summary>
	private void HandleForkAndMastMovement()
	{
		bool up = Keyboard.current.qKey.isPressed && EngineState;
		bool down = Keyboard.current.eKey.isPressed || !EngineState;

		if (up)
		{
			Locker.State = PalleteLockerState.Up;

			// Поднимаем вилы
			fork.transform.localPosition = Vector3.MoveTowards(
				fork.transform.localPosition,
				new Vector3(0, 3f, 0),
				MaxSpeedLift * Time.fixedDeltaTime);

			// Поднимаем мачту если вилы подняты выше 1.5
			if (fork.transform.localPosition.y > 1.5f)
			{
				mast.transform.localPosition = Vector3.MoveTowards(
					mast.transform.localPosition,
					new Vector3(0, 1.5f, 0),
					MaxSpeedLift * Time.fixedDeltaTime);
			}
		}
		else if (down)
		{
			Locker.State = PalleteLockerState.Down;

			fork.transform.localPosition = Vector3.MoveTowards(
				fork.transform.localPosition,
				Vector3.zero,
				MaxSpeedLift * Time.fixedDeltaTime);

			mast.transform.localPosition = Vector3.MoveTowards(
				mast.transform.localPosition,
				Vector3.zero,
				MaxSpeedLift * Time.fixedDeltaTime);
		}
		else
		{
			Locker.State = PalleteLockerState.Stay;
		}
	}

	/// <summary>
	/// Управление движением вперед/назад и тахометрами
	/// </summary>
	private void HandleDriveMovement(float currentMaxSpeed)
	{
		bool forward = Keyboard.current.wKey.isPressed && EngineState;
		bool backward = Keyboard.current.sKey.isPressed && EngineState;

		if (forward)
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
		else if (backward)
		{
			if (MoveSpeed > 0)
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
			// Движение на месте
			if (EngineState)
			{
				if (Tachos < MinTachos)
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

		// Расход топлива
		Fuel = Mathf.Clamp(Fuel - Tachos / MaxTachos * Time.fixedDeltaTime, 0, MaxFuel);
		if (Fuel == 0 && EngineState)
		{
			EngineState = false;
			FuelEnded?.Invoke();
		}
		else if (Fuel <= MaxFuel / 2)
		{
			CurrentMaxSpeedK = Mathf.Clamp(CurrentMaxSpeedK - Time.fixedDeltaTime, 0.5f, 1);
		}
	}

	/// <summary>
	/// Управление рулем и углом поворота
	/// </summary>
	private void HandleSteering()
	{
		// Горизонтальное управление
		float h = 0f;
		if (Keyboard.current.dKey.isPressed) h += 1f;
		if (Keyboard.current.aKey.isPressed) h -= 1f;

		float steerSpeed = 2f;

		if (h == 0)
		{
			if (StearAngle > 0)
				StearAngle = Mathf.Clamp(StearAngle - steerSpeed * Time.fixedDeltaTime, 0, 1);
			else if (StearAngle < 0)
				StearAngle = Mathf.Clamp(StearAngle + steerSpeed * Time.fixedDeltaTime, -1, 0);
		}
		else if (EngineState)
		{
			if (h > 0 && StearAngle < 0)
				steerSpeed = 1;
			if (h < 0 && StearAngle > 0)
				steerSpeed = 1;

			StearAngle += h * steerSpeed * Time.fixedDeltaTime;
			StearAngle = Mathf.Clamp(StearAngle, -1, 1);
		}

		// Визуальный поворот руля
		float steerWheelAngle = StearAngle * 35;
		float steerAngle = StearAngle * 270;
		Stear.localRotation = Quaternion.Euler(0, steerAngle, 0);

		// Вращение колес
		WheelForwardLeft.SetRotation(MoveSpeed, steerWheelAngle);
		WheelForwardRight.SetRotation(MoveSpeed, steerWheelAngle);
		WheelBackwardLeft.SetRotation(MoveSpeed, 0);
		WheelBackwardRight.SetRotation(MoveSpeed, 0);
	}

	/// <summary>
	/// Применение скорости к Rigidbody
	/// </summary>
	private void ApplyVelocity()
	{
		// Преобразуем в глобальные координаты
		Vector3 globalVelocity = transform.TransformDirection(new Vector3(0f, 0f, MoveSpeed));

		// Применяем к Rigidbody, сохраняя Y скорость
		Rg.linearVelocity = new Vector3(globalVelocity.x, Rg.linearVelocity.y, globalVelocity.z);

		// Поворот погрузчика по формуле Ackermann
		float turnRadius = 2f;
		float angularY = 0f;

		float steerWheelAngle = StearAngle * 35; // угол рулевых колес
		if (Mathf.Abs(steerWheelAngle) > 0.01f)
		{
			float steerRad = steerWheelAngle * Mathf.Deg2Rad;
			angularY = (MoveSpeed / turnRadius) * Mathf.Tan(steerRad) * Mathf.Rad2Deg * Time.fixedDeltaTime;
		}

		transform.Rotate(0f, angularY, 0f, Space.World);
	}

	/// <summary>
	/// Обновление панели приборов
	/// </summary>
	private void UpdateDials()
	{
		Dials.SetValues(Tachos / MaxTachos, MathF.Abs(MoveSpeed) / MaxSpeed, Fuel / MaxFuel);
	}
}