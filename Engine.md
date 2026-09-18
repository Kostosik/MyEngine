# My Engine — карта движка

**Обновлено:** 2026-09-17
**Файлов:** ~160

Обозначения:
- ✅ — используется в игре
- ⚠️ — готово в движке, но не подключено к игре
- 🔧 — вспомогательное (тесты, debug, утилиты)

---

## 1. ЯДРО

| Файл | Что |
|---|---|
| ✅ `Application.cs` | Главный цикл: окно, фикс-шаг, ввод, рендер, ImGui, консоль |
| ⚠️ `HeadlessApplication.cs` | Тот же цикл без окна. Для тестов и серверной логики |

---

## 2. ECS

| Файл | Что |
|---|---|
| ✅ `Ecs/Entity.cs` | Сущность — мешок компонентов |
| ✅ `Ecs/World.cs` | Список сущностей + синглтоны |
| ✅ `Ecs/ISystem.cs` | Интерфейс системы |
| ✅ `Ecs/ComponentRegistry.cs` | Type → индекс массива |
| ✅ `Ecs/SystemPhase.cs` | Фазы: PreUpdate/Update/PostUpdate/LateUpdate |
| ✅ `Ecs/SystemSheduler.cs` | Сортирует системы по фазам |
| ✅ `Ecs/BehaviorComponent.cs` | Обёртка для IBehavior |
| ✅ `Ecs/InspectableAttribute.cs` | (не используется) |

---

## 3. КОМПОНЕНТЫ

| Файл | Что |
|---|---|
| ✅ `Components/Transform.cs` | Позиция, вращение, масштаб |
| ✅ `Components/Velocity.cs` | Скорость |
| ✅ `Components/Collider.cs` | AABB, слой, триггер |
| ✅ `Components/Sprite.cs` | Размер, цвет, текстура, UV, Enabled |
| ✅ `Components/Tags.cs` | PlayerTag, EnemyTag, WallTag, NpcTag, PickupTag |
| ✅ `Components/Health.cs` | HP, i-frames, вспышка |
| ✅ `Components/Attack.cs` | Урон, радиус, кулдаун |
| ✅ `Components/Experience.cs` | Награда XP |
| ✅ `Components/AI.cs` | Состояние, скорость, радиус агрессии |
| ✅ `Components/ManualMovement.cs` | Маркер «двигается вручную» |
| ✅ `Components/TopDownController.cs` | TopDown-движение (текущий режим игры) |
| ✅ `Components/Interactable.cs` | Объект для взаимодействия (E) |
| ✅ `Components/Interactor.cs` | Кто ищет Interactable |
| ✅ `Components/AmbientLight.cs` | Базовый свет мира |
| ✅ `Components/PointLight.cs` | Точечный свет |
| ✅ `Components/ParallaxLayer.cs` | Слой параллакса |
| ⚠️ `Components/PlatformerController.cs` | Платформер (не подключён) |
| ⚠️ `Components/OneWayPlatform.cs` | Односторонняя платформа (не подключена) |

---

## 4. СИСТЕМЫ (универсальные)

| Файл | Что |
|---|---|
| ✅ `Systems/MovementSystem.cs` | Двигает всех с Velocity (кроме контроллеров) |
| ✅ `Systems/CollisionSystem.cs` | Разрешение коллизий (TopDown + Platformer) |
| ✅ `Systems/SpriteRenderSystem.cs` | Рисует спрайты, сортирует по Layer |
| ✅ `Systems/AnimationSystem.cs` | Тикает анимации, публикует AnimationEvent |
| ✅ `Systems/AISystem.cs` | Тикает BehaviorComponent |
| ✅ `Systems/TopDownControllerSystem.cs` | TopDown-движение |
| ✅ `Systems/InteractionSystem.cs` | Ищет ближайший Interactable |
| ✅ `Systems/TriggerSystem.cs` | Публикует TriggerEnter/Exit |
| ✅ `Systems/ParallaxSystem.cs` | Обновляет параллакс-слои |
| ✅ `Systems/LightPulseSystem.cs` | Пульсация света |
| ✅ `Systems/PlatformerControllerSystem.cs` | Платформер (не подключён) |
| ⚠️ `Systems/PlatformerInputSystem.cs` | Ввод для платформера |

---

## 5. РЕНДЕР

| Файл | Что |
|---|---|
| ✅ `Rendering/Shader.cs` | Компиляция шейдеров |
| ✅ `Rendering/Texture2D.cs` | Загрузка и upload текстур |
| ✅ `Rendering/SpriteBatch.cs` | Батчинг спрайтов + blend |
| ✅ `Rendering/Camera2D.cs` | Deadzone, look-ahead, shake, bounds |
| ✅ `Rendering/Font.cs` | TTF-шрифт, растеризация в атлас |
| ✅ `Rendering/Glyph.cs` | Метрики глифа |
| ✅ `Rendering/RenderTarget.cs` | Framebuffer для рендера в текстуру |
| ✅ `Rendering/PostProcessStack.cs` | Цепочка постобработки |
| ✅ `Rendering/PostProcessShader.cs` | Один проход |
| ✅ `Rendering/PostProcessContext.cs` | Пул временных RT |
| ✅ `Rendering/IPostProcessPass.cs` | Интерфейс эффекта |
| ✅ `Rendering/BloomPass.cs` | Multipass bloom |
| ✅ `Rendering/PostProcessPresets.cs` | Готовые эффекты (Vignette, Grayscale, ...) |
| ✅ `Rendering/LightmapRenderer.cs` | Рендер света в текстуру |
| ✅ `Rendering/LightingPass.cs` | Умножение сцены на lightmap |
| ⚠️ `Rendering/InstancedRenderer.cs` | Instancing — тысячи спрайтов одним вызовом |
| ⚠️ `Rendering/InstanceData.cs` | Данные одного экземпляра |

### RHI (абстракция над API)

| Файл | Что |
|---|---|
| ✅ `Rendering/RHI/IRenderer.cs` | Интерфейс графического API |
| ✅ `Rendering/RHI/ITexture.cs` | Интерфейс текстуры |
| ✅ `Rendering/RHI/IShader.cs` | Интерфейс шейдера |
| ✅ `Rendering/RHI/IRenderTarget.cs` | Интерфейс RT |
| ✅ `Rendering/RHI/OpenGLRenderer.cs` | Реализация через OpenGL |
| ⚠️ `Rendering/RHI/Null/NullRenderer.cs` | Заглушка (для headless) |

---

## 6. UI

| Файл | Что |
|---|---|
| ✅ `UI/UIElement.cs` | База UI: якорь, layout, события |
| ✅ `UI/UIRect.cs` | Прямоугольник UI |
| ✅ `UI/Anchor.cs` | Якоря (TopLeft, MiddleCenter, ...) |
| ✅ `UI/UIRoot.cs` | Корень UI, стек экранов |
| ✅ `UI/UIRenderContext.cs` | Что нужно UI для отрисовки |
| ✅ `UI/ImGuiLayer.cs` | Обёртка ImGuiController |
| ✅ `UI/DialogueBox.cs` | ImGui-окно диалога |
| ✅ `UI/Widgets/Panel.cs` | Панель |
| ✅ `UI/Widgets/Label.cs` | Текст |
| ✅ `UI/Widgets/Button.cs` | Кнопка |
| ✅ `UI/Widgets/ProgressBar.cs` | HP/XP-бар |
| ✅ `UI/Widgets/Image.cs` | Картинка |

---

## 7. AI

| Файл | Что |
|---|---|
| ✅ `Ai/IBehavior.cs` | Контракт поведения |
| ✅ `Ai/StateMachine.cs` | FSM |
| ✅ `Ai/State.cs` | Состояние FSM |
| ✅ `Ai/Transition.cs` | Переход FSM |
| ✅ `Ai/Steering.cs` | Seek, Arrive, Separation |
| ✅ `Ai/NavGrid.cs` | Сетка проходимости |
| ✅ `Ai/AiStar.cs` | A* pathfinding |
| ⚠️ `Ai/BehaviorTree/*` | Behavior Tree (18 файлов, не подключён) |

---

## 8. АССЕТЫ

| Файл | Что |
|---|---|
| ✅ `Assets/AsyncAssetLoader.cs` | Фоновая загрузка |
| ✅ `Assets/ResourceManager.cs` | Менеджер текстур |
| ✅ `Assets/AssetDatabase.cs` | Сканирование папки Assets |
| ✅ `Assets/AssetEntry.cs` | Узел дерева (файл/папка) |
| ✅ `Assets/AssetPreview.cs` | Ленивое превью |
| ✅ `Assets/AssetBrowserWindow.cs` | ImGui-браузер (F4) |
| ⚠️ `Assets/AssetCache.cs` | Кэш с реф-каунтом |
| ⚠️ `Assets/AssetRefCount.cs` | Счётчик ссылок |

---

## 9. ДИАЛОГИ

| Файл | Что |
|---|---|
| ✅ `Dialogue/DialogueTree.cs` | Дерево диалога |
| ✅ `Dialogue/DialogueNode.cs` | Узел: реплика + выбор |
| ✅ `Dialogue/DialogueChoice.cs` | Вариант ответа |
| ✅ `Dialogue/DialogueContext.cs` | Флаги и условия |
| ✅ `Dialogue/DialogueRunner.cs` | Проход по дереву |
| ✅ `Dialogue/DialogueEvent.cs` | События диалога |

---

## 10. АНИМАЦИЯ

| Файл | Что |
|---|---|
| ✅ `Animation/AnimationEngine.cs` | Описание анимации |
| ✅ `Animation/AnimatorComponent.cs` | Состояние проигрывания |
| ✅ `Animation/SpriteSheet.cs` | Нарезка спрайт-листов |
| ✅ `Animation/AnimationEvent.cs` | Событие на кадре |

---

## 11. ЭФФЕКТЫ И ЧАСТИЦЫ

| Файл | Что |
|---|---|
| ✅ `Effects/Particle.cs` | Одна частица |
| ✅ `Effects/ParticlePool.cs` | Кольцевой буфер |
| ✅ `Effects/ParticleSystem.cs` | Менеджер частиц |
| ✅ `Effects/ParticleEmitterSettings.cs` | Настройки эмиттера |

---

## 12. КОНСОЛЬ И ИНСТРУМЕНТЫ

| Файл | Что |
|---|---|
| ✅ `Console/ConsoleSystem.cs` | Ядро консоли |
| ✅ `Console/ConsoleWindow.cs` | ImGui-окно консоли (F1) |
| ✅ `Console/IConsoleCommand.cs` | Интерфейс команды |
| ✅ `Console/LambdaCommand.cs` | Команда через лямбду |
| ✅ `Console/BuiltInCommands.cs` | help, clear |
| ✅ `Diagnostics/Profiler.cs` | FPS, время кадров, GC, бюджет |
| ✅ `Diagnostics/ProfilerWindow.cs` | ImGui-окно профайлера (F2) |
| ✅ `Diagnostics/PerfTimelineWindow.cs` | Таймлайн задач по воркерам (F9) |
| ✅ `Diagnostics/DebugDraw.cs` | Gizmo (коллайдеры, радиусы) |
| ✅ `Diagnostics/DebugConfig.cs` | Флаги отладки |
| ✅ `Diagnostics/EntityInspector.cs` | Инспектор сущностей (F6) |
| ✅ `Diagnostics/ComponentDrawer.cs` | Рефлекторный ImGui |
| ✅ `Diagnostics/WorldInspector.cs` | Список компонентов |
| ✅ `Diagnostics/WorldInspectorWindow.cs` | ImGui-окно |
| ✅ `Diagnostics/Log.cs` | Логирование с уровнями |
| ✅ `Diagnostics/ErrorLogger.cs` | Обёртка над Log |
| ✅ `Diagnostics/Safe.cs` | Try-catch-хелперы |
| ✅ `Diagnostics/Assert.cs` | Assertions |

---

## 13. ИНФРАСТРУКТУРА

| Файл | Что |
|---|---|
| ✅ `Events/EventBus.cs` | Шина событий с приоритетами |
| ✅ `Events/InteractionRequestedEvent.cs` | Событие взаимодействия |
| ✅ `Memory/Arena.cs` | Bump-allocator |
| ✅ `Pooling/Pool.cs` | Универсальный пул |
| ⚠️ `Pooling/PoolManager.cs` | Реестр пулов |
| ✅ `Spartial/SpatialHash.cs` | Broadphase |
| ✅ `Threading/JobSystem.cs` | Пул воркеров |
| ✅ `Threading/JobGraph.cs` | Задачи с зависимостями |
| ✅ `Threading/JobProfiler.cs` | Профайлер задач |
| ✅ `Time/TimeEngine.cs` | TimeScale, pause, hit-stop |
| ✅ `Timers/Scheduler.cs` | Отложенные и повторяющиеся задачи |
| ⚠️ `Coroutines/*` | Корутины (7 файлов, не подключены) |
| ⚠️ `Scenes/*` | SceneManager (2 файла, не подключён) |
| ✅ `Services/ServicesEngine.cs` | Service Locator |
| ✅ `GameFlow/*` | Game State Machine (3 файла) |
| ⚠️ `Utility/DeterministicRandom.cs` | Детерминированный RNG |

---

## 14. ФИЗИКА

| Файл | Что |
|---|---|
| ✅ `Physics/Layer.cs` | Битовые слои |
| ✅ `Physics/CollisionMatrix.cs` | Правила столкновений |
| ✅ `Physics/TriggerEvents.cs` | TriggerEnter/Exit |
| ⚠️ `Physics/Ray.cs`, `Raycast.cs`, `RaycastHit.cs` | Raycast (не используется) |

---

## 15. ВВОД И МАТЕМАТИКА

| Файл | Что |
|---|---|
| ✅ `InputEngine/Input.cs` | Клавиатура, мышь, буфер нажатий |
| ✅ `Math/Aabb.cs` | Прямоугольник |
| ✅ `Math/IntRect.cs` | Целочисленный прямоугольник |

---

## 16. АУДИО

| Файл | Что |
|---|---|
| ✅ `Audio/AudioEngine.cs` | Обёртка над OpenAL |
| ✅ `Audio/AudioManager.cs` | Пул источников |
| ✅ `Audio/AudioSource.cs` | Один играющий звук |
| ✅ `Audio/AudioClip.cs` | Загруженный звук |
| ✅ `Audio/AudioClipResource.cs` | OpenAL buffer |
| ✅ `Audio/WavLoader.cs` | Парсер WAV |

---

## ЧТО НЕ ПОДКЛЮЧЕНО К ИГРЕ (⚠️)

**Кандидаты на ревизию:** если за 2 недели не понадобились — удалить или перенести в BACKLOG.

1. `HeadlessApplication` — для тестов и серверов
2. `Ai/BehaviorTree/*` (18 файлов) — если не нужен сложный AI
3. `Assets/AssetCache`, `AssetRefCount` — если не нужно шарить текстуры
4. `Components/PlatformerController`, `OneWayPlatform` — если не делаем платформер
5. `Systems/PlatformerControllerSystem`, `PlatformerInputSystem` — то же
6. `Rendering/InstancedRenderer`, `InstanceData` — если спрайтов < 1000
7. `Rendering/RHI/Null/*` — если не делаем headless
8. `Coroutines/*` — если не пишем сценарии
9. `Scenes/*` — если не делаем меню/уровни
10. `Physics/Ray.cs`, `Raycast.cs`, `RaycastHit.cs` — если нет LOS / стрельбы
11. `Utility/DeterministicRandom.cs` — если нет реплеев/мультиплеера
12. `Pooling/PoolManager.cs` — если пул не используется

**Правило:** если что-то не понадобилось за 2 недели активной работы над игрой — в BACKLOG или удалить.

---


## СТРУКТУРА ПАПОК

Engine/
├── Ai/ — AI (FSM, BT, Steering, Pathfinding)
├── Animation/ — спрайтовые анимации
├── Assets/ — загрузка и просмотр ассетов
├── Audio/ — звук через OpenAL
├── Components/ — компоненты ECS
├── Console/ — in-game консоль
├── Coroutines/ — корутины поверх Scheduler
├── Diagnostics/ — отладка и инструменты
├── Dialogue/ — диалоговые деревья
├── Ecs/ — ядро ECS
├── Effects/ — частицы
├── Events/ — EventBus
├── GameFlow/ — состояния игры
├── InputEngine/ — ввод
├── Math/ — математика
├── Memory/ — Arena
├── Physics/ — слои, raycast, триггеры
├── Pooling/ — пулы
├── Rendering/ — графика
│ └── RHI/ — абстракция API
├── Scenes/ — SceneManager
├── Services/ — Service Locator
├── Spartial/ — SpatialHash
├── Systems/ — универсальные системы
├── Threading/ — JobSystem
├── Time/ — TimeEngine
├── Timers/ — Scheduler
├── UI/ — свой UI
│ └── Widgets/ — кнопки, панели, ...
└── Utility/ — утилиты