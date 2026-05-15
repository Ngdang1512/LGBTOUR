#!/bin/bash
echo "🚀 Khởi động hệ thống Saigon Audio Tour..."

API_PORT=5117
ADMIN_PORT=5200

echo "🧹 Đang dọn dẹp các tiến trình cũ bị treo..."
# Tìm và diệt tiến trình đang chiếm dụng port 5117 (API)
lsof -ti:$API_PORT | xargs kill -9 2>/dev/null
lsof -ti:$ADMIN_PORT | xargs kill -9 2>/dev/null

echo "1️⃣ Đang chạy API Backend trên http://localhost:$API_PORT"
(cd /Users/admin/Code/SaigonAudioTour/SaigonAudioTour.Api && dotnet run --project SaigonAudioTour.Api.csproj --urls "http://*:$API_PORT") &
API_PID=$!

echo "2️⃣ Đang chạy Admin Web MVC trên http://localhost:$ADMIN_PORT"
(cd /Users/admin/Code/SaigonAudioTour/SaigonAudioTour.AdminWeb && dotnet run --project SaigonAudioTour.AdminWeb.csproj --urls "http://*:$ADMIN_PORT") &
ADMIN_PID=$!

echo "✅ Cả hai dự án đang khởi động!"
echo "--------------------------------------------------"
echo "👉 Web App (Khách): http://localhost:$API_PORT/webapp/index.html"
echo "👉 Admin (Radar):   http://localhost:$ADMIN_PORT"
echo "--------------------------------------------------"
echo "🛑 Nhấn [CTRL + C] để dừng toàn bộ hệ thống."

# Bắt sự kiện CTRL+C để tự động tắt cả 2 dự án
trap "kill $API_PID $ADMIN_PID; exit" SIGINT SIGTERM

wait