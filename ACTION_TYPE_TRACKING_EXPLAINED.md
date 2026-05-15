# User Activity Tracking Mechanism - Chi tiết

## 🎯 Câu hỏi của bạn
**"Quản lý theo ID của thiết bị hay hành động mà người dùng vừa làm? Tôi chọn 1 cái chi tiết, 1 click qua chỉ đường thì nó chỉ hiện bên admin là chỉ đường hoặc chi tiết chứ không hiển thị cả chi tiết và chỉ đường mặc dù mở 2 tab"**

---

## 📊 Cơ chế Trước (Status Quo)

### Vấn đề
```
Mở 2 tab cùng 1 tài khoản:
├─ Tab 1: Xem chi tiết POI A
│  └─ Gửi: { userId: "u-123", poiId: A, status: "idle" }
├─ Tab 2: Xem chỉ đường POI B  
│  └─ Gửi: { userId: "u-123", poiId: B, status: "idle" }
└─ ADMIN THẤY: Lẫn lộn, user "u-123" vừa ở A vừa ở B???
```

**Nguyên nhân**: Mỗi tab gửi activity riêng, cùng user ID → Admin nhận cả 2 updates

---

## ✅ Giải Pháp Implemented (Chi Tiết)

### 1️⃣ **Track by Action Type** (Hành động, không phải User)

**Mô hình mới:**
```javascript
// Trong webapp (index.html)
let currentActiveAction = {
  type: 'detail',    // 'detail', 'navigation', 'listening', 'idle', 'moving'
  poiId: 42,
  poiName: 'Nhà Thờ Đức Bà',
  timestamp: 1715000000
};

function setCurrentAction(type, poiId, poiName) {
  // Mỗi khi user làm gì, set action hiện tại
  currentActiveAction = { type, poiId, poiName, timestamp: Date.now() };
}
```

**Cách hoạt động:**
```
Mở 2 tab cùng 1 tài khoản:
├─ Tab 1: Xem chi tiết POI A
│  ├─ setCurrentAction('detail', A, 'POI A')
│  └─ Gửi: { userId: "u-123", actionType: "detail", poiId: A }
├─ Tab 2: Xem chỉ đường POI B  
│  ├─ setCurrentAction('navigation', B, 'POI B')
│  └─ Gửi: { userId: "u-123", actionType: "navigation", poiId: B }
└─ ADMIN THẤY: 
   ├─ User "u-123" đang xem DETAIL của POI A
   ├─ (hoặc) User "u-123" đang NAVIGATE đến POI B
   └─ Rõ ràng hành động là gì
```

### 2️⃣ **Activity Payload Enhancement**

**Trước:**
```javascript
{
  deviceId: "u-123",
  latitude: 10.7829,
  longitude: 106.6982,
  status: "idle",
  poiId: 42,
  poiName: "Nhà Thờ Đức Bà",
  timestamp: "2025-05-06T10:00:00Z"
}
```

**Sau (New Field):**
```javascript
{
  deviceId: "u-123",
  latitude: 10.7829,
  longitude: 106.6982,
  status: "idle",
  actionType: "detail",        // ← NEW: Hành động cụ thể
  poiId: 42,
  poiName: "Nhà Thờ Đức Bà",
  timestamp: "2025-05-06T10:00:00Z"
}
```

### 3️⃣ **User Action Triggers**

| Hành động | Trigger | ActionType Set | Mô tả |
|-----------|---------|----------------|----|
| **Click Nghe** | `handlePoiAction('listen')` | `'listening'` | User phát audio POI |
| **Click Chi tiết** | `handlePoiAction('detail')` | `'detail'` | User xem chi tiết POI |
| **Click Chỉ đường** | `handlePoiAction('route')` | `'navigation'` | User mở Google Maps |
| **Đóng Chi tiết** | `closePoiDetail()` | `'idle'` hoặc `'moving'` | User quay lại danh sách |
| **GPS Tracking** | `enableGPS()` | `'moving'` | User di chuyển |
| **Login** | `onLoginSuccess()` | `'idle'` | User vừa đăng nhập |

### 4️⃣ **Webapp Flow Chi Tiết**

```javascript
// Line 2155-2188: getActivityPayload()
function getActivityPayload() {
  // Nếu đang phát audio → 'listening'
  if (listeningPoi) {
    setCurrentAction('listening', listeningPoi.id, listeningPoi.name);
  }
  
  // Nếu không, dùng action hiện tại (detail, navigation, idle, moving)
  const activeAction = currentActiveAction?.type || 'idle';
  
  // Trả về payload với actionType
  return {
    deviceId: authSession?.userId,
    actionType: activeAction,  // ← Type hành động rõ ràng
    poiId: activePoi?.id,
    poiName: activePoi?.name,
    latitude: userPosition?.lat || fallbackLat,
    longitude: userPosition?.lng || fallbackLng,
    timestamp: new Date().toISOString()
  };
}

// Line 1551-1580: handlePoiAction() - Xử lý click
function handlePoiAction(event) {
  const action = btn.dataset.action;  // 'listen', 'detail', 'route'
  
  if (action === 'listen') {
    setCurrentAction('listening', poiId, poi.name);
    playNarration(poiId, true);
  }
  
  if (action === 'detail') {
    setCurrentAction('detail', poiId, poi.name);  // ← Set action
    sendActivityNow();  // ← Gửi ngay (không chờ 5 giây)
    openPoiDetail(poiId);
  }
  
  if (action === 'route') {
    setCurrentAction('navigation', poi.id, poi.name);  // ← Set action
    sendActivityNow();
    openGoogleMaps(lat, lng);
  }
}

// Line 2104-2115: closePoiDetail() - Đóng chi tiết
function closePoiDetail() {
  // ...
  if (!currentlyPlayingPoiId) {
    setCurrentAction('idle');  // ← Reset to idle
    sendActivityNow();
  }
}
```

### 5️⃣ **Backend Update - ActivityTelemetryDto**

**File**: `/SaigonAudioTour.Api/Models/Realtime/ActivityTelemetryDto.cs`

```csharp
public sealed class ActivityTelemetryDto
{
    public string DeviceId { get; set; }           // User ID: "u-123"
    public string SessionId { get; set; }          // Session ID
    public double Latitude { get; set; }           // Tọa độ
    public double Longitude { get; set; }          // Tọa độ
    public string Status { get; set; }             // "idle", "moving"
    public string? ActionType { get; set; }        // ← NEW: "detail", "navigation", "listening"
    public int? PoiId { get; set; }                // POI ID
    public string? PoiName { get; set; }           // POI name
    public DateTimeOffset Timestamp { get; set; }  // Thời gian
}
```

### 6️⃣ **AdminWeb Display**

**Trước:**
```
User: u-123
Status: idle
POI: Nhà Thờ Đức Bà (ID 42)
Position: 10.7829, 106.6982
Last Seen: 10:00 AM
```

**Sau (hiện rõ hành động):**
```
User: u-123
Action: 👁️ Viewing Detail
Status: idle
POI: Nhà Thờ Đức Bà (ID 42)
Position: 10.7829, 106.6982
Last Seen: 10:00 AM
```

---

## 🔄 Behavior Pattern

### Scenario: Mở 2 Tab

**Timeline:**
```
09:00:00 - User "Alice" (u-alice) đăng nhập
           → setCurrentAction('idle')
           → Admin thấy: Alice idle

09:00:15 - Tab 1: Alice click "Chi tiết POI A"
           → setCurrentAction('detail', A, 'POI A')
           → sendActivityNow()
           → Admin thấy: Alice viewing detail of POI A

09:00:30 - Tab 2: Alice mở tab khác, click "Chỉ đường POI B"
           → setCurrentAction('navigation', B, 'POI B')
           → sendActivityNow()
           → Admin thấy: Alice navigating to POI B

09:00:45 - Tab 2: Google Maps mở
           → Admin thấy: Alice still navigating to POI B
           
09:01:00 - Mỗi 5 giây, sendActivityNow() gửi activity hiện tại
           → Admin thấy: Current action = navigation to POI B (mới nhất)
           
09:02:00 - Tab 1: Alice đóng chi tiết
           → setCurrentAction('idle')
           → sendActivityNow()
           → Admin thấy: Alice idle (từ Tab 1 active)
           
09:02:15 - Tab 2: Alice quay lại, click POI C
           → setCurrentAction('listening', C, 'POI C')
           → Admin thấy: Alice listening to POI C
```

**Result**: Admin **luôn thấy hành động hiện tại, không lẫn lộn** giữa 2 tab!

---

## 🎮 User Action Map

```
┌─────────────────────────────────────────────────────────────┐
│                    USER INTERACTION                          │
└─────────────────────────────────────────────────────────────┘

Login Page
├─ Click "Đăng nhập" → setCurrentAction('idle')
├─ Send to Admin: idle
└─ User enters webapp

POI List View
├─ Click "Nghe" button → setCurrentAction('listening', poiId)
│  └─ Send to Admin: listening, POI name + ID
├─ Click "Chi tiết" button → setCurrentAction('detail', poiId)
│  └─ Send to Admin: detail, POI name + ID
├─ Click "Chỉ đường" → setCurrentAction('navigation', poiId)
│  └─ Send to Admin: navigation, POI name + ID
└─ GPS Enabled → setCurrentAction('moving')
   └─ Send to Admin: moving, lat/lng

Detail Panel View
├─ Playing Audio → setCurrentAction('listening', poiId)
│  └─ Every 5s: Send to Admin: listening, POI name + ID
├─ Stopped Audio → setCurrentAction('detail', poiId) or 'idle'
└─ Close Detail Panel → setCurrentAction('idle' or 'moving')
   └─ Send to Admin: idle or moving

Map View
├─ Pinch Zoom/Pan → setCurrentAction('moving')
│  └─ Send to Admin: moving
└─ Click Marker → setCurrentAction('detail', poiId)
   └─ Send to Admin: detail, POI name + ID

Settings Page
├─ Toggle Language → setCurrentAction('idle')
└─ Send to Admin: idle

Navigation (Google Maps)
├─ While in Google Maps → setCurrentAction('navigation', poiId)
└─ Return to App → setCurrentAction('idle' or previous action)
   └─ Send to Admin: updated action
```

---

## 📱 Admin Dashboard Display

**Activity List Item** (với actionType):
```html
<div class="activity-item">
  <span class="user-name">Alice (u-123)</span>
  
  <!-- Action Badge -->
  <span class="action-badge detail">
    <i class="bi bi-eye"></i> Viewing Detail
  </span>
  
  <span class="poi-name">Nhà Thờ Đức Bà</span>
  
  <span class="coordinates">
    📍 10.7829, 106.6982
  </span>
  
  <span class="timestamp">1 minute ago</span>
</div>
```

**CSS for Action Badges:**
```css
.action-badge {
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
  display: inline-flex;
  gap: 4px;
  align-items: center;
}

.action-badge.detail {
  background: #E3F2FD;
  color: #1976D2;
}

.action-badge.navigation {
  background: #F3E5F5;
  color: #7B1FA2;
}

.action-badge.listening {
  background: #E8F5E9;
  color: #388E3C;
}

.action-badge.idle {
  background: #F5F5F5;
  color: #666;
}

.action-badge.moving {
  background: #FFF3E0;
  color: #E65100;
}
```

---

## 🔧 Implementation Status

| Component | Status | Details |
|-----------|--------|---------|
| Webapp - setCurrentAction() | ✅ Implemented | Line 2155-2165 |
| Webapp - getActivityPayload() | ✅ Updated | Added `actionType` field |
| Webapp - handlePoiAction() | ✅ Updated | Calls setCurrentAction() on each action |
| Webapp - closePoiDetail() | ✅ Updated | Resets action to 'idle' |
| ActivityTelemetryDto | ✅ Updated | Added `ActionType` property |
| ActivityHub.cs | ✅ Ready | Already broadcasts payload as-is |
| AdminWeb View | 🔄 Ready to implement | Use `actionType` in display logic |

---

## 🧪 Testing the Implementation

### Test Case 1: Open Detail on Tab 1
```
1. Open webapp: http://localhost:5117/webapp
2. Login
3. Click "Chi tiết" on POI #1
4. Open AdminWeb Activity: http://localhost:5202/Activity
5. Verify: actionType = "detail"
6. Expected Admin display: "Alice viewing detail of Nhà Thờ Đức Bà"
```

### Test Case 2: Open 2 Tabs with Different Actions
```
1. Tab 1: http://localhost:5117/webapp
   - Login Alice
   - Click "Chi tiết POI A"
   - Verify actionType = "detail"
   - Admin shows: Alice viewing detail

2. Tab 2: Open another browser tab
   - http://localhost:5117/webapp (same URL)
   - Already logged in as Alice
   - Click "Chỉ đường POI B"
   - Verify actionType = "navigation"
   - Admin shows: Alice navigating (not showing both!)

3. Verify in AdminWeb Activity:
   - Should see ONE entry for Alice
   - actionType = "navigation" (most recent)
   - NOT showing both "detail" and "navigation"
```

### Test Case 3: Switch Between Tabs
```
1. Click Tab 1 (Detail)
   → Admin: actionType = "detail"

2. Click Tab 2 (Navigation)
   → Admin: actionType = "navigation"
   
3. Go back to Tab 1, close Detail panel
   → Admin: actionType = "idle"

4. Back to Tab 2, still navigating
   → Admin: actionType = "navigation"
```

---

## 💾 Data Flow Summary

```
WEBAPP                          API HUB                         ADMIN
───────                         ───────                         ─────

User clicks "Chi tiết"
  ↓
setCurrentAction('detail', 42, 'POI A')
  ↓
getActivityPayload() returns
{ 
  deviceId: 'u-alice',
  actionType: 'detail',
  poiId: 42,
  ...
}
  ↓
sendActivityNow() via SignalR
  ↓
conn.invoke('UpdateActivity', payload)
  ├────────────────────→ ActivityHub.UpdateActivity()
  │                       ↓
  │                    _store.Upsert(data)  [lưu đè]
  │                       ↓
  │                    Clients.Group("admins").SendAsync("ActivityUpdated", data)
  │                       ↓
  ├──────────────────────────→ receives "ActivityUpdated"
  │                             ↓
  │                          handleUpdate(data)
  │                             ↓
  │                          Display: Alice viewing detail POI A
  └─────────────────────────────────────────→ HTML renders
                                             with actionType badge
```

---

## 📝 Code References

**Webapp Changes:**
- [setCurrentAction()](file:///Users/admin/Code/SaigonAudioTour/SaigonAudioTour.Api/wwwroot/webapp/index.html#L2155)
- [getActivityPayload()](file:///Users/admin/Code/SaigonAudioTour/SaigonAudioTour.Api/wwwroot/webapp/index.html#L2165)
- [handlePoiAction()](file:///Users/admin/Code/SaigonAudioTour/SaigonAudioTour.Api/wwwroot/webapp/index.html#L1551)
- [closePoiDetail()](file:///Users/admin/Code/SaigonAudioTour/SaigonAudioTour.Api/wwwroot/webapp/index.html#L2104)

**Backend Changes:**
- [ActivityTelemetryDto.cs](file:///Users/admin/Code/SaigonAudioTour/SaigonAudioTour.Api/Models/Realtime/ActivityTelemetryDto.cs)

---

## 🎓 Summary

**Trước**: Track by User ID → Confusing when 2 tabs open  
**Sau**: Track by Action Type → Clear what user is doing at any moment

**Benefit**: Admin thấy rõ ràng hành động hiện tại của mỗi user, không bị lẫn lộn từ multiple tabs!
