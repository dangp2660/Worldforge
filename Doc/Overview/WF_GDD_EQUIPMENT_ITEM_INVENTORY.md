WORLD FORGE — EQUIPMENT, ITEM & INVENTORY SYSTEM
Document ID:
WF_GDD_EQUIPMENT_ITEM_INVENTORY
Version:
1.0
Status:
Initial Design Baseline
Project:
Worldforge
Document Type:
Game Design Document — Equipment, Item & Inventory System
1. DOCUMENT PURPOSE
Tài liệu này xác định Equipment, Item & Inventory System cấp Overview của Worldforge.
Mục tiêu là thiết lập các nguyên tắc nền tảng cho:
Item Definition.
Item Instance.
Item Category.
Item State.
Equipment.
Equipment Rarity Tier.
Equipment Requirement.
Equipment Slot.
Creature Anatomy Compatibility.
Weapon Carry.
Equipment Ability.
Equipment Source Contribution.
Equipment Enhancement.
Random Bonus Stat.
Bonus Stat Grade.
Bonus Stat Reroll.
Weapon Evolution.
Inventory Grid.
Item Size.
Item Rotation.
Inventory Weight.
Encumbrance.
Basic Inventory.
Backpack.
Quick Slot.
Consumable.
Tool.
Deployable Item.
Item Container.
Manual Loot.
Loot Search.
Loot Generation.
Expedition Loot Reset.
Storage.
Shared Resource Storage.
Connected Storage Network.
Remote Item Transfer.
Crafting and Building Resource Access.
Item Drop.
Item Despawn.
Salvage.
Vendor Value.
Auto-Sort.
Expedition Item Loss.
Temporary Expedition Session Save.
Major System Relationships.
Major Risks.
Open Design Questions.
Tài liệu tập trung vào:
WHAT the Equipment, Item & Inventory System is.
WHY the system exists.
HOW Equipment, Item, Inventory, Loot and Storage interact with major gameplay systems.
Tài liệu không xác định:
Danh sách Item cụ thể.
Danh sách Weapon cụ thể.
Danh sách Armor cụ thể.
Danh sách Backpack cụ thể.
Danh sách Consumable cụ thể.
Danh sách Tool cụ thể.
Danh sách Deployable cụ thể.
Danh sách Container cụ thể.
Danh sách Bonus Stat cụ thể.
Numerical Item Weight.
Numerical Grid Size cụ thể của từng Item.
Numerical Enhancement Cost.
Numerical Enhancement Growth.
Numerical Reroll Cost.
Numerical Salvage Return.
Numerical Vendor Value.
Numerical Encumbrance Formula.
Numerical Item Despawn Time.
Final Loot Table.
Final Drop Rate.
Final Storage Capacity.
Detailed Runtime Class Architecture.
Detailed Database Schema.
Detailed Save Data Schema.
Các nội dung chi tiết được phát triển trong Content Database, Data/System Architecture, Prototype Requirements hoặc Technical Module Spec khi cần.
2. SYSTEM VISION
Equipment, Item & Inventory System của Worldforge được xây dựng nhằm hỗ trợ:
Meaningful Equipment Choice.
Loot Value.
Inventory Convenience.
Creature Identity.
Crafting & Economy Integration.
Thứ tự ưu tiên thiết kế:
Meaningful Equipment Choice
↓
Loot Value
↓
Inventory Convenience
↓
Creature Identity
↓
Crafting & Economy Integration
Mỗi Equipment phải có Gameplay Identity rõ ràng.
Loot phải tạo ra quyết định:
Có đáng lấy Item hay không.
Có đủ Grid Space hay không.
Có đủ Carry Capacity hay không.
Có nên bỏ Item hiện tại để lấy Item mới hay không.
Có nên đầu tư Resource để Enhancement Equipment hay không.
Có nên tiếp tục Expedition với lượng Loot hiện tại hay rút lui.
Inventory Management phải tạo Gameplay Decision nhưng không trở thành Micromanagement Simulator quá mức.
Equipment Progression phải tạo khác biệt giữa các Item Instance cùng Definition mà không yêu cầu một Random Affix System phức tạp kiểu ARPG.
3. CORE DESIGN PRINCIPLES
Equipment, Item & Inventory System tuân theo các nguyên tắc:
Every physical Item exists as an independent Item Instance unless explicitly defined as a Stackable Item Category.
Item Definition is separated from Item Instance State.
Equipment Rarity Tier belongs to Equipment Definition.
Equipment Rarity does not randomly roll when Item Instance is generated.
Equipment Instances may differ through Enhancement Level and Random Bonus Stats.
Equipment Enhancement is deterministic when Requirements and Resources are satisfied.
Every five Enhancement Levels grants one Random Bonus Stat Roll.
Duplicate Bonus Stat Rolls increase Bonus Stat Grade instead of creating duplicate entries.
Inventory uses Spatial Grid and Weight simultaneously.
Looting is a deliberate Manual Interaction.
Expedition Items are exposed to permanent loss.
Storage and Logistics reduce Inventory Micromanagement outside Expedition Gameplay.
Equipment Slots and Compatibility are Data-Driven by Creature Anatomy.
The system avoids Durability, Repair and Universal Random Affix complexity.
4. ITEM DEFINITION
Item Definition chứa Shared Gameplay Data của một Item Type.
Item Definition có thể xác định:
Item ID.
Item Category.
Display Data.
Grid Width.
Grid Height.
Weight.
Stackability.
Maximum Stack Size.
Vendor Value.
Loot Rule.
Storage Rule.
Usage Rule.
Equipment Data.
Equipment Requirement.
Creature Compatibility.
Rarity Tier.
Maximum Enhancement Level.
Enhancement Growth Rule.
Bonus Stat Pool.
Evolution Path.
Special Gameplay Rule.
Item Definition không chứa Runtime State riêng của Item Instance.
5. ITEM INSTANCE
Mỗi Physical Item tồn tại như một Item Instance độc lập, ngoại trừ Item Category được xác định rõ là Stackable.
Item Instance tham chiếu:
Item Definition ID.
Item Instance có thể chứa Runtime/Persistent State như:
Enhancement Level.
Bonus Stats.
Bonus Stat Grade.
Reroll Count.
Evolution State.
Container Content.
Other Definition-Specific Persistent State.
Worldforge không sử dụng Universal Item State Complexity.
Chỉ Item cần Persistent State mới lưu State tương ứng.
6. ITEM CATEGORY
Item Category có thể bao gồm:
Weapon.
Armor.
Backpack.
Resource.
Material.
Consumable.
Tool.
Deployable.
Container.
Quest Item.
Special Item.
Item Category ảnh hưởng:
Stackability.
Grid Rule.
Weight Rule.
Usage Rule.
Storage Rule.
Loot Rule.
Equipment Rule.
Enhancement Rule.
Loss Rule.
7. ITEM STACKING
Equipment, Backpack, Tool, Deployable và Special Item không Stack mặc định.
Resource và Material có thể Stack.
Consumable cùng loại có thể Stack.
Stacking Rule được xác định bởi Item Definition.
Stackable Item sử dụng:
Item Definition ID.
Quantity.
Maximum Stack Size.
Item có Persistent State khác nhau không thể Stack trong cùng Stack Entry.
8. ITEM GRID SIZE
Inventory sử dụng Spatial Grid.
Mỗi Item Definition có:
Grid Width × Grid Height.
Maximum Item Footprint:
5 × 5 Grid Cells.
Item có thể:
1×1.
1×2.
2×2.
2×3.
3×4.
5×5.
hoặc kích thước khác trong giới hạn cho phép.
Item Size phản ánh:
Physical Size.
Gameplay Value.
Inventory Cost.
Loot Decision.
9. ITEM ROTATION
Player có thể Rotate Item trong Inventory Grid.
Rotation thay đổi:
Width × Height
thành:
Height × Width.
Rotation không thay đổi:
Weight.
Item State.
Item Definition.
Item Identity.
10. INVENTORY MODEL
Worldforge sử dụng:
Spatial Grid Inventory + Weight System.
Player chỉ có thể chứa Item nếu:
Item có thể được bố trí hợp lệ trong Grid.
Weight Rule cho phép Player mang Item.
Grid Space và Weight tạo hai giới hạn độc lập.
Player có thể đủ Grid Space nhưng bị Overweight.
Player có thể còn Carry Capacity nhưng không đủ Grid Space.
11. BASIC INVENTORY
Mọi Character luôn có:
Basic Inventory Grid.
Basic Inventory:
Có kích thước nhỏ.
Không phụ thuộc Backpack.
Không thể bị mất.
Không phải Item.
Không thể Drop.
Không thể Loot.
Không thể Destroy.
Basic Inventory tồn tại nhằm tránh Softlock khi Player mất toàn bộ Equipment và Backpack.
12. BACKPACK
Backpack là Equipment Item.
Backpack cung cấp:
Additional Inventory Grid.
Carry Capacity.
Backpack có:
Item Instance.
Grid Size.
Weight.
Equipment Requirement.
Creature Compatibility.
Backpack thuộc Expedition Risk State.
Nếu Expedition thất bại, Backpack có thể bị mất vĩnh viễn.
13. BACKPACK SWAP
Player không thể Equip Backpack nhỏ hơn nếu Item hiện tại không thể được bố trí hợp lệ trong Inventory Capacity mới.
Player phải:
Rearrange Item.
Transfer Item.
Drop Item.
Store Item.
trước khi Backpack Swap được chấp nhận.
Không sử dụng Temporary Overflow Inventory cho Backpack Swap.
14. INVENTORY WEIGHT
Mỗi Item có Weight.
Total Carried Weight bao gồm:
Basic Inventory Content.
Backpack Content.
Quick Slot Item.
Equipped Equipment.
Container Content.
Carry Capacity chủ yếu tăng thông qua Backpack Equipment.
Các System khác có thể Modifier Carry Capacity nếu được định nghĩa sau này.
15. ENCUMBRANCE
Worldforge sử dụng Soft Encumbrance.
Khi vượt Carry Capacity:
Movement Speed giảm.
Stamina Cost tăng.
Hiện tại:
Không khóa Sprint.
Không khóa Dash.
Không có Hard Carry Cap mặc định.
Encumbrance Formula chưa được xác định ở Overview.
16. EQUIPMENT SLOT MODEL
Equipment Slot sử dụng:
Data-Driven Anatomy Equipment Slots.
Mỗi Creature Type hoặc Body Plan định nghĩa:
Available Equipment Slots.
Slot Compatibility.
Equipment Category Compatibility.
Visual Attachment Rule.
Weapon Compatibility.
Backpack Compatibility.
Humanoid chỉ là một Anatomy Profile.
Humanoid Slot Layout không phải Universal Equipment Baseline.
17. NON-HUMANOID EQUIPMENT
Creature không có Humanoid Anatomy có thể sử dụng:
Creature-Specific Equipment Slots.
Shared Equipment Category.
Anatomy Compatibility Rule.
Equipment Compatibility phụ thuộc:
Creature Type + Body Plan + Anatomy Slot + Equipment Definition.
18. WEAPON CARRY
Player có thể mang:
2–3 Combat Weapons
theo Combat System Baseline.
Weapon Carry Limit cụ thể có thể phụ thuộc:
Equipment Rule.
Creature Anatomy.
Progression.
Special Gameplay Condition.
Weapon Skill Loadout tiếp tục tuân theo Combat và Ability & Skill GDD.
19. EQUIPMENT REQUIREMENTS
Equipment có thể yêu cầu:
Core Attribute.
Creature Compatibility.
Skill.
Weapon Proficiency.
Character Progression.
Knowledge.
Technology.
Faction Requirement.
Special Gameplay Condition.
Requirement được xác định bởi Equipment Definition.
20. EQUIPMENT REQUIREMENT LOSS
Nếu Character đang Equip Item nhưng sau đó mất Requirement:
Equipment remains Equipped and Usable.
Tuy nhiên:
Base Equipment Stats are reduced by 50%.
và:
Special Effects are Disabled.
Equipment Abilities are Disabled.
Source Contributions are Disabled.
Equipment không tự động Unequip.
50%
là Design Baseline và có thể trở thành Tunable Data.
21. EQUIPMENT RARITY TIER
Mỗi Equipment Definition có Rarity Tier cố định.
Baseline Rarity Tier:
Common.
Uncommon.
Rare.
Epic.
Legendary.
Rarity không được roll khi Item Instance được tạo.
Loot System roll Equipment Definition.
Equipment Definition quyết định Rarity.
Rarity đại diện cho:
Equipment Growth Potential.
Maximum Enhancement Level.
Maximum Bonus Stat Roll Count.
Loot Value.
Content Progression Position.
Higher Rarity không mặc định có Current Power cao hơn mọi Equipment Rarity thấp hơn.
22. MAXIMUM ENHANCEMENT BY RARITY
Baseline:
Rarity Tier
Maximum Enhancement
Maximum Bonus Stat Rolls
Common
+5
1
Uncommon
+10
2
Rare
+10
2
Epic
+15
3
Legendary
+20
4
Uncommon và Rare có thể cùng Maximum Enhancement Level.
Sự khác biệt giữa Uncommon và Rare có thể đến từ:
Base Equipment Power.
Bonus Stat Pool.
Bonus Stat Grade Distribution.
Equipment Requirement.
Definition-Specific Behavior.
Loot Availability.
Chi tiết chưa được khóa ở Overview.
23. EQUIPMENT ENHANCEMENT
Equipment có Enhancement Level.
Enhancement bắt đầu từ:
+0.
Maximum Enhancement Level phụ thuộc Equipment Rarity Tier.
Enhancement yêu cầu:
Resource.
Material.
Requirement.
NPC hoặc Facility.
Enhancement luôn thành công khi Requirement và Resource hợp lệ.
Không có:
Enhancement Failure Chance.
Enhancement Level Loss.
Item Destruction.
Enhancement Downgrade.
24. ENHANCEMENT GROWTH
Mỗi Enhancement Level tăng Primary Equipment Power.
Ví dụ:
Weapon tăng Damage.
Armor tăng Defense.
Shield tăng Guard Capability.
Other Equipment tăng Primary Equipment Stat tương ứng.
Enhancement Growth sử dụng:
Equipment Category Baseline + Equipment Definition Override.
Category cung cấp Growth Rule mặc định.
Equipment Definition đặc biệt có thể Override Growth Rule.
25. RANDOM BONUS STAT MILESTONE
Mỗi:
5 Enhancement Levels
Equipment nhận:
1 Random Bonus Stat Roll.
Ví dụ:
+5 → Bonus Stat Roll I.
+10 → Bonus Stat Roll II.
+15 → Bonus Stat Roll III.
+20 → Bonus Stat Roll IV.
Bonus Stat được roll ngay khi Enhancement đạt Milestone.
26. BONUS STAT POOL
Bonus Stat được roll từ:
Equipment Category Bonus Stat Pool
●
Equipment Definition-Specific Bonus Stat Pool.
Player không thể xem trước Possible Bonus Stat Pool.
Bonus Stat chỉ được Reveal khi được roll.
Worldforge không sử dụng Global Bonus Stat Pool cho mọi Equipment.
27. DUPLICATE BONUS STAT
Nếu Bonus Stat Roll trùng Bonus Stat đã tồn tại:
Duplicate Bonus Stat → Increase Bonus Stat Grade.
Không tạo hai Bonus Stat Entry giống nhau.
Bonus Stat Grade có thể:
Tăng Magnitude.
Tăng Scaling.
Unlock Enhanced Behavior.
Unlock Definition-Specific Effect.
Rule cụ thể phụ thuộc Bonus Stat Definition.
28. BONUS STAT COUNT
Không có
Max Unique Bonus Stats
riêng.
Số Bonus Stat Roll bị giới hạn bởi:
Maximum Enhancement Level.
Ví dụ Legendary Weapon +20 có 4 Bonus Stat Rolls.
Weapon có thể sở hữu từ 1 đến 4 Unique Bonus Stats tùy Duplicate Roll.
29. BONUS STAT REROLL
Player có thể:
Lock một Bonus Stat.
Reroll Bonus Stat còn lại.
Reroll Cost sử dụng:
Base Cost by Equipment Rarity Tier
●
Scaling Cost by Item Instance Reroll Count.
Mỗi Item Instance có Reroll Count độc lập.
Reroll Count tăng khi Reroll được thực hiện.
Reroll Cost tăng dần theo lịch sử Reroll của Item Instance.
30. REROLL COUNT RESET
Reroll Count có thể được Reset bằng:
Extremely Rare Resource or Item.
Reset chỉ ảnh hưởng Item Instance được sử dụng.
Reset:
Đưa Reroll Count về 0.
Không thay đổi Enhancement Level.
Không thay đổi Bonus Stats.
Không thay đổi Bonus Stat Grade.
Không thay đổi Equipment Definition.
Resource/Item cụ thể chưa được xác định.
31. BONUS STAT TRANSFER
Bonus Stat không thể chuyển trực tiếp từ Item Instance này sang Item Instance khác.
Worldforge không có:
Bonus Stat Extraction.
Bonus Stat Transfer.
Bonus Stat Material System.
32. WEAPON EVOLUTION
Rarity Tier của Equipment Definition là cố định.
Player không thể trực tiếp nâng:
Common Weapon → Rare Weapon.
Một số Weapon đặc biệt có thể có:
Weapon Evolution Path.
Weapon Evolution tạo:
New Weapon Definition Identity.
Weapon mới có thể có:
Rarity Tier mới.
Base Stats mới.
Maximum Enhancement Level mới.
Bonus Stat Pool mới.
Equipment Requirement mới.
Ability Rule mới.
Weapon Skill Interaction mới.
33. EVOLUTION TRANSFER POLICY
Mỗi Weapon Evolution Path có:
Enhancement Transfer Policy.
và:
Bonus Stat Transfer Policy.
Evolution Path có thể:
Giữ Enhancement Level.
Chuyển một phần Enhancement Level.
Reset Enhancement.
Giữ Bonus Stats.
Chuyển một phần Bonus Stats.
Reroll Bonus Stats.
Reset Bonus Stats.
Không sử dụng Universal Evolution Transfer Rule.
34. EQUIPMENT ABILITY & DUPLICATE SKILL SOURCE
Equipment Ability sử dụng Ability Runtime Pipeline chung.
Nếu Equipment cung cấp Ability hoặc Skill đã tồn tại:
Equipment becomes an Additional Ability Source.
Không tạo Learned Skill thứ hai.
Duplicate Ability/Skill Source sử dụng Source Contribution và Effect Enhancement Rule của Ability & Skill System.
Equipment Source Enhancement không trực tiếp tăng Skill Progression Rank.
Skill Rank ≠ Source Enhancement Level.
35. CONSUMABLE
Consumable cùng Item Definition có thể Stack.
Ngoài Combat:
Consumable có thể được sử dụng trực tiếp từ Inventory.
Trong Combat:
Consumable phải được Equip vào Quick Slot.
Consumable Usage Rule có thể phụ thuộc Item Definition.
36. QUICK SLOT
Quick Slot hỗ trợ:
Consumable.
Tool.
Deployable Item.
Quick Slot không phải Safe Inventory.
Item trong Quick Slot:
Có Weight.
Thuộc Inventory Risk State.
Bị mất khi Expedition thất bại.
37. ITEM CONTAINER
Item Container hoạt động như:
Portable Storage Grid.
Container có thể:
Chứa Item.
Giới hạn Item Category.
Có Grid Capacity.
Có Weight.
Tính Weight của Item bên trong.
Container không chứa Container Item khác.
Không hỗ trợ Nested Container.
38. CONTAINER LOOT RESTRICTION
Trong Expedition/Exploration:
Player cannot loot the Container Item itself into Backpack.
Player chỉ có thể loot:
Items inside the Container.
Rule này áp dụng cho Loot Container trong World.
Portable Container do Player sở hữu hoặc tạo ra có thể có Rule riêng nếu được định nghĩa sau này.
39. MANUAL LOOT
Worldforge sử dụng mô hình rơi và nhặt vật phẩm vật lý trong thế giới (Physical Item Drop & Pickup). Khi kẻ địch bị tiêu diệt: Enemy Death → Xác định Loot (thông qua LootTable/DropTable) → Sinh vật phẩm vật lý (Physical Item Drops) → Vật phẩm văng/rải ra xung quanh (Scatter) → Vật phẩm tiếp đất và tồn tại trong thế giới → Người chơi tiếp cận → Người chơi nhặt vật phẩm → Vật phẩm vào túi đồ (Inventory). Vật phẩm KHÔNG được tự động chuyển thẳng vào inventory ngay khi quái chết.
Loot không tự động chuyển vào Inventory.
Loot Flow:
Interact with Loot Source
↓
Search
↓
Generate / Reveal Loot
↓
Open Loot Grid
↓
Drag / Quick Transfer Item
↓
Arrange Item in Backpack Grid
Player phải tự quyết định:
Item nào lấy.
Item nào bỏ.
Item nào Rotate.
Item nào Rearrange.
Item nào Replace.
40. LOOT INTERFACE
Loot Interface hiển thị:
Loot Grid
và:
Player Inventory Grid.
Player có thể:
Drag Item.
Drop Item.
Rotate Item.
Rearrange Item.
Quick Transfer ngoài Combat.
Quick Transfer vẫn phải tuân theo Grid Space và Weight Rule.
41. LOOT SEARCH
Player phải Search Loot Source trước khi thấy Loot.
Search sử dụng:
Search Time.
Search Time có thể phụ thuộc:
Loot Source.
Skill.
Equipment.
Gameplay Modifier.
Search có thể bị gián đoạn bởi Combat hoặc Gameplay Event.
42. LOOT GENERATION
Loot được Generate khi Player mở/Search Loot Source.
Loot không mặc định được Generate ngay khi Enemy chết.
Điều này cho phép:
Delayed Loot Generation.
Loot Table Context Evaluation.
Session-Based Loot Rule.
Reduced Persistent Loot State.
43. EXPEDITION RESET MODEL
Mỗi lần Player bắt đầu lại Expedition/Exploration Session:
Normal Enemy reset.
Normal Loot Container reset.
Unlooted Item reset.
Normal World Loot reset.
Ngoại lệ có thể bao gồm:
Major Creature.
Boss.
Special Event.
Unique World State.
Explicit Persistent Object.
Persistence Rule chi tiết thuộc Expedition, Quest & World Progression GDD.
44. ITEM DROP & DESPAWN
Trong Settlement/Base:
Player-dropped Item Instance có thể tồn tại lâu dài.
Item được Save theo World Persistence Rule.
Ngoài World/Expedition:
Player-dropped Item Instance sẽ Despawn sau một khoảng thời gian.
Despawn Time chưa được xác định.
45. STORAGE SYSTEM
Worldforge hỗ trợ:
Personal Storage.
Settlement Storage.
Container/Chest.
Shared Resource Storage.
Storage có Capacity.
Storage sử dụng Grid Capacity khi phù hợp.
Muốn tăng Storage Capacity:
Xây thêm Storage.
Nâng cấp Storage.
Mở rộng Settlement Logistics.
46. SHARED RESOURCE STORAGE
Khi Resource hoặc Material được Deposit vào Shared Resource Storage:
Physical Inventory Stack → Aggregate Resource Count.
Shared Resource Storage không cần duy trì từng Resource Item Instance vật lý.
Khi Withdraw:
Aggregate Resource Count → Inventory Item Stack.
Điều này giảm:
Save Data Complexity.
Inventory Complexity.
Crafting Query Cost.
Building Resource Management Cost.
47. CONNECTED STORAGE NETWORK
Storage có thể thuộc:
Connected Storage Network.
Các Storage trong cùng Network có thể:
Remote Transfer Item trực tiếp với nhau.
Share Resource Availability.
Hỗ trợ Crafting.
Hỗ trợ Building.
Storage ngoài Network không thể Remote Transfer.
Network Expansion và Logistics Rule thuộc Crafting, Building & Settlement GDD.
48. CRAFTING & BUILDING RESOURCE ACCESS
Crafting và Building System có thể Consume trực tiếp:
Resource.
Material.
từ Connected Storage Network.
System không tự động Consume:
Equipment.
Consumable.
Tool.
Deployable.
Special Item.
Các Item này phải được Player lấy thủ công khi cần.
49. STORAGE AUTO-SORT
Inventory và Storage hỗ trợ:
Auto-Sort Button.
Auto-Sort có thể:
Rearrange Item.
Rotate Item.
Optimize Grid Space.
Auto-Sort không:
Drop Item.
Destroy Item.
Sell Item.
Salvage Item.
Transfer Item ngoài Storage Context.
50. SALVAGE
Equipment có thể được Salvage.
Salvage trả lại:
A Portion of Resources.
Salvage Return phụ thuộc:
Equipment Definition.
Rarity Tier.
Enhancement Level.
Salvage Rule.
Salvage không trả:
Bonus Stat Material.
Extracted Bonus Stat.
51. VENDOR VALUE
Item có Base Vendor Value cố định theo Item Definition.
Overview hiện tại không sử dụng Dynamic Market Pricing.
Faction, Settlement hoặc Economy System có thể Modifier Transaction Result sau này nếu cần, nhưng Base Vendor Value thuộc Item Definition.
52. EXPEDITION ITEM LOSS
Equipment, Item và Inventory mang vào Expedition thuộc:
Expedition Risk State.
Khi Expedition thất bại:
Equipped Equipment
●
Backpack
●
Backpack Inventory
●
Basic Inventory Content
●
Quick Slot Items
↓
Lost Permanently.
Item đã mất:
Không tạo Recoverable Corpse.
Không thể quay lại loot.
Không được tự động bảo hiểm.
Không được hoàn trả mặc định.
Basic Inventory Grid bản thân không bị mất vì không phải Item.
53. EXPEDITION SESSION SAVE
Worldforge hỗ trợ:
Safe Save & Exit
tại:
Safe Point.
Safe Point / Camp (Exploration POIs do not offer instant extraction).
Nếu Player:
Thoát game.
Crash.
Disconnect.
ngoài Safe Point:
Temporary Expedition Session State is saved.
Player có thể tiếp tục Session hiện tại khi vào lại.
Temporary Session Save phải có Rule chống:
Save Scumming.
Rollback Exploit.
Duplicate Item Exploit.
Session State Manipulation.
Technical Rule chi tiết thuộc Save Architecture và Expedition GDD.
54. SYSTEM RELATIONSHIPS
Combat System
Weapon Carry.
Equipped Weapon.
Quick Slot.
Consumable Usage.
Equipment Requirement.
Equipment Stat.
Equipment Loss.
Ability & Skill System
Equipment Ability.
Additional Ability Source.
Source Contribution.
Duplicate Skill Enhancement.
Equipment Ability Availability.
Equipment Requirement Loss.
Character Progression System
Equipment Requirement.
Core Attribute.
Weapon Proficiency.
Carry Capacity Modifier.
Enhancement Access.
Equipment Evolution Requirement.
Creature System
Anatomy Equipment Slot.
Creature Compatibility.
Backpack Compatibility.
Weapon Compatibility.
Equipment Visual Rule.
Crafting, Building & Settlement System
Enhancement Resource.
Reroll Resource.
Rare Reroll Reset Item.
Salvage.
Storage.
Shared Resource Storage.
Connected Storage Network.
Crafting Resource Access.
Building Resource Access.
Logistics Expansion.
Expedition, Quest & World Progression System
Expedition Reset.
Expedition Failure.
Permanent Item Loss.
Major Creature Persistence.
Boss Persistence.
Special Event Persistence.
Safe Point.
Safe Point / Camp (Exploration POIs do not offer instant extraction).
Temporary Expedition Session Save.
UI/UX System
Inventory Grid.
Loot Grid.
Drag & Drop.
Rotation.
Quick Transfer.
Auto-Sort.
Encumbrance Feedback.
Equipment Requirement Feedback.
Enhancement UI.
Bonus Stat UI.
Reroll UI.
Storage Network UI.
Save System
Item Instance State.
Enhancement Level.
Bonus Stat.
Bonus Stat Grade.
Reroll Count.
Evolution State.
Inventory Layout.
Storage Layout.
Aggregate Resource Count.
Dropped Persistent Item.
Temporary Expedition Session State.
55. MAJOR RISKS
Grid Inventory Complexity
Spatial Grid Inventory yêu cầu Drag & Drop, Rotation, Placement Validation, Auto-Sort và Save Layout.
Scope UI và Testing cao hơn Slot Inventory.
Item Instance Save Data
Equipment Enhancement, Bonus Stats, Reroll Count, Evolution State và Container Content tạo Persistent Item State.
Save Data phải có Stable Item Instance ID và Versioning.
Inventory Micromanagement
Grid + Weight + Manual Loot có thể khiến Player dành quá nhiều thời gian quản lý Inventory.
Quick Transfer và Auto-Sort phải giảm thao tác không cần thiết.
No Durability Economy Sink
Không có Durability/Repair khiến Economy thiếu một Resource Sink phổ biến.
Enhancement, Reroll, Salvage, Crafting, Building và Expedition Loss phải đảm nhiệm vai trò Resource Sink.
Rarity Progression Clarity
Uncommon và Rare cùng Maximum Enhancement +10 có thể khiến người chơi khó hiểu khác biệt.
Equipment Definition cần tạo Gameplay Value khác nhau rõ ràng.
Hidden Bonus Stat Pool Frustration
Player không biết Possible Bonus Stat Pool trước khi Enhancement.
Điều này tăng Discovery nhưng có thể gây Frustration khi Resource Investment cao.
Reroll System phải giảm mức độ Frustration phù hợp.
Reroll Cost Exploit
Player có thể farm Item Instance mới để có Reroll Count thấp.
Tuy nhiên Item mới phải Enhancement lại từ đầu và mất toàn bộ Investment cũ.
Economy Balance cần kiểm chứng trade-off này.
Bonus Stat Power Scaling
Duplicate Roll tăng Bonus Stat Grade có thể tạo Item Instance vượt quá Balance.
Grade Cap và Enhanced Behavior Threshold phải được kiểm soát.
Weapon Evolution Complexity
Mỗi Evolution Path có Transfer Policy riêng.
Nếu có quá nhiều Policy Combination, Content Authoring và Testing Cost sẽ tăng.
Permanent Expedition Item Loss
Mất toàn bộ Equipment và Inventory là Punishment rất mạnh.
Loot Economy, Crafting Recovery và Basic Inventory phải đảm bảo Player có khả năng phục hồi sau thất bại.
Session Save Exploit
Temporary Expedition Session Save có nguy cơ Save Scumming, Rollback và Item Duplication.
Save Architecture phải thiết kế Transaction Boundary rõ ràng.
Storage Network Complexity
Remote Transfer và Shared Resource Access có thể tạo Dependency Complexity giữa Storage, Crafting và Building.
Connected Storage Network cần Contract rõ ràng.
56. OPEN DESIGN QUESTIONS
Các vấn đề chưa cần khóa ở giai đoạn Overview:
Item Instance ID được tạo và quản lý thế nào?
Stack Entry có cần Persistent ID hay không?
Maximum Stack Size được xác định theo Category hay Definition?
Basic Inventory Grid Size là bao nhiêu?
Backpack Grid Size Range là bao nhiêu?
Carry Capacity Formula hoạt động thế nào?
Encumbrance Scaling Formula là gì?
Movement Speed Penalty Cap là bao nhiêu?
Stamina Cost Penalty Cap là bao nhiêu?
Equipment Anatomy Profile được lưu bằng Data Structure nào?
Visual Attachment Rule hoạt động thế nào với Creature khác nhau?
Weapon Carry Limit chính xác là 2 hay 3 trong từng trường hợp?
Equipment Requirement Loss 50% Penalty có áp dụng cho mọi Equipment Category không?
Uncommon và Rare khác nhau chính xác ở những Rule nào?
Enhancement Growth Curve được tính thế nào?
Enhancement Cost Scaling hoạt động ra sao?
Bonus Stat Pool có bao nhiêu Stat tối đa trên mỗi Category?
Bonus Stat Grade có Maximum Grade hay không?
Bonus Stat Grade Distribution phụ thuộc Rarity thế nào?
Duplicate Bonus Stat Unlock Enhanced Behavior tại Grade nào?
Reroll Cost Scaling Formula là gì?
Lock Bonus Stat có tăng thêm Reroll Cost hay không?
Extremely Rare Reroll Reset Item được tạo từ đâu?
Weapon Evolution có giữ Item Instance ID cũ hay tạo ID mới?
Evolution Transfer Policy gồm những Policy chính thức nào?
Loot Search Time được tính thế nào?
Skill/Equipment nào ảnh hưởng Search Time?
Loot Generation Seed được quản lý thế nào?
Loot Table Context gồm những dữ liệu nào?
Normal Expedition Reset xảy ra khi nào chính xác?
Major Creature và Boss sử dụng Persistence Rule nào?
Item Despawn Time ngoài Expedition là bao nhiêu?
Player-owned Portable Container có được mang vào Expedition hay không?
Portable Container có bị mất khi Expedition thất bại hay không?
Storage Grid Capacity tối đa là bao nhiêu?
Connected Storage Network được xác định bằng Distance, Settlement Boundary hay Logistics Connection?
Remote Item Transfer có Instant hay cần Transfer Time?
Aggregate Resource Count được Save theo Storage hay Network?
Crafting Consume Resource theo Storage Priority nào?
Auto-Sort Algorithm ưu tiên Space Efficiency hay Item Category Grouping?
Salvage Return Formula hoạt động thế nào?
Enhancement Resource Return khi Salvage là bao nhiêu?
Safe Point được xác định thế nào?
Temporary Expedition Session Save được Commit/Rollback thế nào?
Crash Recovery khác Manual Quit thế nào?
Basic Inventory Content có thực sự nên mất toàn bộ khi Expedition thất bại hay có Protected Slot hay không?
Item Database được quản lý bằng Excel, ScriptableObject hay External Data Pipeline?
Item Definition Version Migration được xử lý thế nào?
Item Instance Save Migration được xử lý thế nào?
57. EQUIPMENT, ITEM & INVENTORY DESIGN BASELINE
Equipment, Item & Inventory Baseline của Worldforge:
Worldforge uses Item Definitions and persistent Item Instances.
Physical Items exist as independent Item Instances unless their Item Category explicitly supports Stacking.
Resources, Materials and Consumables may Stack.
Equipment and other Stateful Items do not Stack.
Item Definition is separated from Item Instance State.
Inventory uses Spatial Grid and Weight simultaneously.
Each Item Definition has Grid Width and Grid Height with a maximum footprint of 5×5.
Items may be rotated inside Inventory Grids.
Every Character owns a small permanent Basic Inventory Grid to prevent Softlock.
Backpacks are Equipment Items that expand Inventory Grid and Carry Capacity.
Backpacks are exposed to permanent loss during Expedition Failure.
Backpack Swap is rejected when the target Capacity cannot contain the current Item Layout.
Worldforge uses Soft Encumbrance through Movement Speed reduction and increased Stamina Cost.
Equipment Slots are Data-Driven by Creature Anatomy and Body Plan.
Worldforge supports two to three Combat Weapons according to Combat System rules.
Equipment may require Attributes, Creature Compatibility, Skill, Proficiency, Progression, Knowledge, Technology or other Gameplay Conditions.
Equipment that temporarily loses Requirements remains Equipped and Usable but suffers a 50% Base Stat penalty while Special Effects, Equipment Abilities and Source Contributions are Disabled.
Equipment Rarity Tier is permanently defined by Equipment Definition.
Equipment Rarity is not randomly rolled when Item Instances are generated.
Worldforge uses Common, Uncommon, Rare, Epic and Legendary Equipment Rarity Tiers.
Rarity determines Maximum Enhancement Level and Maximum Bonus Stat Roll Count.
Common Equipment may reach +5, Uncommon +10, Rare +10, Epic +15 and Legendary +20 under the current baseline.
Equipment Enhancement always succeeds when Resources and Requirements are satisfied.
Enhancement Growth uses Equipment Category Baseline with Definition-Specific Override.
Every five Enhancement Levels grants one Random Bonus Stat Roll.
Bonus Stats are rolled from Equipment Category Pool and Equipment Definition-Specific Pool.
Possible Bonus Stat Pools are hidden from the Player.
Duplicate Bonus Stat Rolls increase Bonus Stat Grade rather than creating duplicate entries.
The number of Bonus Stat Rolls is naturally limited by Maximum Enhancement Level.
Players may lock one Bonus Stat and reroll another.
Reroll Cost is determined by Equipment Rarity Tier and the Item Instance’s Reroll Count.
Each Item Instance tracks Reroll Count independently.
Reroll Count may be reset through an extremely rare Resource or Item.
Bonus Stats cannot be transferred between Item Instances.
Equipment Rarity cannot be directly upgraded.
Some special Weapons may evolve into new Weapon Definitions through Weapon Evolution Paths.
Each Weapon Evolution Path defines its own Enhancement Transfer Policy and Bonus Stat Transfer Policy.
Equipment Abilities use the shared Ability Runtime Pipeline.
Duplicate Ability or Skill Sources enhance the existing Ability through Source Contribution rules rather than creating duplicate Learned Skills.
Consumables may be used directly from Inventory outside Combat but require Quick Slots during Combat.
Quick Slots support Consumables, Tools and Deployables and are part of Expedition Risk State.
World Loot Containers expose their contents but cannot themselves be looted into the Backpack during Expedition.
Nested Containers are not supported.
Worldforge uses Manual Loot with Search Time, Loot Grid, Inventory Grid, Drag & Drop, Rotation and Quick Transfer.
Loot is generated when the Player searches or opens the Loot Source.
Normal Enemies, Normal Loot Containers, Unlooted Items and Normal World Loot reset between Expedition Sessions.
Major Creatures, Bosses, Special Events, Unique World States and explicitly Persistent Objects may follow separate Persistence Rules.
Player-dropped Items may persist in Settlement/Base but despawn after a tunable duration outside persistent areas.
Storage Capacity is limited and expanded by constructing or upgrading Storage.
Resources and Materials deposited into Shared Resource Storage become Aggregate Resource Counts.
Storage within the same Connected Storage Network can transfer Items directly.
Crafting and Building may directly consume Resources and Materials from the Connected Storage Network.
Equipment, Consumables, Tools, Deployables and Special Items must be manually withdrawn before use.
Inventory and Storage support Auto-Sort without automatic Drop, Destroy, Sell or Salvage behavior.
Equipment may be Salvaged for a portion of Resources.
Items use fixed Base Vendor Value from Item Definition.
Expedition Failure permanently destroys all Equipped Equipment, Backpack, carried Inventory contents and Quick Slot Items.
Lost Expedition Items cannot be recovered through Corpse Loot or automatic Insurance.
The permanent Basic Inventory Grid itself cannot be lost.
Safe Save & Exit is supported at Safe Points and Camps. Exploration points and POIs do not offer instant extraction; the player must physically travel through the world back to the settlement.
Crash, Disconnect or Exit outside Safe Points creates Temporary Expedition Session State so the Player may continue the active Session.
Temporary Expedition Session Save requires anti-save-scumming, rollback protection and item-duplication prevention.