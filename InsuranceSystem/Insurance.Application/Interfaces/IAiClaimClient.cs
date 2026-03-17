using Insurance.Application.DTOs.Claim;

namespace Insurance.Application.Interfaces;

public interface IAiClaimClient
{
    Task<AiClaimResponseDto?> AnalyzeClaimAsync(AiClaimRequestDto request);
    Task<AiProspectOutputDto?> PredictProspectAsync(AiProspectInputDto request);
}
