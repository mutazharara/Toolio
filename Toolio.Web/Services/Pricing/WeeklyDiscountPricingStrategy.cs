namespace Toolio.Web.Services.Pricing;

public class WeeklyDiscountPricingStrategy
    : PricingStrategy
{
    public override decimal CalculatePrice(
        decimal pricePerDay,
        int numberOfDays)
    {
        decimal normalPrice =
            pricePerDay * numberOfDays;

        // 10% discount for rentals of 7 days or more
        return normalPrice * 0.90m;
    }
}
