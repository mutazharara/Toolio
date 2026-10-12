using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Toolio.Web.Data;
using Toolio.Web.Models;
using Toolio.Web.Services;
using Toolio.Web.ViewModels;

namespace Toolio.Web.Controllers;

[Authorize]
public class BookingsController : Controller
{
    private readonly ApplicationDbContext _context;
    private readonly IBookingService _bookingService;

    public BookingsController(
        ApplicationDbContext context,
        IBookingService bookingService)
    {
        _context = context;
        _bookingService = bookingService;
    }

    // GET: /Bookings
    public async Task<IActionResult> Index()
    {
        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        var bookings = await _context.Bookings
            .Include(b => b.Equipment)
            .ThenInclude(e => e!.Category)
            .Where(b =>
                b.RenterId == userId)
            .OrderByDescending(b =>
                b.CreatedAt)
            .ToListAsync();

        return View(bookings);
    }


    // GET: /Bookings/Create?equipmentId=1
    public async Task<IActionResult> Create(
        int equipmentId)
    {
        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        var equipment =
            await _context.Equipment
                .FirstOrDefaultAsync(e =>
                    e.EquipmentId == equipmentId);

        if (equipment == null)
        {
            return NotFound();
        }

        if (!equipment.IsAvailable)
        {
            TempData["Error"] =
                "This equipment is currently unavailable.";

            return RedirectToAction(
                "Details",
                "Equipment",
                new { id = equipmentId });
        }

        if (equipment.OwnerId == userId)
        {
            TempData["Error"] =
                "You cannot rent your own equipment.";

            return RedirectToAction(
                "Details",
                "Equipment",
                new { id = equipmentId });
        }

        var model =
            new BookingCreateViewModel
            {
                EquipmentId =
                    equipment.EquipmentId,

                EquipmentName =
                    equipment.Name,

                PricePerDay =
                    equipment.PricePerDay,

                StartDate =
                    DateTime.Today,

                EndDate =
                    DateTime.Today
            };

        return View(model);
    }


    // POST: /Bookings/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        BookingCreateViewModel model)
    {
        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var equipment =
            await _context.Equipment
                .FirstOrDefaultAsync(e =>
                    e.EquipmentId ==
                    model.EquipmentId);

        if (equipment == null)
        {
            return NotFound();
        }

        model.EquipmentName =
            equipment.Name;

        model.PricePerDay =
            equipment.PricePerDay;


        // User cannot book own equipment
        if (equipment.OwnerId == userId)
        {
            ModelState.AddModelError(
                "",
                "You cannot rent your own equipment."
            );
        }


        // Equipment must be available
        if (!equipment.IsAvailable)
        {
            ModelState.AddModelError(
                "",
                "This equipment is currently unavailable."
            );
        }


        // Start date validation
        if (model.StartDate.Date <
            DateTime.Today)
        {
            ModelState.AddModelError(
                nameof(model.StartDate),
                "Start date cannot be in the past."
            );
        }


        // End date validation
        if (model.EndDate.Date <
            model.StartDate.Date)
        {
            ModelState.AddModelError(
                nameof(model.EndDate),
                "End date cannot be before the start date."
            );
        }


        if (!ModelState.IsValid)
        {
            return View(model);
        }


        // Check booking conflict
        bool available =
            await _bookingService
                .IsAvailableAsync(
                    equipment.EquipmentId,
                    model.StartDate,
                    model.EndDate);


        if (!available)
        {
            ModelState.AddModelError(
                "",
                "This equipment is already booked for some or all of the selected dates."
            );

            return View(model);
        }


        decimal totalPrice =
            _bookingService
                .CalculateTotalPrice(
                    equipment.PricePerDay,
                    model.StartDate,
                    model.EndDate);


        var booking =
            new Booking
            {
                EquipmentId =
                    equipment.EquipmentId,

                RenterId =
                    userId,

                StartDate =
                    model.StartDate.Date,

                EndDate =
                    model.EndDate.Date,

                TotalPrice =
                    totalPrice,

                Status =
                    BookingStatus.Pending,

                CreatedAt =
                    DateTime.UtcNow
            };


        _context.Bookings.Add(booking);

        await _context.SaveChangesAsync();


        TempData["Success"] =
            "Your booking request has been sent to the owner.";

        return RedirectToAction(
            nameof(Index));
    }
}