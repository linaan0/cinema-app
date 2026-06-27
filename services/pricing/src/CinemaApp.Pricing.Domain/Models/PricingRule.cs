using CinemaApp.Pricing.Domain.Common;

namespace CinemaApp.Pricing.Domain.Models;

public enum PricingCondition
{
    Always,
    Weekend,
    PeakHours // 18:00 - 22:00
}

public class PricingRule : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public PricingCondition Condition { get; set; } = PricingCondition.Always;

    // Multiplicative factor applied to the base price, e.g. 1.2 = +20%
    public decimal Multiplier { get; set; } = 1.0m;

    public bool IsActive { get; set; } = true;
}
