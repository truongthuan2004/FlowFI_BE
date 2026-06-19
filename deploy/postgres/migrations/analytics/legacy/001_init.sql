CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS budgets (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  name text NOT NULL,
  limit_amount numeric(18,2) NOT NULL,
  current_amount numeric(18,2) NOT NULL DEFAULT 0,
  start_date date NOT NULL,
  end_date date NOT NULL
);

CREATE TABLE IF NOT EXISTS goals (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  name text NOT NULL,
  target_amount numeric(18,2) NOT NULL,
  current_amount numeric(18,2) NOT NULL DEFAULT 0,
  target_date date NOT NULL
);

CREATE TABLE IF NOT EXISTS saving_streaks (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  days integer NOT NULL DEFAULT 0,
  last_active_date date NOT NULL
);

CREATE TABLE IF NOT EXISTS report_snapshots (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  period text NOT NULL,
  content_json jsonb NOT NULL,
  created_at timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_budgets_user_period ON budgets(user_id, start_date, end_date);
CREATE INDEX IF NOT EXISTS ix_goals_user_target ON goals(user_id, target_date);

