using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PortfolioAdmin.Api.Models;

[Table("RoleMenus")]
public class RoleMenu
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }

    public int RoleId { get; set; }

    public int MenuId { get; set; }

    // 导航属性
    [ForeignKey(nameof(RoleId))]
    public Role? Role { get; set; }

    [ForeignKey(nameof(MenuId))]
    public Menu? Menu { get; set; }
}
