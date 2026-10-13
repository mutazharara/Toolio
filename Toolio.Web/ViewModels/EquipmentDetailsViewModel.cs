using Toolio.Web.Models;

namespace Toolio.Web.ViewModels;

public class EquipmentDetailsViewModel
{
    public Equipment Equipment { get; set; }
        = new Equipment();

    public List<Review> Reviews { get; set; }
        = new List<Review>();

    public double AverageRating { get; set; }

    public int ReviewCount { get; set; }
}