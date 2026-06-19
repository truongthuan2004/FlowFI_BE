using FlowFi.FinanceCoreService.DTOs;

namespace FlowFi.FinanceCoreService.Services;

public interface IInternalTransferService
{
    Task<CreateInternalTransferResult> CreateAsync(
        CreateInternalTransferDto request,
        CancellationToken cancellationToken = default);
}

