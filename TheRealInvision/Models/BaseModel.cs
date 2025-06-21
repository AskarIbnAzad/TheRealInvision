using System.ComponentModel.DataAnnotations;

namespace TheRealInvision.Models
{
    public abstract class BaseModel
    {
        [Key]
        public Guid Id { get; set; }
        public EntityStatus Status { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public Guid? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }
    public enum PermissionLevel 
    {
        None=0,
        Editor=1,
        Viewer=2
    }
    public enum EntityStatus
    {
        Active = 1,
        InActive = 2,
        Deleted=3
    }

    public class ProjectViewModel
    {
        public Guid Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        public List<IFormFile> Images { get; set; } = new List<IFormFile>();

        public List<ProjectImageViewModel> ExistingImages { get; set; } = new List<ProjectImageViewModel>();

        public List<Guid> DeletedImageIds { get; set; } = new List<Guid>();
        public List<ProjectImageViewModel> AvailableImages { get; set; } = new List<ProjectImageViewModel>();
        public List<Guid> SelectedImageIds { get; set; } = new List<Guid>();
    }

    public class ProjectImageViewModel
    {
        public Guid Id { get; set; } // Unique ID of the image
        public string ImageUrl { get; set; } // Path or URL to the image
        public string? Description { get; set; } // Optional description
    }

}
