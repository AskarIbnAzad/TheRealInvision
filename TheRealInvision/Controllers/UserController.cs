using System.Diagnostics;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TheRealInvision.Interfaces;
using TheRealInvision.Models;
using TheRealInvision.Models.DbModels;

namespace TheRealInvision.Controllers;

public class UserController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUserRepository _userRepository;

    public UserController(ILogger<HomeController> logger, IUserRepository userRepository)
    {
        _logger = logger;
        _userRepository = userRepository;
    }

    public async Task<IActionResult> Index()
    {
        string userId = HttpContext.Session.GetString("UserId");
        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (!user.IsAdmin)
            return RedirectToAction("Logout", "Account");
        var users = await _userRepository.GetAllAsync();
        return View(users);
    }
    public async Task<IActionResult> Create()
    {
        string userId = HttpContext.Session.GetString("UserId");
        var user = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (!user.IsAdmin)
            return RedirectToAction("Logout", "Account");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name, string email, string plainPassword, bool isAdmin = false)
    {
        string userId = HttpContext.Session.GetString("UserId");
        var user = new User
        {
            Name = name,
            Email = email,
            Password = "",
            IsAdmin = isAdmin,
            Status = EntityStatus.Active,
            CreatedDate = DateTime.Now,
            CreatedBy = Guid.Parse(userId)
        };
        await _userRepository.AddWithHashedPasswordAsync(user, plainPassword);

        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(Guid id)
    {
        string userId = HttpContext.Session.GetString("UserId");
        var currentUser = await _userRepository.GetByIdAsync(Guid.Parse(userId));
        if (!currentUser.IsAdmin)
            return RedirectToAction("Logout", "Account");

        var user = await _userRepository.GetByIdAsync(id);
        if (user == null) return NotFound();
        return View(user);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Guid id, User user)
    {
        if (id != user.Id) return NotFound();
        string userId = HttpContext.Session.GetString("UserId");
        user.Status = EntityStatus.Active;
        user.UpdatedBy = Guid.Parse(userId);
        user.UpdatedDate = DateTime.Now;
        await _userRepository.UpdateAsync(user);
        return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    public async Task<IActionResult> Delete(Guid id)
    {
        string userId = HttpContext.Session.GetString("UserId");

        // Perform the delete operation
        await _userRepository.DeleteAsync(id, Guid.Parse(userId));


        return Ok();

    }

}
