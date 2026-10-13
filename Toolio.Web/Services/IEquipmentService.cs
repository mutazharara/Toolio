using Toolio.Web.Models;

namespace Toolio.Web.Services;

public interface IEquipmentService
{
    Task<List<Equipment>> SearchEquipmentAsync(
        string? search,
        int? categoryId,
        string availability,
        string sort);

    Task<Equipment?> GetEquipmentByIdAsync(int id);
}
