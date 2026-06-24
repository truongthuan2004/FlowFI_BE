using FlowFi.FinanceCoreService.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FlowFi.FinanceCoreService.Database.Migrations;

[DbContext(typeof(FinanceDbContext))]
[Migration("20260624000000_AddFinanceDatabaseConstraints")]
public partial class AddFinanceDatabaseConstraints : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            DO $$
            BEGIN
                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'ck_transactions_amount_positive'
                ) THEN
                    ALTER TABLE transactions
                    ADD CONSTRAINT ck_transactions_amount_positive CHECK (amount > 0);
                END IF;

                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'ck_internal_transfers_amount_positive'
                ) THEN
                    ALTER TABLE internal_transfers
                    ADD CONSTRAINT ck_internal_transfers_amount_positive CHECK (amount > 0);
                END IF;

                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'ck_recurring_transactions_amount_positive'
                ) THEN
                    ALTER TABLE recurring_transactions
                    ADD CONSTRAINT ck_recurring_transactions_amount_positive CHECK (amount > 0);
                END IF;

                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'ck_transactions_type'
                ) THEN
                    ALTER TABLE transactions
                    ADD CONSTRAINT ck_transactions_type CHECK (type IN ('INCOME', 'EXPENSE'));
                END IF;

                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'ck_recurring_transactions_type'
                ) THEN
                    ALTER TABLE recurring_transactions
                    ADD CONSTRAINT ck_recurring_transactions_type CHECK (type IN ('INCOME', 'EXPENSE'));
                END IF;

                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'ck_tags_type'
                ) THEN
                    ALTER TABLE tags
                    ADD CONSTRAINT ck_tags_type CHECK (type IN ('INCOME', 'EXPENSE'));
                END IF;

                IF NOT EXISTS (
                    SELECT 1 FROM pg_constraint WHERE conname = 'ck_wallets_wallet_type'
                ) THEN
                    ALTER TABLE wallets
                    ADD CONSTRAINT ck_wallets_wallet_type CHECK (wallet_type IN ('CASH', 'BANK', 'SAVING', 'INVESTMENT'));
                END IF;
            END $$;
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE IF EXISTS wallets DROP CONSTRAINT IF EXISTS ck_wallets_wallet_type;
            ALTER TABLE IF EXISTS tags DROP CONSTRAINT IF EXISTS ck_tags_type;
            ALTER TABLE IF EXISTS recurring_transactions DROP CONSTRAINT IF EXISTS ck_recurring_transactions_type;
            ALTER TABLE IF EXISTS recurring_transactions DROP CONSTRAINT IF EXISTS ck_recurring_transactions_amount_positive;
            ALTER TABLE IF EXISTS internal_transfers DROP CONSTRAINT IF EXISTS ck_internal_transfers_amount_positive;
            ALTER TABLE IF EXISTS transactions DROP CONSTRAINT IF EXISTS ck_transactions_type;
            ALTER TABLE IF EXISTS transactions DROP CONSTRAINT IF EXISTS ck_transactions_amount_positive;
            """);
    }
}
