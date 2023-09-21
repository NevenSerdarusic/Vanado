using System.ComponentModel.DataAnnotations;

namespace EquipmentManagement.API.Models;

public class Machine
{
    [Key]
    public int Id { get; set; }

    [Required]
    [MaxLength(50)]
    public string Name { get; set; }

}
