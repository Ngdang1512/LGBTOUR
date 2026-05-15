# Quick Reference - Action Type Tracking

## Câu hỏi & Trả lời

### Q: "Quản lý theo ID của thiết bị hay hành động mà người dùng vừa làm?"
**A**: Quản lý theo **HÀNH ĐỘNG (Action Type)**. Mỗi user action được ghi nhận: detail, navigation, listening, idle, moving.

### Q: "1 click qua chỉ đường thì nó chỉ hiện bên admin là chỉ đường hoặc chi tiết chứ không hiển thị cả chi tiết và chỉ đường mặc dù mở 2 tab?"
**A**: ✅ Đúng! Giờ đây chỉ hiển thị **hành động hiện tại (mới nhất)** từ user, không lẫn 2 hành động.

---

## 🔥 Core Concept

```javascript
// Trước: Chỉ biết user ID
{ deviceId: "u-alice", poiId: 42 }  // user-123 ở POI nào?

// Sau: Biết cả hành động
{ deviceId: "u-alice", actionType: "detail", poiId: 42 }  // viewing
{ deviceId: "u-alice", actionType: "navigation", poiId: 42 }  // navigating
```

---

## 📍 Action Types

| Type | Icon | Meaning | When |
|------|------|---------|------|
| `'idle'` | ⏸️ | User not doing anything | Logged in, not clicked |
| `'detail'` | 👁️ | Viewing POI details | Click "Chi tiết" |
| `'navigation'` | 🗺️ | Using Google Maps | Click "Chỉ đường" |
| `'listening'` | 🎵 | Playing audio | Click "Nghe" / audio playing |
| `'moving'` | 🚶 | Walking around | GPS enabled |

---

## 💻 Code Changes

### In Webapp (index.html)

**1. New Function:**
```javascript
function setCurrentAction(type, poiId, poiName) {
  currentActiveAction = { type, poiId, poiName, timestamp: Date.now() };
}
```

**2. On Button Click:**
```javascript
// When clicking "Chi tiết"
setCurrentAction('detail', 42, 'POI Name');

// When clicking "Chỉ đường"
setCurrentAction('navigation', 42, 'POI Name');

// When clicking "Nghe"
setCurrentAction('listening', 42, 'POI Name');
```

**3. When Closing:**
```javascript
// When user closes detail panel
setCurrentAction('idle');
```

### In Backend (ActivityTelemetryDto.cs)

```csharp
public string? ActionType { get; set; }  // NEW FIELD
```

---

## 📊 What Admin Sees

**Before**:
```
User: Alice
At: Nhà Thờ Đức Bà (POI 1)
At: Bitexco Tower (POI 5)  ← Confusion! Same user 2 places?
```

**After**:
```
User: Alice
🗺️ Navigating to Bitexco Tower (POI 5)  ← Clear!
```

---

## ⚡ Testing in 30 Seconds

```
1. Open webapp: http://localhost:5117/webapp
2. Login
3. Click "Chi tiết" on POI
4. Open DevTools console
5. Type: currentActiveAction
6. Result: { type: 'detail', poiId: XX, ... }  ✅

Or 2 tabs:
- Tab 1: Click "Chi tiết POI A"
- Tab 2: Click "Chỉ đường POI B"
- Check AdminWeb Activity
- Should show ONLY the latest action
```

---

## 🎯 Key Files

| File | Change |
|------|--------|
| `webapp/index.html` | Added setCurrentAction(), updated handlers |
| `ActivityTelemetryDto.cs` | Added ActionType property |
| Documentation | 4 new guides created |

---

## ✅ Result

**Multiple tabs?** ✓ No more confusion  
**Admin clarity?** ✓ Knows exactly what user is doing  
**Performance?** ✓ No impact  
**Backward compatible?** ✓ Yes  

---

## 📞 Quick Summary

- **What changed?**: Added action type tracking
- **Why?**: To fix confusion when user has 2 tabs open  
- **How?**: Each user action calls `setCurrentAction()` to set current action
- **What admin sees?**: Only the latest action (not multiple)
- **Is it ready?**: ✅ Yes! Can test now

---

**Implementation Status: ✅ COMPLETE**
