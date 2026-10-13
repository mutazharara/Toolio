using System.ComponentModel.DataAnnotations;

using NUnit.Framework;

using Toolio.Web.Models;

namespace Toolio.Tests;

public class EquipmentTests
{
    [Test]
    public void Equipment_WithNegativePrice_IsInvalid()
    {
        // Arrange
        var equipment = new Equipment
        {
            Name = "Bosch Drill",

            Description =
                "Cordless drill",

            PricePerDay =
                -10m,

            CategoryId =
                1
        };


        var validationContext =
            new ValidationContext(
                equipment);


        var validationResults =
            new List<ValidationResult>();


        // Act
        bool isValid =
            Validator.TryValidateObject(
                equipment,
                validationContext,
                validationResults,
                true);


        // Assert
        Assert.That(
            isValid,
            Is.False);
    }
}
