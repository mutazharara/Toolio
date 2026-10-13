namespace Toolio.Web.Services.Pricing;

public abstract class PricingStrategy
{
    public abstract decimal CalculatePrice(
        decimal pricePerDay,
        int numberOfDays);
}
