# 🔍 Debug Lỗi 500 khi Login với Google

## 📋 Nguyên nhân có thể

### 1️⃣ **Thiếu Google OAuth Config trong Cloud Run** ⚠️ **PHỔ BIẾN NHẤT**

**Triệu chứng:**
- Response 500 với message: `"Google OAuth chưa được cấu hình."`
- Hoặc exception trong catch block

**Cách kiểm tra:**
1. Vào Google Cloud Console → Cloud Run → Service của bạn
2. Click vào service → **"Edit & Deploy New Revision"**
3. Vào tab **"Variables & Secrets"**
4. Kiểm tra có 2 biến sau không:
   - `Google__ClientId` (hoặc `Google:ClientId` tùy cách config)
   - `Google__ClientSecret` (hoặc `Google:ClientSecret`)

**Cách sửa:**
1. Thêm 2 environment variables vào Cloud Run:
   ```
   Google__ClientId = YOUR_GOOGLE_CLIENT_ID
   Google__ClientSecret = YOUR_GOOGLE_CLIENT_SECRET
   ```
   **Lưu ý:** Nếu dùng `:` trong tên biến, Cloud Run có thể tự động convert thành `__` (double underscore). Hãy thử cả 2 cách:
   - `Google:ClientId` 
   - `Google__ClientId`

2. **Redeploy** service

---

### 2️⃣ **Lỗi khi Exchange Code với Google**

**Triệu chứng:**
- Response 400 với message: `"Không đổi được token từ Google."`
- Hoặc exception trong catch block

**Nguyên nhân:**
- Authorization code đã hết hạn (code chỉ valid trong vài phút)
- `redirect_uri` không khớp với Google Cloud Console
- Google Client Secret sai

**Cách kiểm tra:**
1. Kiểm tra Google Cloud Console → OAuth 2.0 Client ID
2. Đảm bảo **Authorized JavaScript origins** có:
   - `https://www.growmate.site` (hoặc domain FE của bạn)
   - `https://growmateapi-1071992103404.asia-southeast1.run.app` (BE mới)

3. Kiểm tra **Client Secret** trong Google Cloud Console có khớp với `Google:ClientSecret` trong Cloud Run không

**Cách sửa:**
- Đảm bảo code được gửi ngay sau khi Google trả về (không delay quá lâu)
- Kiểm tra lại Client Secret

---

### 3️⃣ **Lỗi Database khi tạo/login user**

**Triệu chứng:**
- Response 500 với exception message từ database
- Có thể là: `KeyNotFoundException`, `SqlException`, etc.

**Nguyên nhân:**
- Database connection string sai
- Database không accessible từ Cloud Run
- Transaction fail khi tạo Customer record

**Cách kiểm tra:**
1. Kiểm tra Cloud Run logs:
   ```bash
   gcloud logging read "resource.type=cloud_run_revision AND resource.labels.service_name=YOUR_SERVICE_NAME" --limit 50
   ```

2. Tìm exception message trong logs

**Cách sửa:**
- Kiểm tra `ConnectionStrings:DefaultConnection` trong Cloud Run environment variables
- Đảm bảo Cloud Run có quyền truy cập database (nếu dùng Cloud SQL, cần enable Cloud SQL connection)

---

## 🔧 CÁCH DEBUG CHI TIẾT

### Bước 1: Xem Response Body chi tiết

Trong browser DevTools → Network tab → Click vào request `POST /api/auth/google` → Xem tab **"Response"** hoặc **"Preview"**

Response body sẽ cho biết chính xác lỗi gì:
- `"Google OAuth chưa được cấu hình."` → Thiếu config
- `"Không đổi được token từ Google."` → Lỗi exchange code
- `"Đăng nhập Google thất bại: [exception message]"` → Lỗi database hoặc logic

### Bước 2: Xem Cloud Run Logs

1. Vào Google Cloud Console → **Cloud Run** → Service của bạn
2. Click tab **"Logs"**
3. Tìm log entries với level **ERROR** hoặc **EXCEPTION**
4. Xem stack trace để biết chính xác lỗi ở đâu

### Bước 3: Test từng bước

#### Test 1: Kiểm tra Google OAuth Config
```bash
# SSH vào Cloud Run container (nếu có thể) hoặc thêm log
# Trong AuthenticationController.cs, thêm log:
_logger.LogInformation("Google ClientId: {ClientId}", _googleOptions.ClientId?.Substring(0, 10) ?? "NULL");
```

#### Test 2: Test Exchange Code
- Thử gọi Google Token API trực tiếp với Postman/curl
- Kiểm tra response từ Google

#### Test 3: Test Database
- Test connection string
- Test query `SELECT * FROM Users WHERE Email = 'test@example.com'`

---

## ✅ CHECKLIST SỬA LỖI

- [ ] **Cloud Run Environment Variables:**
  - [ ] `Google:ClientId` hoặc `Google__ClientId` đã được set
  - [ ] `Google:ClientSecret` hoặc `Google__ClientSecret` đã được set
  - [ ] Giá trị khớp với Google Cloud Console

- [ ] **Google Cloud Console:**
  - [ ] Authorized JavaScript origins có domain FE
  - [ ] Authorized redirect URIs có `/api/auth/google-callback` (cho flow cũ)
  - [ ] Client Secret đúng

- [ ] **Database:**
  - [ ] `ConnectionStrings:DefaultConnection` đã được set trong Cloud Run
  - [ ] Cloud Run có quyền truy cập database
  - [ ] Database service đang chạy

- [ ] **Redeploy:**
  - [ ] Đã redeploy Cloud Run service sau khi thêm environment variables
  - [ ] Đã test lại sau khi redeploy

---

## 🚀 QUICK FIX

Nếu bạn chắc chắn vấn đề là **thiếu Google OAuth config**, làm ngay:

1. **Vào Google Cloud Console → Cloud Run**
2. **Click vào service của bạn → "Edit & Deploy New Revision"**
3. **Tab "Variables & Secrets" → "Add Variable"**
4. **Thêm 2 biến:**
   ```
   Name: Google__ClientId
   Value: [Lấy từ Google Cloud Console → OAuth 2.0 Client ID]
   
   Name: Google__ClientSecret  
   Value: [Lấy từ Google Cloud Console → OAuth 2.0 Client ID → Client secrets]
   ```
5. **Click "Deploy"**
6. **Đợi deploy xong và test lại**

---

## 📝 LƯU Ý

- **Environment Variable Naming:** 
  - Trong `appsettings.json`: `"Google:ClientId"`
  - Trong Cloud Run: Có thể dùng `Google:ClientId` hoặc `Google__ClientId` (double underscore)
  - Nếu không hoạt động, thử cả 2 cách

- **Client Secret:**
  - Nếu bạn đã tạo Client Secret mới trong Google Cloud Console, phải cập nhật lại trong Cloud Run
  - Client Secret chỉ hiển thị 1 lần khi tạo, nếu mất phải tạo mới

- **Code Expiry:**
  - Authorization code từ Google chỉ valid trong vài phút
  - Nếu delay quá lâu giữa lúc nhận code và gửi lên backend, code sẽ expire
  - Frontend nên gửi code ngay sau khi nhận được

---

**Chúc bạn debug thành công! 🎯**


