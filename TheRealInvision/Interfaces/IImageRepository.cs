using TheRealInvision.Models.DbModels;

namespace TheRealInvision.Interfaces
{
    public interface IImageRepository
    {
        Task<IEnumerable<ProjectImage>> GetAllImagesAsync();
        Task<ProjectImage?> GetImageByIdAsync(Guid id);
        Task AddImageAsync(ProjectImage image);
        Task DeleteImageAsync(Guid id);
        Task<IEnumerable<ProjectImage>> GetImagesByIdsAsync(IEnumerable<Guid> ids);
        Task UpdateImagePositions(List<ProjectImage> images, string userId);
    }
}
