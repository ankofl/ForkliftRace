## Обзор проекта

![Readme](https://github.com/user-attachments/assets/39d792d4-c8d9-4a56-a16d-59286505b5cb)

### [Скачать 1.0.0 (36.4 Mb)](https://github.com/ankofl/ForkliftRace/releases/download/1.0.0/Build.zip) | [Релизы](https://github.com/ankofl/ForkliftRace/releases/tag/1.0.0)

Все основные скрипты находятся в папке `Assets/Scripts`.

| Скрипт                  | Описание                                                                                           | Ссылка на код                                                                 |
|-------------------------|----------------------------------------------------------------------------------------------------|-------------------------------------------------------------------------------|
| **Dial.cs**            | Скрипт одного стрелочного датчика                                          | [Dial.cs](Assets/Scripts/Dial.cs)                                            |
| **DialsPanel.cs**      | Управляет группой индикаторов Dial, обновляет их значения                                          | [DialsPanel.cs](Assets/Scripts/DialsPanel.cs)                                |
| **FadeController.cs**  | Плавное затемнение экрана или панели используется при рестарте   | [FadeController.cs](Assets/Scripts/FadeController.cs)                        |
| **ForkliftController.cs** | Главный контроллер погрузчика: движение, руление, скорость, топливо, события (FuelEnded, PalleteLocked и др.) | [ForkliftController.cs](Assets/Scripts/ForkliftController.cs)               |
| **GameInstaller.cs**   | Zenject-инсталлер, связывает зависимости (ForkliftController, зоны, Tooltip и др.)                 | [GameInstaller.cs](Assets/Scripts/GameInstaller.cs)                          |
| **GameManager.cs**     | Центральный менеджер игры: спавн паллет, подписки на события, управление HUD и анимациями (UniTask + UniRx) | [GameManager.cs](Assets/Scripts/GameManager.cs)                              |
| **Pallete.cs**         | Компонент паллеты: фиксация, анимации загрузки/выгрузки, Zenject Factory                           | [Pallete.cs](Assets/Scripts/Pallete.cs)                                      |
| **PalleteLocker.cs**   | Фиксатор паллеты на вилах, поток состояния через UniRx (LockedStream)                              | [PalleteLocker.cs](Assets/Scripts/PalleteLocker.cs)                          |
| **PerInstanceColor.cs**| Изменение цвета отдельных экземпляров через Hybrid GPU Instancing                                  | [PerInstanceColor.cs](Assets/Scripts/PerInstanceColor.cs)                    |
| **Tooltip.cs**         | Элемент подсказки в HUD, анимации DOTween (появление/исчезновение, смена текста, адаптивный фон)  | [Tooltip.cs](Assets/Scripts/Tooltip.cs)                                      |
| **Wheel.cs**           | Компонент колеса: визуальное вращение и анимация движения                                          | [Wheel.cs](Assets/Scripts/Wheel.cs)                                          |
| **ZoneLoading.cs**     | Зона загрузки (спавн паллет через Pallete.Factory), анимация появления                             | [ZoneLoading.cs](Assets/Scripts/ZoneLoading.cs)                              |
| **ZoneUnloading.cs**   | Зона выгрузки: триггер, поток событий доставки (DeliveredStream, UniRx)                            | [ZoneUnloading.cs](Assets/Scripts/ZoneUnloading.cs)

### Согласно ТЗ использованы современные и мощные инструменты:

- **UniRx** — реактивное программирование, потоки событий  
- **UniTask** — асинхронность без блокировок, лёгкие корутины  
- **Zenject** — dependency injection, чистая архитектура  
- **DOTween** — плавные и красивые анимации интерфейса
