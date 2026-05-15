# Database

Thư mục này chứa toàn bộ SQL scripts cho SaigonAudioTour.

## Cấu trúc

```
database/
├── migrations/
│   └── 001_init.sql         # Schema SQLite (dev local — không dùng Docker)
├── init-sqlserver.sql        # T-SQL cho Docker container (SQL Server / Azure SQL Edge)
├── seed.sql                  # Dữ liệu mẫu SQLite syntax
└── README.md
```

---

## Cách chạy (khuyến nghị — Docker)

> Yêu cầu: Docker Desktop đang chạy. Không cần cài .NET SDK hay SQL Server.

```bash
# 1. Copy file env
cp .env.example .env

# 2. Khởi động toàn bộ stack (DB + API)
docker-compose up -d

# 3. Kiểm tra trạng thái
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
```

Kết quả mong đợi:
```
NAMES         STATUS          PORTS
saigon_db     Up (healthy)    0.0.0.0:1433->1433/tcp
saigon_api    Up              0.0.0.0:5117->8080/tcp
```

API sẵn sàng tại: **http://localhost:5117**  
Swagger UI:        **http://localhost:5117/swagger**

> Database được khởi tạo tự động từ `init-sqlserver.sql` khi container `saigon_db` khởi động lần đầu.

---

## Reset toàn bộ (xoá data, tạo lại từ đầu)

```bash
docker-compose down -v   # xoá containers + volumes
docker-compose up -d     # khởi động lại, DB init lại từ đầu
```

---

## Xem logs

```bash
docker logs saigon_api --follow    # API logs
docker logs saigon_db  --follow    # SQL Server logs
```

---

## Kết nối DB trực tiếp (debug)

Dùng Azure Data Studio hoặc DBeaver:
- **Host**: `localhost`
- **Port**: `1433`
- **User**: `sa`
- **Password**: `SaigonTour@2026` (hoặc giá trị trong `.env`)
- **Database**: `SaigonAudioTour`

---

## Quy ước migration

- Mỗi thay đổi schema tạo 1 file mới trong `migrations/`
- Đặt tên: `NNN_mô_tả_ngắn.sql`
- Cập nhật `init-sqlserver.sql` sau mỗi migration (để Docker init luôn đồng bộ)
- Không sửa file migration cũ sau khi đã commit

---

## Dev local không dùng Docker

```bash
# SQLite (đơn giản nhất)
cd backend/SaigonAudioTour.Api
dotnet ef database update

# Hoặc chạy script thủ công
sqlite3 saigon.db < database/migrations/001_init.sql
sqlite3 saigon.db < database/seed.sql
```
