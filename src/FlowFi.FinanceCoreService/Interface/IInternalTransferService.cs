using FlowFi.FinanceCoreService.DTOs;

namespace FlowFi.FinanceCoreService.Interface;

public interface IInternalTransferService
{
    Task<CreateInternalTransferResult> CreateAsync(
        CreateTransferRequest request,
        CancellationToken cancellationToken = default);
}

