using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarifDefteri.Data;
using TarifDefteri.Models;

namespace TarifDefteri.Controllers;

public class AdminController : Controller
{
    private readonly AppDbContext _context;

    public AdminController(AppDbContext context)
    {
        _context = context;
    }

    // Admin kontrolü - Her action'dan önce çalışır
    private bool CheckAdmin()
    {
        var role = HttpContext.Session.GetString("Role");
        if (role != "Admin")
        {
            return false;
        }
        return true;
    }

    // GET: /Admin - Dashboard
    public async Task<IActionResult> Index()
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Account");

        ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = HttpContext.Session.GetString("Role");

        // İstatistikler
        ViewBag.TotalUsers = await _context.Users.CountAsync();
        ViewBag.TotalRecipes = await _context.Recipes.CountAsync();
        ViewBag.TotalCategories = await _context.Categories.CountAsync();
        ViewBag.TotalComments = await _context.Comments.CountAsync();
        ViewBag.PendingApprovals = await _context.Users.CountAsync(u => !u.IsApproved); 

        return View();
    }

    // GET: /Admin/Categories - Kategori Listesi
    public async Task<IActionResult> Categories()
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Account");

        ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = HttpContext.Session.GetString("Role");

        var categories = await _context.Categories
            .Include(c => c.Recipes)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return View(categories);
    }

    // GET: /Admin/CreateCategory - Kategori Ekleme Formu
    public IActionResult CreateCategory()
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Account");

        ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = HttpContext.Session.GetString("Role");

        return View();
    }

    // POST: /Admin/CreateCategory - Kategori Ekleme
    [HttpPost]
    public async Task<IActionResult> CreateCategory(Category category)
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Account");

        // Navigation property'leri ModelState'den çıkar
        ModelState.Remove("Recipes");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Categories.Add(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kategori başarıyla eklendi!";
                return RedirectToAction("Categories");
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }
        }
        else
        {
            // Hataları göster
            var errors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            TempData["Error"] = $"Form hataları: {errors}";
        }

        ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = HttpContext.Session.GetString("Role");

        return View(category);
    }

    // GET: /Admin/EditCategory/5 - Kategori Düzenleme Formu
    public async Task<IActionResult> EditCategory(int id)
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Account");

        ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = HttpContext.Session.GetString("Role");

        var category = await _context.Categories.FindAsync(id);
        if (category == null)
            return NotFound();

        return View(category);
    }

    // POST: /Admin/EditCategory/5 - Kategori Güncelleme
    [HttpPost]
    public async Task<IActionResult> EditCategory(int id, Category category)
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Account");

        if (id != category.Id)
            return NotFound();

        // Navigation property'leri ModelState'den çıkar
        ModelState.Remove("Recipes");

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(category);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Kategori başarıyla güncellendi!";
                return RedirectToAction("Categories");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Categories.AnyAsync(c => c.Id == id))
                    return NotFound();
                throw;
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Hata: {ex.Message}";
            }
        }
        else
        {
            var errors = string.Join(", ", ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage));
            TempData["Error"] = $"Form hataları: {errors}";
        }

        ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = HttpContext.Session.GetString("Role");

        return View(category);
    }

    // POST: /Admin/DeleteCategory/5 - Kategori Silme
    [HttpPost]
    public async Task<IActionResult> DeleteCategory(int id)
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Account");

        var category = await _context.Categories
            .Include(c => c.Recipes)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (category == null)
            return NotFound();

        // Kategoriye ait tarif varsa silme
        if (category.Recipes.Any())
        {
            TempData["Error"] = "Bu kategoriye ait tarifler var! Önce tarifleri silmelisiniz.";
            return RedirectToAction("Categories");
        }

        _context.Categories.Remove(category);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Kategori başarıyla silindi!";

        return RedirectToAction("Categories");
    }

    // GET: /Admin/Users - Kullanıcı Listesi
    public async Task<IActionResult> Users()
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Account");

        ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = HttpContext.Session.GetString("Role");

        var users = await _context.Users
            .Include(u => u.Recipes)
            .Include(u => u.Comments)
            .OrderByDescending(u => u.CreatedDate)
            .ToListAsync();

        return View(users);
    }

    // POST: /Admin/ApproveUser/5 - Kullanıcı Onaylama
    [HttpPost]
    public async Task<IActionResult> ApproveUser(int id)
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Account");

        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.IsApproved = true;
        await _context.SaveChangesAsync();
        
        TempData["Success"] = $"{user.Username} başarıyla onaylandı!";
        return RedirectToAction("Users");
    }

    // POST: /Admin/RejectUser/5 - Kullanıcı Reddetme
    [HttpPost]
    public async Task<IActionResult> RejectUser(int id)
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Account");

        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        user.IsApproved = false;
        await _context.SaveChangesAsync();
        
        TempData["Success"] = $"{user.Username} onayı geri alındı!";
        return RedirectToAction("Users");
    }

    // POST: /Admin/DeleteUser/5 - Kullanıcı Silme
    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Account");

        var currentUserId = HttpContext.Session.GetInt32("UserId");
        
        // Kendi hesabını silmeye çalışıyorsa
        if (id == currentUserId)
        {
            TempData["Error"] = "Kendi hesabınızı silemezsiniz!";
            return RedirectToAction("Users");
        }

        var user = await _context.Users.FindAsync(id);
        if (user == null)
            return NotFound();

        _context.Users.Remove(user);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Kullanıcı başarıyla silindi!";

        return RedirectToAction("Users");
    }

    // GET: /Admin/Recipes - Tüm Tarifler
    public async Task<IActionResult> Recipes()
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Account");

        ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = HttpContext.Session.GetString("Role");

        var recipes = await _context.Recipes
            .Include(r => r.Category)
            .Include(r => r.User)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

        return View(recipes);
    }

    // POST: /Admin/DeleteRecipe/5 - Tarif Silme
    [HttpPost]
    public async Task<IActionResult> DeleteRecipe(int id)
    {
        if (!CheckAdmin())
            return RedirectToAction("Login", "Account");

        var recipe = await _context.Recipes.FindAsync(id);
        if (recipe == null)
            return NotFound();

        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Tarif başarıyla silindi!";

        return RedirectToAction("Recipes");
    }
}