CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS login_audit_events (
  id uuid PRIMARY KEY,
  user_id uuid NULL,
  ip_address text NOT NULL,
  device_name text NULL,
  browser text NULL,
  event_type text NOT NULL,
  occurred_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS service_health_snapshots (
  id uuid PRIMARY KEY,
  service_name text NOT NULL,
  status text NOT NULL,
  details_json jsonb NOT NULL,
  captured_at timestamptz NOT NULL
);

