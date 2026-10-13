using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Toolio.Web.Data;
using Toolio.Web.Models;
using Toolio.Web.ViewModels;

namespace Toolio.Web.Controllers;

[Authorize]
public class ReviewsController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReviewsController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Reviews/Create?bookingId=1
    public async Task<IActionResult> Create(int bookingId)
    {
        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        var booking = await _context.Bookings
            .Include(b => b.Equipment)
            .FirstOrDefaultAsync(b =>
                b.BookingId == bookingId &&
                b.RenterId == userId);

        if (booking == null)
        {
            return NotFound();
        }

        if (booking.Status != BookingStatus.Completed)
        {
            TempData["Error"] =
                "You can only review a completed rental.";

            return RedirectToAction(
                "Index",
                "Bookings");
        }

        bool alreadyReviewed =
            await _context.Reviews.AnyAsync(r =>
                r.BookingId == bookingId);

        if (alreadyReviewed)
        {
            TempData["Error"] =
                "You have already reviewed this rental.";

            return RedirectToAction(
                "Index",
                "Bookings");
        }

        var model =
            new ReviewCreateViewModel
            {
                BookingId =
                    booking.BookingId,

                EquipmentName =
                    booking.Equipment?.Name
                    ?? "Equipment"
            };

        return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        ReviewCreateViewModel model)
    {
        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var booking =
            await _context.Bookings
                .Include(b => b.Equipment)
                .FirstOrDefaultAsync(b =>
                    b.BookingId == model.BookingId &&
                    b.RenterId == userId);

        if (booking == null)
        {
            return NotFound();
        }

        model.EquipmentName =
            booking.Equipment?.Name
            ?? "Equipment";


        if (booking.Status != BookingStatus.Completed)
        {
            ModelState.AddModelError(
                "",
                "Only completed rentals can be reviewed."
            );
        }


        bool alreadyReviewed =
            await _context.Reviews.AnyAsync(r =>
                r.BookingId == booking.BookingId);


        if (alreadyReviewed)
        {
            ModelState.AddModelError(
                "",
                "You have already reviewed this rental."
            );
        }


        if (!ModelState.IsValid)
        {
            return View(model);
        }


        var review =
            new Review
            {
                BookingId =
                    booking.BookingId,

                EquipmentId =
                    booking.EquipmentId,

                ReviewerId =
                    userId,

                Rating =
                    model.Rating,

                Comment =
                    model.Comment,

                CreatedAt =
                    DateTime.UtcNow
            };


        _context.Reviews.Add(review);

        await _context.SaveChangesAsync();


        TempData["Success"] =
            "Thank you for reviewing this equipment.";

        return RedirectToAction(
            "Details",
            "Equipment",
            new
            {
                id = booking.EquipmentId
            });
    }
}