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
