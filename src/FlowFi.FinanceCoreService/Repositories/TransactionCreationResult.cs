using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public sealed record TransactionCreationResult(
    TransactionCreationStatus Status,
    Transaction? Transaction = null);

