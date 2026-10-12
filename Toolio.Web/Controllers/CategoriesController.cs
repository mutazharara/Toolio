using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Toolio.Web.Data;
using Toolio.Web.Models;

namespace Toolio.Web.Controllers;

[Authorize(Roles = "Admin")]
public class CategoriesController : Controller
{
    private readonly ApplicationDbContext _context;

    public CategoriesController(
        ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: Categories
    public async Task<IActionResult> Index()
    {
        var categories =
        await _context.Categories
            .Include(c => c.Equipment)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }

    // GET: Categories/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Categories/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        Category category)
    {
        if (!ModelState.IsValid)
        {
            return View(category);
        }

        bool categoryExists =
            await _context.Categories
                .AnyAsync(c =>
                    c.Name.ToLower()
                    == category.Name.ToLower());

        if (categoryExists)
        {
            ModelState.AddModelError(
                "Name",
                "This category already exists."
            );

            return View(category);
        }

        _context.Categories.Add(category);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category =
            await _context.Categories.FindAsync(id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: Categories/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(
        int id,
        Category category)
    {
        if (id != category.CategoryId)
        {
            return NotFound();
        }

        if (!ModelState.IsValid)
        {
            return View(category);
        }

        try
        {
            _context.Update(category);

            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            bool exists =
                await _context.Categories
                    .AnyAsync(c =>
                        c.CategoryId ==
                        category.CategoryId);

            if (!exists)
            {
                return NotFound();
            }

            throw;
        }

        return RedirectToAction(nameof(Index));
    }

    // GET: Categories/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var category =
            await _context.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == id);

        if (category == null)
        {
            return NotFound();
        }

        return View(category);
    }

    // POST: Categories/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(
        int id)
    {
        var category =
            await _context.Categories
                .Include(c => c.Equipment)
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == id);

        if (category == null)
        {
            return NotFound();
        }

        if (category.Equipment.Any())
        {
            TempData["Error"] =
                "You cannot delete a category that contains equipment.";

            return RedirectToAction(nameof(Index));
        }

        _context.Categories.Remove(category);

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }
}