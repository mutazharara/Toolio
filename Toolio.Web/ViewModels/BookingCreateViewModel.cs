using System.ComponentModel.DataAnnotations;

namespace Toolio.Web.ViewModels;

public class BookingCreateViewModel
{
    public int EquipmentId { get; set; }

    public string EquipmentName { get; set; }
        = string.Empty;

    public decimal PricePerDay { get; set; }

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Start date")]
    public DateTime StartDate { get; set; }
        = DateTime.Today;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "End date")]
    public DateTime EndDate { get; set; }
        = DateTime.Today;
}