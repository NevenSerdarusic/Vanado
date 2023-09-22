using EquipmentManagement.API.Helper;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Principal;

namespace EquipmentManagement.API.Models;

public class Failure
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

    [Required]
    public int MachineId { get; set; }

    [ForeignKey("MachineId")]
    public Machine? Machine { get; set; }

    [Required]
    public Priority Priority { get; set; }

    [Required]
    public DateTime StartTime { get; set; }

    public DateTime? EndTime
    {
        get
        {
            if (IsResolved)
            {
                return DateTime.UtcNow;
            }
            return null;
        }
        set
        {

        }
    }

    [Required]
    [Column(TypeName = "nvarchar(3000)")]
    public string Description { get; set; }

    public bool IsResolved { get; set; } = false;
}
