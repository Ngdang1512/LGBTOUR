using Microsoft.AspNetCore.SignalR;
using SaigonAudioTour.Api.Models.Realtime;
using SaigonAudioTour.Api.Services;

namespace SaigonAudioTour.Api.Hubs
{
    public class ActivityHub : Hub
    {
        private readonly IUserActivityStore _store;

        public ActivityHub(IUserActivityStore store)
        {
            _store = store;
        }

        // Hàm này được WebApp gọi lên mỗi 5 giây
        public async Task UpdateActivity(ActivityTelemetryDto data)
        {
            if (data == null || string.IsNullOrEmpty(data.DeviceId)) return;

            data.Timestamp = DateTimeOffset.UtcNow;
            _store.Upsert(data);
            _store.LinkConnection(Context.ConnectionId, data.DeviceId);

            // Broadcast theo cả tên sự kiện cũ và mới để tương thích ngược
            await Clients.All.SendAsync("ReceiveActivityUpdate", data);
            await Clients.Group("admins").SendAsync("ActivityUpdated", data);
            await Clients.Group("admins").SendAsync("OnlineCount", _store.OnlineCount);
        }

        // Hàm này được trang Admin gọi 1 lần khi vừa mở trang để lấy dữ liệu cũ
        public async Task GetAllActivities()
        {
            _store.RemoveInactive(TimeSpan.FromMinutes(5));
            var activeList = _store.GetSnapshot();

            await Clients.Caller.SendAsync("ReceiveAllActivities", activeList);
            await Clients.Caller.SendAsync("ActivitySnapshot", activeList);
            await Clients.Caller.SendAsync("OnlineCount", _store.OnlineCount);
        }

        public async Task JoinAdminGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "admins");
            await Clients.Caller.SendAsync("OnlineCount", _store.OnlineCount);
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            _store.RemoveByConnection(Context.ConnectionId);
            await Clients.Group("admins").SendAsync("OnlineCount", _store.OnlineCount);
            await base.OnDisconnectedAsync(exception);
        }
    }
}