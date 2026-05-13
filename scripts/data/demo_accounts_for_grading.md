# Demo accounts for grading

## 1) Account thường (FREE)
- Email: demo.free@saigontour.local
- Password: Demo@123
- Raw payload sau khi quét: `demo.free@saigontour.local|Demo@123|FREE`
- Tách theo dấu `|`:
  - Phần 1 = Email: `demo.free@saigontour.local`
  - Phần 2 = Password: `Demo@123`
  - Phần 3 = Gói: `FREE`
- QR (text payload):
  https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=demo.free%40saigontour.local%7CDemo%40123%7CFREE
- QR (webapp login, không cần cài app):
  https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=http%3A%2F%2Flocalhost%3A5117%2Fwebapp%2Findex.html%3Fstation%3Dben%2520thanh%26u%3Ddemo.free%40saigontour.local%26p%3DDemo%40123%26plan%3DFREE
- Link trực tiếp (Click để mở web):
  http://localhost:5117/webapp/index.html?station=ben%20thanh&u=demo.free@saigontour.local&p=Demo@123&plan=FREE

## 2) Account Premium
- Email: demo.premium@saigontour.local
- Password: Demo@123
- Raw payload sau khi quét: `demo.premium@saigontour.local|Demo@123|PREMIUM`
- Tách theo dấu `|`:
  - Phần 1 = Email: `demo.premium@saigontour.local`
  - Phần 2 = Password: `Demo@123`
  - Phần 3 = Gói: `PREMIUM`
- QR (text payload):
  https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=demo.premium%40saigontour.local%7CDemo%40123%7CPREMIUM
- QR (webapp login, không cần cài app):
  https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=http%3A%2F%2Flocalhost%3A5117%2Fwebapp%2Findex.html%3Fstation%3Dben%2520thanh%26u%3Ddemo.premium%40saigontour.local%26p%3DDemo%40123%26plan%3DPREMIUM
- Link trực tiếp (Click để mở web):
  http://localhost:5117/webapp/index.html?station=ben%20thanh&u=demo.premium@saigontour.local&p=Demo@123&plan=PREMIUM

## 3) Webapp chung (Khách vãng lai / Backup)
- Dùng để mở thẳng trang webapp mà không cần đăng nhập (chế độ Guest).
- QR (webapp trực tiếp):
  https://api.qrserver.com/v1/create-qr-code/?size=300x300&data=http%3A%2F%2Flocalhost%3A5117%2Fwebapp%2Findex.html%3Fstation%3Dben%2520thanh
- Link trực tiếp (Click để mở web):
  http://localhost:5117/webapp/index.html?station=ben%20thanh

## Verify quickly
- Free: login and check Upgrade page should show trạng thái FREE.
- Premium: login and check Upgrade page should show Premium active.

## Cách dùng 2 mã QR này trong buổi chấm
- Đây là QR chứa **text credential** theo format: `email|password|role`.
- App mobile hiện tại **không có chức năng quét QR để tự đăng nhập**.
- Vì vậy cách dùng là:
  1) Quét QR để xem thông tin account demo.
  2) Mở app, vào màn hình Login.
  3) Nhập email + password tương ứng rồi đăng nhập thủ công.
  4) Vào Upgrade để chứng minh FREE/PREMIUM.

## Cách dùng QR mở webapp trực tiếp
- Domain/IP hiện tại của máy dev: `localhost`.
- Đảm bảo API đang chạy cổng `5117`.
- Quét mã QR "webapp login" sẽ mở trực tiếp trang web và tự động điền thông tin đăng nhập.

## Notes
- These QR codes contain plain-text demo credentials only for grading/demo.
- Do not use in production.
