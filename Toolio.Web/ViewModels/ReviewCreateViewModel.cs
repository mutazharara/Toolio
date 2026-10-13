using System.ComponentModel.DataAnnotations;

namespace Toolio.Web.ViewModels;

public class ReviewCreateViewModel
{
    public int BookingId { get; set; }

    public string EquipmentName { get; set; }
        = string.Empty;

    [Required]
    [Range(1, 5)]
    public int Rating { get; set; }

    [Required]
    [StringLength(500)]
    public string Comment { get; set; }
        = string.Empty;
}