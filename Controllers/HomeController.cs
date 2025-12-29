using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarifDefteri.Data;
using TarifDefteri.Models;

namespace TarifDefteri.Controllers;

public class HomeController : Controller
{
    private readonly AppDbContext _context;

    public HomeController(AppDbContext context)
    {
        _context = context;
    }

    // Ana Sayfa - Tüm Tarifler
    public async Task<IActionResult> Index(string search, int? categoryId)
    {
        // Kullanıcı bilgilerini ViewBag'e koy (navbar için)
        ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = HttpContext.Session.GetString("Role");

        // Kategorileri getir (filtre için)
        ViewBag.Categories = await _context.Categories.ToListAsync();

        // Tarifleri getir
        var recipes = _context.Recipes
            .Include(r => r.Category)
            .Include(r => r.User)
            .Include(r => r.Ratings)
            .AsQueryable();

        // Arama filtresi
        if (!string.IsNullOrEmpty(search))
        {
            recipes = recipes.Where(r => 
                r.Title.Contains(search) || 
                r.Description.Contains(search) ||
                r.Ingredients.Contains(search));
            ViewBag.Search = search;
        }

        // Kategori filtresi
        if (categoryId.HasValue)
        {
            recipes = recipes.Where(r => r.CategoryId == categoryId.Value);
            ViewBag.SelectedCategory = categoryId;
        }

        var recipeList = await recipes
            .OrderByDescending(r => r.CreatedDate)
            .ToListAsync();

        return View(recipeList);
    }

    // Tarif Detay Sayfası
    public async Task<IActionResult> Details(int id)
    {
        ViewBag.UserId = HttpContext.Session.GetInt32("UserId");
        ViewBag.Username = HttpContext.Session.GetString("Username");
        ViewBag.Role = HttpContext.Session.GetString("Role");

        var recipe = await _context.Recipes
            .Include(r => r.Category)
            .Include(r => r.User)
            .Include(r => r.Comments)
                .ThenInclude(c => c.User)
            .Include(r => r.Ratings)
            .Include(r => r.Favorites)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (recipe == null)
            return NotFound();

        // Görüntülenme sayısını artır
        recipe.ViewCount++;
        await _context.SaveChangesAsync();

        // Ortalama puanı hesapla
        ViewBag.AverageRating = recipe.Ratings.Any() 
            ? recipe.Ratings.Average(r => r.Score) 
            : 0;
        
        // Kullanıcının puanını kontrol et
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId.HasValue)
        {
            ViewBag.UserRating = await _context.Ratings
                .FirstOrDefaultAsync(r => r.RecipeId == id && r.UserId == userId.Value);
            
            ViewBag.IsFavorite = await _context.Favorites
                .AnyAsync(f => f.RecipeId == id && f.UserId == userId.Value);
        }

        return View(recipe);
    }

    // Yorum Ekle
    [HttpPost]
    public async Task<IActionResult> AddComment(int recipeId, string text)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var comment = new Comment
        {
            RecipeId = recipeId,
            UserId = userId.Value,
            Text = text,
            CreatedDate = DateTime.Now
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();

        return RedirectToAction("Details", new { id = recipeId });
    }

    // Puan Ver
    [HttpPost]
    public async Task<IActionResult> AddRating(int recipeId, int score)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        // Önceki puanı kontrol et
        var existingRating = await _context.Ratings
            .FirstOrDefaultAsync(r => r.RecipeId == recipeId && r.UserId == userId.Value);

        if (existingRating != null)
        {
            // Güncelle
            existingRating.Score = score;
        }
        else
        {
            // Yeni puan ekle
            var rating = new Rating
            {
                RecipeId = recipeId,
                UserId = userId.Value,
                Score = score
            };
            _context.Ratings.Add(rating);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { id = recipeId });
    }

    // Favorilere Ekle/Çıkar
    [HttpPost]
    public async Task<IActionResult> ToggleFavorite(int recipeId)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (!userId.HasValue)
            return RedirectToAction("Login", "Account");

        var favorite = await _context.Favorites
            .FirstOrDefaultAsync(f => f.RecipeId == recipeId && f.UserId == userId.Value);

        if (favorite != null)
        {
            // Favorilerden çıkar
            _context.Favorites.Remove(favorite);
        }
        else
        {
            // Favorilere ekle
            var newFavorite = new Favorite
            {
                RecipeId = recipeId,
                UserId = userId.Value,
                AddedDate = DateTime.Now
            };
            _context.Favorites.Add(newFavorite);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction("Details", new { id = recipeId });
    }
    // GET: /Home/Favorites - Favorilerim
public async Task<IActionResult> Favorites()
{
    var userId = HttpContext.Session.GetInt32("UserId");
    if (!userId.HasValue)
        return RedirectToAction("Login", "Account");

    ViewBag.UserId = userId;
    ViewBag.Username = HttpContext.Session.GetString("Username");
    ViewBag.Role = HttpContext.Session.GetString("Role");

    var favorites = await _context.Favorites
        .Include(f => f.Recipe)
            .ThenInclude(r => r.Category)
        .Include(f => f.Recipe)
            .ThenInclude(r => r.User)
        .Include(f => f.Recipe)
            .ThenInclude(r => r.Ratings)
        .Where(f => f.UserId == userId.Value)
        .OrderByDescending(f => f.AddedDate)
        .ToListAsync();

    return View(favorites);
}
}