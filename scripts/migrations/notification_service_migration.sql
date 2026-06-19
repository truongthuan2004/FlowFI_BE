-- ============================================
-- FlowFi Notification Service - Database Migration
-- Run this script on PostgreSQL
-- ============================================

-- Bật extension
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- Xóa bảng cũ (nếu có)
DROP TABLE IF EXISTS notifications CASCADE;
DROP TABLE IF EXISTS notification_settings CASCADE;

-- ============================================
-- TABLE: NOTIFICATION_SETTINGS
-- ============================================
CREATE TABLE notification_settings (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL UNIQUE,
    enable_email BOOLEAN NOT NULL DEFAULT TRUE,
    enable_push BOOLEAN NOT NULL DEFAULT TRUE,
    enable_budget_warning BOOLEAN NOT NULL DEFAULT TRUE,
    updated_at TIMESTAMP
);

CREATE INDEX idx_notification_settings_user_id ON notification_settings(user_id);

-- ============================================
-- TABLE: NOTIFICATIONS
-- ============================================
CREATE TABLE notifications (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    title VARCHAR(255) NOT NULL,
    content TEXT,
    notification_type VARCHAR(50) NOT NULL CHECK (notification_type IN ('BUDGET_WARNING', 'SYSTEM', 'TRANSACTION')),
    is_read BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP
);

CREATE INDEX idx_notifications_user_id ON notifications(user_id);
CREATE INDEX idx_notifications_notification_type ON notifications(notification_type);
CREATE INDEX idx_notifications_is_read ON notifications(is_read);
CREATE INDEX idx_notifications_created_at ON notifications(created_at);

-- ============================================
-- INSERT MẪU
-- ============================================
INSERT INTO notification_settings (id, user_id, enable_email, enable_push, enable_budget_warning, updated_at)
VALUES
    ('77777777-7777-7777-7777-777777777777', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', TRUE, TRUE, TRUE, '2026-06-16 17:00:00'),
    ('88888888-8888-8888-8888-888888888888', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', TRUE, FALSE, FALSE, '2026-06-16 17:05:00');

INSERT INTO notifications (id, user_id, title, content, notification_type, is_read, created_at)
VALUES
    ('99999999-9999-9999-9999-999999999999', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Cảnh báo ngân sách', 'Hạng mục Ăn uống của bạn đã sử dụng vượt quá 80% giới hạn tháng.', 'BUDGET_WARNING', FALSE, '2026-06-16 17:10:00'),
    ('00000000-0000-0000-0000-000000000000', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'Biến động số dư', 'Ví Tiền mặt: +5,000,000 VND từ Thu nhập lương.', 'TRANSACTION', TRUE, '2026-06-16 17:15:00');
