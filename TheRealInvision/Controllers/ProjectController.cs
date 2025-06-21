using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Build.Evaluation;
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using TheRealInvision.Interfaces;
using TheRealInvision.Models;
using TheRealInvision.Models.DbModels;
using TheRealInvision.Repositories;
using static System.Net.Mime.MediaTypeNames;
using Project = TheRealInvision.Models.DbModels.Project;

namespace TheRealInvision.Controllers;

public class ProjectController : Controller
{
    private readonly IImageRepository _imageRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;

    public ProjectController(IProjectRepository projectRepository, IImageRepository imageRepository, IUserRepository userRepository)
    {
        _userRepository = userRepository;
        _imageRepository = imageRepository;
        _projectRepository = projectRepository;
    }

    // READ: Get all projects
    public async Task<IActionResult> Index()
    {
        string userId = HttpContext.Session.GetString("UserId");
        var projects = await _projectRepository.GetProjectsByUserIdAsync(Guid.Parse(userId));
        return View(projects);
    }
    public async Task<JsonResult> GetProjectPermission(Guid projectId)
    {
        var users = await _projectRepository.GetProjectPermission(projectId);
        return Json(users);
    }
    public async Task ModifyPermission(Guid projectId, Guid userId, int permission)
    {
        string createdBy = HttpContext.Session.GetString("UserId");
        await _projectRepository.ModifyPermission(projectId, userId, permission, createdBy);
    }
    [HttpGet]
    public async Task<IActionResult> Create(Guid Id)
    {
        if (Id == Guid.Empty)
        {
            return View(new Project());
        }
        else
        {
          var project= await _projectRepository.GetByIdAsync(Id);
          return View(project);
        }        
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Project model, List<IFormFile> images)
    {
        if (model.Id == Guid.Empty)
        {
            string userId = HttpContext.Session.GetString("UserId");
            model.CreatedBy = Guid.Parse(userId);
            model.CreatedDate = DateTime.Now;
            model.ProjectOwner= Guid.Parse(userId);
            foreach (var file in images)
            {
                if (file != null && file.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/projects");
                    // Ensure the directory exists
                    if (!Directory.Exists(folderPath))
                    {
                        Directory.CreateDirectory(folderPath);
                    }
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/projects", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    model.ProjectImages.Add(new ProjectImage
                    {
                        Title = file.FileName.Replace(".png", "").Replace(".jpeg", "").Replace(".jpg", ""),
                        ImageUrl = $"/images/projects/{fileName}",
                        Description = "Uploaded Image",
                        Status= EntityStatus.Active,
                        CreatedBy= Guid.Parse(userId),
                        CreatedDate=DateTime.Now
                    });
                }
            }
            await _projectRepository.AddAsync(model);
        }
        else
        {
            foreach (var file in images)
            {
                string userId = HttpContext.Session.GetString("UserId");
                model.UpdatedBy = Guid.Parse(userId);
                model.UpdatedDate = DateTime.Now;
                if (file != null && file.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
                    var filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images/projects", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    model.ProjectImages.Add(new ProjectImage
                    {
                        Title = file.FileName.Replace(".png", "").Replace(".jpeg", "").Replace(".jpg", ""),
                        ImageUrl = $"/images/projects/{fileName}",
                        Description = "Uploaded Image",
                        Status = EntityStatus.Active,
                        CreatedBy = Guid.Parse(userId),
                        CreatedDate = DateTime.Now
                    });
                }
            }
            await _projectRepository.UpdateAsync(model);
        }
        return RedirectToAction("Details", new {id= model.Id });
    }

    public async Task<IActionResult> Details(Guid id)
    {
        string userId = HttpContext.Session.GetString("UserId");
        var users = await _userRepository.GetAllAsync();
        users = users.Where(p => p.Id != Guid.Parse(userId));
        ViewBag.CapableUser = users;
        var project = await _projectRepository.GetByIdAsGetByIdWithPermissionDetailsAsync(id, Guid.Parse(userId));
        if (project == null)
        {
            return NotFound();
        }
        return View(project);
    }
    [HttpPost]
    public async Task<IActionResult> TransferOwnership(Guid projectId, Guid newOwner)
    {
        string userId = HttpContext.Session.GetString("UserId");
        await _projectRepository.TransferOwnership(projectId, newOwner, Guid.Parse(userId));
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateImageTitle(Guid imageId, string title)
    {
        string userId = HttpContext.Session.GetString("UserId");
        await _projectRepository.UpdateImageTitle(imageId, title, Guid.Parse(userId));
        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateImagePositions([FromBody] List<ProjectImage> images)
    {
        string userId = HttpContext.Session.GetString("UserId");
        if (images == null || !images.Any())
        {
            return BadRequest("Invalid data.");
        }
        await _imageRepository.UpdateImagePositions(images, userId);
        return Ok(new { message = "Positions updated successfully." });
    }

    [HttpPost]
    public async Task<JsonResult> Delete(Guid id)
    {
        try
        {
            await _projectRepository.DeleteAsync(id);
            return Json(new { success = true });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    [HttpPost]
    public async Task<IActionResult> DeleteImage(Guid imageId)
    {
        await _imageRepository.DeleteImageAsync(imageId);
        return Json(new { success = true });
    }

}
