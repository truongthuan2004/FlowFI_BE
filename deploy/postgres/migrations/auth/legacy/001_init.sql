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
  email TEXT NOT NULL,
  password_hash TEXT NULL,
  full_name TEXT NULL,
  avatar_url TEXT NULL,
  date_of_birth DATE NULL,
  currency_code TEXT NOT NULL DEFAULT 'VND',
  monthly_budget_limit NUMERIC NULL,
  auth_provider TEXT NOT NULL DEFAULT 'LOCAL',
  is_verified BOOLEAN NOT NULL DEFAULT FALSE,
  role VARCHAR(50) NOT NULL DEFAULT 'User',
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NULL,
  deleted_at TIMESTAMPTZ NULL,
  CONSTRAINT uq_users_email UNIQUE (email)
);

CREATE TABLE refresh_tokens (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token TEXT NOT NULL,
  expires_at TIMESTAMPTZ NOT NULL,
  is_revoked BOOLEAN NOT NULL DEFAULT FALSE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE password_reset_tokens (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  token TEXT NOT NULL,
  otp_code TEXT NULL,
  expires_at TIMESTAMPTZ NOT NULL,
  is_used BOOLEAN NOT NULL DEFAULT FALSE,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE user_devices (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  device_id TEXT NOT NULL,
  device_name TEXT NULL,
  platform TEXT NOT NULL,
  push_token TEXT NULL,
  last_synced_at TIMESTAMPTZ NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE user_logs (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL REFERENCES users(id) ON DELETE CASCADE,
  action TEXT NOT NULL,
  status TEXT NOT NULL,
  ip_address TEXT NULL,
  user_agent TEXT NULL,
  failure_reason TEXT NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS ix_users_email ON users(email);
CREATE INDEX IF NOT EXISTS ix_refresh_tokens_user_id ON refresh_tokens(user_id);
CREATE INDEX IF NOT EXISTS ix_refresh_tokens_token ON refresh_tokens(token);
CREATE INDEX IF NOT EXISTS ix_password_reset_tokens_user_id ON password_reset_tokens(user_id);
CREATE INDEX IF NOT EXISTS ix_password_reset_tokens_token ON password_reset_tokens(token);
CREATE INDEX IF NOT EXISTS ix_user_devices_user_id ON user_devices(user_id);
CREATE INDEX IF NOT EXISTS ix_user_logs_user_id ON user_logs(user_id);
CREATE INDEX IF NOT EXISTS ix_user_logs_user_id_created_at ON user_logs(user_id, created_at DESC);
