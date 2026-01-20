using System.ComponentModel.DataAnnotations.Schema;

namespace MobileWebApi.Models
{
    [Table("WorkRole")]
    public class WorkRole
    {
        public int WorkRoleId { get; set; }
        public string WorkRoleName { get; set; } = string.Empty;
    }

}
