-- ============================================
-- DROP ALL TABLES & RECREATE
-- Chạy script này trên database: FlowFi_AuthUser
-- ============================================

-- Drop theo thứ tự: bảng con trước, bảng cha sau (do foreign key)
DROP TABLE IF EXISTS user_logs CASCADE;
DROP TABLE IF EXISTS user_devices CASCADE;
DROP TABLE IF EXISTS password_reset_tokens CASCADE;
DROP TABLE IF EXISTS refresh_tokens CASCADE;
DROP TABLE IF EXISTS users CASCADE;

-- Bật extension
CREATE EXTENSION IF NOT EXISTS pgcrypto;

-- ============================================
-- TABLE: USERS
-- ============================================
CREATE TABLE users (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    email VARCHAR(255) NOT NULL UNIQUE,
    password_hash TEXT,
    full_name VARCHAR(255),
    avatar_url TEXT,
    date_of_birth DATE,
    currency_code VARCHAR(10) NOT NULL DEFAULT 'VND',
    monthly_budget_limit DECIMAL(18, 2),
    auth_provider VARCHAR(20) NOT NULL DEFAULT 'LOCAL',
    is_verified BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    updated_at TIMESTAMPTZ,
    deleted_at TIMESTAMPTZ
);

CREATE INDEX idx_users_email ON users(email);
CREATE INDEX idx_users_auth_provider ON users(auth_provider);

-- ============================================
-- TABLE: REFRESH_TOKENS
-- ============================================
CREATE TABLE refresh_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    token TEXT NOT NULL,
    expires_at TIMESTAMP NOT NULL,
    is_revoked BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_refresh_tokens_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX idx_refresh_tokens_token ON refresh_tokens(token);
CREATE INDEX idx_refresh_tokens_user_id ON refresh_tokens(user_id);

-- ============================================
-- TABLE: PASSWORD_RESET_TOKENS
-- ============================================
CREATE TABLE password_reset_tokens (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    token VARCHAR(255) NOT NULL,
    otp_code VARCHAR(10),
    expires_at TIMESTAMP NOT NULL,
    is_used BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_password_reset_tokens_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX idx_password_reset_tokens_token ON password_reset_tokens(token);
CREATE INDEX idx_password_reset_tokens_user_id ON password_reset_tokens(user_id);

-- ============================================
-- TABLE: USER_DEVICES
-- ============================================
CREATE TABLE user_devices (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    device_id VARCHAR(255) NOT NULL,
    device_name VARCHAR(255),
    platform VARCHAR(50) CHECK (platform IN ('ANDROID', 'IOS')),
    push_token TEXT,
    last_synced_at TIMESTAMP,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_user_devices_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX idx_user_devices_user_id ON user_devices(user_id);
CREATE INDEX idx_user_devices_device_id ON user_devices(device_id);

-- ============================================
-- TABLE: USER_LOGS
-- ============================================
CREATE TABLE user_logs (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL,
    action VARCHAR(100) NOT NULL CHECK (action IN ('REGISTER', 'LOGIN', 'LOGOUT', 'CHANGE_PASSWORD', 'UPDATE_PROFILE', 'FORGOT_PASSWORD', 'RESET_PASSWORD')),
    status VARCHAR(20) NOT NULL CHECK (status IN ('SUCCESS', 'FAILED')),
    ip_address VARCHAR(45),
    user_agent TEXT,
    failure_reason TEXT,
    created_at TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT fk_user_logs_users FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX idx_user_logs_user_id ON user_logs(user_id);
CREATE INDEX idx_user_logs_action ON user_logs(action);
CREATE INDEX idx_user_logs_created_at ON user_logs(created_at);

-- ============================================
-- INSERT MẪU
-- ============================================
INSERT INTO users (id, email, password_hash, full_name, currency_code, monthly_budget_limit, auth_provider, is_verified)
VALUES
    ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'nguyenvana@gmail.com', '$2b$12$LQv3c1yqBWVHxkd0LHAkCOYz6TtxMQJqhN8/X4.UFMpA8E1P5DXLK', 'Nguyen Van A', 'VND', 5000000, 'LOCAL', TRUE),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'tranvanb@gmail.com', NULL, 'Tran Van B', 'USD', 500, 'GOOGLE', TRUE);

INSERT INTO refresh_tokens (id, user_id, token, expires_at, is_revoked)
VALUES
    ('11111111-1111-1111-1111-111111111111', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'refresh_token_example_abc123...', '2026-07-19 23:59:59', FALSE);

INSERT INTO password_reset_tokens (id, user_id, token, otp_code, expires_at, is_used)
VALUES
    ('22222222-2222-2222-2222-222222222222', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'reset_token_xyz', '123456', '2026-06-20 23:59:59', FALSE);

INSERT INTO user_devices (id, user_id, device_id, device_name, platform, push_token)
VALUES
    ('33333333-3333-3333-3333-333333333333', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'device_iphone_15', 'iPhone 15 Pro', 'IOS', 'apns_token_here'),
    ('44444444-4444-4444-4444-444444444444', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', 'device_samsung_s24', 'Samsung S24', 'ANDROID', 'fcm_token_here');

INSERT INTO user_logs (id, user_id, action, status, ip_address, user_agent, failure_reason)
VALUES
    ('55555555-5555-5555-5555-555555555555', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'LOGIN', 'SUCCESS', '192.168.1.1', 'Mozilla/5.0', NULL),
    ('66666666-6666-6666-6666-666666666666', 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', 'LOGIN', 'FAILED', '192.168.1.1', 'Mozilla/5.0', 'WRONG_PASSWORD');

-- Done!
SELECT 'Reset complete!' as status;