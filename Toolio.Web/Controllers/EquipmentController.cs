using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Toolio.Web.Data;
using Toolio.Web.Models;
using Toolio.Web.ViewModels;

namespace Toolio.Web.Controllers;

[Authorize]
public class EquipmentController : Controller
{
    private readonly ApplicationDbContext _context;

    public EquipmentController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /Equipment/Browse
    [AllowAnonymous]
    public async Task<IActionResult> Browse(
        string? search,
        int? categoryId,
        string availability = "available",
        string sort = "newest")
    {
        var query = _context.Equipment
            .Include(e => e.Category)
            .AsNoTracking()
            .AsQueryable();

        // Search
        if (!string.IsNullOrWhiteSpace(search))
        {
            string searchTerm = search.Trim();

            query = query.Where(e =>
                e.Name.Contains(searchTerm) ||
                e.Description.Contains(searchTerm) ||
                (e.Category != null &&
                e.Category.Name.Contains(searchTerm)));
        }

        // Category filter
        if (categoryId.HasValue)
        {
            query = query.Where(e =>
                e.CategoryId == categoryId.Value);
        }

        // Availability filter
        if (availability != "all")
        {
            query = query.Where(e =>
                e.IsAvailable);
        }

        // Sorting
        query = sort switch
        {
            "price-low" =>
                query.OrderBy(e => e.PricePerDay),

            "price-high" =>
                query.OrderByDescending(e => e.PricePerDay),

            "name" =>
                query.OrderBy(e => e.Name),

            _ =>
                query.OrderByDescending(e => e.CreatedAt)
        };

        var model = new EquipmentBrowseViewModel
        {
            Equipment = await query.ToListAsync(),

            Categories = await _context.Categories
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

    // GET: /Equipment/Details/5
    [AllowAnonymous]
    public async Task<IActionResult> Details(int id)
    {
        var equipment = await _context.Equipment
            .Include(e => e.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.EquipmentId == id);

        if (equipment == null)
        {
            return NotFound();
        }

        return View(equipment);
    }


    // GET: /Equipment
    // Shows only equipment owned by the logged-in user
    public async Task<IActionResult> Index()
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        var equipment = await _context.Equipment
            .Include(e => e.Category)
            .Where(e => e.OwnerId == userId)
            .OrderByDescending(e => e.CreatedAt)
            .ToListAsync();

        return View(equipment);
    }

    // GET: /Equipment/Create
    public async Task<IActionResult> Create()
    {
        await LoadCategories();

        return View(new Equipment());
    }

    // POST: /Equipment/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Equipment equipment)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId == null)
        {
            return Challenge();
        }

        // Owner is always taken from the logged-in account
        equipment.OwnerId = userId;
        equipment.CreatedAt = DateTime.UtcNow;

        if (!ModelState.IsValid)
        {
            await LoadCategories(equipment.CategoryId);

            return View(equipment);
        }

        _context.Equipment.Add(equipment);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Equipment listing created successfully.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Equipment/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var equipment = await _context.Equipment
            .FirstOrDefaultAsync(e =>
                e.EquipmentId == id &&
                e.OwnerId == userId);

        if (equipment == null)
        {
            return NotFound();
        }

        await LoadCategories(equipment.CategoryId);

        return View(equipment);
    }

    // POST: /Equipment/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Equipment model)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var equipment = await _context.Equipment
            .FirstOrDefaultAsync(e =>
                e.EquipmentId == id &&
                e.OwnerId == userId);

        if (equipment == null)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            await LoadCategories(model.CategoryId);

            return View(model);
        }

        equipment.Name = model.Name;
        equipment.Description = model.Description;
        equipment.PricePerDay = model.PricePerDay;
        equipment.CategoryId = model.CategoryId;
        equipment.IsAvailable = model.IsAvailable;
        equipment.ImageUrl = model.ImageUrl;

        await _context.SaveChangesAsync();

        TempData["Success"] = "Equipment listing updated successfully.";

        return RedirectToAction(nameof(Index));
    }

    // GET: /Equipment/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var equipment = await _context.Equipment
            .Include(e => e.Category)
            .FirstOrDefaultAsync(e =>
                e.EquipmentId == id &&
                e.OwnerId == userId);

        if (equipment == null)
        {
            return NotFound();
        }

        return View(equipment);
    }

    // POST: /Equipment/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        string? userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var equipment = await _context.Equipment
            .FirstOrDefaultAsync(e =>
                e.EquipmentId == id &&
                e.OwnerId == userId);

        if (equipment == null)
        {
            return NotFound();
        }

        _context.Equipment.Remove(equipment);

        await _context.SaveChangesAsync();

        TempData["Success"] = "Equipment listing deleted.";

        return RedirectToAction(nameof(Index));
    }

    private async Task LoadCategories(int? selectedCategoryId = null)
    {
        var categories = await _context.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();

        ViewBag.CategoryId = new SelectList(
            categories,
            "CategoryId",
            "Name",
            selectedCategoryId
        );
    }
}