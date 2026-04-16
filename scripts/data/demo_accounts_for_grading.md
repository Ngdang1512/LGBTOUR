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

## Notes
- These QR codes contain plain-text demo credentials only for grading/demo.
- Do not use in production.
