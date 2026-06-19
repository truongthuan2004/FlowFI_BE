# Báo cáo tiến độ — Auth & Notification Service

> Phạm vi: `FlowFi.AuthUserService` (port **5101**) và `FlowFi.NotificationService` (port **5105**)  
> Cập nhật: 20/06/2026

---

## 1. Tổng quan

Hai service này phục vụ **đăng nhập / quản lý tài khoản** và **thông báo cho người dùng** trong app FlowFI.

| Service | Chạy tại | Database | Trạng thái chung |
|---------|----------|----------|------------------|
| AuthUserService | `http://localhost:5101` | `FlowFi_AuthUser` — port **6001** | Đã chạy được, đã test register/login/profile |
| NotificationService | `http://localhost:5105` | `FlowFi_Notification` — port **6004** | Code xong, **chưa setup DB / chưa test đầy đủ** |

**Format response (Auth):** tất cả API Auth trả về dạng chuẩn:

```json
{
  "success": true,
  "message": "...",
  "data": { ... },
  "errors": { "code": "...", "details": "..." }
}
```

**Swagger JWT:** Auth service có nút **Authorize** — dán token dạng `Bearer <accessToken>` để gọi API cần đăng nhập.

---

## 2. AuthUserService — Đã làm được

| Chức năng | Trạng thái | Ghi chú |
|-----------|------------|---------|
| Đăng ký tài khoản (email + mật khẩu) | ✅ Xong | Lưu DB, trả JWT + refresh token |
| Đăng nhập | ✅ Xong | Kiểm tra email/mật khẩu, trả token |
| Làm mới token (refresh) | ✅ Xong | Đổi refresh token cũ → token mới |
| Đăng xuất | ✅ Xong | Thu hồi refresh token (cần Bearer token) |
| Quên mật khẩu | ✅ Một phần | Tạo token + OTP lưu DB, **chưa gửi email thật** |
| Đặt lại mật khẩu | ✅ Xong | Dùng token từ forgot-password |
| Đổi mật khẩu | ✅ Xong | User đang đăng nhập, cần mật khẩu cũ |
| Xem hồ sơ (`/users/me`) | ✅ Xong | Đã test với Bearer token |
| Cập nhật hồ sơ | ✅ Xong | Tên, avatar, ngày sinh |
| Cập nhật tùy chỉnh | ✅ Xong | Tiền tệ, hạn mức ngân sách tháng |
| Ghi log hoạt động (user_logs) | ✅ Nội bộ | Tự ghi khi register/login/... — **không có API xem** |
| Database 5 bảng | ✅ Xong | `users`, `refresh_tokens`, `password_reset_tokens`, `user_devices`, `user_logs` |
| Migration SQL | ✅ Có file | `scripts/migrations/auth_service_reset.sql` |

---

## 3. AuthUserService — Chưa làm

| Chức năng | Trạng thái | Ghi chú |
|-----------|------------|---------|
| Đăng nhập Google (OAuth) | ❌ Chưa | Có DTO `GoogleLoginRequest` nhưng **không có API** |
| Gửi email quên mật khẩu | ❌ Chưa | Token/OTP chỉ lưu DB, user không nhận email |
| API quản lý thiết bị đăng nhập | ❌ Chưa | Bảng `user_devices` có trong DB, chưa có endpoint |
| API xem lịch sử hoạt động (user_logs) | ❌ Chưa | Repository có sẵn, chưa expose API |
| Đăng xuất tất cả phiên (logout-all) | ❌ Chưa | Chỉ logout 1 refresh token |
| Xác thực email (verify) | ❌ Chưa | Field `is_verified` có nhưng chưa có flow |
| RabbitMQ publish `user.created` | ⚠️ Tùy môi trường | Code có, lỗi thì bỏ qua (không crash) |

---

## 4. AuthUserService — Danh sách API

**Base URL:** `http://localhost:5101`

| STT | Chức năng | Endpoint | Method | Mô tả | Trạng thái |
|-----|-----------|----------|--------|-------|------------|
| 1 | Đăng ký tài khoản | `/auth/register` | POST | Tạo tài khoản mới bằng email + mật khẩu, trả JWT + refresh token | Done |
| 2 | Đăng nhập | `/auth/login` | POST | Kiểm tra email/mật khẩu, trả access token + refresh token | Done |
| 3 | Làm mới token | `/auth/refresh` | POST | Đổi refresh token cũ, lấy access token mới | Done |
| 4 | Đăng xuất | `/auth/logout` | POST | Thu hồi refresh token hiện tại (cần Bearer token) | Done |
| 5 | Quên mật khẩu | `/auth/forgot-password` | POST | Tạo token + OTP lưu DB, luôn trả 200 (chưa gửi email) | Một phần |
| 6 | Đặt lại mật khẩu | `/auth/reset-password` | POST | Đặt mật khẩu mới bằng token từ forgot-password | Done |
| 7 | Đổi mật khẩu | `/auth/change-password` | POST | Đổi mật khẩu khi đang đăng nhập, cần mật khẩu cũ | Done |
| 8 | Lấy hồ sơ | `/users/me` | GET | Lấy thông tin user hiện tại (cần Bearer token) | Done |
| 9 | Cập nhật hồ sơ | `/users/me` | PUT | Sửa tên, avatar, ngày sinh | Done |
| 10 | Cập nhật tùy chỉnh | `/users/me/preferences` | PUT | Sửa tiền tệ, hạn mức ngân sách tháng | Done |
| 11 | Đăng nhập Google | `/auth/google` | POST | Đăng nhập bằng Google OAuth | Chưa |
| 12 | Đăng xuất tất cả phiên | `/auth/logout-all` | POST | Thu hồi toàn bộ refresh token của user | Chưa |
| 13 | Quản lý thiết bị | `/users/devices` | GET/POST/DELETE | Đăng ký, xem, hủy thiết bị đăng nhập | Chưa |
| 14 | Lịch sử hoạt động | `/users/logs` | GET | Xem log đăng nhập, đổi mật khẩu, ... | Chưa |

**Cách test nhanh trên Swagger:** Gọi `POST /auth/login` → copy `accessToken` → **Authorize** → nhập `Bearer <token>` → gọi `GET /users/me`.

---

## 5. NotificationService — Đã làm được

| Chức năng | Trạng thái | Ghi chú |
|-----------|------------|---------|
| Code API notifications | ✅ Có | List, chi tiết, đếm chưa đọc, đánh dấu đọc, xóa |
| Code API cài đặt thông báo | ✅ Có | Bật/tắt email, push, cảnh báo ngân sách |
| Code API thiết bị push | ✅ Có | Đăng ký token, sync thông báo chưa gửi |
| API nội bộ tạo thông báo | ✅ Có | `POST /notifications/internal/send` — cho service khác gọi |
| RabbitMQ consumer | ⚠️ Một phần | Lắng nghe sự kiện budget/transaction — **thường lỗi kết nối** |
| Migration SQL | ✅ Có file | `scripts/migrations/notification_service_migration.sql` |

---

## 6. NotificationService — Chưa làm / chưa xong

| Chức năng | Trạng thái | Ghi chú |
|-----------|------------|---------|
| Setup database port 6004 | ❌ Chưa chắc | Cần chạy migration SQL trên `FlowFi_Notification` |
| Test end-to-end với JWT | ❌ Chưa | Chưa verify đầy đủ trên môi trường local |
| Gửi push thật (FCM/APNS) | ❌ Chưa | Chỉ lưu push token vào DB |
| Gửi email thông báo | ❌ Chưa | Chỉ lưu notification trong DB |
| RabbitMQ đúng credential | ❌ Chưa | Consumer dùng `guest/guest`, config appsettings khác |
| Hủy đăng ký push token | ❌ Stub | `DELETE /devices/push-tokens/{id}` trả 204 nhưng **không xóa DB** |
| Format ApiResponse chuẩn | ❌ Chưa | Response khác Auth (trả object trực tiếp, không bọc `success/message`) |

---

## 7. NotificationService — Danh sách API

**Base URL:** `http://localhost:5105`

| STT | Chức năng | Endpoint | Method | Mô tả | Trạng thái |
|-----|-----------|----------|--------|-------|------------|
| 1 | Lấy danh sách thông báo | `/notifications` | GET | Lấy danh sách thông báo, phân trang, lọc đọc/chưa đọc | Một phần |
| 2 | Đếm thông báo chưa đọc | `/notifications/unread-count` | GET | Trả về số lượng thông báo chưa đọc | Một phần |
| 3 | Lấy chi tiết thông báo | `/notifications/{id}` | GET | Lấy thông tin một thông báo | Một phần |
| 4 | Đánh dấu đã đọc | `/notifications/{id}/read` | PATCH | Đánh dấu một thông báo là đã đọc | Done |
| 5 | Đánh dấu tất cả đã đọc | `/notifications/read-all` | PATCH | Đánh dấu toàn bộ thông báo là đã đọc | Done |
| 6 | Xóa thông báo | `/notifications/{id}` | DELETE | Xóa một thông báo | Done |
| 7 | Tạo thông báo nội bộ | `/notifications/internal/send` | POST | Service khác gọi để tạo thông báo cho user | Done |
| 8 | Lấy cài đặt thông báo | `/notification-settings` | GET | Xem cài đặt bật/tắt email, push, cảnh báo ngân sách | Một phần |
| 9 | Cập nhật cài đặt thông báo | `/notification-settings` | PUT | Cập nhật bật/tắt email, push, cảnh báo ngân sách | Một phần |
| 10 | Đăng ký push token | `/devices/push-tokens` | POST | Lưu token push (FCM/APNS) của thiết bị vào DB | Một phần |
| 11 | Đồng bộ thiết bị | `/devices/sync` | POST | Lấy danh sách thông báo chưa gửi tới thiết bị | Một phần |
| 12 | Hủy push token | `/devices/push-tokens/{deviceFingerprint}` | DELETE | Hủy đăng ký push token của thiết bị | Chưa |

---

## 8. Hạ tầng cần nhớ

### Auth DB (đã dùng được)

| Thông tin | Giá trị |
|-----------|---------|
| Host | `localhost` |
| Port | `6001` |
| Database | `FlowFi_AuthUser` |
| User | `flowfi` |
| Password | Xem biến `POSTGRES_PASSWORD` trong file `.env` local |

### Notification DB (cần setup)

| Thông tin | Giá trị |
|-----------|---------|
| Host | `localhost` |
| Port | `6004` |
| Database | `FlowFi_Notification` |
| User | `flowfi` |
| Password | Xem biến `POSTGRES_PASSWORD` trong file `.env` local |

### Chạy service

```powershell
cd d:\FPTLearn\ki8\FlowFI_BE

# Auth
dotnet run --project src/FlowFi.AuthUserService

# Notification
dotnet run --project src/FlowFi.NotificationService
```

---

## 9. Việc nên làm tiếp (ưu tiên)

1. **Notification:** chạy migration SQL trên DB port 6004, đồng bộ password connection string
2. **Notification:** test API với JWT từ Auth service
3. **Auth:** tích hợp gửi email cho forgot-password (hoặc trả token trong response khi dev)
4. **Auth:** thêm API Google login (nếu cần theo yêu cầu dự án)
5. **Notification:** sửa RabbitMQ consumer dùng đúng config từ `appsettings.json`
6. **Notification:** implement xóa push token thật sự

---

## 10. Tóm tắt 1 dòng

- **Auth:** Core đăng ký / đăng nhập / profile / đổi mật khẩu **đã chạy được**; thiếu OAuth, email, API devices/logs.
- **Notification:** **Code API đủ** nhưng **chưa setup DB + chưa test đầy đủ**; chưa gửi push/email thật.
