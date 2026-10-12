using Toolio.Web.Models;

namespace Toolio.Web.ViewModels;

public class EquipmentBrowseViewModel
{
    public List<Equipment> Equipment { get; set; }
        = new List<Equipment>();

    public List<Category> Categories { get; set; }
        = new List<Category>();

    public string? SearchTerm { get; set; }

    public int? CategoryId { get; set; }

    public string Availability { get; set; } = "available";

    public string Sort { get; set; } = "newest";
}