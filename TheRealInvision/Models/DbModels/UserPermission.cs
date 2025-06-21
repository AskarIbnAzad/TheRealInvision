using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TheRealInvision.Models.DbModels
{
    public class UserPermission : BaseModel
    {
        public Guid UserId { get; set; }

        [JsonIgnore]
        public User User { get; set; } = null!;

        public Guid ProjectId { get; set; }

        [JsonIgnore]
        public Project Project { get; set; } = null!;

        public PermissionLevel Permission { get; set; } = 0;
    }
}
