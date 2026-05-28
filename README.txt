# 📦 Tài liệu Kỹ thuật: TimedBoxV3 Module

## 1. Tổng quan (Overview)

### 🌟 1.1. Tính năng "Rương Tính Giờ" (Timed Box) là gì?
"Rương Tính Giờ" là một hệ thống phần thưởng cốt lõi trong game, cung cấp cho người chơi các hòm đồ (rương) có thời gian ấp (hatching time). 
- **Cách nhận rương:** Người chơi nhận được rương thông qua việc vượt ải (Drop rate theo Story Stage) hoặc nhận qua các sự kiện, Gacha.
- **Tiến trình mở rương:** Mỗi rương yêu cầu một khoảng thời gian chờ (countdown) để mở. Người chơi có thể dùng tài nguyên (gem/vé) để mở ngay lập tức.
- **Phần thưởng cung cấp:** Rương cung cấp 2 loại phần thưởng khi mở:
  1. **Progression Rewards (Quà cố định):** Những phần thưởng đảm bảo nhận được, scale theo cấp độ hiện tại của người chơi (Player Level).
  2. **Pool Rewards (Quà ngẫu nhiên):** Các vật phẩm xóc đĩa ngẫu nhiên theo trọng số (weight) từ một tập hợp các pool quà (ví dụ: Mảnh tàu, Vàng, Kim cương).

### 🎯 1.2. Mục tiêu Kỹ thuật của `TimedBoxV3.Module`
`TimedBoxV3.Module` là một khối chức năng (module) độc lập chịu trách nhiệm quản lý toàn bộ vòng đời của hệ thống Timed Box nói trên. 

**Mục tiêu cốt lõi:**
- **Plug and Play:** Dễ dàng tích hợp vào các dự án khác có chung Code Base (dùng chung `ItemInfo.ItemID`).
- **Encapsulation:** Đóng gói 100% bộ quy tắc random (xóc đĩa) bên trong module. Các dự án đích không cần (và không được phép) tự viết lại logic random phần thưởng.
- **Decoupling:** Tách biệt hoàn toàn phần Logic/Data ra khỏi UI (Presentation). UI chỉ làm nhiệm vụ hiển thị và lắng nghe event từ core logic.

---

## 2. Kiến trúc (Architecture)
Module được thiết kế theo tư tưởng **Clean Architecture** pha trộn với tính thực dụng (Pragmatic), bao gồm 4 layer chính:

### 🎯 2.1. Domain Layer (Interfaces cốt lõi)
Nơi định nghĩa các "hợp đồng" giao tiếp, hoàn toàn không phụ thuộc vào các thư viện bên ngoài hay UI.
- `ITimedBoxRepository`: Interface quản lý việc đọc/ghi dữ liệu trạng thái các rương đang ấp (`EggStoreDataV3`).
- `ITimedBoxRewardService`: Interface quan trọng nhất, chứa API để lấy danh sách quà hiển thị trên Info (đã resolve) và API xóc đĩa khi Claim rương.
- `ITimedBoxV3Feature`: Interface quản lý logic Gameplay (cooldown rớt rương, map đang đứng, v.v.).

### 🧱 2.2. Entity Layer (Data & Cấu hình)
Nơi chứa cấu trúc dữ liệu thuần túy và các Scriptable Object (SO) cấu hình.
- `EggStoreDataV3.cs`: Data model lưu trạng thái các slot rương hiện tại của người chơi.
- `TimedBoxV3ConfigSO.cs`: Cấu hình toàn bộ phần thưởng (Reward Pools) và quà tặng cố định (Progression).

### ⚙️ 2.3. Infrastructure Layer (Triển khai Logic)
Nơi "thực thi" các interface ở tầng Domain. Tầng này sẽ tương tác với hệ thống chung của game (PlayerPrefs, IStorage, System.Random).
- `TimedBoxRepository.cs`: Xử lý load/save dữ liệu rương.
- `TimedBoxRewardService.cs`: Chứa logic xóc đĩa Deterministic (dùng seed) đảm bảo quà hiển thị trên Info UI khớp 100% với quà nhận được lúc Claim.
- **`TimedBoxModuleFactory.cs` (Composition Root):** Nơi duy nhất khởi tạo và bơm (inject) tất cả các dependencies lại với nhau.

### 🎨 2.4. Presentation Layer (Giao diện) - *Tùy chọn*
Phần UI được tách biệt hoàn toàn. Khi mang sang project mới, có thể dùng lại các prefab UI cũ hoặc tự xây UI mới chỉ bằng cách gọi vào tầng Domain. Dưới đây là các script UI chính đang liên kết với module:
- **`UIPanelDroneBoxV3.cs`**: Quản lý hiển thị tổng quan các rương đang ấp trên màn hình Home (hiển thị thời gian countdown, trạng thái sẵn sàng mở).
- **`TimedBoxSlotViewV3.cs`**: Quản lý trạng thái chi tiết của từng slot rương (đang rảnh, đang ấp, đang chờ mở).
- **`EggDetailControllerV3.cs`**: Màn hình xem trước (Preview/Info) thông tin rương. Lắng nghe `RewardService.GetResolvedPools()` để hiển thị danh sách pool quà có thể nhận được từ rương mà chưa cần mở.
- **`EggRewardUiController.cs`**: Màn hình chúc mừng khi người chơi chính thức mở rương (Claim).
- **`TimeboxPoolRowItem.cs`**: Component tái sử dụng (legacy) để hiển thị chi tiết của một pool quà trên giao diện (ảnh icon, số lượng, v.v.).
- ** IconTimedBoxHolderView.cs**: Quản lý hiển thị icon ở ngoài story map
---

## 3. Quy trình Tích hợp (Integration Guide)

> [!IMPORTANT]
> **Dependency bắt buộc:** Module yêu cầu project đích phải có sẵn các class: `ItemInfo.ItemID` và cấu trúc `WSRewardData`.

Để tích hợp module vào một dự án mới, thực hiện các bước sau:

**Bước 1: Khởi tạo Module (Lúc Boot Game)**
Sử dụng `TimedBoxModuleFactory` để khởi tạo toàn bộ hệ thống 1 lần duy nhất:
```csharp
// configSO là Scriptable Object TimedBoxV3ConfigSO
// storage là implementation của IStorage (ví dụ: UnityStorage wrapper cho PlayerPrefs)
TimedBoxModuleBundle timedBoxModule = TimedBoxModuleFactory.Create(configSO, storage);
```

**Bước 2: Sử dụng các API của Module**
Sau khi có `timedBoxModule`, bạn có thể inject các service vào nơi cần thiết:

*Để kiểm tra rớt rương sau khi qua màn:*
```csharp
// Trả về data rương nếu rớt, hoặc null nếu chưa đến giờ (cooldown)
var droppedBox = timedBoxModule.Feature.GetReceiveTimedBoxData(gameMode);
```

*Để hiển thị Info phần thưởng rương trên UI:*
```csharp
// Lấy danh sách pool đã được resolve các điều kiện (percentPriority)
// Dùng seed là thời điểm hoàn thành ấp rương (endTime.Ticks) để cố định kết quả
var infoPools = timedBoxModule.RewardService.GetResolvedPools(seed, boxType);
```

*Để mở rương (Claim):*
```csharp
// Xóc đĩa phần thưởng. Module tự lo logic. Game đích chỉ việc nhận List<WSRewardData> và add vào Inventory.
List<WSRewardData> rewards = timedBoxModule.RewardService.ClaimRewards(seed, boxType);
InventorySystem.AddRewards(rewards);
```

---

## 4. Giải thích Core Logic (Design Decisions)

### 4.1. Vấn đề "Deterministic Random" (Info & Claim)
Hệ thống cũ gặp lỗi: Bấm vào xem Info thì hiển thị pool quà đã loại trừ `percentPriority`, nhưng lúc Claim thật thì lại random khác. 
**Cách giải quyết:** `TimedBoxRewardService` sử dụng chung 1 hạt giống ngẫu nhiên (`seed` dựa trên Ticks thời gian) cho cả 2 hàm `GetResolvedPools` và `ClaimRewards`. Nhờ đó, danh sách pool hiển thị ở Info UI sẽ **khớp 100%** với danh sách pool được mang đi random lúc Claim.

### 4.2. Data Mapper Pattern
Để Module không bị kẹt cứng (hard-coupled) vào logic lấy Cấp độ (Level) hay Map của game đích, Module sử dụng giao thức `ITimedBoxProgressionMapper`. 
Khi cắm vào game, game sẽ tự implement mapper này (ví dụ: `AtlantisTimedBoxProgressionMapper`) để "dịch" cấp độ người chơi và nhét vào Module.

---

## 5. Cấu trúc Thư mục 

Khi copy sang project mới, chỉ cần bê nguyên thư mục `TimedBoxV3.Module`:
```text
TimedBoxV3.Module/
├── Domain/           # Interfaces (ITimedBoxRepository, ITimedBoxRewardService,...)
├── Entity/           # Data & Cấu hình (EggStoreDataV3, TimedBoxV3ConfigSO,...)
├── Infrastructure/   # Logic triển khai (RewardService, Factory, Helper,...)
├── Presentation/     # (Tùy chọn) Chứa UI Scripts
└── Extensions/       # (Tùy chọn) Chứa các file UI cho Gacha Banner
```
