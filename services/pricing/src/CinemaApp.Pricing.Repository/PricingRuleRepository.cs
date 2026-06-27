using CinemaApp.Pricing.Domain.Models;
using MongoDB.Driver;

namespace CinemaApp.Pricing.Repository;

public interface IPricingRuleRepository : IMongoRepository<PricingRule>
{
}

public class PricingRuleRepository : MongoRepository<PricingRule>, IPricingRuleRepository
{
    public PricingRuleRepository(IMongoDatabase database) : base(database, "pricing_rules")
    {
    }
}
