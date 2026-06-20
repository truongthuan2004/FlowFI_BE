from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.enums import TA_LEFT
from reportlab.lib.pagesizes import landscape, letter
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import inch
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import PageBreak, Paragraph, SimpleDocTemplate, Spacer, Table, TableStyle


ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "output" / "pdf" / "FlowFi_All_Services_API_Catalog_for_UI.pdf"
OUTPUT.parent.mkdir(parents=True, exist_ok=True)

font_dir = Path(r"C:\Windows\Fonts")
pdfmetrics.registerFont(TTFont("Arial", str(font_dir / "arial.ttf")))
pdfmetrics.registerFont(TTFont("Arial-Bold", str(font_dir / "arialbd.ttf")))
pdfmetrics.registerFont(TTFont("Consolas", str(font_dir / "consola.ttf")))

PAGE = landscape(letter)
PAGE_W, PAGE_H = PAGE
MX = 0.52 * inch
MT = 0.48 * inch
MB = 0.48 * inch


def footer(canvas, doc):
    canvas.saveState()
    canvas.setStrokeColor(colors.HexColor("#D1D5DB"))
    canvas.line(MX, 0.34 * inch, PAGE_W - MX, 0.34 * inch)
    canvas.setFont("Arial", 7.5)
    canvas.setFillColor(colors.HexColor("#626262"))
    canvas.drawString(MX, 0.18 * inch, "FlowFi Backend - Frontend API Catalog")
    canvas.drawRightString(PAGE_W - MX, 0.18 * inch, f"Trang {doc.page}")
    canvas.restoreState()


base = getSampleStyleSheet()
title = ParagraphStyle("Title", parent=base["Title"], fontName="Arial-Bold", fontSize=21, leading=25, alignment=TA_LEFT, spaceAfter=10)
subtitle = ParagraphStyle("Subtitle", parent=base["BodyText"], fontName="Arial", fontSize=9, leading=13, textColor=colors.HexColor("#4B5563"), spaceAfter=10)
section = ParagraphStyle("Section", parent=base["Heading2"], fontName="Arial-Bold", fontSize=14, leading=17, spaceBefore=4, spaceAfter=7)
subsection = ParagraphStyle("Subsection", parent=base["Heading3"], fontName="Arial-Bold", fontSize=10.5, leading=13, spaceBefore=5, spaceAfter=5)
body = ParagraphStyle("Body", parent=base["BodyText"], fontName="Arial", fontSize=8.1, leading=11.3, textColor=colors.HexColor("#202020"))
small = ParagraphStyle("Small", parent=body, fontSize=7.2, leading=9.6)
tiny = ParagraphStyle("Tiny", parent=body, fontSize=6.7, leading=8.8)
header = ParagraphStyle("Header", parent=small, fontName="Arial-Bold", textColor=colors.white)
endpoint = ParagraphStyle("Endpoint", parent=tiny, fontName="Consolas", fontSize=6.6, leading=8.7)
method = ParagraphStyle("Method", parent=small, fontName="Arial-Bold")
auth = ParagraphStyle("Auth", parent=tiny, fontName="Arial-Bold", textColor=colors.HexColor("#166534"))
public = ParagraphStyle("Public", parent=tiny, fontName="Arial-Bold", textColor=colors.HexColor("#075985"))
warning = ParagraphStyle("Warning", parent=body, fontSize=7.8, leading=11, backColor=colors.HexColor("#FFF7E6"), borderColor=colors.HexColor("#DCA54C"), borderWidth=0.6, borderPadding=7)
info = ParagraphStyle("Info", parent=body, fontSize=7.8, leading=11, backColor=colors.HexColor("#F4F7FA"), borderColor=colors.HexColor("#B8C2CC"), borderWidth=0.6, borderPadding=7)


def p(value, style=body):
    return Paragraph(str(value), style)


def api_table(rows, color="#1F2937"):
    data = [[p("Method", header), p("Gateway endpoint", header), p("Auth", header), p("Input chính", header), p("Mô tả / UI sử dụng", header)]]
    for row in rows:
        m, ep, au, inp, desc = row
        auth_style = auth if au in ("JWT", "JWT*") else public
        data.append([p(m, method), p(ep, endpoint), p(au, auth_style), p(inp, tiny), p(desc, small)])
    table = Table(data, colWidths=[0.58*inch, 2.72*inch, 0.62*inch, 2.80*inch, 3.65*inch], repeatRows=1, hAlign="LEFT")
    table.setStyle(TableStyle([
        ("BACKGROUND", (0,0), (-1,0), colors.HexColor(color)),
        ("VALIGN", (0,0), (-1,-1), "TOP"),
        ("GRID", (0,0), (-1,-1), 0.35, colors.HexColor("#CDD1D5")),
        ("ROWBACKGROUNDS", (0,1), (-1,-1), [colors.white, colors.HexColor("#F8F9FA")]),
        ("LEFTPADDING", (0,0), (-1,-1), 5),
        ("RIGHTPADDING", (0,0), (-1,-1), 5),
        ("TOPPADDING", (0,0), (-1,-1), 5),
        ("BOTTOMPADDING", (0,0), (-1,-1), 5),
    ]))
    return table


def service_page(name, summary, groups, color):
    flow = [p(name.upper(), title), p(summary, subtitle)]
    for group_name, rows in groups:
        flow.extend([p(group_name, subsection), api_table(rows, color), Spacer(1, 6)])
    return flow


services = [
    ("API Gateway", "http://localhost:8080", "/", "Điểm vào duy nhất cho Web/Mobile; reverse proxy và Swagger tổng hợp."),
    ("Auth User", "http://localhost:5101", "/auth, /users", "Đăng ký, đăng nhập, token và hồ sơ cá nhân."),
    ("Finance Core", "http://localhost:5102", "/finance", "Ví, tag, giao dịch, chuyển ví, giao dịch định kỳ và sync."),
    ("AI Processing", "http://localhost:5103", "/ai", "OCR/voice, Supabase và tạo transaction qua gRPC."),
    ("Analytics", "http://localhost:5104", "/analytics", "Ngân sách, mục tiêu tiết kiệm và đóng góp."),
    ("Notification", "http://localhost:5105", "/notifications", "Thông báo, cài đặt và thiết bị push."),
    ("WebSocket Gateway", "http://localhost:5106", "/ws", "SignalR realtime cho trạng thái và notification."),
]

auth_rows = [
    ("POST", "/auth/register", "Public", "Email, Password, FullName", "Màn hình đăng ký; trả User và access/refresh token."),
    ("POST", "/auth/login", "Public", "Email, Password", "Màn hình đăng nhập; lưu token an toàn và chuyển vào app."),
    ("POST", "/auth/refresh", "Public", "RefreshToken", "Làm mới access token khi API trả 401 do hết hạn."),
    ("POST", "/auth/logout", "JWT", "RefreshToken", "Đăng xuất và thu hồi refresh token."),
    ("POST", "/auth/forgot-password", "Public", "Email", "Gửi yêu cầu khôi phục; luôn trả 200 để tránh dò email."),
    ("POST", "/auth/reset-password", "Public", "Email, Token, NewPassword", "Form đặt lại mật khẩu từ email."),
    ("POST", "/auth/change-password", "JWT", "CurrentPassword, NewPassword", "Đổi mật khẩu trong trang tài khoản."),
    ("GET", "/users/me", "JWT", "-", "Lấy hồ sơ user hiện tại."),
    ("PUT", "/users/me", "JWT", "FullName?, AvatarUrl?, DateOfBirth?", "Cập nhật hồ sơ cá nhân."),
    ("PUT", "/users/me/preferences", "JWT", "CurrencyCode?, MonthlyBudgetLimit?", "Cấu hình tiền tệ và hạn mức tháng."),
]

finance_wallet_rows = [
    ("GET", "/finance/api/wallets", "JWT", "-", "Danh sách ví cho dashboard và picker."),
    ("GET", "/finance/api/wallets/{id}", "JWT", "id", "Chi tiết ví."),
    ("POST", "/finance/api/wallets", "JWT", "UserId, Name, WalletType, Balance, Currency, IsActive", "Tạo ví."),
    ("PUT", "/finance/api/wallets/{id}", "JWT", "UserId, Name, WalletType, Balance, Currency, IsActive", "Sửa thông tin ví."),
    ("DELETE", "/finance/api/wallets/{id}", "JWT", "id", "Xóa ví; thành công trả 204."),
    ("GET", "/finance/api/tags", "JWT", "-", "Danh sách tag cho form transaction và bộ lọc."),
    ("GET", "/finance/api/tags/{id}", "JWT", "id", "Chi tiết tag."),
    ("POST", "/finance/api/tags", "JWT", "UserId, Name, Type, Icon, Color", "Tạo tag thu/chi."),
    ("PUT", "/finance/api/tags/{id}", "JWT", "UserId, Name, Type, Icon, Color", "Cập nhật tag."),
    ("DELETE", "/finance/api/tags/{id}", "JWT", "id", "Xóa tag; thành công trả 204."),
]

finance_transaction_rows = [
    ("POST", "/finance/api/wallets/{walletId}/transactions", "JWT", "TagId, Amount, Type, Title, Note, Source", "Tạo giao dịch; UserId và WalletId lấy từ JWT/path."),
    ("POST", "/finance/api/internal-transfers", "JWT", "UserId, FromWalletId, ToWalletId, Amount, Note, SyncStatus, TransferDate", "Chuyển tiền giữa hai ví."),
    ("GET", "/finance/api/recurring-transactions", "JWT", "-", "Danh sách giao dịch định kỳ."),
    ("POST", "/finance/api/recurring-transactions", "JWT", "WalletId, TagId, Amount, Type, Frequency, StartDate...", "Tạo khoản thu/chi định kỳ."),
    ("PUT", "/finance/api/recurring-transactions/{id}", "JWT", "Các trường recurring transaction", "Sửa lịch định kỳ."),
    ("DELETE", "/finance/api/recurring-transactions/{id}", "JWT", "id", "Xóa lịch định kỳ."),
    ("GET", "/finance/api/sync-queue", "JWT", "-", "Theo dõi hàng đợi đồng bộ; chủ yếu debug/offline."),
    ("POST", "/finance/api/sync-queue", "JWT", "UserId, EntityType, EntityId, Action, Payload", "Đưa thao tác offline vào queue."),
]

ai_rows = [
    ("GET", "/ai/health", "Public", "-", "Health trực tiếp qua prefix AI."),
    ("GET", "/ai/api/ai-processing/requests", "Public*", "userId?", "Lịch sử AI request; hiện controller chưa có Authorize."),
    ("GET", "/ai/api/ai-processing/requests/{id}", "Public*", "id", "Chi tiết request và result."),
    ("POST", "/ai/api/ai-processing/requests", "Public*", "UserId, InputType, RequestType, InputUrl?", "Tạo request thủ công; không dành cho UI production."),
    ("GET", "/ai/api/ai-processing/results/{requestId}", "Public*", "requestId", "Lấy kết quả AI."),
    ("POST", "/ai/api/ai-processing/results", "Public*", "RequestId, Amount, TransactionType, Tag, Date, RawResponse", "Ghi result thủ công; không dành cho UI production."),
    ("POST", "/ai/api/ai-processing/images/extract-text", "JWT", "multipart: Image; MockExtractedText?", "OCR và preview dữ liệu; ảnh tối đa 5 MB."),
    ("POST", "/ai/api/ai-processing/images/ocr", "JWT", "multipart: Image; MockExtractedText?", "Alias của extract-text."),
    ("POST", "/ai/api/ai-processing/images/transactions", "JWT", "multipart: WalletId, Image; MockExtractedText?", "Ảnh -> OCR -> Supabase -> tag -> Finance transaction."),
    ("POST", "/ai/api/ai-processing/voices/transactions", "JWT", "multipart: WalletId, Voice; MockTranscribedText?", "Voice -> text -> Supabase -> tag -> Finance transaction; tối đa 20 MB."),
]

budget_rows = [
    ("GET", "/analytics/budgets", "JWT", "-", "Danh sách ngân sách của user."),
    ("GET", "/analytics/budgets/{id}", "JWT", "id", "Chi tiết ngân sách."),
    ("POST", "/analytics/budgets", "JWT", "TagId?/TagName?, Name, PeriodType?, BudgetAmount, Threshold?, Currency?, StartDate, EndDate", "Tạo ngân sách."),
    ("PUT", "/analytics/budgets/{id}", "JWT", "Các trường budget + Status?", "Cập nhật ngân sách."),
    ("DELETE", "/analytics/budgets/{id}", "JWT", "id", "Xóa ngân sách."),
]

goal_rows = [
    ("GET", "/analytics/goals", "JWT", "-", "Danh sách mục tiêu tiết kiệm."),
    ("GET", "/analytics/goals/{id}", "JWT", "id", "Chi tiết mục tiêu."),
    ("POST", "/analytics/goals", "JWT", "Name, Description?, TargetAmount, CurrentAmount?, Currency?, TargetDate?, Priority?", "Tạo mục tiêu."),
    ("PUT", "/analytics/goals/{id}", "JWT", "Các trường goal + Status?", "Cập nhật mục tiêu."),
    ("POST", "/analytics/goals/{goalId}/progress", "JWT", "CurrentAmount", "Đặt tiến độ hiện tại."),
    ("DELETE", "/analytics/goals/{id}", "JWT", "id", "Xóa mục tiêu."),
    ("GET", "/analytics/goals/{goalId}/contributions", "JWT", "goalId", "Lịch sử đóng góp vào mục tiêu."),
    ("POST", "/analytics/goals/{goalId}/contributions", "JWT", "Amount, ContributionDate?, SourceType?, SourceReferenceId?, Note?", "Thêm khoản đóng góp."),
]

notification_rows = [
    ("GET", "/notifications/notifications", "JWT", "page, pageSize, isRead?, type?, channel?", "Danh sách thông báo phân trang và lọc."),
    ("GET", "/notifications/notifications/unread-count", "JWT", "-", "Badge số thông báo chưa đọc."),
    ("GET", "/notifications/notifications/{id}", "JWT", "id", "Chi tiết thông báo."),
    ("PATCH", "/notifications/notifications/{id}/read", "JWT", "id", "Đánh dấu một thông báo đã đọc."),
    ("PATCH", "/notifications/notifications/read-all", "JWT", "-", "Đánh dấu tất cả đã đọc."),
    ("DELETE", "/notifications/notifications/{id}", "JWT", "id", "Xóa thông báo."),
    ("POST", "/notifications/notifications/internal/send", "JWT*", "UserId, Title, Content?, Type, Channel, Priority?, TargetUrl?, Metadata?", "API nội bộ gửi thông báo; FE không nên gọi."),
]

device_rows = [
    ("POST", "/notifications/devices/push-tokens", "JWT", "DeviceFingerprint, Platform, PushToken, DeviceName?, OsVersion?, AppVersion?", "Đăng ký push token sau login/refresh token."),
    ("POST", "/notifications/devices/sync", "JWT", "DeviceFingerprint, LastSyncedAt", "Đồng bộ notification bị bỏ lỡ khi thiết bị online lại."),
    ("DELETE", "/notifications/devices/push-tokens/{deviceFingerprint}", "JWT", "deviceFingerprint", "Hủy push token; hiện controller trả 204 nhưng logic là stub."),
    ("GET", "/notifications/notification-settings", "JWT", "-", "Lấy cài đặt thông báo."),
    ("PUT", "/notifications/notification-settings", "JWT", "EnableEmail/Push/InApp/BudgetWarning/TransactionAlert/SystemAlert?, QuietHours?", "Cập nhật tùy chọn thông báo."),
]

ws_rows = [
    ("CONNECT", "/ws/realtime", "JWT*", "access_token khi kết nối SignalR", "Kết nối realtime qua Gateway; direct URL là :5106/realtime."),
    ("INVOKE", "SubscribeUser(userId)", "JWT*", "userId", "Join group user:{userId}. Nên tự lấy user từ JWT ở backend."),
    ("EVENT", "connected", "-", "connectionId", "Server gửi ngay sau khi client kết nối."),
    ("EVENT", "notification.received", "-", "notification payload", "UI cập nhật badge/list/toast realtime."),
    ("INVOKE", "BroadcastNotification(userId,payload)", "JWT*", "userId, payload", "Không cho FE gọi; cần giới hạn cho backend/internal."),
]


doc = SimpleDocTemplate(str(OUTPUT), pagesize=PAGE, leftMargin=MX, rightMargin=MX, topMargin=MT, bottomMargin=MB, title="FlowFi All Services API Catalog for UI", author="FlowFi Backend Team")

story = [
    p("FLOWFI - FRONTEND API CATALOG", title),
    p("Danh sách API toàn hệ thống để chuẩn bị Web/Mobile UI | Ngày 20/06/2026 | Branch main", subtitle),
    p("1. Service map", section),
]

service_data = [[p("Service", header), p("Direct URL", header), p("Gateway prefix", header), p("Vai trò", header)]]
for name, direct, prefix, desc in services:
    service_data.append([p(name, small), p(direct, endpoint), p(prefix, endpoint), p(desc, small)])
service_table = Table(service_data, colWidths=[1.48*inch, 1.62*inch, 1.40*inch, 6.05*inch], repeatRows=1)
service_table.setStyle(TableStyle([
    ("BACKGROUND", (0,0), (-1,0), colors.HexColor("#111827")),
    ("GRID", (0,0), (-1,-1), 0.35, colors.HexColor("#CCD0D5")),
    ("ROWBACKGROUNDS", (0,1), (-1,-1), [colors.white, colors.HexColor("#F8F9FA")]),
    ("VALIGN", (0,0), (-1,-1), "TOP"),
    ("LEFTPADDING", (0,0), (-1,-1), 6), ("RIGHTPADDING", (0,0), (-1,-1), 6),
    ("TOPPADDING", (0,0), (-1,-1), 6), ("BOTTOMPADDING", (0,0), (-1,-1), 6),
]))
story.extend([
    service_table,
    Spacer(1, 10),
    p("2. Quy ước tích hợp UI", section),
    p("<b>Base URL:</b> UI chỉ gọi API Gateway tại <font name='Consolas'>http://localhost:8080</font>. Direct URL chỉ dùng debug Swagger.<br/>"
      "<b>JWT:</b> Gửi <font name='Consolas'>Authorization: Bearer &lt;access_token&gt;</font>. Khi 401, thử refresh một lần rồi logout nếu refresh thất bại.<br/>"
      "<b>Date/time:</b> DateOnly dùng YYYY-MM-DD; DateTimeOffset dùng ISO-8601 có timezone.<br/>"
      "<b>Money:</b> Gửi số decimal, không gửi chuỗi đã format; UI format theo CurrencyCode.<br/>"
      "<b>Uploads:</b> Dùng multipart/form-data; không gửi MockExtractedText/MockTranscribedText ở production.<br/>"
      "<b>Response:</b> Auth dùng ApiResponse wrapper; các service còn lại chủ yếu trả DTO trực tiếp.", info),
    Spacer(1, 9),
    p("Điểm cần chốt trước khi dựng UI", section),
    p("- Finance chưa có GET transaction list/detail, update hoặc delete transaction.<br/>"
      "- Nhiều Finance DTO vẫn yêu cầu UserId trong body dù endpoint có JWT; cần thống nhất trước khi FE khóa contract.<br/>"
      "- AI request/result đang public; không dùng các POST thủ công trong UI production.<br/>"
      "- Notification unregister device là stub. WebSocket BroadcastNotification không được phép mở cho client.<br/>"
      "- Gateway notification tạo đường dẫn lặp: /notifications/notifications.", warning),
    PageBreak(),
])

story += service_page("Auth User Service", "Prefix Gateway: /auth và /users | Direct Swagger: http://localhost:5101/swagger | Auth response dùng ApiResponse<T>.", [("Authentication & profile", auth_rows)], "#1D4ED8")
story.append(PageBreak())
story += service_page("Finance Core Service - Wallets & Tags", "Prefix Gateway: /finance | Tất cả API chức năng yêu cầu JWT.", [("Wallets và Tags", finance_wallet_rows)], "#047857")
story.append(PageBreak())
story += service_page("Finance Core Service - Transactions & Sync", "Transaction create lấy UserId từ JWT; các DTO còn lại vẫn chứa UserId và cần backend review.", [("Transactions, recurring và offline sync", finance_transaction_rows)], "#047857")
story.extend([Spacer(1, 8), p("Khoảng trống API cho UI Finance", section), p("Hiện chưa có endpoint GET danh sách/chi tiết transaction, PUT/PATCH transaction và DELETE transaction. Vì vậy màn hình lịch sử giao dịch, chỉnh sửa và xóa chưa thể tích hợp đầy đủ chỉ từ REST contract hiện tại.", warning), PageBreak()])
story += service_page("AI Processing Service", "Prefix Gateway: /ai | Image/voice yêu cầu JWT; file lưu Supabase và transaction được tạo qua Finance gRPC.", [("AI requests, OCR và voice", ai_rows)], "#6D28D9")
story.append(PageBreak())
story += service_page("Analytics Service", "Prefix Gateway: /analytics | UserId luôn lấy từ JWT.", [("Budgets", budget_rows), ("Saving goals & contributions", goal_rows)], "#B45309")
story.append(PageBreak())
story += service_page("Notification Service", "Prefix Gateway: /notifications | Vì controller cũng có route notifications, list API dùng /notifications/notifications.", [("Notifications", notification_rows), ("Devices & settings", device_rows)], "#BE123C")
story.append(PageBreak())
story += service_page("WebSocket Gateway", "Gateway URL: ws/http://localhost:8080/ws/realtime | SignalR hub direct: http://localhost:5106/realtime.", [("SignalR methods & events", ws_rows)], "#0F766E")
story.extend([
    Spacer(1, 10),
    p("Luồng UI realtime đề xuất", section),
    p("1. Login thành công -> lưu token -> đăng ký push token.<br/>"
      "2. Kết nối SignalR với access token -> lắng nghe connected và notification.received.<br/>"
      "3. Khi app foreground: cập nhật badge/list trực tiếp từ event.<br/>"
      "4. Khi reconnect: gọi devices/sync với LastSyncedAt để lấy dữ liệu bị bỏ lỡ.<br/>"
      "5. Logout: hủy SignalR, gọi logout và unregister push token sau khi backend hoàn thiện logic.", info),
    Spacer(1, 10),
    p("Security blockers", section),
    p("Hub hiện chưa gọi RequireAuthorization và method SubscribeUser nhận userId từ client. BroadcastNotification cũng là public hub method. Trước production cần bắt buộc JWT, lấy userId từ claims, và chuyển broadcast thành backend-only mechanism.", warning),
    PageBreak(),
    p("UI IMPLEMENTATION CHECKLIST", title),
    p("Checklist dùng khi chia task Web/Mobile", subtitle),
    Table([
        [
            p("<b>Authentication</b><br/><br/>[ ] Register/login/refresh/logout interceptor<br/>[ ] Secure token storage<br/>[ ] Route guard và xử lý 401<br/>[ ] Forgot/reset/change password<br/>[ ] Profile và preferences", body),
            p("<b>Finance</b><br/><br/>[ ] Wallet list/detail/create/edit/delete<br/>[ ] Tag picker và color/icon swatch<br/>[ ] Create transaction<br/>[ ] Internal transfer<br/>[ ] Recurring transaction<br/>[ ] Chờ backend bổ sung history/update/delete", body),
        ],
        [
            p("<b>AI & Analytics</b><br/><br/>[ ] Image picker/camera + upload progress<br/>[ ] OCR preview<br/>[ ] Image-to-transaction result<br/>[ ] Voice recorder + upload/progress<br/>[ ] Budget CRUD<br/>[ ] Goal CRUD, progress và contributions", body),
            p("<b>Notifications & realtime</b><br/><br/>[ ] Notification paged list/filter<br/>[ ] Unread badge/read/read-all/delete<br/>[ ] Settings & quiet hours<br/>[ ] Push token lifecycle<br/>[ ] SignalR reconnect + sync fallback", body),
        ],
        [
            p("<b>Common UI states</b><br/><br/>[ ] Loading/skeleton<br/>[ ] Empty state<br/>[ ] Inline validation 400<br/>[ ] Unauthorized 401<br/>[ ] Not found 404", body),
            p("<b>Error & resilience</b><br/><br/>[ ] Conflict 409<br/>[ ] AI validation 422<br/>[ ] gRPC/upstream 502<br/>[ ] Retry action<br/>[ ] Offline state", body),
        ],
    ], colWidths=[5.17*inch, 5.17*inch], style=[
        ("GRID", (0,0), (-1,-1), 0.5, colors.HexColor("#C7CDD3")),
        ("VALIGN", (0,0), (-1,-1), "TOP"),
        ("BACKGROUND", (0,0), (-1,-1), colors.HexColor("#F8F9FA")),
        ("LEFTPADDING", (0,0), (-1,-1), 10), ("RIGHTPADDING", (0,0), (-1,-1), 10),
        ("TOPPADDING", (0,0), (-1,-1), 9), ("BOTTOMPADDING", (0,0), (-1,-1), 9),
    ]),
    Spacer(1, 9),
    p("Swagger nhanh", section),
    p("Gateway docs: /docs/auth/swagger/index.html, /docs/finance/swagger/index.html, /docs/ai/swagger/index.html, /docs/analytics/swagger/index.html, /docs/notifications/swagger/index.html.<br/>Direct Swagger: ports 5101-5105 tương ứng từng service.", info),
])

doc.build(story, onFirstPage=footer, onLaterPages=footer)
print(OUTPUT)
