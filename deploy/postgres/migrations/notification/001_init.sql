CREATE EXTENSION IF NOT EXISTS pgcrypto;

DROP TABLE IF EXISTS notification_events CASCADE;
DROP TABLE IF EXISTS notification_deliveries CASCADE;
DROP TABLE IF EXISTS notifications CASCADE;
DROP TABLE IF EXISTS notification_settings CASCADE;
DROP TABLE IF EXISTS notification_templates CASCADE;

CREATE TABLE notification_settings (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL,
  enable_email BOOLEAN NOT NULL DEFAULT TRUE,
  enable_push BOOLEAN NOT NULL DEFAULT TRUE,
  enable_in_app BOOLEAN NOT NULL DEFAULT TRUE,
  enable_budget_warning BOOLEAN NOT NULL DEFAULT TRUE,
  enable_transaction_alert BOOLEAN NOT NULL DEFAULT TRUE,
  enable_system_alert BOOLEAN NOT NULL DEFAULT TRUE,
  quiet_hours_start TIME NULL,
  quiet_hours_end TIME NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NULL,
  CONSTRAINT uq_notification_settings_user UNIQUE (user_id)
);

CREATE TABLE notifications (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  user_id UUID NOT NULL,
  title VARCHAR(255) NOT NULL,
  content TEXT NULL,
  notification_type VARCHAR(50) NOT NULL,
  channel VARCHAR(20) NOT NULL DEFAULT 'IN_APP',
  priority VARCHAR(20) NOT NULL DEFAULT 'NORMAL',
  is_read BOOLEAN NOT NULL DEFAULT FALSE,
  read_at TIMESTAMPTZ NULL,
  sent_at TIMESTAMPTZ NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NULL,
  deleted_at TIMESTAMPTZ NULL,
  target_url TEXT NULL,
  metadata JSONB NULL,
  CONSTRAINT chk_notifications_type CHECK (
    notification_type IN ('BUDGET_WARNING', 'SYSTEM', 'TRANSACTION', 'SECURITY', 'REMINDER')
  ),
  CONSTRAINT chk_notifications_channel CHECK (channel IN ('IN_APP', 'PUSH', 'EMAIL', 'SMS')),
  CONSTRAINT chk_notifications_priority CHECK (priority IN ('LOW', 'NORMAL', 'HIGH', 'URGENT'))
);

CREATE TABLE notification_deliveries (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  notification_id UUID NOT NULL REFERENCES notifications(id) ON DELETE CASCADE,
  channel VARCHAR(20) NOT NULL,
  delivery_status VARCHAR(20) NOT NULL DEFAULT 'PENDING',
  provider_message_id VARCHAR(255) NULL,
  error_message TEXT NULL,
  delivered_at TIMESTAMPTZ NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NULL,
  CONSTRAINT chk_notification_deliveries_channel CHECK (channel IN ('PUSH', 'EMAIL', 'SMS', 'IN_APP')),
  CONSTRAINT chk_notification_deliveries_status CHECK (delivery_status IN ('PENDING', 'SENT', 'FAILED', 'RETRYING'))
);

CREATE TABLE notification_templates (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  template_code VARCHAR(100) NOT NULL,
  channel VARCHAR(20) NOT NULL,
  title_template TEXT NOT NULL,
  content_template TEXT NOT NULL,
  is_active BOOLEAN NOT NULL DEFAULT TRUE,
  metadata_schema JSONB NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  updated_at TIMESTAMPTZ NULL,
  CONSTRAINT uq_notification_templates_code_channel UNIQUE (template_code, channel),
  CONSTRAINT chk_notification_templates_channel CHECK (channel IN ('PUSH', 'EMAIL', 'SMS', 'IN_APP'))
);

CREATE TABLE notification_events (
  id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
  event_type VARCHAR(100) NOT NULL,
  aggregate_id UUID NULL,
  user_id UUID NULL,
  payload JSONB NOT NULL,
  processed_at TIMESTAMPTZ NULL,
  created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
  CONSTRAINT uq_notification_events UNIQUE (event_type, aggregate_id)
);

CREATE INDEX IF NOT EXISTS idx_notifications_user_id_created_at ON notifications(user_id, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_notifications_user_id_is_read ON notifications(user_id, is_read);
CREATE INDEX IF NOT EXISTS idx_notifications_type_created_at ON notifications(notification_type, created_at DESC);
CREATE INDEX IF NOT EXISTS idx_notification_deliveries_notification_id ON notification_deliveries(notification_id);
CREATE INDEX IF NOT EXISTS idx_notification_events_created_at ON notification_events(created_at DESC);

