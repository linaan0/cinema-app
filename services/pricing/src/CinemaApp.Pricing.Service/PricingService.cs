using CinemaApp.Pricing.Domain.Dto;
using CinemaApp.Pricing.Domain.Models;
using CinemaApp.Pricing.Repository;

namespace CinemaApp.Pricing.Service;

public interface IPricingService
{
    Task<IEnumerable<PricingRuleDto>> GetRulesAsync();
    Task<PricingRuleDto> CreateRuleAsync(UpsertPricingRuleRequest request);
    Task<CalculatePriceResponse> CalculateAsync(CalculatePriceRequest request);
}

public class PricingService : IPricingService
{
    private readonly IPricingRuleRepository _repository;

    public PricingService(IPricingRuleRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<PricingRuleDto>> GetRulesAsync()
    {
        var rules = await _repository.GetAllAsync();
        return rules.Select(ToDto);
    }

    public async Task<PricingRuleDto> CreateRuleAsync(UpsertPricingRuleRequest request)
    {
        var rule = new PricingRule
        {
            Name = request.Name,
            Condition = request.Condition,
            Multiplier = request.Multiplier,
            IsActive = request.IsActive
        };

        await _repository.InsertAsync(rule);
        return ToDto(rule);
    }

    public async Task<CalculatePriceResponse> CalculateAsync(CalculatePriceRequest request)
    {
        var rules = await _repository.GetAllAsync(r => r.IsActive);

        var finalPrice = request.BasePrice;
        var applied = new List<string>();

        foreach (var rule in rules)
        {
            if (Matches(rule.Condition, request.ScreeningStartTime))
            {
                finalPrice *= rule.Multiplier;
                applied.Add($"{rule.Name} (x{rule.Multiplier})");
            }
        }

        return new CalculatePriceResponse(request.BasePrice, Math.Round(finalPrice, 2), applied);
    }

    private static bool Matches(PricingCondition condition, DateTime screeningStartTime) => condition switch
    {
        PricingCondition.Always => true,
        PricingCondition.Weekend => screeningStartTime.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday,
        PricingCondition.PeakHours => screeningStartTime.Hour is >= 18 and < 22,
        _ => false
    };

    private static PricingRuleDto ToDto(PricingRule rule) => new(rule.Id, rule.Name, rule.Condition, rule.Multiplier, rule.IsActive);
}
