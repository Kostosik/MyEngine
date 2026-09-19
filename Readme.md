## Движок — Engine/

### Ядро

| Файл | Описание |
|---|---|
| `Application.cs` | Главный цикл: окно, фиксированный шаг (60 Гц), переменный апдейт, рендер, ImGui, обработка ошибок |
| `HeadlessApplication.cs` | Тот же цикл без окна и GL. Для тестов и серверной симуляции |
| `Engine.csproj` | Конфигурация проекта (net8.0, unsafe, nullable) |

### ECS

| Файл | Описание |
|---|---|
| `Ecs/Entity.cs` | Сущность — контейнер компонентов с уникальным Id |
| `Ecs/World.cs` | Список сущностей, синглтоны, реестр по Id |
| `Ecs/ISystem.cs` | Интерфейс системы (Update + Phase + Priority) |
| `Ecs/SystemPhase.cs` | Фазы: PreUpdate / Update / PostUpdate / LateUpdate |
| `Ecs/SystemSheduler.cs` | Сортирует и тикает системы по фазам |
| `Ecs/ComponentRegistry.cs` | Type → int индекс для быстрого доступа |
| `Ecs/EntityQueryExtensions.cs` | `world.Query().With<A>().Without<B>()` |
| `Ecs/EntityFactoryRegistry.cs` | Реестр именованных фабрик: `Spawn(world, "slime", pos)` |
| `Ecs/RequireComponentAttribute.cs` | Декларация зависимостей между компонентами |
| `Ecs/BehaviorComponent.cs` | Обёртка для IBehavior (FSM/BT) |
| `Ecs/InspectableAttribute.cs` | Маркер для инспектора |

### Компоненты

| Файл | Описание |
|---|---|
| `Components/Transform.cs` | Позиция, поворот, масштаб. Поддерживает иерархию (Parent/Child) |
| `Components/Velocity.cs` | Скорость |
| `Components/Collider.cs` | AABB + слой + триггер |
| `Components/Sprite.cs` | Размер, цвет, текстура, эффект |
| `Components/Health.cs` | HP, i-frames, вспышка |
| `Components/Attack.cs` | Урон, радиус, кулдаун |
| `Components/AI.cs` | Состояние, радиус агрессии, скорость |
| `Components/TopDownController.cs` | Управление top-down персонажем |
| `Components/PlatformerController.cs` | Управление платформером (jump, coyote, buffer) |
| `Components/Interactable.cs` | Объект, с которым можно взаимодействовать (E) |
| `Components/Interactor.cs` | Кто ищет Interactable |
| `Components/PointLight.cs`, `AmbientLight.cs` | Источники света |
| `Components/ParallaxLayer.cs` | Слой параллакса |
| `Components/OneWayPlatform.cs` | Односторонняя платформа |
| `Components/ManualMovement.cs` | Маркер «двигается вручную» |
| `Components/Tags.cs` | PlayerTag, EnemyTag, WallTag, NpcTag, PickupTag |

### Системы (универсальные)

| Файл | Описание |
|---|---|
| `Systems/MovementSystem.cs` | Двигает всех с Velocity (кроме контроллеров) |
| `Systems/CollisionSystem.cs` | Разрешение коллизий (TopDown + Platformer) |
| `Systems/SpriteRenderSystem.cs` | Рисует спрайты с сортировкой по Layer |
| `Systems/AnimationSystem.cs` | Тикает анимации, публикует события кадров |
| `Systems/AISystem.cs` | Тикает BehaviorComponent (FSM/BT) |
| `Systems/TopDownControllerSystem.cs` | Применяет TopDownController |
| `Systems/PlatformerControllerSystem.cs` | Прыжки, гравитация, coyote time |
| `Systems/PlatformerInputSystem.cs` | Ввод для платформера |
| `Systems/InteractionSystem.cs` | Поиск ближайшего Interactable |
| `Systems/TriggerSystem.cs` | Публикует TriggerEnter/Exit |
| `Systems/TilemapCollisionSystem.cs` | Коллизии со solid-тайлами |
| `Systems/ParallaxSystem.cs` | Обновляет параллакс-слои |
| `Systems/LightPulseSystem.cs` | Пульсация света |
| `Systems/TransformSystem.cs` | Обновляет иерархию Transform (Parent → Child) |
| `Systems/ChunkSystem.cs` | Подгрузка/выгрузка чанков вокруг игрока |

### Рендер

| Файл | Описание |
|---|---|
| `Rendering/Shader.cs` | Компиляция и управление шейдерами |
| `Rendering/Texture2D.cs` | Загрузка PNG/JPG, upload в GPU |
| `Rendering/SpriteBatch.cs` | Батчинг спрайтов + встроенные эффекты |
| `Rendering/Camera2D.cs` | Deadzone, look-ahead, shake, bounds, ScreenToWorld |
| `Rendering/Font.cs` | TTF → атлас глифов, DrawString |
| `Rendering/Glyph.cs` | Метрики одного глифа |
| `Rendering/RenderTarget.cs` | Framebuffer для рендера в текстуру |
| `Rendering/PostProcessStack.cs` | Цепочка эффектов постобработки |
| `Rendering/PostProcessShader.cs` | Один проход |
| `Rendering/PostProcessContext.cs` | Пул временных RT |
| `Rendering/PostProcessPresets.cs` | Готовые эффекты (Vignette, Bloom, Grayscale…) |
| `Rendering/BloomPass.cs` | Multipass bloom |
| `Rendering/LightmapRenderer.cs` | Рендер света в текстуру |
| `Rendering/LightingPass.cs` | Умножение сцены на lightmap |
| `Rendering/InstancedRenderer.cs` | Instancing — 100k спрайтов одним вызовом |
| `Rendering/InstanceData.cs` | Данные одного экземпляра |
| `Rendering/UIRoundedRenderer.cs` | Скруглённые прямоугольники для UI |
| `Rendering/Effects/SpriteEffect.cs` | Enum эффектов на спрайте (Flash, Outline, Dissolve…) |

#### RHI — абстракция над графическим API

| Файл | Описание |
|---|---|
| `Rendering/RHI/IRenderer.cs` | Интерфейс графического API |
| `Rendering/RHI/ITexture.cs`, `IShader.cs`, `IRenderTarget.cs` | Интерфейсы ресурсов |
| `Rendering/RHI/OpenGLRenderer.cs` | Реализация через OpenGL |
| `Rendering/RHI/Null/NullRenderer.cs` | Заглушка для headless-режима |

### Ассеты

| Файл | Описание |
|---|---|
| `Assets/ResourceManager.cs` | Кэш текстур, фоновая загрузка |
| `Assets/AsyncAssetLoader.cs` | Универсальная фоновая загрузка с колбэком |
| `Assets/AssetCache.cs` | Кэш с реф-каунтом |
| `Assets/AssetRefCount.cs` | Счётчик ссылок на ресурс |
| `Assets/AssetDatabase.cs` | Сканирование папки Assets |
| `Assets/AssetEntry.cs` | Узел дерева (файл/папка) |
| `Assets/AssetPreview.cs` | Ленивое превью для браузера |
| `Assets/AssetBrowserWindow.cs` | ImGui-браузер ассетов |
| `Assets/AssetManifest.cs`, `AssetManifestData.cs` | Манифест ассетов (по имени) |

### Аудио

| Файл | Описание |
|---|---|
| `Audio/AudioEngine.cs` | Обёртка над OpenAL |
| `Audio/AudioManager.cs` | Пул источников, Play/PlayAt |
| `Audio/AudioSource.cs` | Один играющий звук |
| `Audio/AudioClip.cs` | Загруженный PCM |
| `Audio/AudioClipResource.cs` | OpenAL buffer |
| `Audio/WavLoader.cs` | Парсер WAV-файлов |

### AI

| Файл | Описание |
|---|---|
| `Ai/IBehavior.cs` | Контракт поведения (FSM, BT или своё) |
| `Ai/StateMachine.cs` | Конечный автомат с приоритетами переходов |
| `Ai/State.cs`, `Ai/Transition.cs` | Состояние и переход |
| `Ai/Steering.cs` | Seek, Flee, Arrive, Separation, AvoidObstacles |
| `Ai/NavGrid.cs` | Сетка проходимости |
| `Ai/AiStar.cs` | Поиск пути A* |
| `Ai/NavGridBuilder.cs` | Строит NavGrid из статичных коллайдеров |
| `Ai/BehaviorTree/*` | Behavior Tree: Node, Sequence, Selector, Decorator, Action, Condition… |

### Диалоги

| Файл | Описание |
|---|---|
| `Dialogue/DialogueTree.cs` | Граф узлов диалога |
| `Dialogue/DialogueNode.cs` | Узел: реплика + варианты ответа |
| `Dialogue/DialogueChoice.cs` | Вариант ответа с условиями и эффектами |
| `Dialogue/DialogueContext.cs` | Флаги для условий |
| `Dialogue/DialogueRunner.cs` | Проход по дереву |
| `Dialogue/DialogueEvent.cs` | События Started/NodeChanged/Ended |

### Анимации

| Файл | Описание |
|---|---|
| `Animation/AnimationEngine.cs` | Описание анимации (кадры, fps, loop) |
| `Animation/AnimatorComponent.cs` | Состояние проигрывания |
| `Animation/SpriteSheet.cs` | Нарезка спрайт-листов |
| `Animation/AnimationEvent.cs` | Событие на конкретном кадре |

### Частицы

| Файл | Описание |
|---|---|
| `Effects/Particle.cs` | Одна частица (struct) |
| `Effects/ParticlePool.cs` | Кольцевой буфер без аллокаций |
| `Effects/ParticleSystem.cs` | Менеджер, Emit/Update |
| `Effects/ParticleEmitterSettings.cs` | Настройки эмиттера |
| `Effects/ParticleRenderer.cs` | Универсальный рендер частиц |

### Ввод

| Файл | Описание |
|---|---|
| `InputEngine/Input.cs` | Клавиатура, мышь, буфер нажатий |
| `InputEngine/GameAction.cs` | Enum игровых действий |
| `InputEngine/DebugAction.cs` | Enum отладочных действий |
| `InputEngine/Bindings.cs` | Раскладка: action → key |

### Инфраструктура

| Файл | Описание |
|---|---|
| `Events/EventBus.cs` | Шина событий с приоритетами |
| `Events/EntityLifecycleEvents.cs` | EntityCreatedEvent, EntityDestroyedEvent |
| `Events/InteractionRequestedEvent.cs` | Событие взаимодействия |
| `Memory/Arena.cs` | Bump-аллокатор (без GC-мусора) |
| `Pooling/Pool.cs` | Универсальный пул объектов |
| `Pooling/PoolManager.cs` | Реестр пулов по типу |
| `Spartial/SpatialHash.cs` | Broadphase для быстрых запросов «что рядом» |
| `Threading/JobSystem.cs` | Пул воркеров + ParallelFor |
| `Threading/JobGraph.cs` | Задачи с зависимостями |
| `Threading/JobProfiler.cs` | Профайлер задач по воркерам |
| `Time/TimeEngine.cs` | TimeScale, pause, hit-stop, DeltaTime |
| `Timers/Scheduler.cs` | Отложенные и повторяющиеся задачи |
| `Coroutines/CoroutineRunner.cs` | Корутины поверх Scheduler |
| `Coroutines/WaitForSeconds.cs` и др. | Ожидания для корутин |
| `Services/ServicesEngine.cs` | Service Locator |
| `Utility/DeterministicRandom.cs` | Детерминированный RNG (для реплеев) |
| `Math/Aabb.cs` | Прямоугольник для коллизий |
| `Math/IntRect.cs` | Целочисленный прямоугольник |
| `Physics/Layer.cs`, `CollisionMatrix.cs` | Слои столкновений |
| `Physics/Ray.cs`, `Raycast.cs`, `RaycastHit.cs` | Raycast |
| `Physics/TriggerEvents.cs` | TriggerEnter/Exit |

### Tilemap

| Файл | Описание |
|---|---|
| `Tilemap/Tileset.cs` | Атлас тайлов |
| `Tilemap/TilemapLayer.cs` | Описание слоя (метаданные) |
| `Tilemap/TilemapChunk.cs` | Тайлы одного региона |
| `Tilemap/TilemapComponent.cs` | Карта целиком, API GetTile/SetTile |
| `Tilemap/TilemapRenderer.cs` | Рисует только видимые тайлы |
| `Tilemap/TilemapLoader.cs` | Загрузка карты из JSON |
| `Tilemap/TilemapData.cs` | DTO для JSON |

### Мир

| Файл | Описание |
|---|---|
| `WorldEngine/Chunk.cs` | Один чанк мира |
| `WorldEngine/ChunkCoord.cs` | Координата чанка |
| `WorldEngine/ChunkManager.cs` | Подгрузка/выгрузка чанков |

### Транспорт (для factory-игр)

| Файл | Описание |
|---|---|
| `Transport/TransportSlot.cs` | Один слот линии (ContentId + Amount) |
| `Transport/TransportLine.cs` | Универсальная линия: конвейер, труба, кабель |
| `Transport/TransportSystem.cs` | Движение слотов + передача соседям |

### Инвентарь и крафт

| Файл | Описание |
|---|---|
| `Items/Item.cs` | Описание типа предмета |
| `Items/ItemStack.cs` | Стек (itemId, count) |
| `Items/ItemRegistry.cs` | Реестр всех типов |
| `Items/Inventory.cs` | Массив слотов |
| `Items/InventoryOperations.cs` | Add/Remove/Transfer/Split |
| `Crafting/Recipe.cs` | Рецепт (вход, выход, время) |
| `Crafting/RecipeRegistry.cs` | Реестр рецептов |
| `Crafting/CraftingMachine.cs` | Состояние машины-крафтера |
| `Crafting/CraftingSystem.cs` | Автоматический крафт |

### Ticking (для factory-игр)

| Файл | Описание |
|---|---|
| `Ticking/TickRate.cs` | Enum частоты (EveryTick, Every4, Every16…) |
| `Ticking/Tickable.cs` | Компонент «обновляется по расписанию» |
| `Ticking/ITickGroup.cs` | Группа тикающих систем |
| `Ticking/TickScheduler.cs` | Планировщик по частоте и фазам |
| `Ticking/TickSchedulerSystem.cs` | Обёртка для SystemScheduler |

### Размещение (для factory-игр)

| Файл | Описание |
|---|---|
| `Placement/PlaceableComponent.cs` | Маркер «ставится на сетку» |
| `Placement/GridPlacementSystem.cs` | Ставит здания мышью + призрак |

### Сцены и поток

| Файл | Описание |
|---|---|
| `GameFlow/GameSession.cs` | Каркас игровой сессии (InitializeResources / StartSession / CompleteSession) |
| `GameFlow/GameContext.cs` | Общий контекст (App, World, Camera, Events…) |
| `GameFlow/IGameState.cs` | Интерфейс состояния игры |
| `GameFlow/GameStateBase.cs` | База для состояний |
| `GameFlow/GameStateMachine.cs` | Менеджер состояний (Loading → Playing → Dead…) |
| `Scenes/Scene.cs` | Описание сцены |
| `Scenes/SceneManager.cs` | Стек сцен, Load/Push/Pop |

### UI (свой, не ImGui)

| Файл | Описание |
|---|---|
| `UI/UIRoot.cs` | Корень, стек экранов |
| `UI/UIElement.cs` | Базовый элемент: якорь, layout, события |
| `UI/UIRect.cs`, `Anchor.cs` | Прямоугольник и якоря |
| `UI/UIRenderContext.cs` | Что доступно при отрисовке |
| `UI/UITheme.cs` | Цвета и стили по умолчанию |
| `UI/FontProvider.cs` | Провайдер шрифта |
| `UI/DialogueBox.cs` | ImGui-окно диалога |
| `UI/ImGuiLayer.cs` | Обёртка ImGuiController |
| `UI/Widgets/Panel.cs`, `Label.cs`, `Button.cs`, `Image.cs`, `ProgressBar.cs` | Виджеты |
| `UI/Widgets/StackPanel.cs`, `GridPanel.cs` | Layout-контейнеры |

### Отладка и инструменты

| Файл | Описание |
|---|---|
| `Diagnostics/DebugConfig.cs` | Флаги: ShowGizmos, ShowGrid, ShowProfiler… |
| `Diagnostics/DebugOverlay.cs` | Реестр инструментов отладки |
| `Diagnostics/IDebugTool.cs` | Контракт инструмента |
| `Diagnostics/DebugDraw.cs` | Gizmo: Box, Circle, Line, Arrow, Grid, Text |
| `Diagnostics/EntityInspector.cs` | Клик по сущности → редактирование полей |
| `Diagnostics/WorldInspectorWindow.cs` | Список компонентов по типам |
| `Diagnostics/ComponentDrawer.cs` | Рефлекторный ImGui-drawer |
| `Diagnostics/Profiler.cs` | FPS, время кадра, бюджет, GC |
| `Diagnostics/ProfilerWindow.cs` | ImGui-окно профайлера |
| `Diagnostics/PerfTimelineWindow.cs` | Таймлайн задач по воркерам |
| `Diagnostics/Watch.cs` | Реестр значений для наблюдения |
| `Diagnostics/WatchWindow.cs` | ImGui-панель Watch |
| `Diagnostics/Log.cs` | Логирование с уровнями |
| `Diagnostics/ErrorLogger.cs` | Обёртка над Log |
| `Diagnostics/FatalErrorOverlay.cs` | Экран фатальной ошибки |
| `Diagnostics/Safe.cs` | Try-catch-хелперы для опциональных подсистем |
| `Diagnostics/Assert.cs` | Assertions (стираются в Release) |
| `Diagnostics/Guard.cs` | Проверки аргументов (стираются в Release) |
| `Diagnostics/CommandLine.cs` | Парсер `--fullscreen --level=forest` |
| `Diagnostics/Console/ConsoleSystem.cs` | Ядро консоли |
| `Diagnostics/Console/ConsoleWindow.cs` | ImGui-окно консоли (F9) |
| `Diagnostics/Console/IConsoleCommand.cs` | Интерфейс команды |
| `Diagnostics/Console/LambdaCommand.cs` | Команда через лямбду |
| `Diagnostics/Console/BuiltInCommands.cs` | help, clear |
| `Diagnostics/Tools/ConsoleTool.cs`, `WatchTool.cs`, `ProfilerTool.cs`, `TimelineTool.cs`, `DebugFlagsTool.cs`, `AssetBrowserTool.cs` | Обёртки IDebugTool |
| `Diagnostics/Validation/ComponentValidator.cs` | Валидация компонентов по атрибутам |
| `Diagnostics/Validation/*Attribute.cs` | Range, Positive, NonNegative, NotNull, NotEmpty |

### Сериализация

| Файл | Описание |
|---|---|
| `Serialization/Binary/IBinarySerializable.cs` | Интерфейс бинарной сериализации |
| `Serialization/Binary/BinaryWorldSerializer.cs` | Save/Load мира в бинарный формат |
| `Serialization/Binary/BinaryComponentRegistry.cs` | Реестр компонентов для бинаря |
| `Serialization/Binary/BinaryExtensions.cs` | Хелперы Write/Read для типов |

---

## Игра — Game/ (Lighthouse Keeper, top-down RPG)

| Файл | Описание |
|---|---|
| `Program.cs` | Точка входа |
| `MyGame.cs` | Наследник GameSession — 4 метода жизненного цикла |
| `GameState.cs` | Игровое состояние (HP, XP, уровень, искры) |
| `HeadlessGameRunner.cs` | Запуск логики без графики (для тестов) |
| `Ai/EnemyState.cs`, `SlimeBehavior.cs` | FSM слизня |
| `Components/DialogueData.cs`, `Pickup.cs` | Игровые компоненты |
| `Data/MapData.cs` | DTO карты |
| `Spawning/EntityFactory.cs` | Фабрика игрока/врага/NPC/пикапа |
| `Spawning/MapLoader.cs`, `AsyncMapLoader.cs` | Загрузка карты |
| `States/GameContext.cs` | Игровой контекст (наследник движкового) |
| `States/*State.cs` | Loading, Playing, Dialogue, Dead, Victory |
| `Systems/CombatSystem.cs` | Бой, урон, i-frames |
| `Systems/ProgressionSystem.cs` | XP, уровни, перки |
| `Systems/PickupSystem.cs` | Подбор искр |
| `Systems/QuestSystem.cs` | Квесты |
| `Systems/PlayerInputSystem.cs` | Ввод игрока (top-down) |
| `Systems/PlayerFlickerSystem.cs`, `HitFlashSystem.cs` | Визуальные эффекты |
| `Systems/LightFollowSystem.cs` | Свет за игроком |
| `Systems/AttackHitboxRenderer.cs` | Рисует хитбокс при атаке |
| `Systems/DamagePopupSystem.cs` | Всплывающие цифры урона |
| `UI/GameHud.cs`, `GameOverScreen.cs`, `LevelUpMenu.cs` | UI игры |
| `DebugGame/GameDebugOverlay.cs`, `NavGridDebugDraw.cs`, `GameGiagnostics.cs` | Отладка игры |
| `Effects/ParticlePresets.cs` | Пресеты частиц (HitSpark, DeathPuff…) |
| `Events/GameEvents.cs` | Игровые события |

---

## Игра — MyGame/ (FactoryGame)

| Файл | Описание |
|---|---|
| `Program.cs` | Точка входа |
| `FactoryGameApp.cs` | Наследник GameSession |
| `GameState.cs` | Игровое состояние |

---

## Тесты — Tests/

Юнит-тесты движка (xUnit). ~35 файлов, покрывают:

- **ECS:** Entity, World, Query, Lifecycle, Transform hierarchy
- **Ассеты:** AssetCache
- **Аудио:** (не покрыто)
- **AI:** AStar, BehaviorTree, StateMachine, Steering
- **Коллизии:** Aabb, Raycast, SpatialHash
- **Мир:** ChunkManager, Tilemap, TickScheduler
- **Инфраструктура:** Arena, Pool, JobSystem, Scheduler, EventBus, Scene, SceneManager
- **Сериализация:** BinarySerializer
- **Утилиты:** DeterministicRandom, Guard, ComponentValidator, RequireComponent
- **UI:** AutoSize, StackPanel, GridPanel

---

## Технологии

| Слой | Технология |
|---|---|
| Язык | C# 12 / .NET 8 |
| Графика | OpenGL 3.3 Core |
| Окно/ввод | Silk.NET (GLFW) |
| Звук | Silk.NET.OpenAL |
| UI отладки | ImGui.NET |
| Шрифты | StbTrueTypeSharp |
| Изображения | StbImageSharp |
| Тесты | xUnit |
| JSON | System.Text.Json |

---

## Ключевые принципы

1. **Движок не знает про игру.** Обратные ссылки запрещены — проверяется компилятором.
2. **Разделение слоёв.** `Application` → `GameSession` → `MyGame`. Каждый слой — своя ответственность.
3. **Opt-in для всего.** Системы, инструменты, эффекты — подключаются по необходимости.
4. **Не пиши код “на будущее”.** Абстракция на 3-м повторении, не раньше.
5. **Тестируемость.** Критичные системы покрыты юнит-тестами.
6. **Detect-early.** Guard, Assert, ComponentValidator — ловят баги в Debug, стираются в Release.

---