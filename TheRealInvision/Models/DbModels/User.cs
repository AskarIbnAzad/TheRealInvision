using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheRealInvision.Models.DbModels
{
    public class User : BaseModel
    {
        public required string Name { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public bool IsAdmin { get; set; } = false;
        [NotMapped]
        public PermissionLevel Permission { get; set; } = 0;
    }
}
