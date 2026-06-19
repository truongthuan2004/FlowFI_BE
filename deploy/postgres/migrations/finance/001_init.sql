CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS wallets (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  name varchar(100) NOT NULL,
  wallet_type varchar(30) NOT NULL,
  balance numeric(18,2) NOT NULL DEFAULT 0,
  currency varchar(10) NOT NULL,
  is_active boolean NOT NULL DEFAULT true,
  created_at timestamptz NOT NULL,
  updated_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS tags (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  name varchar(100) NOT NULL,
  type varchar(20) NOT NULL,
  icon varchar(100) NOT NULL,
  color varchar(20) NOT NULL,
  created_at timestamptz NOT NULL,
  updated_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS transactions (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  wallet_id uuid NOT NULL REFERENCES wallets(id) ON DELETE RESTRICT,
  tag_id uuid NOT NULL REFERENCES tags(id) ON DELETE RESTRICT,
  amount numeric(18,2) NOT NULL,
  type varchar(20) NOT NULL,
  title varchar(150) NOT NULL,
  note text NOT NULL,
  source varchar(30) NOT NULL,
  sync_status varchar(20) NOT NULL,
  transaction_date timestamptz NOT NULL,
  created_at timestamptz NOT NULL,
  updated_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS internal_transfers (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  from_wallet_id uuid NOT NULL REFERENCES wallets(id) ON DELETE RESTRICT,
  to_wallet_id uuid NOT NULL REFERENCES wallets(id) ON DELETE RESTRICT,
  amount numeric(18,2) NOT NULL,
  note text NOT NULL,
  sync_status varchar(20) NOT NULL,
  transfer_date timestamptz NOT NULL,
  created_at timestamptz NOT NULL,
  updated_at timestamptz NOT NULL,
  CONSTRAINT ck_internal_transfers_distinct_wallets CHECK (from_wallet_id <> to_wallet_id)
);

CREATE TABLE IF NOT EXISTS wallet_balance_logs (
  id uuid PRIMARY KEY,
  wallet_id uuid NOT NULL REFERENCES wallets(id) ON DELETE RESTRICT,
  transaction_id uuid NULL REFERENCES transactions(id) ON DELETE RESTRICT,
  transfer_id uuid NULL REFERENCES internal_transfers(id) ON DELETE RESTRICT,
  old_balance numeric(18,2) NOT NULL,
  change_amount numeric(18,2) NOT NULL,
  new_balance numeric(18,2) NOT NULL,
  reason varchar(50) NOT NULL,
  created_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS sync_queue (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  entity_type varchar(30) NOT NULL,
  entity_id uuid NOT NULL,
  action varchar(20) NOT NULL,
  payload jsonb NOT NULL,
  status varchar(20) NOT NULL,
  retry_count integer NOT NULL DEFAULT 0,
  last_error text NULL,
  created_at timestamptz NOT NULL,
  synced_at timestamptz NULL
);

CREATE TABLE IF NOT EXISTS recurring_transactions (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  wallet_id uuid NOT NULL REFERENCES wallets(id) ON DELETE RESTRICT,
  tag_id uuid NOT NULL REFERENCES tags(id) ON DELETE RESTRICT,
  amount numeric(18,2) NOT NULL,
  type varchar(20) NOT NULL,
  title varchar(150) NOT NULL,
  note text NOT NULL,
  frequency varchar(20) NOT NULL,
  start_date date NOT NULL,
  end_date date NULL,
  next_run_at timestamptz NOT NULL,
  is_active boolean NOT NULL DEFAULT true,
  created_at timestamptz NOT NULL,
  updated_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS finance_audit_logs (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  entity_type varchar(30) NOT NULL,
  entity_id uuid NOT NULL,
  action varchar(20) NOT NULL,
  old_data jsonb NULL,
  new_data jsonb NULL,
  created_at timestamptz NOT NULL
);

CREATE INDEX IF NOT EXISTS ix_wallets_user_id ON wallets(user_id);
CREATE INDEX IF NOT EXISTS ix_tags_user_id ON tags(user_id);
CREATE INDEX IF NOT EXISTS ix_transactions_user_date ON transactions(user_id, transaction_date DESC);
CREATE INDEX IF NOT EXISTS ix_transactions_wallet_id ON transactions(wallet_id);
CREATE INDEX IF NOT EXISTS ix_internal_transfers_user_date ON internal_transfers(user_id, transfer_date DESC);
CREATE INDEX IF NOT EXISTS ix_wallet_balance_logs_wallet_created ON wallet_balance_logs(wallet_id, created_at DESC);
CREATE INDEX IF NOT EXISTS ix_sync_queue_status_created ON sync_queue(status, created_at);
CREATE INDEX IF NOT EXISTS ix_recurring_transactions_next_run ON recurring_transactions(is_active, next_run_at);
CREATE INDEX IF NOT EXISTS ix_finance_audit_logs_entity ON finance_audit_logs(entity_type, entity_id, created_at DESC);
