CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS ai_jobs (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  kind text NOT NULL,
  source_uri text NOT NULL,
  status text NOT NULL,
  created_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS ai_insights (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  insight_type text NOT NULL,
  content text NOT NULL,
  confidence numeric(5,2) NULL,
  created_at timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_ai_insights_user_created ON ai_insights(user_id, created_at DESC);

