using CinemaApp.Pricing.Domain.Dto;
using CinemaApp.Pricing.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CinemaApp.Pricing.Api.Controllers;

[ApiController]
[Route("api/pricing")]
public class PricingController : ControllerBase
{
    private readonly IPricingService _pricingService;

    public PricingController(IPricingService pricingService)
    {
        _pricingService = pricingService;
    }

    [HttpGet("rules")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<PricingRuleDto>>> GetRules()
        => Ok(await _pricingService.GetRulesAsync());

    [HttpPost("rules")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PricingRuleDto>> CreateRule(UpsertPricingRuleRequest request)
        => Ok(await _pricingService.CreateRuleAsync(request));

    // Used by the Bookings service (or the frontend) to display the final
    // price for a screening before the user confirms a booking.
    [HttpPost("calculate")]
    [AllowAnonymous]
    public async Task<ActionResult<CalculatePriceResponse>> Calculate(CalculatePriceRequest request)
        => Ok(await _pricingService.CalculateAsync(request));
}
