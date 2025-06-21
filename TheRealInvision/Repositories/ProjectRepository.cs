using Microsoft.EntityFrameworkCore;
using TheRealInvision.Interfaces;
using TheRealInvision.Models;
using TheRealInvision.Models.DbModels;

namespace TheRealInvision.Repositories
{
    public class ProjectRepository : IProjectRepository
    {
        private readonly AppDbContext _context;

        public ProjectRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Project>> GetAllAsync()
        {
            return await _context.Projects.Where(p=>p.Status== EntityStatus.Active).Include(p => p.ProjectImages).ToListAsync();
        }

        public async Task<Project?> GetByIdAsync(Guid id)
        {
            return await _context.Projects.Include(p => p.ProjectImages).FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Project project)
        {
            project.Status = EntityStatus.Active;
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();
            await ManageImagePositions(project.Id);
        }

        public async Task UpdateAsync(Project project)
        {
            project.Status = EntityStatus.Active;
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            await ManageImagePositions(project.Id);
        }
        private async Task ManageImagePositions(Guid ProjectId)
        {
            List<ProjectImage> sortedList = new List<ProjectImage>();
            List<ProjectImage> images = await _context.ProjectImages.Where(p => p.ProjectId == ProjectId).OrderBy(p=>p.Position).ToListAsync();
            int index = 1;
            foreach (var img in images)
            {
                img.Position = index++;
                sortedList.Add(img);
            }
            _context.ProjectImages.UpdateRange(sortedList);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Guid id)
        {
            var project = await GetByIdAsync(id);
            if (project != null)
            {
                project.Status = EntityStatus.Deleted;
                _context.Projects.Update(project);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<User>> GetProjectPermission(Guid projectId)
        {
            List<UserPermission> permissions = await _context.UserPermissions.Where(p => p.ProjectId == projectId).ToListAsync();
            List<User> allUsers = await _context.Users.ToListAsync();
            foreach (var obj in allUsers)
            {
                if (permissions.Any(p => p.UserId == obj.Id))
                {
                    obj.Permission = permissions.FirstOrDefault(p => p.UserId == obj.Id).Permission;
                }
            }
            return allUsers;
        }

        public async Task ModifyPermission(Guid projectId, Guid userId, int permission, string createdBy)
        {
            var permissions = _context.UserPermissions.Where(p => p.ProjectId == projectId && p.UserId == userId).ToList();
            if (permissions != null && permissions.Count > 0)
            {
                _context.UserPermissions.RemoveRange(permissions);
                await _context.SaveChangesAsync();            
            }
            if (permission != 0)
            {
                UserPermission node = new UserPermission();
                node.ProjectId = projectId;
                node.UserId = userId;
                node.Permission = (PermissionLevel)permission;
                node.Status = EntityStatus.Active;
                node.CreatedBy = Guid.Parse(createdBy);
                node.CreatedDate = DateTime.Now;
                await _context.UserPermissions.AddAsync(node);
                await _context.SaveChangesAsync();
            }
        }

        public async Task TransferOwnership(Guid projectId, Guid newOwnerId, Guid createdBy)
        {
            var project = await _context.Projects.FirstOrDefaultAsync(p => p.Id == projectId);
            Guid previousOwnerId = project.ProjectOwner;
            project.ProjectOwner = newOwnerId;
            project.UpdatedBy = createdBy;
            project.UpdatedDate = DateTime.Now;
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();

            var newOwner = await _context.Users.FirstOrDefaultAsync(p => p.Id == newOwnerId);
            var newOwnerPermission = await _context.UserPermissions.Where(p => p.ProjectId == projectId && p.UserId == newOwner.Id).ToListAsync();
            _context.UserPermissions.RemoveRange(newOwnerPermission);
            _context.SaveChanges();

            var previousOwner = await _context.Users.FirstOrDefaultAsync(p => p.Id == previousOwnerId);
            if (previousOwner.IsAdmin)
            {
                var previousOwnerPermission = await _context.UserPermissions.Where(p => p.ProjectId == projectId && p.UserId == previousOwner.Id).ToListAsync();
                _context.UserPermissions.RemoveRange(previousOwnerPermission);
                _context.SaveChanges();
            }
            else 
            {
                UserPermission permission = new UserPermission();
                permission.ProjectId = projectId;
                permission.UserId = previousOwner.Id;
                permission.Permission = PermissionLevel.Editor;
                permission.Status = EntityStatus.Active;
                permission.CreatedBy = createdBy;
                permission.CreatedDate = DateTime.Now;

                await _context.UserPermissions.AddAsync(permission);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateImageTitle(Guid imageId, string title, Guid createdBy)
        {
            var image = await _context.ProjectImages.FirstOrDefaultAsync(p => p.Id == imageId);
            image.Title = title;
            image.UpdatedBy = createdBy;
            image.UpdatedDate = DateTime.Now;
            _context.Update(image);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Project>> GetProjectsByUserIdAsync(Guid userId)
        {
            List<Project> UserProjects = new List<Project>();

            var currentUser = await _context.Users.FirstOrDefaultAsync(p=>p.Id== userId);
            if (currentUser.IsAdmin)
            {
                UserProjects= await _context.Projects.Where(p => p.Status == EntityStatus.Active).Include(p => p.ProjectImages).ToListAsync();
                UserProjects.ForEach(project => project.IsAdmin = true);
            }
            else 
            {
                UserProjects = await _context.Projects.Where(p => p.ProjectOwner == userId && p.Status == EntityStatus.Active).Include(p => p.ProjectImages).ToListAsync();
                UserProjects.ForEach(project => project.IsOwner = true);
                List<Guid> ownedProjectId = await _context.UserPermissions.Where(p => p.UserId == userId).Select(p => p.ProjectId).ToListAsync();
                var ownedProject = await _context.Projects.Where(p => ownedProjectId.Contains(p.Id) && p.Status == EntityStatus.Active).Include(p => p.ProjectImages).ToListAsync();
                foreach (var obj in ownedProject)
                {
                    var isEditor = await _context.UserPermissions.AnyAsync(p => p.ProjectId == obj.Id && p.UserId == userId && p.Permission == PermissionLevel.Editor);
                    if (isEditor)
                    {
                        obj.IsEditor = true;
                    }
                }
                UserProjects.AddRange(ownedProject);
            }
            return UserProjects;
        }

        public async Task<Project?> GetByIdAsGetByIdWithPermissionDetailsAsync(Guid id, Guid userId)
        {
            var Project= await _context.Projects.Include(p => p.ProjectImages).FirstOrDefaultAsync(p => p.Id == id);
            var user = await _context.Users.FirstOrDefaultAsync(p => p.Id == userId);
            if (user.IsAdmin)
            {
                Project.IsAdmin = true;
            }
            if (Project.ProjectOwner == userId)
            {
                Project.IsOwner = true;
            }
            else 
            {
                bool IsEditor = await _context.UserPermissions.AnyAsync(p => p.ProjectId == Project.Id && p.UserId == user.Id && p.Permission == PermissionLevel.Editor);
                if (IsEditor)
                {
                    Project.IsEditor = true;
                }
            }
            return Project;     
        }
    }
}
