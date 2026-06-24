using FlowFi.FinanceCoreService.Database;
using FlowFi.FinanceCoreService.DTOs;
using FlowFi.FinanceCoreService.Repositories;
using FlowFi.FinanceCoreService.Services;
using Microsoft.EntityFrameworkCore;

var connectionString = "Host=localhost;Port=6000;Database=FlowFi_Finance;Username=postgres;Password=123456";
var userId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa");
var cashWalletId = Guid.Parse("11111111-1111-1111-1111-111111111111");
var bankWalletId = Guid.Parse("22222222-2222-2222-2222-222222222222");
var incomeTagId = Guid.Parse("44444444-4444-4444-4444-444444444444");
var expenseTagId = Guid.Parse("55555555-5555-5555-5555-555555555555");

var options = new DbContextOptionsBuilder<FinanceDbContext>()
    .UseNpgsql(connectionString)
    .UseSnakeCaseNamingConvention()
    .Options;

await using var db = new FinanceDbContext(options);
var unitOfWork = new UnitOfWork(db);
var walletRepository = new WalletRepository(db);
var tagRepository = new TagRepository(db);
var transactionRepository = new TransactionRepository(db);
var transferRepository = new InternalTransferRepository(db);
var balanceLogRepository = new WalletBalanceLogRepository(db);
var auditRepository = new FinanceAuditRepository(db);

var transactionService = new TransactionService(
    transactionRepository,
    walletRepository,
    tagRepository,
    balanceLogRepository,
    auditRepository,
    unitOfWork);

var transferService = new InternalTransferService(
    transferRepository,
    walletRepository,
    balanceLogRepository,
    auditRepository,
    unitOfWork);

var beforeCash = await db.Wallets.AsNoTracking().SingleAsync(w => w.Id == cashWalletId);
var beforeBank = await db.Wallets.AsNoTracking().SingleAsync(w => w.Id == bankWalletId);

var incomeResult = await transactionService.CreateAsync(
    userId,
    cashWalletId,
    new CreateTransactionRequest
    {
        WalletId = cashWalletId,
        TagId = incomeTagId,
        Amount = 1234.56m,
        Type = "INCOME",
        Title = "Probe income",
        Note = "Probe income",
        Source = "MANUAL"
    },
    null);

var afterIncome = await db.Wallets.AsNoTracking().SingleAsync(w => w.Id == cashWalletId);

var expenseResult = await transactionService.CreateAsync(
    userId,
    cashWalletId,
    new CreateTransactionRequest
    {
        WalletId = cashWalletId,
        TagId = expenseTagId,
        Amount = 234.56m,
        Type = "EXPENSE",
        Title = "Probe expense",
        Note = "Probe expense",
        Source = "MANUAL"
    },
    null);

var afterExpense = await db.Wallets.AsNoTracking().SingleAsync(w => w.Id == cashWalletId);

var transferResult = await transferService.CreateAsync(
    new CreateTransferRequest
    {
        UserId = userId,
        FromWalletId = cashWalletId,
        ToWalletId = bankWalletId,
        Amount = 500m,
        Note = "Probe transfer",
        SyncStatus = "SYNCED",
        TransferDate = DateTimeOffset.UtcNow
    });

var afterTransferCash = await db.Wallets.AsNoTracking().SingleAsync(w => w.Id == cashWalletId);
var afterTransferBank = await db.Wallets.AsNoTracking().SingleAsync(w => w.Id == bankWalletId);

var logRows = await db.WalletBalanceLogs
    .AsNoTracking()
    .Where(log =>
        log.TransactionId == incomeResult.Transaction!.Id ||
        log.TransactionId == expenseResult.Transaction!.Id ||
        log.TransferId == transferResult.Transfer!.Id)
    .OrderBy(log => log.CreatedAt)
    .Select(log => new
    {
        log.WalletId,
        log.TransactionId,
        log.TransferId,
        log.OldBalance,
        log.ChangeAmount,
        log.NewBalance,
        log.Reason
    })
    .ToListAsync();

Console.WriteLine(System.Text.Json.JsonSerializer.Serialize(new
{
    beforeCash = beforeCash.Balance,
    beforeBank = beforeBank.Balance,
    incomeStatus = incomeResult.Status.ToString(),
    incomeTransactionId = incomeResult.Transaction?.Id,
    afterIncome = afterIncome.Balance,
    expenseStatus = expenseResult.Status.ToString(),
    expenseTransactionId = expenseResult.Transaction?.Id,
    afterExpense = afterExpense.Balance,
    transferStatus = transferResult.Status.ToString(),
    transferId = transferResult.Transfer?.Id,
    afterTransferCash = afterTransferCash.Balance,
    afterTransferBank = afterTransferBank.Balance,
    walletBalanceLogs = logRows
}, new System.Text.Json.JsonSerializerOptions { WriteIndented = true }));
