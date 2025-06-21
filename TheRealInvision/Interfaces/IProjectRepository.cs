using Microsoft.AspNetCore.Http.HttpResults;
using TheRealInvision.Models.DbModels;

namespace TheRealInvision.Interfaces
{
    public interface IProjectRepository
    {
        Task<IEnumerable<Project>> GetAllAsync();
        Task<IEnumerable<Project>> GetProjectsByUserIdAsync(Guid userId);
        Task<Project?> GetByIdAsync(Guid id);
        Task<Project?> GetByIdAsGetByIdWithPermissionDetailsAsync(Guid id, Guid userId);
        Task AddAsync(Project project);
        Task UpdateAsync(Project project);
        Task DeleteAsync(Guid id);
        Task<List<User>> GetProjectPermission(Guid projectId);
        Task ModifyPermission(Guid projectId, Guid userId, int permission,string createdBy);
        Task TransferOwnership(Guid projectId, Guid newOwner,Guid createdBy);
        Task UpdateImageTitle(Guid imageId,string title, Guid createdBy);
    }
}
