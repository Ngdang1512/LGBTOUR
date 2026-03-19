# Tổng quan về đồ án 
**Tên đồ án:** Ứng dụng Thuyết minh tự động điểm tham quan 
**Nền tảng:** Mobile App ( Android) & Web CMS (Admin)
**Mục tiêu :** Quảng bá về địa danh và cung cấp trải nghiệm tham quan tự động cho du khách. Khi người dùng đi dạo đến gần các điểm tham quan , ứng dụng sẽ tự động phát ra âm thanh và thuyết mình về địa danh

---

## 1. Chức năng cần có 

### 1.1. Ứng dụng di động (Dành cho Du khách)
*   **Bản đồ & Định vị (Map View):** 
    *   Hiển thị bản đồ tổng quan, vị trí hiện tại của người dùng .
    *   Hiển thị các điểm tham quan (POI) trên tuyến đường.
*   **Thuyết minh tự động (Geofencing & Narration):** 
    *   Tự động phát Audio thu sẵn hoặc Text-to-Speech (TTS) khi người dùng bước vào bán kính của một POI.
    *   Chống Spam: Không lặp lại một nội dung trong cùng một POI

### 1.2. Hệ thống Quản trị (Web Admin / CMS)
*   **Quản lý Điểm tham quan (POI):**
    *   Thêm/Sửa/Xóa POI.
    *   Thiết lập tọa độ (Latitude/Longitude), độ ưu tiên , bán kính kích hoạt (Radius).
*   **Quản lý Nội dung Đa ngôn ngữ (Content):**
    *   Nhập kịch bản (Script) hoặc Upload file Audio/Voiceover cho từng POI theo nhiều ngôn ngữ khác nhau.
*   **Quản lý Lộ trình (Tour Management):**
    *   Nhóm các POI lại thành một tuyến Tour cụ thể .
*   **Thống kê & Phân tích (Analytics):**
    *   Thống kê top địa điểm được nghe nhiều nhất, thời gian lưu lại trung bình.
    *   Bản đồ nhiệt (Heatmap) thể hiện vị trí người dùng hay tập trung.

---

## 2. LUỒNG HOẠT ĐỘNG CHÍNH (WORKFLOWS)

### 2.1. Luồng trải nghiệm của Du khách (User Journey)
1. **Khởi động:** Mở app, cấp quyền truy cập Vị trí (Luôn luôn / Always Allow).
2. **Chuẩn bị:** Chọn Tour muốn đi -> Tải dữ liệu Offline (Tùy chọn).
3. **Bắt đầu:** Nhấn "Bắt đầu", cất điện thoại và đi bộ tham quan.
4. **Trải nghiệm:** App chạy ngầm. Khi đến gần POI (VD: Tượng Phật), app tự động phát âm thanh giới thiệu.
5. **Kết thúc:** Hoàn thành lộ trình, nhấn "Kết thúc Tour". Khi có mạng, app đồng bộ lịch sử sử dụng lên hệ thống.

### 2.2. Sơ đồ Luồng Quản trị nội dung (Admin Workflow)
*(Mô tả cách Admin tạo dữ liệu để App có thể hoạt động)*

```mermaid
graph TD
    A([1. Nhập tài khoản ]) --> B[2. Xem Dashboard thống kê ]
    
    B --> C[3. Thêm một địa danh mới]
    C --> D[Chấm tọa độ & Bán kính]
    
    D --> E[4. Tải file âm thanh lên ]
    
    E --> F[5. Tạo một Tuyến Tour mới]
    F --> G[6. Tích chọn các điểm ghép vào lộ trình tour]
    
    G --> H([ Dữ liệu sẵn sàng])

    %% Trang trí màu sắc cho bình dân và dễ nhìn
    classDef start fill:#e3f2fd,stroke:#2196f3,stroke-width:2px;
    classDef action fill:#fff3e0,stroke:#ff9800,stroke-width:2px;
    classDef finish fill:#e8f5e9,stroke:#4caf50,stroke-width:2px;
    
    class A start;
    class B,C,D,E,F,G action;
    class H finish;
```

### 2.2. Luồng Khách Du Lịch (Thao tác trên App)
*Những gì khách du lịch bấm trên màn hình điện thoại trước khi cất vào túi.*

```mermaid
graph TD
    A([1. Mở App trên điện thoại]) --> B[2. Cấp quyền cho App theo dõi Vị trí]
    B --> C[3. Chọn Tour muốn đi & Chọn ngôn ngữ]
    C --> D[4. Bấm tải sẵn Bản đồ và Âm thanh về máy]
    D --> E[5. Bấm nút BẮT ĐẦU TOUR]
    E --> F[6. Cất điện thoại vào túi, đeo tai nghe và đi bộ]
    F --> G([7. Đi dạo xong, mở App bấm KẾT THÚC TOUR])

    classDef user fill:#e1bee7,stroke:#8e24aa,stroke-width:2px;
    class A,B,C,D,E,F,G user;
```
### 2.3. Luồng App Chạy Ngầm (Xử lý thông minh)
*Cách App tự động tính toán để bật âm thanh đúng lúc.*

```mermaid
graph TD
    A([Khách đang đi bộ...]) --> B{App ngầm quét GPS liên tục}
    B -->|Tính toán khoảng cách| C{Khách lọt vào vòng tròn 20m<br>của Điểm nào không?}
    C -->|KHÔNG| B
    C -->|CÓ LỌT VÀO| D{Kiểm tra: Điểm này<br>khách vừa nghe chưa?}
    D -->|Nghe rồi - Chống Spam| B
    D -->|Chưa nghe| E[Tự động bật file MP3 thuyết minh]
    E --> F[Ghi lại lịch sử: Khách này đứng nghe bao lâu]
    F --> G([Gửi dữ liệu về Web cho Admin vẽ Bản đồ nhiệt])
    G --> B

    classDef background fill:#fff9c4,stroke:#fbc02d,stroke-width:2px;
    classDef check fill:#ffccbc,stroke:#d84315,stroke-width:2px;
    classDef action fill:#c8e6c9,stroke:#388e3c,stroke-width:2px;
    class A,B background; class C,D check; class E,F,G action;
```

---

## 3. CẤU TRÚC CƠ SỞ DỮ LIỆU (DATABASE SCHEMA)

Hệ thống được thiết kế với 5 bảng chính, đủ để đáp ứng mọi tính năng mà không bị rườm rà.

### Bảng chi tiết
1.  **Bảng `POIs` (Điểm tham quan):** Chứa thông tin cơ bản (Tên, Tọa độ Lat/Lng, Bán kính nhận diện, Ảnh minh họa).
2.  **Bảng `Audios` (File âm thanh):** Tách riêng khỏi bảng POIs để hỗ trợ đa ngôn ngữ (1 điểm POI có thể có 1 file Tiếng Việt, 1 file Tiếng Anh). Chứa đường dẫn file MP3.
3.  **Bảng `Tours` (Tuyến Tour):** Chứa thông tin tên Tour, lời giới thiệu và giá vé.
4.  **Bảng `TourPOIs` (Sắp xếp lộ trình):** Bảng nối giúp Admin gắp các điểm POI bỏ vào Tour và đánh số thứ tự (Điểm 1, Điểm 2...).
5.  **Bảng `UserLogs` (Lịch sử nghe):** Nơi nhận dữ liệu từ điện thoại gửi về (Khách đứng ở tọa độ nào, nghe bài gì, nghe bao lâu) để vẽ Bản đồ nhiệt.

### Sơ đồ quan hệ (ERD)

```mermaid
erDiagram
    TOURS ||--o{ TOUR_POIS : "Chứa nhiều"
    POIS ||--o{ TOUR_POIS : "Nằm trong"
    POIS ||--o{ AUDIOS : "Có nhiều file MP3"
    POIS ||--o{ USER_LOGS : "Được ghi lại lịch sử"

    TOURS {
        int Id
        string Name
        decimal Price
    }
    
    POIS {
        int Id
        string Name
        double Lat_Lng
        double Radius
    }

    TOUR_POIS {
        int TourId
        int POI_Id
        int DisplayOrder
    }

    AUDIOS {
        int Id
        int POI_Id
        string LanguageCode
        string AudioUrl
    }

    USER_LOGS {
        int Id
        int POI_Id
        double Lat_Lng
        int DurationSeconds
    }
```

---
