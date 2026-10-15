using NUnit.Framework;
using Toolio.Web.Services.Pricing;

namespace Toolio.Tests;

public class PricingStrategyTests
{
    [Test]
    public void StandardPricing_ReturnsCorrectPrice()
    {
        // Arrange
        PricingStrategy strategy =
            new StandardPricingStrategy();


        // Act
        decimal result =
            strategy.CalculatePrice(
                20m,
                3);


        // Assert
        Assert.That(
            result,
            Is.EqualTo(60m));
    }


    [Test]
    public void WeeklyDiscountPricing_AppliesTenPercentDiscount()
    {
        // Arrange
        PricingStrategy strategy =
            new WeeklyDiscountPricingStrategy();


        // Act
        decimal result =
            strategy.CalculatePrice(
                20m,
                7);


        // Assert
        Assert.That(
            result,
            Is.EqualTo(126m));
    }
}
