using Toolio.Web.Models;

namespace Toolio.Web.ViewModels;

public class OwnerBookingRequestViewModel
{
    public int BookingId { get; set; }

    public int EquipmentId { get; set; }

    public string EquipmentName { get; set; }
        = string.Empty;

    public string RenterEmail { get; set; }
        = string.Empty;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public decimal TotalPrice { get; set; }

    public BookingStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}