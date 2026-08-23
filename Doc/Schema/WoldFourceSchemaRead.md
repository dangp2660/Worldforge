# WoldFourceSchemaRead — Hướng Dẫn Đọc Schema

**Dự án:** Worldforge  
**Phiên bản:** v0.1  
**Loại tài liệu:** Schema Reference — Dành cho Developer và Technical Designer

---

## 1. Mục Đích Tài Liệu

Tài liệu này giải thích cách đọc, hiểu và sử dụng `WoldFourceSchema.md` (DBML format).  
Đồng thời ánh xạ từng bảng schema sang Runtime Implementation tương ứng trong Unity.

---

## 2. Cú Pháp DBML Cơ Bản

Schema được viết bằng **DBML (Database Markup Language)**.

```dbml
Table TênBảng {
  TênCột   KiểuDữLiệu  [modifier]
}
Ref: BảngA.CộtA > BảngB.CộtB   // BảngA.CộtA là FK trỏ tới BảngB.CộtB
```

**Modifier phổ biến:**
| Modifier | Ý nghĩa |
|---|---|
| `[pk]` | Primary Key |
| `[pk, increment]` | PK tự tăng |
| `[not null]` | Bắt buộc có giá trị |
| `[unique]` | Giá trị duy nhất |
| `[default: x]` | Giá trị mặc định |

---

## 3. Cấu Trúc Schema Theo Part

| Part | Module | Các Bảng Chính |
|---|---|---|
| Part 1 | Core | `GameplayTag`, `DamageType`, `Rarity`, `AttributeType`, `StatType`, `GameConfig` |
| Part 2 | Character | `CharacterDefinition`, `Race`, `SubRace`, `CharacterProgressionProfile` |
| Part 3 | Item | `ItemDefinition`, `WeaponComponent`, `ArmorComponent`, `LootTable` |
| Part 4 | Inventory & Equipment | `InventoryDefinition`, `EquipmentLoadout`, `EquippedItem` |
| Part 5 | Ability & Crafting | `AbilityDefinition`, `RecipeDefinition` |
| Part 6 | Building & Settlement | `BuildingDefinition`, `SettlementDefinition` |
| Part 7 | World & Exploration | `WorldDefinition`, `RegionDefinition`, `BiomeDefinition` |
| Part 8 | AI, NPC & Faction | `FactionDefinition`, `AIProfile`, `NPCDefinition` |

---

## 4. Character State Architecture — Ánh Xạ Schema → Runtime

### 4.1 CharacterDefinition (Schema) → CharacterStateMachine (Runtime)

| Schema Field | Runtime Equivalent | Ghi chú |
|---|---|---|
| `CharacterDefinitionId` | `PlayerId` trong `PlayerAvatar` | Định danh character |
| `DefaultMoveSpeed` | `CharacterMovementConfiguration.WalkSpeed` | ScriptableObject |
| `DefaultHP` | `CharacterStateContext.IsAlive` | v0.1 dùng bool; sẽ map sang Health System |
| `CanRespawn` | `DeadState` → `ForceTransition(Idle)` | Logic respawn future |
| `IsPlayer` | Phân biệt Player vs NPC flow | `CharacterStateInitializationSystem` chỉ xử lý player |

### 4.2 Bảng State Machine — Logic (không phải DB table)

Bảng dưới đây mô tả transition policy của v0.1 Character State Machine:

| From State | To State | Điều kiện | Priority |
|---|---|---|---|
| **Mọi state** | `Dead` | `!IsAlive` | 0 |
| `Idle` | `Airborne` | `IsAlive && !IsGrounded` | 10 |
| `Idle` | `Locomotion` | `IsAlive && IsGrounded && HasMoveInput` | 20 |
| `Locomotion` | `Airborne` | `IsAlive && !IsGrounded` | 10 |
| `Locomotion` | `Idle` | `IsAlive && IsGrounded && !HasMoveInput` | 20 |
| `Airborne` | `Locomotion` | `IsAlive && IsGrounded && HasMoveInput` | 10 |
| `Airborne` | `Idle` | `IsAlive && IsGrounded && !HasMoveInput` | 20 |

### 4.3 AbilityDefinition (Schema) → Future State Integration

| Schema Field | Future Runtime Hook |
|---|---|
| `CastTime` | `InteractingState` duration trong Ability System |
| `IsPassive` | Không ảnh hưởng state machine |
| `IsChanneled` | Giữ `InteractingState` trong suốt channel |
| `IsToggle` | Toggle `InteractingState` on/off |

---

## 5. Data Flow Runtime

```
Input System
    │
    ▼
CharacterStateBehaviour.Update()
    │  (xây dựng CharacterStateContext)
    ▼
RuntimeCharacterStateService.Tick(context)
    │
    ▼
CharacterStateMachine.Tick(context)
    ├── EvaluateTransitions() theo Priority
    ├── ExecuteTransition() → OnExit / OnEnter
    │       └── ICharacterState.OnEnter() → publish CharacterAnimationIntent
    │               └── ICharacterAnimationDriver.ApplyIntent()
    │                       └── [v0.2] Animator Parameters
    └── Phát StateChanged event
            └── Subscriber: AI, UI, Ability System...
```

---

## 6. Quy Ước Naming Runtime

| Layer | Pattern | Ví dụ |
|---|---|---|
| State ID | `CharacterStateId` enum | `CharacterStateId.Idle` |
| State class | `{State}State` | `IdleState`, `LocomotionState` |
| Event payload | `Character{Event}Event` | `CharacterStateChangedEvent` |
| Animation intent | `CharacterAnimationIntent` | struct immutable |
| Service interface | `ICharacter{Domain}Service` | `ICharacterStateService` |
| MonoBehaviour | `Character{Domain}Behaviour` | `CharacterStateBehaviour` |

---

## 7. Điểm Mở Rộng Cho Milestone Tương Lai

| Tính năng | Điểm mở rộng |
|---|---|
| Health System | Gọi `ICharacterStateService.ForceTransitionTo(Dead)` khi HP = 0 |
| Interaction System | Gọi `ForceTransitionTo(Interacting)` khi bắt đầu interact |
| Ability System | Đọc `CurrentStateId` để validate điều kiện kích hoạt |
| AI State Machine | Subscribe `StateChanged` để cập nhật AI perception |
| Animation v0.2 | Implement `ICharacterAnimationDriver` trong AnimationController |
| Save System | Đọc `CurrentStateId` để serialize state khi lưu game |
| ScriptableObject Transition | Mở rộng `CharacterTransitionRegistry` để load từ SO asset |
