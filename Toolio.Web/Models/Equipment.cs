using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Toolio.Web.Models;

public class Equipment
{
    public int EquipmentId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0.01, 10000)]
    [Column(TypeName = "decimal(10,2)")]
    public decimal PricePerDay { get; set; }

    public bool IsAvailable { get; set; } = true;

    public string? ImageUrl { get; set; }

    // Category relationship
    [Required]
    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    // We will connect this to the logged-in user next
    public string OwnerId { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}