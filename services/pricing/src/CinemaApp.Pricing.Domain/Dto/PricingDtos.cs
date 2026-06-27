using CinemaApp.Pricing.Domain.Models;

namespace CinemaApp.Pricing.Domain.Dto;

public record PricingRuleDto(string Id, string Name, PricingCondition Condition, decimal Multiplier, bool IsActive);

public record UpsertPricingRuleRequest(string Name, PricingCondition Condition, decimal Multiplier, bool IsActive);

public record CalculatePriceRequest(decimal BasePrice, DateTime ScreeningStartTime);

public record CalculatePriceResponse(decimal BasePrice, decimal FinalPrice, List<string> AppliedRules);
