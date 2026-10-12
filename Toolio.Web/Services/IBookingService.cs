namespace Toolio.Web.Services;

public interface IBookingService
{
    Task<bool> IsAvailableAsync(
        int equipmentId,
        DateTime startDate,
        DateTime endDate);

    decimal CalculateTotalPrice(
        decimal pricePerDay,
        DateTime startDate,
        DateTime endDate);
}