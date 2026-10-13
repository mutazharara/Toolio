using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Toolio.Web.Data;
using Toolio.Web.Models;
using Toolio.Web.Services;
using Toolio.Web.ViewModels;

namespace Toolio.Web.Controllers;

[Authorize]
public class EquipmentController : Controller
{
    private readonly ApplicationDbContext _context;

    private readonly IEquipmentService _equipmentService;

    public EquipmentController(ApplicationDbContext context, IEquipmentService equipmentService)
    {
        _context = context;
        _equipmentService = equipmentService;
    }


    // =========================================================
    // BROWSE EQUIPMENT
    // GET: /Equipment/Browse
    // =========================================================

    [AllowAnonymous]
    public async Task<IActionResult> Browse(
        string? search,
        int? categoryId,
        string availability = "available",
        string sort = "newest")
    {
        var equipment =
            await _equipmentService.SearchEquipmentAsync(
            search,
            categoryId,
            availability,
            sort);


        // Create ViewModel for Browse page
        var model = new EquipmentBrowseViewModel
        {
            Equipment = equipment,

            Categories =
                await _context.Categories
                    .AsNoTracking()
                    .OrderBy(c => c.Name)
                    .ToListAsync(),

            SearchTerm = search,

            CategoryId = categoryId,

            Availability = availability,

            Sort = sort
        };


        return View(model);
    }


    // =========================================================
    // EQUIPMENT DETAILS
    // GET: /Equipment/Details/5
    // =========================================================

    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var equipment =
            await _context.Equipment
                .Include(e => e.Category)
                .AsNoTracking()
                .FirstOrDefaultAsync(e =>
                    e.EquipmentId == id);


        if (equipment == null)
        {
            return NotFound();
        }


        // Get reviews for this equipment
        var reviews =
            await _context.Reviews
                .Where(r =>
                    r.EquipmentId == id)
                .OrderByDescending(r =>
                    r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();


        // Calculate average rating
        double averageRating = 0;

        if (reviews.Any())
        {
            averageRating =
                reviews.Average(r =>
                    r.Rating);
        }


        // Prepare data for Details page
        var model =
            new EquipmentDetailsViewModel
            {
                Equipment =
                    equipment,

                Reviews =
                    reviews,

                AverageRating =
                    averageRating,

                ReviewCount =
                    reviews.Count
            };


        return View(model);
    }


    // =========================================================
    // MY LISTINGS
    // GET: /Equipment
    // =========================================================

    public async Task<IActionResult> Index()
    {
        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);


        if (userId == null)
        {
            return Challenge();
        }


        // Only show equipment owned by logged-in user
        var equipment =
            await _context.Equipment
                .Include(e => e.Category)
                .Where(e =>
                    e.OwnerId == userId)
                .OrderByDescending(e =>
                    e.CreatedAt)
                .ToListAsync();


        return View(equipment);
    }


    // =========================================================
    // CREATE EQUIPMENT
    // GET: /Equipment/Create
    // =========================================================

    public async Task<IActionResult> Create()
    {
        await LoadCategories();

        return View(new Equipment());
    }


    // =========================================================
    // CREATE EQUIPMENT
    // POST: /Equipment/Create
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Equipment equipment)
    {
        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);


        if (userId == null)
        {
            return Challenge();
        }


        // Owner is always taken from logged-in account
        // Never trust OwnerId sent from browser
        equipment.OwnerId = userId;

        equipment.CreatedAt =
            DateTime.UtcNow;


        // OwnerId is assigned by the server,
        // so remove any client-side validation issue
        ModelState.Remove(
            nameof(Equipment.OwnerId));


        if (!ModelState.IsValid)
        {
            await LoadCategories(
                equipment.CategoryId);

            return View(equipment);
        }


        _context.Equipment.Add(
            equipment);


        await _context.SaveChangesAsync();


        TempData["Success"] =
            "Equipment listing created successfully.";


        return RedirectToAction(
            nameof(Index));
    }


    // =========================================================
    // EDIT EQUIPMENT
    // GET: /Equipment/Edit/5
    // =========================================================

    public async Task<IActionResult> Edit(
        int? id)
    {
        if (id == null)
        {
            return NotFound();
        }


        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);


        if (userId == null)
        {
            return Challenge();
        }


        // User can only edit their own equipment
        var equipment =
            await _context.Equipment
                .FirstOrDefaultAsync(e =>
                    e.EquipmentId == id &&
                    e.OwnerId == userId);


        if (equipment == null)
        {
            TempData["Error"] =
                "You do not have permission to edit this equipment.";

            return RedirectToAction(
                nameof(Index));
        }


        await LoadCategories(
            equipment.CategoryId);


        return View(equipment);
    }


    // =========================================================
    // EDIT EQUIPMENT
    // POST: /Equipment/Edit/5
    // =========================================================

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Equipment model)
    {
        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);


        if (userId == null)
        {
            return Challenge();
        }


        // Load the real record from database
        // and confirm ownership
        var equipment =
            await _context.Equipment
                .FirstOrDefaultAsync(e =>
                    e.EquipmentId == id &&
                    e.OwnerId == userId);


        if (equipment == null)
        {
            TempData["Error"] =
                "You do not have permission to edit this equipment.";

            return RedirectToAction(
                nameof(Index));
        }


        // OwnerId is controlled by the server
        ModelState.Remove(
            nameof(Equipment.OwnerId));


        if (!ModelState.IsValid)
        {
            await LoadCategories(
                model.CategoryId);

            return View(model);
        }


        // Only update fields the owner is allowed to change
        equipment.Name =
            model.Name;

        equipment.Description =
            model.Description;

        equipment.PricePerDay =
            model.PricePerDay;

        equipment.CategoryId =
            model.CategoryId;

        equipment.IsAvailable =
            model.IsAvailable;

        equipment.ImageUrl =
            model.ImageUrl;


        await _context.SaveChangesAsync();


        TempData["Success"] =
            "Equipment listing updated successfully.";


        return RedirectToAction(
            nameof(Index));
    }


    // =========================================================
    // DELETE EQUIPMENT
    // GET: /Equipment/Delete/5
    // =========================================================

    public async Task<IActionResult> Delete(
        int? id)
    {
        if (id == null)
        {
            return NotFound();
        }


        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);


        if (userId == null)
        {
            return Challenge();
        }


        // Only owner can access delete page
        var equipment =
            await _context.Equipment
                .Include(e => e.Category)
                .FirstOrDefaultAsync(e =>
                    e.EquipmentId == id &&
                    e.OwnerId == userId);


        if (equipment == null)
        {
            TempData["Error"] =
                "You do not have permission to delete this equipment.";

            return RedirectToAction(
                nameof(Index));
        }


        return View(equipment);
    }


    // =========================================================
    // DELETE EQUIPMENT
    // POST: /Equipment/Delete/5
    // =========================================================

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id)
    {
        string? userId =
            User.FindFirstValue(
                ClaimTypes.NameIdentifier);


        if (userId == null)
        {
            return Challenge();
        }


        // Load equipment plus related bookings/reviews
        var equipment =
            await _context.Equipment
                .Include(e => e.Bookings)
                .Include(e => e.Reviews)
                .FirstOrDefaultAsync(e =>
                    e.EquipmentId == id &&
                    e.OwnerId == userId);


        if (equipment == null)
        {
            TempData["Error"] =
                "You do not have permission to delete this equipment.";

            return RedirectToAction(
                nameof(Index));
        }


        // Do not delete equipment with rental history
        if (equipment.Bookings.Any())
        {
            TempData["Error"] =
                "You cannot delete equipment that has rental bookings.";

            return RedirectToAction(
                nameof(Index));
        }


        // Do not delete equipment with reviews
        if (equipment.Reviews.Any())
        {
            TempData["Error"] =
                "You cannot delete equipment that has reviews.";

            return RedirectToAction(
                nameof(Index));
        }


        _context.Equipment.Remove(
            equipment);


        await _context.SaveChangesAsync();


        TempData["Success"] =
            "Equipment listing deleted successfully.";


        return RedirectToAction(
            nameof(Index));
    }


    // =========================================================
    // LOAD CATEGORIES
    // Used by Create and Edit
    // =========================================================

    private async Task LoadCategories(
        int? selectedCategoryId = null)
    {
        var categories =
            await _context.Categories
                .OrderBy(c =>
                    c.Name)
                .ToListAsync();


        ViewBag.CategoryId =
            new SelectList(
                categories,
                "CategoryId",
                "Name",
                selectedCategoryId
            );
    }
}