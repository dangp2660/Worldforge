# WF_CodingStandard_V1.0
Project: Worldforge
Document Type: Technical Design Document — Coding Standard
Version: V1.0
Status: Initial Engineering Standard Baseline
Target Engine: Unity
Primary Language: C#


## 1. DOCUMENT PURPOSE

Tài liệu xác định các tiêu chuẩn kỹ thuật và quy tắc phát triển phần mềm chính thức cho Worldforge.

Mục tiêu của tài liệu là bảo đảm toàn bộ Source Code, Gameplay System, AI System, Animation System, Data System và Infrastructure System được phát triển theo cùng một Architecture, Convention và tiêu chuẩn chất lượng.

Tài liệu được sử dụng làm nguồn quy tắc chung cho Developer, Technical Designer, Technical Artist, Code Reviewer và AI Coding Agent.

Mọi Implementation mới, Refactor hoặc thay đổi Architecture phải tuân thủ các quy tắc được xác định trong tài liệu này.

## 2. DOCUMENT SCOPE

Tài liệu bao gồm:
- Engineering Roles.
- Coding Philosophy.
- Architecture Principles.
- Unity Project Architecture.
- Assembly Definition Rules.
- Module Ownership.
- Dependency Rules.
- Folder Structure Rules.
- Namespace Rules.
- Naming Convention.
- Class Design Rules.
- Interface Rules.
- Inheritance Rules.
- Composition Rules.
- MonoBehaviour Rules.
- Unity Lifecycle Rules.
- Dependency Management.
- Composition Root.
- FSM / HFSM Standard.
- Behavior Manager Standard.
- AI Architecture Rules.
- Gameplay System Rules.
- Creature System Rules.
- Ability System Rules.
- Animation Architecture Rules.
- Data Architecture.
- ScriptableObject Rules.
- Event Architecture.
- Runtime State Ownership.
- Save System Rules.
- Addressables Rules.
- Async Operation Rules.
- Memory Management.
- Garbage Collection Rules.
- Performance Rules.
- Logging Standard.
- Error Handling.
- Testing Standard.
- Code Review Standard.
- Definition of Done.
- Document Governance.

## 3. ENGINEERING ROLES

Worldforge xác định các Engineering Role chính:

**Architecture Engineer**

Chịu trách nhiệm duy trì kiến trúc tổng thể, Module Boundary, Dependency Direction, Assembly Structure, Abstraction Policy và Architectural Consistency.

**Gameplay Engineer**

Chịu trách nhiệm Character, Combat, Ability, Attribute, Status Effect, Creature, Inventory, Equipment và Interaction System.

**State Machine Engineer**

Chịu trách nhiệm FSM, HFSM, State Lifecycle, Transition Policy, State Ownership và State Conflict Resolution.

**Behavior Engineer**

Chịu trách nhiệm Behavior Manager, Behavior Lifecycle, Behavior Selection, Priority, Interruption và Conflict Policy.

**AI Engineer**

Chịu trách nhiệm AI Brain, Perception, Memory, Decision System, Navigation, AI Behavior và AI Performance.

**Animation Engineer**

Chịu trách nhiệm Animation Architecture, Animator Integration, Animation Layer, Animation Synchronization, Root Motion và Animation Naming Standard.

**Data Engineer**

Chịu trách nhiệm Definition Data, Runtime Data, Save Data, Configuration Data, ScriptableObject và Data Validation.

**Performance Engineer**

Chịu trách nhiệm CPU, Memory, GC Allocation, Update Cost, Asset Lifetime, Object Lifetime và Profiling Policy.

**Testing Engineer**

Chịu trách nhiệm Unit Test, Integration Test, PlayMode Test, EditMode Test và Regression Test.

**Code Reviewer**

Chịu trách nhiệm xác nhận Source Code tuân thủ Coding Standard, Architecture Contract và Definition of Done.

## 4. CODING PHILOSOPHY

Worldforge áp dụng các nguyên tắc:
- Clean Code.
- SOLID.
- Composition over Inheritance.
- Data-Driven Design.
- Event-Driven Communication khi phù hợp.
- Dependency Inversion.
- Explicit Dependency.
- High Cohesion.
- Low Coupling.
- Separation of Concerns.
- Single Source of Truth.
- Clear Ownership.
- Controlled Lifetime.
- Testable Gameplay Logic.
- Modular Architecture.
- Designer-Friendly Data Workflow.
- Performance-Aware Development.
- Refactorability.
- Maintainability.

Mọi Abstraction phải phục vụ Dependency Boundary, Variation Point hoặc Architectural Requirement thực tế.

Không tạo Abstraction chỉ vì khả năng có thể cần trong tương lai.

Không ưu tiên giảm số lượng Class nếu điều đó làm tăng Coupling hoặc phá vỡ Responsibility Boundary.

Không ưu tiên Pattern phức tạp khi giải pháp đơn giản đáp ứng đầy đủ yêu cầu kiến trúc.

## 5. UNITY PROJECT ARCHITECTURE

Worldforge sử dụng Modular Layered Architecture.

Các Layer chính:
- Core Layer.
- Domain Layer.
- Application Layer.
- Presentation Layer.
- Infrastructure Layer.

Dependency phải tuân theo hướng Architecture đã xác định.

Layer cấp thấp không được phụ thuộc ngược vào Layer cấp cao.

Domain Logic phải độc lập tối đa với Unity Engine.

Domain Logic không được phụ thuộc trực tiếp vào GameObject, MonoBehaviour, Animator, UI, Scene hoặc Unity Presentation API nếu không có Architectural Justification được phê duyệt.

Presentation Layer chịu trách nhiệm kết nối Gameplay State với Unity Runtime Representation.

Infrastructure Layer chịu trách nhiệm implementation liên quan Persistence, Addressables, Save, Pooling và External Technology.

## 6. ASSEMBLY DEFINITION & MODULE OWNERSHIP

Worldforge bắt buộc sử dụng Assembly Definition cho các Module chính.

Không duy trì toàn bộ Runtime Code trong Assembly-CSharp.

Mỗi Module phải xác định:
- Responsibility.
- Public API.
- Internal Implementation.
- Owned Data.
- Runtime State.
- Dependencies.
- Events.
- Initialization Policy.
- Lifetime.
- Disposal Policy.
- Testing Boundary.

Assembly Dependency phải có hướng rõ ràng.

Circular Dependency bị cấm.

Runtime Assembly không được phụ thuộc Editor Assembly.

Test Assembly phải tách khỏi Production Assembly.

Không được tạo Dependency mới giữa các Assembly nếu chưa xác định rõ Ownership và Architectural Necessity.

## 7. FOLDER STRUCTURE & NAMESPACE STANDARD

Folder Structure phải phản ánh Module Ownership và Feature Ownership.

Không tổ chức toàn bộ Project chỉ dựa trên loại File.

Không sử dụng Folder tổng hợp không có Ownership rõ ràng.

Mỗi File phải thuộc về một Module hoặc Feature cụ thể.

Namespace phải phản ánh Module và Feature sở hữu Class.

Namespace không được phụ thuộc vào vị trí Scene.

Mỗi Class phải có một vị trí Ownership duy nhất.

Không tạo các khu vực chứa Code không xác định Ownership như Misc, General, CommonStuff hoặc Temporary.

## 8. NAMING CONVENTION

Naming phải thể hiện Intent, Responsibility và Domain Meaning.

Class, Struct, Enum, Property, Method và Public Member sử dụng PascalCase.

Private Field sử dụng `_camelCase`.

Parameter và Local Variable sử dụng camelCase.

Interface sử dụng Prefix `I`.

Boolean Naming sử dụng Semantic Prefix phù hợp như `Is`, `Has`, `Can`, `Should` hoặc `Requires`.

Method Naming phải thể hiện hành động hoặc Intent.

Tên Class không được sử dụng các từ chung chung nếu không phản ánh Responsibility thực tế.

Tên `Manager` chỉ được sử dụng khi Class thực sự quản lý Lifecycle, Registration hoặc Collection của nhiều Object thuộc cùng một Domain.

Abbreviation phải được thống nhất toàn Project.

Không sử dụng Naming tạm thời trong Production Code.

## 9. CLASS, INTERFACE, INHERITANCE & COMPOSITION RULES

Mỗi Class phải có một Primary Responsibility.

Class phải có Ownership rõ ràng.

Class không được sở hữu quá nhiều Dependency không liên quan.

God Class bị cấm.

Inheritance Depth phải được kiểm soát.

Gameplay Architecture ưu tiên Composition over Inheritance.

Inheritance chỉ được sử dụng khi tồn tại quan hệ is-a ổn định và Subclass tuân thủ Substitution Contract.

Không tạo Base Class chỉ nhằm mục đích tái sử dụng một lượng nhỏ Code.

Interface phải đại diện cho Capability, Contract hoặc Dependency Boundary thực tế.

Không tạo Interface cho mọi Class.

Không sử dụng Interface như một Naming Convention bắt buộc.

Class vượt quá Complexity Threshold phải được Architecture Review.

## 10. MONOBEHAVIOUR & UNITY LIFECYCLE STANDARD

MonoBehaviour là Unity Integration Boundary.

MonoBehaviour chịu trách nhiệm nhận Unity Lifecycle Callback, quản lý Serialized Unity Reference và kết nối Unity Runtime với Gameplay Logic.

MonoBehaviour không được mặc định trở thành nơi chứa Domain Logic.

Không sử dụng Update nếu không có nhu cầu Frame-Based Processing thực tế.

FixedUpdate chỉ được sử dụng cho Logic phụ thuộc Physics Timestep.

LateUpdate chỉ được sử dụng khi Execution Order Requirement được xác định rõ.

Không thực hiện Expensive Lookup lặp lại trong Gameplay Loop.

Không phụ thuộc vào Script Execution Order để sửa Architectural Dependency không đúng.

Object Initialization phải có Policy rõ ràng.

Object Cleanup phải phù hợp với Lifetime của Object.

## 11. DEPENDENCY MANAGEMENT & COMPOSITION ROOT

Dependency phải Explicit.

Gameplay Object không tự tìm Global Dependency.

Service Locator và Global Singleton phải được hạn chế nghiêm ngặt.

Static Mutable State không được sử dụng làm Gameplay State Ownership mặc định.

Dependency phải được Resolve tại Composition Boundary.

Mỗi Gameplay Context phải xác định Composition Root.

Composition Root chịu trách nhiệm Creation, Dependency Resolution, Initialization, Lifetime Coordination và Disposal.

Dependency Lifetime không được vượt quá Owner Lifetime nếu không có Ownership Policy rõ ràng.

Circular Runtime Dependency bị cấm.

## 12. FSM & HFSM STANDARD

FSM được sử dụng cho State Domain đơn giản.

HFSM được sử dụng cho State Domain có Hierarchy và Complexity cao.

Mỗi State phải có Responsibility rõ ràng.

State Lifecycle phải thống nhất.

State Transition phải có Rule rõ ràng.

State không được trực tiếp điều khiển System ngoài Responsibility Boundary.

State không được trở thành nơi tập trung toàn bộ Character Logic.

Transition Conflict phải có Resolution Policy.

Interruptible State phải xác định Interruption Policy.

Concurrent State Domain phải được tách thành State Machine độc lập khi phù hợp.

Gameplay State và Animation State phải được phân biệt.

State Machine phải hỗ trợ Debugging và State Tracing.

## 13. BEHAVIOR MANAGER STANDARD

Behavior Manager chịu trách nhiệm lựa chọn, kích hoạt, duy trì, Interrupt và kết thúc Behavior.

Behavior Manager không thay thế State Machine.

Behavior phải có Lifecycle rõ ràng.

Behavior Selection phải sử dụng Policy xác định.

Priority và Score phải có Ownership rõ ràng.

Behavior Conflict phải được giải quyết theo Policy thống nhất.

Behavior không được Bypass Gameplay System API.

Behavior Manager không được trở thành God Object điều khiển toàn bộ Character.

Behavior Evaluation Frequency phải phù hợp với Performance Requirement.

Behavior System phải hỗ trợ Debugging và Behavior Tracing.

## 14. AI ARCHITECTURE STANDARD

AI phải được chia thành các Responsibility độc lập.

Perception chịu trách nhiệm thu thập Stimulus.

Memory chịu trách nhiệm lưu trữ Knowledge của Agent.

Decision System chịu trách nhiệm lựa chọn Intent.

Behavior System chịu trách nhiệm điều phối Behavior.

Action System chịu trách nhiệm gửi Request tới Gameplay System.

Navigation chịu trách nhiệm Movement Planning và Path Execution.

AI không được trực tiếp thay đổi Authoritative Gameplay State thuộc System khác.

AI không được Bypass Combat, Ability, Movement hoặc Interaction Contract.

Decision Model phải được xác định rõ cho từng AI Archetype.

Không trộn nhiều Decision Architecture nếu không có Responsibility Boundary rõ ràng.

AI Update Frequency phải có Scalability Policy.

AI System phải hỗ trợ LOD, Throttling hoặc Scheduled Evaluation khi cần.

## 15. GAMEPLAY SYSTEM STANDARD

Mỗi Gameplay System phải xác định:
- Responsibility.
- Public Contract.
- Runtime State.
- Owned Data.
- Dependencies.
- Events.
- Initialization.
- Lifetime.
- Failure Policy.
- Save Responsibility.
- Testing Boundary.

System không được trực tiếp sửa Runtime State thuộc Ownership của System khác.

Cross-System Interaction phải thông qua Contract được xác định.

Gameplay Rule không được phụ thuộc trực tiếp vào UI.

Presentation không được trở thành Authoritative Gameplay Source.

Gameplay System phải hỗ trợ mở rộng mà không yêu cầu sửa đổi hàng loạt Module không liên quan.

## 16. CREATURE SYSTEM STANDARD

Creature Type được triển khai theo Data + Feature Composition.

Không sử dụng Inheritance Hierarchy sâu để biểu diễn Creature Type.

Creature Definition phải tách khỏi Creature Runtime State.

Racial Trait phải có Ownership rõ ràng.

Racial Ability phải tuân thủ Ability Contract.

Racial Weakness phải được triển khai thông qua Gameplay Rule hoặc Feature Contract phù hợp.

Creature Visual Representation phải tách khỏi Authoritative Gameplay Identity.

Creature Animation Profile phải tách khỏi Creature Gameplay Definition khi phù hợp.

Creature Feature phải có Lifecycle và Dependency rõ ràng.

## 17. ABILITY SYSTEM STANDARD

Ability Definition phải tách khỏi Ability Runtime State.

Ability phải có Lifecycle thống nhất.

Activation phải trải qua Validation.

Resource Consumption phải có Ownership rõ ràng.

Cooldown State phải có Single Source of Truth.

Ability Cancellation phải có Policy.

Ability Interruption phải có Policy.

Concurrent Ability phải có Conflict Resolution Rule.

Ability không được phụ thuộc trực tiếp vào Concrete Player hoặc Enemy Class.

Ability Effect phải tách khỏi Presentation Effect.

Ability System phải hỗ trợ Debugging, Runtime Inspection và Execution Tracing.

## 18. EVENT ARCHITECTURE STANDARD

Event phải có Scope rõ ràng.

Worldforge phân biệt Local Event, Module Event và Global Game Event.

Global Event không được sử dụng làm Communication Mechanism mặc định.

Direct Dependency được ưu tiên khi Ownership Relationship rõ ràng.

Event Subscription phải có Lifetime Policy.

Mọi Subscription phải có Unsubscribe hoặc Automatic Lifetime Handling.

Không sử dụng String làm Event Identifier.

Event Payload phải Immutable khi phù hợp.

Event không được sử dụng để che giấu Dependency quan trọng.

Event Chain quá dài phải được Architecture Review.

## 19. ANIMATION ARCHITECTURE & NAMING STANDARD

Gameplay State và Animation State phải độc lập về Authority.

Animator không phải Authoritative Gameplay State Machine.

Gameplay System gửi Animation Intent hoặc Presentation State.

Animation System chịu trách nhiệm Resolve Presentation.

Gameplay System không được gọi Animator rải rác từ nhiều Module.

Animator Parameter phải có Ownership.

Animation Event không được trực tiếp thay đổi Authoritative Gameplay State nếu không thông qua Gameplay Contract.

Root Motion phải có Policy theo Action Category.

Animation Synchronization phải có Interruption và Failure Policy.

Animation Asset Naming phải thống nhất toàn Project.

Animation Clip, Controller, Blend Tree, Avatar Mask, Override Controller, Animation Profile và Animator Parameter phải có Naming Pattern chính thức.

## 20. DATA ARCHITECTURE & SCRIPTABLEOBJECT STANDARD

Worldforge phân biệt:
- Definition Data.
- Configuration Data.
- Runtime State.
- Session State.
- Save Data.
- Presentation Data.

ScriptableObject chủ yếu được sử dụng cho Definition Data và Authoring Workflow.

Shared ScriptableObject Asset không được chứa Runtime Mutable State nếu không có Architecture đặc biệt được phê duyệt.

Runtime State phải có Owner.

Data Validation phải được thực hiện trước khi Content được sử dụng trong Production Build.

Không Duplicate Authoritative Data giữa nhiều System.

Data Migration phải được hỗ trợ đối với Persistent Content Format.

## 21. ADDRESSABLES & ASSET LIFETIME STANDARD

Asset phải có Ownership và Lifetime rõ ràng.

Load và Release phải đối xứng.

Không Synchronous Load Asset lớn trong Gameplay Critical Path.

Không giữ Asset Reference vượt quá Lifetime cần thiết.

Không sử dụng Addressables cho mọi Asset nếu không có nhu cầu quản lý Memory hoặc Dynamic Loading.

Asset Loading Failure phải có Policy.

Cancellation phải được hỗ trợ khi Operation Lifetime yêu cầu.

Asset Dependency phải được kiểm soát.

Memory Impact phải được Profiling.

## 22. SAVE SYSTEM & PERSISTENCE STANDARD

Save Data phải tách khỏi Runtime Object.

Gameplay Object không trực tiếp thực hiện File I/O.

Save System phải có Version.

Persistent Data Format phải hỗ trợ Migration Policy.

Save Operation phải có Failure Handling.

Load Operation phải có Validation.

Corrupted Save phải có Recovery Policy.

System sở hữu Runtime State chịu trách nhiệm cung cấp Save Representation thông qua Contract phù hợp.

UI không được trực tiếp sửa Save Data.

## 23. ASYNC, COROUTINE & CANCELLATION STANDARD

Coroutine được sử dụng cho Unity Frame Sequencing.

Async Operation được sử dụng cho Workload phù hợp với Asynchronous Execution.

Không sử dụng `async void` ngoại trừ Boundary bắt buộc.

Long-Running Operation phải có Cancellation Policy.

Operation Lifetime phải liên kết với Owner Lifetime.

Fire-and-Forget Operation bị hạn chế.

Exception trong Asynchronous Operation phải được xử lý.

Không tạo Parallel Operation không kiểm soát.

Concurrency-Sensitive State phải có Ownership và Synchronization Policy.

## 24. MEMORY, GC & OBJECT LIFETIME STANDARD

Không tạo Allocation không cần thiết trong Hot Path.

GC Allocation phải được Profiling.

LINQ phải được hạn chế trong Performance-Critical Path.

Component Reference phải được Cache khi sử dụng thường xuyên.

Object Pooling được sử dụng cho Object có Spawn/Despawn Frequency cao khi Profiling chứng minh cần thiết.

Closure Allocation phải được kiểm soát.

Boxing phải được kiểm soát.

String Allocation trong Gameplay Loop phải được hạn chế.

Object Lifetime phải có Owner.

Dispose Policy phải được xác định cho Resource cần Cleanup.

Không giữ Reference tới Destroyed Unity Object ngoài Lifetime cần thiết.

## 25. PERFORMANCE STANDARD

Performance Optimization phải dựa trên Profiling.

Không Premature Optimization làm phá vỡ Maintainability khi chưa có Evidence.

Mỗi Performance-Sensitive System phải xác định Update Frequency.

Không mặc định Update mọi Agent mỗi Frame.

Ưu tiên Event, Scheduled Update, Dirty Flag, Distance-Based Update và LOD khi phù hợp.

Main Thread Cost phải được kiểm soát.

Physics Query phải được kiểm soát.

Animator Cost phải được Profiling.

AI Cost phải có Scalability Policy.

Memory Budget phải được xác định cho System lớn.

Performance Regression phải được kiểm tra trước Release Milestone.

## 26. LOGGING & ERROR HANDLING STANDARD

Logging phải có Category và Severity.

Production Code không được Spam Log trong Gameplay Loop.

Development Log phải có Configuration Policy.

Exception không được sử dụng cho Normal Gameplay Flow.

Recoverable Error phải có Recovery Policy.

Fatal Error phải được phân biệt với Gameplay Failure thông thường.

Error không được Silently Ignored.

Fallback Behavior phải được xác định khi phù hợp.

Log Message phải cung cấp đủ Context để Debugging.

## 27. TESTING STANDARD

Domain Logic phải có khả năng Unit Test.

Integration Boundary phải có Integration Test khi cần.

Unity Runtime Behavior sử dụng PlayMode Test khi phù hợp.

Editor Tool sử dụng EditMode Test.

Critical System phải có Regression Test.

State Transition phải được Test.

Ability Activation, Cancellation và Interruption phải được Test.

Save, Load và Migration phải được Test.

Object Lifetime và Cleanup phải được Test.

Failure Path phải được Test.

Performance-Sensitive System phải có Performance Validation phù hợp.

## 28. CODE REVIEW STANDARD

Code Review phải kiểm tra:
- Architecture Compliance.
- Module Ownership.
- Dependency Direction.
- Responsibility.
- Naming.
- Complexity.
- Inheritance Depth.
- Composition Quality.
- Runtime State Ownership.
- Event Lifetime.
- Unity Lifecycle.
- Update Cost.
- GC Allocation.
- Async Lifetime.
- Cancellation.
- Animation Synchronization.
- Save Compatibility.
- Error Handling.
- Testability.
- Documentation Impact.

Code không đạt Architecture Contract không được Merge chỉ vì Functionality hoạt động đúng.

## 29. DEFINITION OF DONE

Một Feature chỉ được coi là hoàn thành khi đáp ứng toàn bộ Functional Requirement và Technical Requirement.

Feature phải tuân thủ Coding Standard.

Architecture Boundary không bị vi phạm.

Runtime State Ownership rõ ràng.

Dependency được kiểm soát.

Naming tuân thủ Convention.

Không tồn tại God Class hoặc God Component.

Unity Lifecycle được sử dụng đúng.

Không tồn tại Update không cần thiết.

Event Lifetime được xử lý đúng.

Async Operation có Ownership và Cancellation phù hợp.

Gameplay và Animation Authority được phân tách đúng.

Data được Validation.

Save Compatibility được đánh giá.

Error Handling tồn tại.

Critical Logic có Test.

Không tồn tại Error Log chưa xử lý.

Performance-Sensitive Feature đã được Profiling.

Documentation được cập nhật nếu Feature thay đổi Architecture Contract.

## 30. DOCUMENT GOVERNANCE

`WF_CodingStandard` là tài liệu kỹ thuật cấp nền tảng của Worldforge.

Mọi Technical Design Document khác phải tuân thủ Coding Standard.

Thay đổi Coding Standard phải có lý do kỹ thuật rõ ràng.

Rule mới không được thêm chỉ để xử lý một trường hợp Implementation cục bộ.

Exception đối với Coding Standard phải được Document.

Architecture Decision có ảnh hưởng dài hạn phải được ghi nhận bằng Architecture Decision Record.

Coding Standard phải được cập nhật khi Project thay đổi Technology, Architecture Requirement hoặc Production Constraint.
