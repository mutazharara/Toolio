using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Toolio.Web.Models;

public enum BookingStatus
{
    Pending,
    Approved,
    Rejected,
    Cancelled,
    Completed
}

public class Booking
{
    public int BookingId { get; set; }

    [Required]
    public int EquipmentId { get; set; }

    public Equipment? Equipment { get; set; }

    [Required]
    public string RenterId { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime StartDate { get; set; }

    [Required]
    [DataType(DataType.Date)]
    public DateTime EndDate { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal TotalPrice { get; set; }

    public BookingStatus Status { get; set; }
        = BookingStatus.Pending;

    public DateTime CreatedAt { get; set; }
        = DateTime.UtcNow;
}