CREATE EXTENSION IF NOT EXISTS "uuid-ossp";

CREATE TABLE IF NOT EXISTS wallets (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  name text NOT NULL,
  currency varchar(8) NOT NULL,
  balance numeric(18,2) NOT NULL DEFAULT 0,
  type text NOT NULL
);

CREATE TABLE IF NOT EXISTS categories (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  name text NOT NULL,
  direction text NOT NULL CHECK (direction IN ('inflow', 'outflow', 'internal'))
);

CREATE TABLE IF NOT EXISTS tags (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  name text NOT NULL
);

CREATE TABLE IF NOT EXISTS transactions (
  id uuid PRIMARY KEY,
  user_id uuid NOT NULL,
  wallet_id uuid NOT NULL REFERENCES wallets(id),
  category_id uuid NULL REFERENCES categories(id),
  amount numeric(18,2) NOT NULL,
  currency varchar(8) NOT NULL,
  direction text NOT NULL CHECK (direction IN ('inflow', 'outflow', 'internal')),
  note text NULL,
  occurred_at timestamptz NOT NULL,
  created_at timestamptz NOT NULL
);

CREATE TABLE IF NOT EXISTS transaction_tags (
  transaction_id uuid NOT NULL REFERENCES transactions(id) ON DELETE CASCADE,
  tag_id uuid NOT NULL REFERENCES tags(id) ON DELETE CASCADE,
  PRIMARY KEY (transaction_id, tag_id)
);

CREATE INDEX IF NOT EXISTS ix_transactions_user_occurred ON transactions(user_id, occurred_at DESC);

