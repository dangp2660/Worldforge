# Worldforge v0.1 Application Initialization Sequence

## Startup order

1. `Input`
2. `SceneFlow`
3. `Gameplay.Inventory`
4. `Gameplay.Gathering`

## Rules

- `BootstrapManager` là composition root của runtime bootstrap.
- `ApplicationStartupFlow.CreateDefault()` tạo startup flow mặc định cho application.
- Core systems được khai báo trong `Core`.
- Gameplay modules tự đăng ký qua `IApplicationSystemProvider` trong assembly của module.
- `ApplicationStartupFlow` tự resolve thứ tự khởi tạo dựa trên `Dependencies`.
- Nếu dependency bị thiếu, bootstrap sẽ ném lỗi sớm.
- Nếu có circular dependency, bootstrap sẽ ném lỗi sớm.
- Nếu một system bị khai báo trùng tên, chỉ system đầu tiên được giữ lại.
- Mỗi system chỉ được initialize một lần trong mỗi vòng đời bootstrap.
- Shutdown chạy theo thứ tự ngược lại của execution plan.
- Runtime shutdown snapshot được lưu trước khi service container bị dispose.
- Event subscriptions, temporary objects, và runtime resources được cleanup tập trung qua `ApplicationBootstrapContext`.

## Current dependencies

- `SceneFlow` depends on `Input`
- `Gameplay.Inventory` depends on `Input`, `SceneFlow`
- `Gameplay.Gathering` depends on `SceneFlow`, `Gameplay.Inventory`

## Extension pattern

Để thêm gameplay module mới:

1. Implement `IApplicationSystem` trong assembly của module.
2. Implement `IApplicationSystemProvider` để trả về system init của module.
3. Khai báo `Dependencies` rõ ràng.
4. Để `ApplicationStartupFlow.CreateDefault()` discovery tự động.

## Shutdown sequence

1. Đánh dấu application bước vào trạng thái shutdown và snapshot metadata runtime.
2. Chạy các save operations đã đăng ký để thu thập dữ liệu runtime cần persist.
3. Gọi `Shutdown()` của từng `IApplicationSystem` theo thứ tự ngược của execution plan.
4. Cleanup các event subscriptions và cleanup callbacks theo thứ tự unwind.
5. Release các runtime resources đã đăng ký (`IDisposable`).
6. Destroy các temporary `UnityEngine.Object` được tạo trong runtime.
7. Persist snapshot ra `Application.persistentDataPath/Worldforge/worldforge-runtime-shutdown.json`.
8. Dispose service container và kết thúc lifecycle một cách graceful.

## Saved runtime data

- Metadata cơ bản của application: name, version, shutdown reason, timestamp UTC.
- Danh sách `LoadedSystems` và `LoadedGameplayModules`.
- Scene information: startup scene và active scene tại thời điểm shutdown.
- Runtime state do module đóng góp qua `RegisterSaveOperation`, ví dụ:
  - `inventory.registeredContainerCount`
  - `gathering.serviceLifetime`
