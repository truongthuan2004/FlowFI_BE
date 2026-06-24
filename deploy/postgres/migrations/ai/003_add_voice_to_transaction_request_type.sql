ALTER TABLE ai_processing_request
    DROP CONSTRAINT IF EXISTS chk_ai_request_type;

ALTER TABLE ai_processing_request
    ADD CONSTRAINT chk_ai_request_type
    CHECK (request_type IN ('VOICE_TO_TEXT', 'VOICE_TO_TRANSACTION', 'OCR', 'SPENDING_ANALYSIS'));
