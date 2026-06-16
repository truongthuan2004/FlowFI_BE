CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS notifications (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  channel text NOT NULL,
  subject text NOT NULL,
  body text NOT NULL,
  status text NOT NULL,
  created_at timestamptz NOT NULL,
  sent_at timestamptz NULL
);

CREATE TABLE IF NOT EXISTS push_devices (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  platform text NOT NULL,
  token text NOT NULL,
  is_active boolean NOT NULL DEFAULT true,
  last_seen_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS device_sync_states (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  device_id text NOT NULL,
  last_synced_at timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_notifications_user_created ON notifications(user_id, created_at DESC);
CREATE UNIQUE INDEX IF NOT EXISTS ux_push_devices_token ON push_devices(token);

