# Worldforge v0.1 Application Initialization Sequence

## Startup order

1. `Input`
2. `SceneFlow`
3. `GameSession`

## Rules

- `BootstrapManager` là composition root của runtime bootstrap.
- `ApplicationStartupFlow.CreateDefault()` tạo startup flow mặc định cho application.
- Core systems được khai báo trong `Core`.
- Gameplay modules cấp session tự đăng ký qua `IGameSessionSystemProvider` trong assembly của module.
- `ApplicationStartupFlow` tự resolve thứ tự khởi tạo dựa trên `Dependencies`.
- Nếu dependency bị thiếu, bootstrap sẽ ném lỗi sớm.
- Nếu có circular dependency, bootstrap sẽ ném lỗi sớm.
- Nếu một system bị khai báo trùng tên, chỉ system đầu tiên được giữ lại.
- Mỗi system chỉ được initialize một lần trong mỗi vòng đời bootstrap.
- Shutdown chạy theo thứ tự ngược lại của execution plan.
- `GameSessionManager` quản lý session lifecycle riêng, dùng `ServiceScope` riêng cho session-level services.
- Runtime gameplay systems chỉ được initialize khi `StartNewGame()` được gọi.
- Runtime shutdown snapshot được lưu trước khi service container bị dispose.
- Event subscriptions, temporary objects, và runtime resources được cleanup tập trung qua `ApplicationBootstrapContext`.

## Application-level dependencies

- `SceneFlow` depends on `Input`
- `GameSession` depends on `SceneFlow`

## Session-level dependencies

1. `Gameplay.Inventory`
2. `Gameplay.Gathering`

- `Gameplay.Gathering` depends on `Gameplay.Inventory`

## Extension pattern

Để thêm gameplay module mới:

1. Implement `IGameSessionSystem` trong assembly của module gameplay có session lifetime.
2. Implement `IGameSessionSystemProvider` để trả về session system của module.
3. Khai báo `Dependencies` rõ ràng.
4. Để `GameSessionManager.StartNewGame()` discovery tự động.

Đối với app-level core system, tiếp tục dùng `IApplicationSystem` và `IApplicationSystemProvider`.

## Shutdown sequence

1. Đánh dấu application bước vào trạng thái shutdown và snapshot metadata runtime.
2. Nếu có active game session, `GameSessionManager` shutdown session-level systems trước.
3. Release toàn bộ cleanup callbacks, runtime resources, temporary objects, và session scope của session.
4. Chạy các save operations đã đăng ký để thu thập dữ liệu runtime cần persist.
5. Gọi `Shutdown()` của từng `IApplicationSystem` theo thứ tự ngược của execution plan.
6. Cleanup các event subscriptions và cleanup callbacks theo thứ tự unwind.
7. Release các runtime resources đã đăng ký (`IDisposable`).
8. Destroy các temporary `UnityEngine.Object` được tạo trong runtime.
9. Persist snapshot ra `Application.persistentDataPath/Worldforge/worldforge-runtime-shutdown.json`.
10. Dispose service container và kết thúc lifecycle một cách graceful.

## Saved runtime data

- Metadata cơ bản của application: name, version, shutdown reason, timestamp UTC.
- Danh sách `LoadedSystems` và `LoadedGameplayModules`.
- Scene information: startup scene và active scene tại thời điểm shutdown.
- Runtime state do module đóng góp qua `RegisterSaveOperation`, ví dụ:
  - `inventory.registeredContainerCount`
  - `gathering.serviceLifetime`
- Session state do `GameSessionManager` đóng góp, ví dụ:
  - `session.activeSessionId`
  - `session.lastShutdownReason`
  - `session.playerSpawn.source`
