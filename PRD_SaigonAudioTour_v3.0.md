# Saigon Audio Tour — Product Requirements Document (PRD) v3.0

**Version:** 3.0  
**Date:** 2026-04-16  
**Status:** Phase A-C Delivery In Progress (Phases A & B Complete)  
**Owner:** Product / Engineering

**Delivery Timeline:**
- ✅ **Phase A (Apr 15):** Heatmap Analytics Dashboard
- ✅ **Phase B (Apr 16):** VNPay Payment Gateway (Production)
- ✅ **Phase C (Apr 16):** RBAC & 2FA Infrastructure

---

## 1) Executive Summary

Saigon Audio Tour là nền tảng thuyết minh du lịch tự động cho tuyến tham quan tại TP.HCM (trọng tâm Quận 1), gồm:
- **Mobile App (du khách):** bản đồ, geofencing, tự động phát thuyết minh đa ngôn ngữ, quản lý gói Premium.
- **Admin Web (vận hành):** quản lý POI, tuyến tour, nội dung thuyết minh.
- **API Backend:** cung cấp dữ liệu, auth, subscription, ghi nhận hành vi nghe.

Mục tiêu chính:
1. Tăng trải nghiệm tham quan tự động, ít thao tác.
2. Quản trị nội dung nhanh, chính xác theo tuyến.
3. Theo dõi hành vi người dùng để tối ưu vận hành tour.

---

## 2) Product Goals & KPI

### 2.1 Goals
- G1. Tự động phát đúng nội dung tại đúng POI.
- G2. Giảm thời gian vận hành nội dung cho admin.
- G3. Mở rộng trải nghiệm đa ngôn ngữ.
- G4. Tạo doanh thu bổ sung qua Premium.

### 2.2 KPI (đề xuất)
- K1. Tỷ lệ phát thuyết minh thành công khi vào geofence: **>= 95%**.
- K2. Tỷ lệ crash mobile trong splash/flow chính: **< 1% session**.
- K3. API p95 cho endpoint đọc dữ liệu POI/tour: **< 300ms**.
- K4. Tỷ lệ chuyển đổi Free -> Premium: **>= 3%** user active/tháng.

---

## 3) Personas

- **P1 Du khách:** cần trải nghiệm tự động, ít thao tác, rõ ngôn ngữ.
- **P2 Điều hành tour/Admin:** cần CRUD nội dung nhanh, ít lỗi, dễ kiểm soát.
- **P3 Chủ vận hành:** cần số liệu nghe để tối ưu tuyến, điểm dừng và nội dung.

---

## 4) Scope

### 4.1 In-Scope (v3)
- Đăng nhập/đăng ký user, đăng nhập admin.
- Quản lý POI (CRUD + ảnh + radius + ưu tiên).
- Quản lý tuyến tour (CRUD + gán POI vào tour).
- Quản lý narration/audio theo POI-ngôn ngữ.
- Geofencing tự động phát nội dung trên mobile.
- Ghi nhận listen event + thống kê top POI.
- Subscription Premium (plan, order mock, mark-paid, cancel).
- Đa ngôn ngữ UI/narration (vi, en, zh, ja, ko, fr).

### 4.2 Out-of-Scope (tạm thời)
- Authorization middleware (endpoint-level enforcement) - core done.
- 2FA admin UI (verification flow in web portal).
- Offline full sync + conflict resolution hoàn chỉnh.
- MoMo/Stripe gateway adapters (VNPay implemented).

### 4.3 Recently Completed (Phase B-C)
- ✅ Production payment gateway (VNPay) with HMAC SHA512 signatures.
- ✅ Webhook IPN handling for payment confirmation.
- ✅ Idempotency protection (duplicate payment prevention).
- ✅ RBAC (Role-Based Access Control) with 3 system roles.
- ✅ 2FA TOTP implementation with QR code generation.
- ✅ Permission-based authorization service.
- ✅ Role/Permission seeding on startup.

---

## 5) Functional Requirements

| ID | Requirement | Priority | Acceptance Criteria |
|---|---|---|---|
| FR-01 | Auth user/admin | P0 | User login/register thành công; admin login có token hợp lệ |
| FR-02 | POI Management | P0 | Admin tạo/sửa/xóa POI; upload ảnh POI; radius & tọa độ lưu đúng |
| FR-03 | Tour Management | P0 | Admin CRUD tour; gán POI vào tour theo thứ tự |
| FR-04 | Narration Management | P0 | Admin upload audio + content theo POI/language |
| FR-05 | Geofencing Auto-Play | P0 | App kiểm tra vị trí định kỳ; vào vùng POI thì trigger narration; chống phát lặp trong phiên |
| FR-06 | Map Experience | P1 | Hiển thị POI/tuyến trên bản đồ; fallback OSM khi thiếu provider key |
| FR-07 | Subscription Premium | P1 | Lấy plans; tạo order production gateway; webhook validation; hủy gói; đồng bộ trạng thái user |
| FR-08 | User Analytics (basic) | P1 | Ghi listen event; truy vấn top POI được nghe; heatmap dashboard |
| FR-11 | RBAC Admin Control | P2 | Multiple roles per admin; permission-based access; audit trail |
| FR-12 | 2FA Authentication | P2 | TOTP setup with QR code; verification during login; compatible with authenticator apps |
| FR-09 | Localization | P1 | Đổi app language và narration language theo cài đặt |
| FR-10 | Settings & Account | P1 | Hiển thị profile, trạng thái premium, logout/login flow |

---

## 6) Non-Functional Requirements

| ID | Requirement | Target |
|---|---|---|
| NFR-01 | Reliability | API uptime mục tiêu 99.5% (staging/prod) |
| NFR-02 | Performance | p95 read API < 300ms, p95 write API < 600ms |
| NFR-03 | Security | JWT cho API, password hash BCrypt, HTTPS ở môi trường deploy |
| NFR-04 | Observability | Có log lỗi quan trọng, tracing request chính |
| NFR-05 | Maintainability | Tách service theo module (Auth/POI/Tour/Narration/Subscription) |
| NFR-06 | Compatibility | Mobile Android là target chính; iOS/MacCatalyst theo môi trường build |

---

## 7) Architecture (Current + Target)

### 7.1 Current
- **Mobile:** .NET MAUI
- **Admin:** ASP.NET Core MVC
- **API:** ASP.NET Core Web API
- **Data:** EF Core; SQL Server nếu connect được, fallback SQLite
- **Auth:** JWT (API), Cookie (Admin Web)

### 7.2 Notes
- Kiến trúc đã chia module service tương đối tốt.
- Dashboard analytics hiện ở mức cơ bản, chưa đủ heatmap production.

---

## 8) API Capability Baseline (High-level)

- `Auth`: login, register, admin-login, profile
- `Pois`: list, nearby, create/update/delete, upload image
- `Tours`: list/detail, create/update/delete, add POI to tour
- `Narrations`: get by POI, create narration
- `Dashboard`: top-pois, record-listen
- `Subscription`: plans, user status, create-order, order-status, mark-paid, cancel

---

## 9) Traceability: PRD v2.0 vs Current Codebase (Audit)

**Audit date:** 2026-04-16

| PRD v2.0 item | Status | Assessment |
|---|---|---|
| Quản lý POI (CRUD + geofence) | ✅ Done | Có đầy đủ API + Admin UI |
| Upload audio đa ngôn ngữ | ✅ Done | Có form upload narration/audio |
| Quản lý tour | ✅ Done | Có CRUD tour + add POI |
| GPS tracking + auto-play | ✅ Done | Geofencing loop + trigger narration |
| Anti-spam narration | ✅ Done | Có đánh dấu POI đã phát trong phiên |
| Heatmap analytics | ⚠️ Partial | Có record-listen + top POI, chưa có heatmap dashboard hoàn chỉnh |
| User & permissions (RBAC + JWT) | ⚠️ Partial | Có JWT + Admin role cơ bản, chưa có RBAC nhiều cấp |
| 2FA | ❌ Not done | Chưa thấy flow 2FA |
| Drag-drop route builder | ❌ Not done | Chưa có UI kéo-thả |
| Offline download đầy đủ | ❌ Not done | Chưa có cơ chế sync offline hoàn chỉnh |

### 9.1 Mismatch cần chỉnh từ PRD v2.0
- Mô tả địa lý đang lẫn TP.HCM và Hà Nội.
- Stack ghi “Flutter” nhưng code hiện là .NET MAUI.
- Stack ghi “SQL Server 2022” nhưng hệ thống đang chạy SQL Server **hoặc** SQLite fallback.
- Một số KPI/claims chưa có bằng chứng đo đạc.

---

## 10) Release Plan (đề xuất)

### Phase A — Stabilize (1–2 sprint)
- Hoàn thiện dashboard analytics (chart + heatmap).
- Chuẩn hóa dữ liệu location log (lat/lng, duration, timestamp).
- Thêm health checks + structured logging.

### Phase B — Premium Production (1 sprint)
- Tích hợp payment gateway thật.
- Webhook xác nhận thanh toán.
- Chống gian lận và retry strategy.

### Phase C — Security & Scale (1–2 sprint)
- RBAC nhiều cấp cho admin.
- Optional 2FA cho admin.
- Tối ưu cache/read model cho POI gần nhất.

---

## 11) Risks & Mitigation

- **R1: Sai trigger geofence do GPS nhiễu** → debounce + hysteresis radius.
- **R2: Nội dung thiếu ngôn ngữ** → fallback vi/en + cảnh báo admin.
- **R3: Payment mock không phản ánh production** → tách payment adapter từ sớm.
- **R4: PRD drift theo code** → bắt buộc cập nhật traceability mỗi sprint.

---

## 12) Definition of Done (DoD)

- Yêu cầu có AC rõ ràng và test pass.
- Endpoint/API docs cập nhật.
- Không tạo warning/error mới nghiêm trọng ở project liên quan.
- PRD traceability table được cập nhật.

---

## Appendix A — Recommended Governance

- Chu kỳ cập nhật PRD: 2 tuần/lần hoặc khi đổi phạm vi lớn.
- Mỗi feature phải có: Owner, AC, rollout plan, rollback plan.
- Dùng tag release theo module: `mobile/*`, `api/*`, `admin/*`.
