using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarifDefteri.Data;
using TarifDefteri.Models;

namespace TarifDefteri.Controllers;

public class ChefController : Controller
{
    private readonly AppDbContext _context;

    public ChefController(AppDbContext context)
    {
        _context = context;
    }

    // Chef kontrolü
    private bool CheckChef()
    {
        var role = HttpContext.Session.GetString("Role");
        return role == "Chef" || role == "Admin";
    }

    private int? GetUserId()
    {
        return HttpContext.Session.GetInt32("UserId");
    }

    // GET: /Chef - Dashboard
    public async Task<IActionResult> Index()
    {
        if (!CheckChef())
            return RedirectToAction("Login", "Account");

        var userId = GetUserId();
        ViewBag.UserId = userId;
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = HttpContext.Session.GetString("Role");

        // Chef'in tarifleri
        var recipes = await _context.Recipes
            .Include(r => r.Category)
            .Include(r => r.Ratings)
            .Include(r => r.Comments)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

        // İstatistikler
        ViewBag.TotalRecipes = recipes.Count;
        ViewBag.TotalViews = recipes.Sum(r => r.ViewCount);
        ViewBag.TotalComments = recipes.Sum(r => r.Comments.Count);
        ViewBag.AverageRating = recipes
            .Where(r => r.Ratings.Any())
            .Average(r => r.Ratings.Average(rating => (double?)rating.Score)) ?? 0;

        return View(recipes);
    }

    // GET: /Chef/Create - Tarif Ekleme Formu
    public async Task<IActionResult> Create()
    {
        if (!CheckChef())
            return RedirectToAction("Login", "Account");

        ViewBag.UserId = GetUserId();
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = HttpContext.Session.GetString("Role");
        ViewBag.Categories = await _context.Categories.ToListAsync();

        return View();
    }

    // POST: /Chef/Create - Tarif Ekleme
    
[HttpPost]
public async Task<IActionResult> Create(Recipe recipe)
{
    if (!CheckChef())
        return RedirectToAction("Login", "Account");

    var userId = GetUserId();
    if (!userId.HasValue)
        return RedirectToAction("Login", "Account");

    recipe.UserId = userId.Value;
    recipe.CreatedDate = DateTime.Now;
    recipe.ViewCount = 0;

    // ImageUrl boşsa varsayılan değer
    if (string.IsNullOrEmpty(recipe.ImageUrl))
    {
        recipe.ImageUrl = "";
    }

    // ModelState'i temizle (Navigation property'ler sorun çıkarabiliyor)
    ModelState.Remove("Category");
    ModelState.Remove("User");
    ModelState.Remove("Comments");
    ModelState.Remove("Ratings");
    ModelState.Remove("Favorites");

    if (ModelState.IsValid)
    {
        _context.Recipes.Add(recipe);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Tarif başarıyla eklendi!";
        return RedirectToAction("Index");
    }

    // Hata varsa göster
    var errors = ModelState.Values.SelectMany(v => v.Errors);
    foreach (var error in errors)
    {
        Console.WriteLine($"Model Error: {error.ErrorMessage}");
    }

    ViewBag.UserId = userId;
    ViewBag.Username = HttpContext.Session.GetString("Username");
    ViewBag.Role = HttpContext.Session.GetString("Role");
    ViewBag.Categories = await _context.Categories.ToListAsync();

    return View(recipe);
}

    // GET: /Chef/Edit/5 - Tarif Düzenleme Formu
    public async Task<IActionResult> Edit(int id)
    {
        if (!CheckChef())
            return RedirectToAction("Login", "Account");

        var userId = GetUserId();
        var recipe = await _context.Recipes.FindAsync(id);

        if (recipe == null)
            return NotFound();

        // Sadece kendi tarifini düzenleyebilir (Admin hariç)
        var role = HttpContext.Session.GetString("Role");
        if (recipe.UserId != userId && role != "Admin")
            return Forbid();

        ViewBag.UserId = userId;
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = role;
        ViewBag.Categories = await _context.Categories.ToListAsync();

        return View(recipe);
    }

    // POST: /Chef/Edit/5 - Tarif Güncelleme
    [HttpPost]
    public async Task<IActionResult> Edit(int id, Recipe recipe)
    {
        if (!CheckChef())
            return RedirectToAction("Login", "Account");

        if (id != recipe.Id)
            return NotFound();

        var userId = GetUserId();
        var existingRecipe = await _context.Recipes.AsNoTracking().FirstOrDefaultAsync(r => r.Id == id);

        if (existingRecipe == null)
            return NotFound();

        // Sadece kendi tarifini düzenleyebilir (Admin hariç)
        var role = HttpContext.Session.GetString("Role");
        if (existingRecipe.UserId != userId && role != "Admin")
            return Forbid();

        // Değişmeyen alanları koru
        recipe.UserId = existingRecipe.UserId;
        recipe.CreatedDate = existingRecipe.CreatedDate;
        recipe.ViewCount = existingRecipe.ViewCount;

        if (ModelState.IsValid)
        {
            try
            {
                _context.Update(recipe);
                await _context.SaveChangesAsync();
                TempData["Success"] = "Tarif başarıyla güncellendi!";
                return RedirectToAction("Index");
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Recipes.AnyAsync(r => r.Id == id))
                    return NotFound();
                throw;
            }
        }

        ViewBag.UserId = userId;
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = role;
        ViewBag.Categories = await _context.Categories.ToListAsync();

        return View(recipe);
    }

    // GET: /Chef/Details/5 - Tarif Detay (Chef Görünümü)
    public async Task<IActionResult> Details(int id)
    {
        if (!CheckChef())
            return RedirectToAction("Login", "Account");

        var userId = GetUserId();
        var recipe = await _context.Recipes
            .Include(r => r.Category)
            .Include(r => r.User)
            .Include(r => r.Comments)
                .ThenInclude(c => c.User)
            .Include(r => r.Ratings)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe == null)
            return NotFound();

        // Sadece kendi tarifini görebilir (Admin hariç)
        var role = HttpContext.Session.GetString("Role");
        if (recipe.UserId != userId && role != "Admin")
            return Forbid();

        ViewBag.UserId = userId;
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = role;

        // İstatistikler
        ViewBag.AverageRating = recipe.Ratings.Any() 
            ? recipe.Ratings.Average(r => r.Score) 
            : 0;

        return View(recipe);
    }

    // POST: /Chef/Delete/5 - Tarif Silme
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        if (!CheckChef())
            return RedirectToAction("Login", "Account");

        var userId = GetUserId();
        var recipe = await _context.Recipes.FindAsync(id);

        if (recipe == null)
            return NotFound();

        // Sadece kendi tarifini silebilir (Admin hariç)
        var role = HttpContext.Session.GetString("Role");
        if (recipe.UserId != userId && role != "Admin")
            return Forbid();

        _context.Recipes.Remove(recipe);
        await _context.SaveChangesAsync();
        TempData["Success"] = "Tarif başarıyla silindi!";

        return RedirectToAction("Index");
    }
}