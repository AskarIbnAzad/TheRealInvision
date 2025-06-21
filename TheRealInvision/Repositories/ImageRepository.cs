using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TheRealInvision.Interfaces;
using TheRealInvision.Models.DbModels;

namespace TheRealInvision.Repositories
{
    public class ImageRepository : IImageRepository
    {
        private readonly AppDbContext _context;

        public ImageRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProjectImage>> GetAllImagesAsync()
        {
            return await _context.ProjectImages.ToListAsync();
        }

        public async Task<ProjectImage?> GetImageByIdAsync(Guid id)
        {
            return await _context.ProjectImages.FindAsync(id);
        }

        public async Task AddImageAsync(ProjectImage image)
        {
            await _context.ProjectImages.AddAsync(image);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteImageAsync(Guid id)
        {
            var image = await GetImageByIdAsync(id);
            if (image != null)
            {
                _context.ProjectImages.Remove(image);
                await _context.SaveChangesAsync();
            }
        }
        public async Task<IEnumerable<ProjectImage>> GetImagesByIdsAsync(IEnumerable<Guid> ids)
        {
            return await _context.ProjectImages
                .Where(img => ids.Contains(img.Id))
                .ToListAsync();
        }

        public async Task UpdateImagePositions(List<ProjectImage> images, string userId)
        {
            List<ProjectImage> editableImages = new List<ProjectImage>();
            foreach (var img in images)
            {
                ProjectImage obj = await _context.ProjectImages.FirstOrDefaultAsync(p => p.Id == img.Id);
                obj.Position = img.Position;
                obj.UpdatedBy = Guid.Parse(userId);
                obj.UpdatedDate = DateTime.Now;
                editableImages.Add(obj);
            }
            _context.ProjectImages.UpdateRange(editableImages);
            _context.SaveChanges();
        }
    }
}
