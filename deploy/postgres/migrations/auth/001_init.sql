CREATE EXTENSION IF NOT EXISTS pgcrypto;

DROP TABLE IF EXISTS user_logs CASCADE;
DROP TABLE IF EXISTS user_sessions CASCADE;
DROP TABLE IF EXISTS user_devices CASCADE;
DROP TABLE IF EXISTS password_reset_tokens CASCADE;
DROP TABLE IF EXISTS refresh_tokens CASCADE;
DROP TABLE IF EXISTS user_auth_identities CASCADE;
DROP TABLE IF EXISTS users CASCADE;

CREATE TABLE users (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  email VARCHAR(255) NOT NULL,
  password_hash TEXT NULL,
  full_name VARCHAR(255) NULL,
  avatar_url TEXT NULL,
  date_of_birth DATE NULL,
  currency_code VARCHAR(10) NOT NULL DEFAULT 'VND',
  monthly_budget_limit NUMERIC(18,2) NULL,
  auth_provider VARCHAR(20) NOT NULL DEFAULT 'LOCAL',
  status VARCHAR(30) NOT NULL DEFAULT 'ACTIVE',
  email_verified_at TIMESTAMPTZ NULL,
  last_login_at TIMESTAMPTZ NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NULL,
  deleted_at TIMESTAMPTZ NULL,
  CONSTRAINT uq_users_email UNIQUE (email),
  CONSTRAINT chk_users_auth_provider CHECK (auth_provider IN ('LOCAL', 'GOOGLE')),
  CONSTRAINT chk_users_status CHECK (status IN ('ACTIVE', 'LOCKED', 'PENDING_VERIFICATION', 'DELETED'))
);

CREATE TABLE user_auth_identities (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  provider VARCHAR(30) NOT NULL,
  provider_user_id VARCHAR(255) NULL,
  password_hash TEXT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NULL,
  CONSTRAINT uq_user_auth_identities UNIQUE (provider, provider_user_id),
  CONSTRAINT chk_user_auth_identity_provider CHECK (provider IN ('LOCAL', 'GOOGLE'))
);

CREATE TABLE refresh_tokens (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token_hash TEXT NOT NULL,
  jti VARCHAR(100) NOT NULL,
  device_id UUID NULL,
  expires_at TIMESTAMPTZ NOT NULL,
  revoked_at TIMESTAMPTZ NULL,
  replaced_by_token_id UUID NULL,
  ip_address VARCHAR(45) NULL,
  user_agent TEXT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_refresh_tokens_jti UNIQUE (jti)
);

CREATE TABLE password_reset_tokens (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token_hash TEXT NULL,
  otp_hash TEXT NULL,
  reset_method VARCHAR(20) NOT NULL DEFAULT 'EMAIL',
  expires_at TIMESTAMPTZ NOT NULL,
  used_at TIMESTAMPTZ NULL,
  attempt_count INT NOT NULL DEFAULT 0,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT chk_password_reset_method CHECK (reset_method IN ('EMAIL', 'OTP'))
);

CREATE TABLE user_devices (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  device_fingerprint VARCHAR(255) NOT NULL,
  device_name VARCHAR(255) NULL,
  platform VARCHAR(20) NOT NULL,
  os_version VARCHAR(50) NULL,
  app_version VARCHAR(50) NULL,
  push_token TEXT NULL,
  is_trusted BOOLEAN NOT NULL DEFAULT FALSE,
  last_active_at TIMESTAMPTZ NULL,
  last_synced_at TIMESTAMPTZ NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NULL,
  deleted_at TIMESTAMPTZ NULL,
  CONSTRAINT uq_user_devices_fingerprint UNIQUE (user_id, device_fingerprint),
  CONSTRAINT chk_user_devices_platform CHECK (platform IN ('ANDROID', 'IOS', 'WEB'))
);

CREATE TABLE user_sessions (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  device_id UUID NULL REFERENCES user_devices(id) ON DELETE SET NULL,
  access_jti VARCHAR(100) NOT NULL,
  refresh_token_id UUID NULL REFERENCES refresh_tokens(id) ON DELETE SET NULL,
  started_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  last_seen_at TIMESTAMPTZ NULL,
  ended_at TIMESTAMPTZ NULL,
  ip_address VARCHAR(45) NULL,
  user_agent TEXT NULL
);

CREATE TABLE user_logs (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NULL REFERENCES users(id) ON DELETE SET NULL,
  action VARCHAR(50) NOT NULL,
  status VARCHAR(20) NOT NULL,
  resource_type VARCHAR(50) NULL,
  resource_id UUID NULL,
  ip_address VARCHAR(45) NULL,
  user_agent TEXT NULL,
  failure_reason TEXT NULL,
  metadata JSONB NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT chk_user_logs_action CHECK (
    action IN (
      'REGISTER',
      'LOGIN',
      'LOGOUT',
      'REFRESH_TOKEN',
      'FORGOT_PASSWORD',
      'RESET_PASSWORD',
      'CHANGE_PASSWORD',
      'UPDATE_PROFILE',
      'UPDATE_PREFERENCE',
      'GOOGLE_LOGIN'
    )
  ),
  CONSTRAINT chk_user_logs_status CHECK (status IN ('SUCCESS', 'FAILED'))
);

CREATE INDEX IF NOT EXISTS idx_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX IF NOT EXISTS idx_refresh_tokens_expires_at ON refresh_tokens(expires_at);
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_user_id ON password_reset_tokens(user_id);
CREATE INDEX IF NOT EXISTS idx_password_reset_tokens_expires_at ON password_reset_tokens(expires_at);
CREATE INDEX IF NOT EXISTS idx_user_devices_user_id ON user_devices(user_id);
CREATE INDEX IF NOT EXISTS idx_user_devices_last_active_at ON user_devices(last_active_at);
CREATE INDEX IF NOT EXISTS idx_user_sessions_user_id ON user_sessions(user_id);
CREATE INDEX IF NOT EXISTS idx_user_logs_user_id_created_at ON user_logs(user_id, created_at);
CREATE INDEX IF NOT EXISTS idx_user_logs_action_created_at ON user_logs(action, created_at);

