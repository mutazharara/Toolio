using Microsoft.EntityFrameworkCore;
using Toolio.Web.Data;
using Toolio.Web.Models;

namespace Toolio.Web.Services;

public class EquipmentService : IEquipmentService
{
    private readonly ApplicationDbContext _context;

    public EquipmentService(
        ApplicationDbContext context)
    {
        _context = context;
    }


    public async Task<List<Equipment>> SearchEquipmentAsync(
        string? search,
        int? categoryId,
        string availability,
        string sort)
    {
        var query = _context.Equipment
            .Include(e => e.Category)
            .AsNoTracking()
            .AsQueryable();


        if (!string.IsNullOrWhiteSpace(search))
        {
            string searchTerm = search.Trim();

            query = query.Where(e =>
                e.Name.Contains(searchTerm) ||
                e.Description.Contains(searchTerm) ||
                (
                    e.Category != null &&
                    e.Category.Name.Contains(searchTerm)
                ));
        }


        if (categoryId.HasValue)
        {
            query = query.Where(e =>
                e.CategoryId == categoryId.Value);
        }


        if (availability != "all")
        {
            query = query.Where(e =>
                e.IsAvailable);
        }


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


        return await query.ToListAsync();
    }


    public async Task<Equipment?> GetEquipmentByIdAsync(
        int id)
    {
        return await _context.Equipment
            .Include(e => e.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(e =>
                e.EquipmentId == id);
    }
}
