# Hướng dẫn cập nhật tên miền Backend mới

**Backend URL mới:** `https://growmateapi-1071992103404.asia-southeast1.run.app`

---

## 📋 DANH SÁCH CẦN CẬP NHẬT

### 1️⃣ **BACKEND (Code)**

#### A. File `GrowMate/Program.cs`

**Vị trí 1: CORS Origins (dòng 209-213)**

```csharp
policy.WithOrigins(
    "http://localhost:5173",
    "https://growmate.site",
    "https://www.growmate.site",
    "https://growmate-xdig.vercel.app"
)
```

✅ **KHÔNG CẦN ĐỔI** - Đây là danh sách domain Frontend được phép gọi API, không phải Backend URL.

**Vị trí 2: OnRemoteFailure redirect (dòng 156)**

```csharp
var feUrl = "https://www.growmate.site/login?error=" + Uri.EscapeDataString(error);
```

✅ **KHÔNG CẦN ĐỔI** - Đây là URL Frontend để redirect khi có lỗi, không phải Backend.

---

#### B. File `GrowMate/Controllers/AuthenticationController.cs`

**Vị trí: GoogleCallback redirect (dòng 228)**

```csharp
var redirectUrl = $"https://www.growmate.site/google-callback?token={Uri.EscapeDataString(user.Token)}";
```

✅ **KHÔNG CẦN ĐỔI** - Đây là URL Frontend để redirect sau khi login Google thành công.

---

### 2️⃣ **GOOGLE CLOUD CONSOLE** ⚠️ **QUAN TRỌNG**

Bạn **PHẢI** cập nhật Google OAuth settings:

#### Bước 1: Truy cập Google Cloud Console

1. Vào: https://console.cloud.google.com/
2. Chọn project của bạn
3. Vào **APIs & Services** → **Credentials**
4. Tìm OAuth 2.0 Client ID của bạn

#### Bước 2: Cập nhật **Authorized redirect URIs**

Thêm URL mới:

```
https://growmateapi-1071992103404.asia-southeast1.run.app/api/auth/google-callback
```

**Lưu ý:**

- Giữ lại các URL cũ nếu vẫn cần dùng (localhost, Azure cũ, etc.)
- URL phải khớp **CHÍNH XÁC** (có/không có trailing slash, http/https)

#### Bước 3: Cập nhật **Authorized JavaScript origins** (nếu dùng Google Identity Services)

Thêm:

```
https://growmateapi-1071992103404.asia-southeast1.run.app
```

**Danh sách đầy đủ nên có:**

- `http://localhost:5173` (cho localhost FE)
- `https://www.growmate.site` (FE production)
- `https://growmate-xdig.vercel.app` (FE Vercel)
- `https://growmateapi-1071992103404.asia-southeast1.run.app` (BE mới)

---

### 3️⃣ **SEPAY WEBHOOK** ⚠️ **QUAN TRỌNG**

Sepay cần biết URL webhook mới để gửi thông báo thanh toán.

#### Bước 1: Xác định webhook endpoint

URL webhook của bạn:

```
https://growmateapi-1071992103404.asia-southeast1.run.app/api/Payment/webhook/sepay
```

#### Bước 2: Cập nhật trong Sepay Dashboard

1. Đăng nhập vào Sepay Dashboard/Admin panel
2. Tìm mục **Webhook Settings** hoặc **Callback URL**
3. Cập nhật URL webhook thành:
   ```
   https://growmateapi-1071992103404.asia-southeast1.run.app/api/Payment/webhook/sepay
   ```
4. Đảm bảo **Webhook Token** (API Key) trong `appsettings.json` hoặc Azure App Settings khớp với Sepay

#### Bước 3: Kiểm tra cấu hình Backend

File `GrowMate.Services/Payments/PaymentService.cs` đọc từ config:

```csharp
var expected = _configuration["Sepay:WebhookToken"];
```

Đảm bảo trong Azure App Settings hoặc `appsettings.json` có:

```json
{
  "Sepay": {
    "WebhookToken": "YOUR_SEPAY_WEBHOOK_TOKEN_HERE"
  }
}
```

---

### 4️⃣ **FRONTEND** ⚠️ **QUAN TRỌNG**

#### File `src/services/axiosClient.ts`

Tìm dòng:

```typescript
const DEFAULT_API_BASE_URL = "https://growmate.azurewebsites.net/api";
```

Đổi thành:

```typescript
const DEFAULT_API_BASE_URL =
  "https://growmateapi-1071992103404.asia-southeast1.run.app/api";
```

**Hoặc** nếu dùng environment variable:

```typescript
export const API_BASE_URL =
  import.meta.env.VITE_API_BASE_URL ??
  "https://growmateapi-1071992103404.asia-southeast1.run.app/api";
```

#### Cập nhật Vercel Environment Variables

1. Vào Vercel Dashboard → Project → Settings → Environment Variables
2. Tìm hoặc tạo biến: `VITE_API_BASE_URL`
3. Đặt giá trị: `https://growmateapi-1071992103404.asia-southeast1.run.app/api`
4. **Redeploy** Frontend

---

### 5️⃣ **AZURE APP SERVICE (Nếu vẫn dùng Azure để deploy)**

Nếu bạn deploy Backend lên Azure App Service, cần cập nhật:

#### A. App Settings

Vào Azure Portal → App Service → Configuration → Application settings

Kiểm tra các biến:

- `Google:ClientId` - ✅ Không cần đổi
- `Google:ClientSecret` - ✅ Không cần đổi
- `Sepay:WebhookToken` - ✅ Không cần đổi (chỉ cần đổi URL webhook trong Sepay Dashboard)
- `Jwt:Key` - ✅ Không cần đổi
- `ConnectionStrings:DefaultConnection` - ✅ Không cần đổi

#### B. Custom Domain (nếu có)

Nếu bạn đã map custom domain cho Azure App Service, cần cập nhật:

- Google Cloud Console → Authorized redirect URIs
- Sepay Dashboard → Webhook URL

---

## ✅ CHECKLIST

- [ ] **Backend Code:** Không cần đổi (vì không hardcode Backend URL)
- [ ] **Google Cloud Console:** Đã thêm redirect URI mới
- [ ] **Google Cloud Console:** Đã thêm JavaScript origin mới (nếu dùng GIS)
- [ ] **Sepay Dashboard:** Đã cập nhật webhook URL
- [ ] **Frontend:** Đã đổi `API_BASE_URL` trong code
- [ ] **Vercel:** Đã cập nhật `VITE_API_BASE_URL` environment variable
- [ ] **Vercel:** Đã redeploy Frontend
- [ ] **Test:** Đã test Google login trên production
- [ ] **Test:** Đã test Sepay webhook (tạo order và thanh toán thử)

---

## 🧪 KIỂM TRA SAU KHI CẬP NHẬT

### Test Google Login:

1. Mở Frontend production: `https://www.growmate.site/login`
2. Click "Login with Google"
3. Đăng nhập Google
4. Kiểm tra xem có redirect về Frontend và lưu token không

### Test Sepay Webhook:

1. Tạo một order trên Frontend
2. Chọn thanh toán Sepay
3. Quét QR và thanh toán
4. Kiểm tra trong Backend logs xem có nhận được webhook từ Sepay không
5. Kiểm tra order status có chuyển sang "PAID" không

### Test API Endpoints:

1. Mở Swagger: `https://growmateapi-1071992103404.asia-southeast1.run.app/swagger/index.html`
2. Test một vài endpoints (login, get products, etc.)
3. Kiểm tra CORS có hoạt động không (gọi từ Frontend)

---

## 📝 LƯU Ý

1. **Google OAuth:** Nếu quên cập nhật Google Cloud Console, bạn sẽ gặp lỗi `redirect_uri_mismatch` khi login Google.

2. **Sepay Webhook:** Nếu quên cập nhật Sepay Dashboard, thanh toán sẽ không được cập nhật tự động (order vẫn ở trạng thái PENDING).

3. **Frontend:** Nếu quên đổi `API_BASE_URL`, Frontend sẽ vẫn gọi API cũ (Azure) và có thể gặp lỗi CORS hoặc 404.

4. **Caching:** Sau khi cập nhật, có thể cần clear browser cache hoặc hard refresh (Ctrl+Shift+R) để test.

---

## 🔗 TÓM TẮT URL CẦN CẬP NHẬT

| Service           | Vị trí cần cập nhật                                  | URL mới                                                                               |
| ----------------- | ---------------------------------------------------- | ------------------------------------------------------------------------------------- |
| **Google OAuth**  | Google Cloud Console → Authorized redirect URIs      | `https://growmateapi-1071992103404.asia-southeast1.run.app/api/auth/google-callback`  |
| **Google OAuth**  | Google Cloud Console → Authorized JavaScript origins | `https://growmateapi-1071992103404.asia-southeast1.run.app`                           |
| **Sepay Webhook** | Sepay Dashboard → Webhook URL                        | `https://growmateapi-1071992103404.asia-southeast1.run.app/api/Payment/webhook/sepay` |
| **Frontend API**  | `axiosClient.ts` → `API_BASE_URL`                    | `https://growmateapi-1071992103404.asia-southeast1.run.app/api`                       |
| **Frontend API**  | Vercel → `VITE_API_BASE_URL`                         | `https://growmateapi-1071992103404.asia-southeast1.run.app/api`                       |

---

**Chúc bạn deploy thành công! 🚀**
