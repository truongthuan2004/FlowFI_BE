using FlowFi.FinanceCoreService.Entities;

namespace FlowFi.FinanceCoreService.Repositories;

public interface IInternalTransferRepository
{
    Task<InternalTransferCreationResult> CreateAsync(
        InternalTransfer transfer,
        CancellationToken cancellationToken = default);
}

