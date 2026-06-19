using FlowFi.FinanceCoreService.DTOs;

namespace FlowFi.FinanceCoreService.Interface;

public interface IInternalTransferService
{
    Task<CreateInternalTransferResult> CreateAsync(
        CreateInternalTransferDto request,
        CancellationToken cancellationToken = default);
}

