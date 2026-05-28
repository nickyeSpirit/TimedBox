# Timed Box V3 - Technical Documentation

Tài liệu kỹ thuật giải thích chi tiết cấu trúc, luồng hoạt động, và cách sử dụng của Module `TimedBoxV3` sau quá trình tái cấu trúc toàn diện (Refactoring) dựa trên nguyên tắc Clean Architecture.

## 1. Tổng quan Kiến trúc (Architecture)

Module `TimedBoxV3.Module` được thiết kế hoàn toàn độc lập, tách biệt 100% khỏi các logic của Game Layer (chẳng hạn như `ItemInfo.ItemID`, `DataManager`, `MonsterTrainer`...). Module đi theo nguyên lý Clean Architecture, chia thành các Layer chính:

- **Entity**: Các cấu trúc dữ liệu nguyên thủy, các enum nội bộ của module (ví dụ: `TimeBoxDefine`, `TimedBoxV3SlotStatus`, `TimeBoxModuleRewardData`).
- **Domain**: Các interface (hợp đồng) định nghĩa nghiệp vụ của module (`ITimedBoxRepository`, `ITimedBoxIdMapper`, `ITimedBoxProgressionMapper`, `ITimedBoxRewardService`).
- **Infrastructure**: Các logic thực thi chi tiết, bao gồm Repository đọc ghi file (`TimedBoxRepository`, `UnityStorage`), các service như `TimedBoxRewardService`, và Controller tổng (`TimedBoxV3FeatureController`).

## 2. Entry Point & Cấu hình khởi tạo

**Composition Root (Nơi lắp ráp module)**: `TimedBoxModuleFactory`
Nơi lắp ráp và kết nối tất cả các dependencies lại với nhau để tạo thành một khối hoàn chỉnh (`TimedBoxModuleBundle`).

**Khởi tạo (Initialization)**:
Module được khởi tạo **duy nhất một lần** trong hàm `Init()` của `DataManager.cs`. Quá trình khởi tạo diễn ra như sau:

```csharp
private static void InitTimedBoxV3Module()
{
    // Lấy ScriptableObject chứa config
    var config = GameInformation.instance.TimedBoxV3SO;
    // Khởi tạo UnityStorage (bọc PlayerPrefs/JSON)
    var storage = new UnityStorage();
    // Khởi tạo Mapper để module có thể giao tiếp với Game Layer một cách lỏng lẻo (loosely coupled)
    var mapper = new AtlantisTimedBoxProgressionMapper();
    
    // Gắn kết tất cả lại bằng Factory
    var bundle = TimedBoxModuleFactory.Create(config, storage, mapper, mapper);
    
    // Lưu trữ Singleton cho toàn game
    TimedBoxV3Core.Init(bundle);
}
```

**API Giao tiếp chính (Entry Point cho UI/Game Layer)**:
Thay vì gọi các service một cách lộn xộn, Game Layer tương tác trực tiếp với module thông qua `TimedBoxV3Core.Instance`.
- Tương tác với dữ liệu (Save/Load): `TimedBoxV3Core.Instance.Repository.GetStoreData()`
- Gọi các tính năng (Mở hộp, Check Slot, v.v.): `TimedBoxV3Core.Instance.Feature`

## 3. Luồng Quản lý Dữ liệu (Data Flow)

Trước đây, `TimedBoxV3StoreData` nằm trực tiếp trong `InventoryData`. Điều này gây ra sự phụ thuộc chặt chẽ và không thể tách rời module.
Hiện tại, **Module tự quản lý dữ liệu của chính mình**:

- Dữ liệu `TimedBoxV3StoreData` được nạp lên thông qua `TimedBoxV3Core.Instance.Repository.GetStoreData()`.
- Khi có bất kỳ thay đổi nào (thêm trứng, mở khoá slot, thay đổi trạng thái), Game Layer phải gọi hàm lưu lại dữ liệu:
  `TimedBoxV3Core.Instance.Repository.SaveStoreData(v3Store);`

## 4. Luồng xử lý Phần thưởng (Reward Flow)

Quá trình "Hatching" (mở rương) và sinh ra phần thưởng đã được chuyển hoàn toàn vào trong module:

1. **Randomize**: Lấy `pool_id` từ `TimedBoxV3ConfigSO` dựa theo loại rương (Common, Epic...). Random ra danh sách `infoId` (int) nguyên thủy.
2. **Merge Progression**: Trong `TimedBoxRewardService`, module gọi `ProgressionMapper` (do Game Layer implement) để lấy Player Level. Sau đó, nó đối chiếu với bảng `timedBoxV3Progression` trong `TimedBoxV3ConfigSO` để chèn thêm số lượng (quantity) cho các item tương ứng (như Gold, Shard).
3. **Sort Rewards**: Module tự động phân tích các `infoId` dựa theo khoảng giá trị (ví dụ: Gold = 21, Random Equipment = 198..202) để sắp xếp thứ tự các vật phẩm hiển thị cho người chơi.

Tất cả các logic này đều nằm trong `TimedBoxRewardService.cs` và **không cần** bất kỳ processor dư thừa nào ở bên ngoài Game Layer. (Đã xoá sổ `ITimedBoxRewardProcessor` và `AtlantisTimedBoxRewardProcessor`).

## 5. Tương thích và Chuyển đổi (Backward Compatibility)

Do đã ngắt dữ liệu ra khỏi `InventoryData`, khi người dùng cũ đăng nhập, module vẫn đảm bảo an toàn thông qua lớp `AtlantisTimedBoxMigrationHelper`.

Lớp này sẽ được kích hoạt tại `DataManager.ValidateDataAfterLoading()`, với nhiệm vụ:
- Kiểm tra các slot mua bằng IAP từ hệ thống cũ.
- Validate và fix các lỗi liên quan đến `DateTime` của các hộp đang ấp dở dang.
- Đổ dữ liệu cũ vào kho lưu trữ mới thông qua `TimedBoxV3Core.Instance.Repository`.

## Tóm tắt

- **Độc lập tuyệt đối**: Có thể bê nguyên folder `TimedBoxV3.Module` sang một project Unity khác mà không báo lỗi thiếu class.
- **Dễ bảo trì**: Phân tách rõ ràng giữa Data (Repository), Logic (Service/Feature), và Config (SO).
- **Mở rộng dễ dàng**: Khi cần thêm loại phần thưởng mới, chỉ cần khai báo ID bằng số nguyên `int` trong `TimeBoxModuleRewardData` và thiết lập trong bảng Config.
