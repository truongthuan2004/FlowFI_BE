ALTER TABLE ai_processing_result
    DROP CONSTRAINT IF EXISTS chk_ai_result_tag;

ALTER TABLE ai_processing_result
    ADD CONSTRAINT chk_ai_result_tag CHECK (
        tag IN ('FOOD', 'TRANSPORT', 'SHOPPING', 'EDUCATION', 'TRANSFER', 'SALARY', 'UTILITIES', 'HEALTH')
    );
