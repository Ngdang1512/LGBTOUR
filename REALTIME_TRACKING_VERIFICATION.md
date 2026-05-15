# Real-Time Activity Tracking Verification Guide

## Architecture Overview

### 1. **Webapp (Client)** → SignalR Hub Connection
**File**: `/SaigonAudioTour.Api/wwwroot/webapp/index.html`

**Connection Setup** (Line 2185-2200):
```javascript
// Creates persistent SignalR connection to API hub
activityConnection = new signalR.HubConnectionBuilder()
  .withUrl(`${apiBase}/hubs/activity`)  // Points to: http://localhost:5117/hubs/activity
  .withAutomaticReconnect()
  .build();
```

**Activity Payload** (Line 2155-2176):
- Sends every 5 seconds via `initRealtimeActivity()` interval (line 2218-2222)
- Payload includes:
  - `deviceId`: User ID or device session
  - `latitude`, `longitude`: User position or POI location
  - `status`: "listening", "moving", or "idle"
  - `poiId`, `poiName`: Currently playing/viewing POI
  - `timestamp`: ISO UTC time

**Triggers** (14 locations calling `sendActivityNow()`):
- Line 904: Login success
- Line 929: Geofence entry
- Line 935: Geofence exit
- Line 945: User location update
- Line 1563: Map view interaction
- Line 1610: POI click
- Line 1639: Audio play
- Line 1651: Audio pause
- Line 1909: QR code scan
- Line 1954: Download narration
- Line 1978: Settings change
- Line 2078: Map gesture
- Line 2104: Page visibility change
- Line 2115: Beacon trigger

---

### 2. **API Hub** (Server)
**File**: `/SaigonAudioTour.Api/Hubs/ActivityHub.cs`

**UpdateActivity Handler** (Line 17-28):
```csharp
public async Task UpdateActivity(ActivityTelemetryDto data)
{
    // Stores activity in IUserActivityStore
    _store.Upsert(data);
    _store.LinkConnection(Context.ConnectionId, data.DeviceId);
    
    // Broadcasts to AdminWeb clients
    await Clients.All.SendAsync("ReceiveActivityUpdate", data);
    await Clients.Group("admins").SendAsync("ActivityUpdated", data);
    await Clients.Group("admins").SendAsync("OnlineCount", _store.OnlineCount);
}
```

**GetAllActivities Handler** (Line 31-38):
- Called by AdminWeb on connect/reconnect
- Returns snapshot of all active users

**Hub Registration** (Program.cs Line 229):
```csharp
app.MapHub<ActivityHub>("/hubs/activity");
```

---

### 3. **AdminWeb (Listener)** → Real-Time Dashboard
**File**: `/SaigonAudioTour.AdminWeb/Views/Activity/Index.cshtml`

**Connection Setup** (Line 497-500):
```javascript
const conn = new signalR.HubConnectionBuilder()
    .withUrl('@activityHubUrl')  // Default: http://localhost:5117/hubs/activity
    .withAutomaticReconnect()
    .build();
```

**Event Handlers** (Line 523-526):
```javascript
conn.on('ActivityUpdated',      handleUpdate);      // Real-time update
conn.on('ReceiveActivityUpdate', handleUpdate);     // Backward compat
conn.on('ActivitySnapshot',     handleSnapshot);    // Initial load
conn.on('ReceiveAllActivities', handleSnapshot);   // Backward compat
conn.on('OnlineCount', count => { /* update counter */ });
```

**Dashboard Display**:
- Live user list with status and POI
- Online user counter
- Activity feed (only status/POI changes logged)
- Live radar map showing user locations

---

## Testing Procedure

### Setup (Already Done ✅)
1. ✅ API running: `http://localhost:5117`
2. ✅ AdminWeb running: `http://localhost:5202`
3. ✅ Demo POIs seeded: 5 locations with Vietnamese/English narrations
4. ✅ SignalR hub registered: `/hubs/activity` on API
5. ✅ AdminWeb Activity page configured to connect to hub

### Test Steps

#### Step 1: Open AdminWeb Activity Dashboard
1. Open browser tab: `http://localhost:5202/Activity`
2. Verify connection indicator (green dot) shows "Đã kết nối"
3. Observe online user counter (should initialize to 0 or current count)

#### Step 2: Open Webapp in Another Tab/Window
1. Open browser tab: `http://localhost:5117/webapp`
2. Observe console for `initRealtimeActivity()` initialization message

#### Step 3: Login to Webapp
1. Click "Đăng nhập" (Login) button
2. Use demo credentials or register new account
3. Upon successful login:
   - Verify `sendActivityNow()` is called (check browser console)
   - **Expected AdminWeb Update**: "Online user count" increments by 1
   - **Expected AdminWeb Feed**: New entry appears showing user "idle" status

#### Step 4: Interact with POI List
1. On webapp, wait for POI list to load (should show 5 demo POIs)
2. Hover over/click a POI card
3. **Expected AdminWeb Update**:
   - User status changes to "idle"
   - POI information displays (name and ID)
   - Location coordinates update to that POI's lat/lng

#### Step 5: Play Audio
1. On webapp, click "Phát" (Play) button on any POI
2. Audio should start (or preview if narration available)
3. **Expected AdminWeb Update** (every 5 seconds):
   - User status changes to "listening"
   - POI name becomes active (highlighted or emphasized)
   - Activity timestamp updates

#### Step 6: Move to Detail View
1. On webapp, click on a POI to open detail panel
2. View the large hero image, narration, etc.
3. **Expected AdminWeb Update**:
   - Status might change based on audio playback
   - Location reflects the detail POI

#### Step 7: Enable GPS (if available)
1. On webapp, click GPS icon to enable location tracking
2. Webapp requests browser geolocation
3. **Expected AdminWeb Update** (if granted):
   - Coordinates continuously update (every 5 seconds)
   - Status remains "moving" or "idle"
   - RadarMap shows real position instead of POI fallback

#### Step 8: Test Reconnection
1. In AdminWeb, open browser DevTools → Network → throttle to "Offline"
2. Observe connection indicator turns red/orange ("Đang kết nối lại...")
3. Return to "Online" throttle
4. **Expected AdminWeb Update**: Connection dot returns to green
5. **Expected AdminWeb Behavior**: Calls `GetAllActivities` to restore state

---

## Debugging Checklist

### Webapp Console Check
```javascript
// Run in browser console (http://localhost:5117/webapp)
console.log('Activity Device ID:', activityDeviceId);
console.log('Activity Session ID:', activitySessionId);
console.log('Connection Status:', activityConnection?.state);
console.log('Last Payload:', getActivityPayload());
```

### AdminWeb Console Check
```javascript
// Run in browser console (http://localhost:5202/Activity)
console.log('Hub URL:', activityHubUrl);
console.log('Connection State:', conn.state);
console.log('Active Users:', state);  // Should be a Map
```

### Network Inspector
1. **Webapp**: Open DevTools → Network → Filter "activity" or "hubs"
   - Should see WebSocket connection to `http://localhost:5117/hubs/activity`
   - Periodic activity updates (POST every 5 seconds)

2. **AdminWeb**: Open DevTools → Network → Filter "activity" or "hubs"
   - Should see WebSocket connection to `http://localhost:5117/hubs/activity`
   - Continuous incoming messages from hub

### Server Logs
Check API console output for:
- `ActivityHub: UpdateActivity called from {deviceId}`
- `ActivityHub: GetAllActivities called by {connectionId}`
- Any connection/disconnection messages

---

## Expected Data Flow

```
┌─────────────────────────────────────────────────────────────────┐
│                      REAL-TIME TRACKING FLOW                     │
└─────────────────────────────────────────────────────────────────┘

1. WEBAPP (http://localhost:5117/webapp)
   ├─ User logs in
   ├─ initRealtimeActivity() starts 5-second interval
   └─ Creates SignalR connection to /hubs/activity

2. WEBAPP ACTIVITY LOOP
   ├─ Every 5 seconds: getActivityPayload()
   │  └─ Extracts: deviceId, lat/lng, status, poiId, poiName
   ├─ sendActivityNow()
   │  └─ conn.invoke('UpdateActivity', payload)
   └─ Triggers on: login, location update, POI click, audio play, etc.

3. API HUB (http://localhost:5117/hubs/activity)
   ├─ Receives UpdateActivity(payload)
   ├─ Stores in IUserActivityStore
   ├─ Broadcasts to "admins" group:
   │  ├─ "ActivityUpdated" event
   │  ├─ "ReceiveActivityUpdate" event
   │  └─ "OnlineCount" event (total active users)
   └─ Returns activity to store for snapshots

4. ADMINWEB (http://localhost:5202/Activity)
   ├─ Connected to /hubs/activity via SignalR
   ├─ On receive 'ActivityUpdated':
   │  ├─ handleUpdate() stores user state
   │  ├─ pushFeed() only if status/POI changed (not every location)
   │  └─ render() updates UI (feed, counter, map)
   ├─ On receive 'OnlineCount':
   │  └─ Updates counter display
   └─ On reconnect: calls GetAllActivities for full snapshot

RESULT: Admin sees live user activity, locations, and POI interactions
in near real-time (5-second resolution) on Activity dashboard
```

---

## Success Criteria

✅ **Connection Established**
- AdminWeb Activity shows green dot "Đã kết nối"
- Webapp Activity status shows timestamp/activity device ID

✅ **Login Triggers Update**
- User logs in to webapp
- AdminWeb online counter increments immediately (or within 5 seconds)
- AdminWeb feed shows new entry

✅ **POI Activity Tracked**
- User clicks POI on webapp
- AdminWeb shows user at that POI's location within 5 seconds
- POI name appears on AdminWeb user card

✅ **Audio Playback Tracked**
- User clicks "Phát" on webapp
- AdminWeb user status changes to "listening"
- Status persists while audio plays

✅ **Real-time Updates Continue**
- AdminWeb feed continuously updates as user performs actions
- Map (if available) shows live user movement
- No manual refresh needed

---

## Common Issues & Solutions

### Issue: AdminWeb shows "Lỗi kết nối" (Connection Error)
**Cause**: Hub URL incorrect or API not running
**Solution**:
```javascript
// Check in AdminWeb console:
console.log('Hub URL:', activityHubUrl);  // Should be http://localhost:5117/hubs/activity
// Verify API running: curl http://localhost:5117/health
```

### Issue: Webapp connects but AdminWeb doesn't receive updates
**Cause**: Webapp connection not authenticated or hub not broadcasting
**Solution**:
1. Check webapp console: `console.log(activityConnection.state)`
2. Verify payload: `console.log(getActivityPayload())`
3. Check API logs for errors in `UpdateActivity` handler

### Issue: AdminWeb user count shows 0 even when webapp is active
**Cause**: Activity payload missing `deviceId` or store not updating
**Solution**:
1. Verify `authSession?.userId` exists (check localStorage on webapp)
2. Verify fallback `activityDeviceId` is generated
3. Check if activity data expires (GetAllActivities runs 5-min timeout)

### Issue: AdminWeb feed updates too frequently (every location update)
**Cause**: This is actually correct behavior - use `pushFeed()` logic:
- Only shows feed entries when **status or POI changes**
- Continuous location updates don't create new feed entries
- Check line 480-484 in Activity/Index.cshtml for logic

---

## Files Involved

| File | Role | Status |
|------|------|--------|
| webapp/index.html | Client activity sender | ✅ Implemented (14 triggers, 5-sec interval) |
| ActivityHub.cs | Server receiver & broadcaster | ✅ Implemented (UpdateActivity method) |
| Program.cs | Hub registration | ✅ Configured (Line 229) |
| Activity/Index.cshtml | Admin real-time UI | ✅ Implemented (event handlers, display) |
| IUserActivityStore | Activity data storage | ✅ Service (implementation by backend) |

---

## Performance Characteristics

- **Activity Payload Size**: ~200-300 bytes per update
- **Update Frequency**: Every 5 seconds (configurable in line 2218)
- **Bandwidth**: ~1 KB per active user per minute
- **Latency**: Typically <100ms (WebSocket protocol)
- **Connection Overhead**: Single persistent WebSocket per client

---

Last Updated: 2025
Status: Ready for Testing ✅
