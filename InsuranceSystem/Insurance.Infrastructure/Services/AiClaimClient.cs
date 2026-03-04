using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Insurance.Application.DTOs.Claim;
using Insurance.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Insurance.Infrastructure.Services;

public class AiClaimClient : IAiClaimClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiClaimClient> _logger;

    public AiClaimClient(IHttpClientFactory httpClientFactory, ILogger<AiClaimClient> logger)
    {
        _httpClient = httpClientFactory.CreateClient("AiService");
        _logger = logger;
    }

    public async Task<AiClaimResponseDto?> AnalyzeClaimAsync(AiClaimRequestDto request)
    {
        try
        {
            _logger.LogInformation("Sending claim for AI analysis: {@Request}", request);
            
            var response = await _httpClient.PostAsJsonAsync("analyze-claim", request);
            
            if (response.IsSuccessStatusCode)
            {
                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                var result = await response.Content.ReadFromJsonAsync<AiClaimResponseDto>(options);
                _logger.LogInformation("AI analysis completed successfully: {@Result}", result);
                return result;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("AI Service returned an error. Status: {StatusCode}, Content: {Content}", response.StatusCode, errorContent);
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to connect to AI Service at {BaseAddress}", _httpClient.BaseAddress);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unexpected error occurred during AI analysis.");
            return null;
        }
    }
}
