using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace TheRealInvision.Models.DbModels
{
    public class ProjectImage : BaseModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public int Position { get; set; } = 0;
        public Guid ProjectId { get; set; }

        [JsonIgnore]
        public Project Project { get; set; } = null!;
    }
}
