from pathlib import Path

from reportlab.lib import colors
from reportlab.lib.enums import TA_CENTER, TA_LEFT
from reportlab.lib.pagesizes import letter
from reportlab.lib.styles import ParagraphStyle, getSampleStyleSheet
from reportlab.lib.units import inch
from reportlab.pdfbase import pdfmetrics
from reportlab.pdfbase.ttfonts import TTFont
from reportlab.platypus import (
    KeepTogether,
    PageBreak,
    Paragraph,
    SimpleDocTemplate,
    Spacer,
    Table,
    TableStyle,
)


ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "output" / "pdf" / "FlowFi_AIProcessing_API_Checklist_Report.pdf"
OUTPUT.parent.mkdir(parents=True, exist_ok=True)

font_dir = Path(r"C:\Windows\Fonts")
pdfmetrics.registerFont(TTFont("Arial", str(font_dir / "arial.ttf")))
pdfmetrics.registerFont(TTFont("Arial-Bold", str(font_dir / "arialbd.ttf")))
pdfmetrics.registerFont(TTFont("Consolas", str(font_dir / "consola.ttf")))

PAGE_WIDTH, PAGE_HEIGHT = letter
MARGIN_X = 0.72 * inch
MARGIN_TOP = 0.72 * inch
MARGIN_BOTTOM = 0.62 * inch


def footer(canvas, doc):
    canvas.saveState()
    canvas.setStrokeColor(colors.HexColor("#D0D0D0"))
    canvas.setLineWidth(0.5)
    canvas.line(MARGIN_X, 0.47 * inch, PAGE_WIDTH - MARGIN_X, 0.47 * inch)
    canvas.setFont("Arial", 8)
    canvas.setFillColor(colors.HexColor("#666666"))
    canvas.drawString(MARGIN_X, 0.28 * inch, "FlowFi - AI Processing Service")
    canvas.drawRightString(PAGE_WIDTH - MARGIN_X, 0.28 * inch, str(doc.page))
    canvas.restoreState()


styles = getSampleStyleSheet()
title = ParagraphStyle(
    "Title",
    parent=styles["Title"],
    fontName="Arial-Bold",
    fontSize=20,
    leading=24,
    alignment=TA_LEFT,
    textColor=colors.black,
    spaceAfter=14,
)
section = ParagraphStyle(
    "Section",
    parent=styles["Heading2"],
    fontName="Arial-Bold",
    fontSize=14,
    leading=18,
    textColor=colors.black,
    spaceBefore=5,
    spaceAfter=9,
)
body = ParagraphStyle(
    "Body",
    parent=styles["BodyText"],
    fontName="Arial",
    fontSize=9.2,
    leading=13,
    textColor=colors.HexColor("#202020"),
)
small = ParagraphStyle(
    "Small",
    parent=body,
    fontSize=8.3,
    leading=11.5,
)
label = ParagraphStyle(
    "Label",
    parent=body,
    fontName="Arial-Bold",
)
table_header = ParagraphStyle(
    "TableHeader",
    parent=small,
    fontName="Arial-Bold",
    alignment=TA_LEFT,
)
endpoint = ParagraphStyle(
    "Endpoint",
    parent=small,
    fontName="Consolas",
    fontSize=7.5,
    leading=10,
    backColor=colors.HexColor("#F1F1F1"),
    borderColor=colors.HexColor("#D6D6D6"),
    borderWidth=0.5,
    borderPadding=(2, 3, 2, 3),
    borderRadius=2,
)
status_done = ParagraphStyle(
    "StatusDone",
    parent=small,
    fontName="Arial-Bold",
    textColor=colors.HexColor("#1B6E3A"),
)
status_test = ParagraphStyle(
    "StatusTest",
    parent=small,
    fontName="Arial-Bold",
    textColor=colors.HexColor("#9A5A00"),
)
callout = ParagraphStyle(
    "Callout",
    parent=body,
    fontSize=8.8,
    leading=12.5,
    leftIndent=8,
    rightIndent=8,
    borderColor=colors.HexColor("#B9B9B9"),
    borderWidth=0.6,
    borderPadding=8,
    backColor=colors.HexColor("#F7F7F7"),
)


def p(text, style=body):
    return Paragraph(text, style)


def info_table():
    data = [
        [p("Service:", label), p("FlowFi.AIProcessingService")],
        [p("Người thực hiện:", label), p("FlowFi Backend Team")],
        [p("Ngày báo cáo:", label), p("20/06/2026")],
        [p("Branch / Build:", label), p("main / Build succeeded - 0 warnings, 0 errors")],
        [p("Base URL:", label), p("http://localhost:5103")],
    ]
    table = Table(data, colWidths=[1.28 * inch, 5.35 * inch], hAlign="LEFT")
    table.setStyle(
        TableStyle(
            [
                ("VALIGN", (0, 0), (-1, -1), "TOP"),
                ("LEFTPADDING", (0, 0), (-1, -1), 0),
                ("RIGHTPADDING", (0, 0), (-1, -1), 4),
                ("TOPPADDING", (0, 0), (-1, -1), 2),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 2),
            ]
        )
    )
    return table


api_rows = [
    ("1", "Health check", "/health", "GET", "Kiểm tra trạng thái service.", "Done"),
    ("2", "Danh sách AI request", "/api/ai-processing/requests", "GET", "Lọc tùy chọn theo userId.", "Done"),
    ("3", "Chi tiết AI request", "/api/ai-processing/requests/{id}", "GET", "Trả request cùng processing result.", "Done"),
    ("4", "Tạo AI request", "/api/ai-processing/requests", "POST", "Tạo request PENDING thủ công.", "Done"),
    ("5", "Lấy processing result", "/api/ai-processing/results/{requestId}", "GET", "Lấy kết quả theo requestId.", "Done"),
    ("6", "Tạo processing result", "/api/ai-processing/results", "POST", "Ghi kết quả AI thủ công.", "Done"),
    ("7", "OCR / extract text", "/api/ai-processing/images/extract-text", "POST", "Đọc ảnh, parse dữ liệu, lưu Supabase.", "Needs E2E"),
    ("8", "OCR alias", "/api/ai-processing/images/ocr", "POST", "Alias của extract-text.", "Needs E2E"),
    ("9", "Ảnh thành transaction", "/api/ai-processing/images/transactions", "POST", "OCR, tag, gRPC tạo transaction.", "Needs E2E"),
    ("10", "Voice thành transaction", "/api/ai-processing/voices/transactions", "POST", "Speech-to-text, Supabase, tag, gRPC.", "Needs E2E"),
]


def api_table():
    data = [[
        p("STT", table_header),
        p("Chức năng", table_header),
        p("API", table_header),
        p("Method", table_header),
        p("Mô tả ngắn", table_header),
        p("Trạng thái", table_header),
    ]]
    for number, feature, api, method, description, state in api_rows:
        state_style = status_done if state == "Done" else status_test
        data.append([
            p(number, small),
            p(feature, small),
            p(api, endpoint),
            p(method, small),
            p(description, small),
            p(state, state_style),
        ])

    widths = [0.38 * inch, 1.18 * inch, 2.00 * inch, 0.58 * inch, 1.50 * inch, 0.82 * inch]
    table = Table(data, colWidths=widths, repeatRows=1, hAlign="LEFT")
    table.setStyle(
        TableStyle(
            [
                ("VALIGN", (0, 0), (-1, -1), "MIDDLE"),
                ("LINEBELOW", (0, 0), (-1, 0), 0.8, colors.black),
                ("LINEBELOW", (0, 1), (-1, -1), 0.35, colors.HexColor("#C8C8C8")),
                ("LEFTPADDING", (0, 0), (-1, -1), 4),
                ("RIGHTPADDING", (0, 0), (-1, -1), 4),
                ("TOPPADDING", (0, 0), (-1, 0), 6),
                ("BOTTOMPADDING", (0, 0), (-1, 0), 6),
                ("TOPPADDING", (0, 1), (-1, -1), 7),
                ("BOTTOMPADDING", (0, 1), (-1, -1), 7),
            ]
        )
    )
    return table


def checklist_table():
    rows = [
        ("Controller -> Service -> Repository -> DbContext", "Đạt", "UnitOfWork commit tại Service."),
        ("JWT cho API ảnh và voice", "Đạt", "UserId lấy từ access token."),
        ("Validate file upload", "Đạt", "Ảnh tối đa 5 MB; voice tối đa 20 MB."),
        ("Supabase Storage", "Đạt", "Bucket Image; thư mục images/ và voices/."),
        ("AI provider", "Cần E2E", "OCR và input_audio qua Responses API."),
        ("Finance gRPC", "Cần E2E", "Tìm/tạo tag và tạo transaction."),
        ("PostgreSQL", "Đạt", "Migration 002 mở rộng tag đã áp dụng local."),
        ("Swagger", "Đạt", "Multipart form và response 201 được khai báo."),
        ("Global error handling/logging", "Đạt", "Correlation ID và request logging."),
    ]
    data = [[p("Hạng mục", table_header), p("Kết quả", table_header), p("Ghi chú", table_header)]]
    for name, result, note in rows:
        style = status_done if result == "Đạt" else status_test
        data.append([p(name, small), p(result, style), p(note, small)])
    table = Table(data, colWidths=[2.55 * inch, 0.92 * inch, 3.12 * inch], repeatRows=1, hAlign="LEFT")
    table.setStyle(
        TableStyle(
            [
                ("VALIGN", (0, 0), (-1, -1), "TOP"),
                ("LINEBELOW", (0, 0), (-1, 0), 0.8, colors.black),
                ("LINEBELOW", (0, 1), (-1, -1), 0.35, colors.HexColor("#C8C8C8")),
                ("LEFTPADDING", (0, 0), (-1, -1), 5),
                ("RIGHTPADDING", (0, 0), (-1, -1), 5),
                ("TOPPADDING", (0, 0), (-1, -1), 6),
                ("BOTTOMPADDING", (0, 0), (-1, -1), 6),
            ]
        )
    )
    return table


doc = SimpleDocTemplate(
    str(OUTPUT),
    pagesize=letter,
    rightMargin=MARGIN_X,
    leftMargin=MARGIN_X,
    topMargin=MARGIN_TOP,
    bottomMargin=MARGIN_BOTTOM,
    title="FlowFi AI Processing API Checklist Report",
    author="FlowFi Backend Team",
)

story = [
    p("BACKEND API CHECKLIST REPORT", title),
    p("Thông tin chung", section),
    info_table(),
    Spacer(1, 11),
    Table([[""]], colWidths=[6.65 * inch], rowHeights=[0.01 * inch], style=[("LINEBELOW", (0, 0), (-1, -1), 0.7, colors.HexColor("#AFAFAF"))]),
    Spacer(1, 13),
    p("Checklist chức năng & API", section),
    api_table(),
    PageBreak(),
    p("CHECKLIST KỸ THUẬT", title),
    p("Tổng quan triển khai", section),
    p(
        "AI Processing hỗ trợ OCR ảnh và xử lý voice thành giao dịch. File được lưu trên Supabase, "
        "kết quả phân tích được lưu PostgreSQL, sau đó service gọi Finance Core qua gRPC để tái sử dụng "
        "tag hoặc tạo tag mới và ghi transaction.",
        body,
    ),
    Spacer(1, 10),
    checklist_table(),
    Spacer(1, 9),
    p("Ghi chú kiểm thử", section),
    p(
        "<b>API đã test trên Swagger/Postman:</b> Cần chạy E2E lại sau khi restart AI Service.<br/>"
        "<b>Database:</b> Kết nối local port 6000; migration 002 đã áp dụng.<br/>"
        "<b>Supabase:</b> Kiểm tra file xuất hiện trong bucket Image và URL được ghi vào input_url.<br/>"
        "<b>Finance:</b> Finance Core gRPC phải chạy tại địa chỉ cấu hình trước khi test transaction.<br/>"
        "<b>AI voice:</b> Xác nhận provider hỗ trợ input_audio cho model đang cấu hình.",
        callout,
    ),
    Spacer(1, 9),
    p("Vấn đề cần xử lý", section),
    p(
        "• <b>Authorization:</b> AiProcessingRequestsController và AiProcessingResultsController hiện chưa có [Authorize].<br/>"
        "• <b>Security:</b> Secret key đã từng được chia sẻ dạng plaintext; cần rotate trước khi deploy.<br/>"
        "• <b>Storage:</b> Bucket phải Public nếu frontend sử dụng public URL; nếu Private cần signed URL.<br/>"
        "• <b>Automated tests:</b> Chưa có integration test bao phủ AI provider, Supabase và Finance gRPC.<br/>"
        "• <b>Migration runner:</b> 001_init.sql có DROP TABLE; không chạy lại trên database có dữ liệu.",
        body,
    ),
    Spacer(1, 9),
    p("Tiêu chí nghiệm thu", section),
    p(
        "□ JWT hợp lệ và WalletId thuộc user hiện tại.<br/>"
        "□ Ảnh/voice upload thành công và URL mở được.<br/>"
        "□ AI trả amount, transactionType và tag hợp lệ.<br/>"
        "□ Tag được tái sử dụng hoặc tạo đúng loại giao dịch.<br/>"
        "□ Transaction xuất hiện trong Finance Core và API trả HTTP 201.<br/>"
        "□ Trường hợp lỗi trả 400/401/422/502 đúng contract.",
        body,
    ),
]

doc.build(story, onFirstPage=footer, onLaterPages=footer)
print(OUTPUT)
