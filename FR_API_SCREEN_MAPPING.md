# FR → API → Screen Mapping

Cập nhật: 2026-04-16

| FR | API Endpoint | Mobile Screen / Service | Admin Screen | Status |
|---|---|---|---|---|
| FR-01 Auth | `/api/Auth/login`, `/api/Auth/register`, `/api/Auth/admin-login`, `/api/Auth/profile` | `LoginPage`, `RegisterPage`, `SettingsPage`, `AuthApiService` | `Auth/Login` | Done |
| FR-02 POI Management | `/api/Pois` (GET/POST/PUT/DELETE), `/api/Pois/{id}/image` | `MainPage`, `MapPage`, `DetailPage`, `PoiApiService` | `POIs/Index`, `POIs/Create`, `POIs/Edit`, `POIs/Delete` | Done |
| FR-03 Tour Management | `/api/Tours` (GET/POST/PUT/DELETE), `/api/Tours/{tourId}/pois` | `MainPage`, `MapPage` | `Tours/Index`, `Tours/Create`, `Tours/Edit`, `Tours/Delete` | Done |
| FR-04 Narration | `/api/Narrations/{poiId}`, `/api/Narrations` (POST) | `NarrationEngine`, `GeofencingService` | `Narrations/Create` | Done |
| FR-05 Geofencing Auto-play | `/api/Pois`, `/api/Narrations/{poiId}` | `MapPage`, `MainPage`, `GeofencingService` | N/A | Done |
| FR-07 Subscription Premium | `/api/Subscription/plans`, `/api/Subscription/user/{id}/status`, `/api/Subscription/create-order` (VNPay), `/api/Subscription/mark-paid/{orderId}`, `/api/Subscription/cancel/{userId}`, `/api/payment/webhook/vnpay` | `UpgradePage`, `SettingsPage`, `SubscriptionApiService` | N/A | Done |
| FR-08 Analytics | `/api/Dashboard/record-listen`, `/api/Dashboard/top-pois`, `/api/Dashboard/heatmap` | `MainPage` (log listen) | `Analytics/Dashboard` (heatmap + Chart.js) | Done |
| FR-11 RBAC | `/api/Admin/roles`, `/api/Admin/permissions` | N/A | `Roles/Index`, `Roles/Manage` | Partial |
| FR-12 2FA | `/api/Auth/2fa-setup`, `/api/Auth/2fa-verify` | N/A | Admin Portal 2FA | Partial |

## Ghi chú phần còn thiếu

### ✅ Hoàn tất
- **Phase A:** Heatmap analytics end-to-end trên Admin Dashboard.
- **Phase B:** Payment gateway production (VNPay) với webhook support.
- **Phase C (Core):** RBAC infrastructure + 2FA TOTP implementation.

### 🔄 Còn lại (aspirational)
- Authorization middleware (endpoint-level permission enforcement).
- 2FA admin UI flow (setup/verification in web portal).
- Role management CRUD UI.
- Offline sync conflict resolution hoàn chỉnh.
- MoMo/Stripe gateway adapters.
