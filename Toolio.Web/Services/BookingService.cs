using Microsoft.EntityFrameworkCore;
using Toolio.Web.Data;
using Toolio.Web.Models;

namespace Toolio.Web.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;

    public BookingService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<bool> IsAvailableAsync(
        int equipmentId,
        DateTime startDate,
        DateTime endDate)
    {
        bool conflict = await _context.Bookings
            .AnyAsync(b =>
                b.EquipmentId == equipmentId &&

                (
                    b.Status == BookingStatus.Pending ||
                    b.Status == BookingStatus.Approved
                ) &&

                startDate.Date <= b.EndDate.Date &&
                endDate.Date >= b.StartDate.Date
            );

        return !conflict;
    }

    public decimal CalculateTotalPrice(
        decimal pricePerDay,
        DateTime startDate,
        DateTime endDate)
    {
        int numberOfDays =
            (endDate.Date - startDate.Date).Days + 1;

        return pricePerDay * numberOfDays;
    }
}