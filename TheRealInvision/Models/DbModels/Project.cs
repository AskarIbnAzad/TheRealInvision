using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TheRealInvision.Models.DbModels
{
    public class Project : BaseModel
    {

        public string Title { get; set; }
        public string Description { get; set; }
        public Guid ProjectOwner { get; set; }
        public List<ProjectImage> ProjectImages { get; set; } = new List<ProjectImage>();
     
        [NotMapped]
        public bool IsOwner { get; set; } = false;

        [NotMapped]
        public bool IsAdmin { get; set; } = false;
        [NotMapped]
        public bool IsEditor { get; set; } = false;
    }
}
