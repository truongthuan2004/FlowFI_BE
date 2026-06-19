CREATE EXTENSION IF NOT EXISTS pgcrypto;

DROP TABLE IF EXISTS ai_processing_result;
DROP TABLE IF EXISTS ai_processing_request;

CREATE TABLE ai_processing_request (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    input_type VARCHAR(20) NOT NULL,
    request_type VARCHAR(50) NOT NULL,
    input_url TEXT,
    status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    completed_at TIMESTAMP,
    CONSTRAINT chk_ai_request_input_type CHECK (input_type IN ('AUDIO', 'IMAGE', 'TEXT')),
    CONSTRAINT chk_ai_request_type CHECK (request_type IN ('VOICE_TO_TEXT', 'OCR', 'SPENDING_ANALYSIS')),
    CONSTRAINT chk_ai_request_status CHECK (status IN ('PENDING', 'PROCESSING', 'COMPLETED', 'FAILED'))
);

CREATE TABLE ai_processing_result (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    request_id UUID NOT NULL UNIQUE,
    amount DECIMAL(18, 2),
    transaction_type VARCHAR(20),
    tag VARCHAR(50),
    transaction_date TIMESTAMP,
    raw_response TEXT,
    CONSTRAINT fk_ai_result_request
        FOREIGN KEY (request_id)
        REFERENCES ai_processing_request(id)
        ON DELETE CASCADE,
    CONSTRAINT chk_ai_result_transaction_type CHECK (transaction_type IN ('INCOME', 'EXPENSE')),
    CONSTRAINT chk_ai_result_tag CHECK (tag IN ('FOOD', 'TRANSPORT', 'SHOPPING', 'EDUCATION'))
);

INSERT INTO ai_processing_request
(
    id,
    user_id,
    input_type,
    request_type,
    input_url,
    status,
    created_at,
    completed_at
)
VALUES
(
    '11111111-1111-1111-1111-111111111111',
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    'AUDIO',
    'VOICE_TO_TEXT',
    'https://storage.flowfi.com/audio/audio-001.mp3',
    'COMPLETED',
    '2026-06-16 08:00:00',
    '2026-06-16 08:00:10'
),
(
    '22222222-2222-2222-2222-222222222222',
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
    'IMAGE',
    'OCR',
    'https://storage.flowfi.com/images/bill-001.jpg',
    'COMPLETED',
    '2026-06-16 09:00:00',
    '2026-06-16 09:00:15'
),
(
    '33333333-3333-3333-3333-333333333333',
    'cccccccc-cccc-cccc-cccc-cccccccccccc',
    'TEXT',
    'SPENDING_ANALYSIS',
    NULL,
    'COMPLETED',
    '2026-06-16 10:00:00',
    '2026-06-16 10:00:20'
);

INSERT INTO ai_processing_result
(
    id,
    request_id,
    amount,
    transaction_type,
    tag,
    transaction_date,
    raw_response
)
VALUES
(
    'aaaa1111-aaaa-aaaa-aaaa-aaaaaaaa1111',
    '11111111-1111-1111-1111-111111111111',
    50000,
    'EXPENSE',
    'FOOD',
    '2026-06-16 07:30:00',
    'Nguoi dung noi: chi 50 nghin an sang. AI phan tich: so tien 50000, loai giao dich EXPENSE, tag FOOD.'
),
(
    'bbbb2222-bbbb-bbbb-bbbb-bbbbbbbb2222',
    '22222222-2222-2222-2222-222222222222',
    120000,
    'EXPENSE',
    'SHOPPING',
    '2026-06-16 08:45:00',
    'OCR hoa don thanh cong. AI phat hien tong tien 120000, loai giao dich EXPENSE, tag SHOPPING.'
),
(
    'cccc3333-cccc-cccc-cccc-cccccccc3333',
    '33333333-3333-3333-3333-333333333333',
    200000,
    'EXPENSE',
    'TRANSPORT',
    '2026-06-16 09:30:00',
    'Nguoi dung nhap: hom nay ton 200k tien xang xe. AI phan tich: amount 200000, type EXPENSE, tag TRANSPORT.'
);
