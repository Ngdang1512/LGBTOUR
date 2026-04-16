# FR → API → Screen Mapping

Cập nhật: 2026-04-16

| FR | API Endpoint | Mobile Screen / Service | Admin Screen | Status |
|---|---|---|---|---|
| FR-01 Auth | `/api/Auth/login`, `/api/Auth/register`, `/api/Auth/admin-login`, `/api/Auth/profile` | `LoginPage`, `RegisterPage`, `SettingsPage`, `AuthApiService` | `Auth/Login` | Done |
| FR-02 POI Management | `/api/Pois` (GET/POST/PUT/DELETE), `/api/Pois/{id}/image` | `MainPage`, `MapPage`, `DetailPage`, `PoiApiService` | `POIs/Index`, `POIs/Create`, `POIs/Edit`, `POIs/Delete` | Done |
| FR-03 Tour Management | `/api/Tours` (GET/POST/PUT/DELETE), `/api/Tours/{tourId}/pois` | `MainPage`, `MapPage` | `Tours/Index`, `Tours/Create`, `Tours/Edit`, `Tours/Delete` | Done |
| FR-04 Narration | `/api/Narrations/{poiId}`, `/api/Narrations` (POST) | `NarrationEngine`, `GeofencingService` | `Narrations/Create` | Done |
| FR-05 Geofencing Auto-play | `/api/Pois`, `/api/Narrations/{poiId}` | `MapPage`, `MainPage`, `GeofencingService` | N/A | Done |
| FR-07 Subscription Premium | `/api/Subscription/plans`, `/api/Subscription/user/{id}/status`, `/api/Subscription/create-order`, `/api/Subscription/mark-paid/{orderId}`, `/api/Subscription/cancel/{userId}` | `UpgradePage`, `SettingsPage`, `SubscriptionApiService` | N/A | Done |
| FR-08 Analytics | `/api/Dashboard/record-listen`, `/api/Dashboard/top-pois` | `MainPage` (log listen) | Dashboard (một phần) | Partial |

## Ghi chú phần còn thiếu

- Heatmap analytics end-to-end trên Admin Dashboard: **chưa hoàn tất**.
- RBAC nhiều cấp + 2FA: **chưa triển khai**.
- Offline sync đầy đủ: **chưa triển khai**.
