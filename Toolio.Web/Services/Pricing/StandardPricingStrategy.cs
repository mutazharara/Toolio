namespace Toolio.Web.Services.Pricing;

public class StandardPricingStrategy
    : PricingStrategy
{
    public override decimal CalculatePrice(
        decimal pricePerDay,
        int numberOfDays)
    {
        return pricePerDay * numberOfDays;
    }
}
