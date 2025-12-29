using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TarifDefteri.Data;
using TarifDefteri.Models;

namespace TarifDefteri.Controllers;

public class AccountController : Controller
{
    private readonly AppDbContext _context;

    public AccountController(AppDbContext context)
    {
        _context = context;
    }

    // GET: /Account/Login
    public IActionResult Login()
    {
        return View();
    }

    // POST: /Account/Login
    // POST: /Account/Login
[HttpPost]
public async Task<IActionResult> Login(string email, string password)
{
    var user = await _context.Users
        .FirstOrDefaultAsync(u => u.Email == email && u.Password == password);

    if (user == null)
    {
        ViewBag.Error = "Email veya şifre hatalı!";
        return View();
    }

    // ONAY KONTROLÜ
    if (!user.IsApproved)
    {
        ViewBag.Error = "Hesabınız henüz onaylanmamış. Lütfen admin onayını bekleyin.";
        return View();
    }

    // Session'a kullanıcı bilgilerini kaydet
    HttpContext.Session.SetInt32("UserId", user.Id);
    HttpContext.Session.SetString("Username", user.Username);
    HttpContext.Session.SetString("Role", user.Role);

    // Role'e göre yönlendir
    if (user.Role == "Admin")
        return RedirectToAction("Index", "Admin");
    else if (user.Role == "Chef")
        return RedirectToAction("Index", "Chef");
    else
        return RedirectToAction("Index", "Home");
}

    // GET: /Account/Register
    public IActionResult Register()
{
    return View();
}

// POST: /Account/Register
    // POST: /Account/Register
[HttpPost]
public async Task<IActionResult> Register(User user, string confirmPassword, string selectedRole)
{
    if (user.Password != confirmPassword)
    {
        ViewBag.Error = "Şifreler eşleşmiyor!";
        return View();
    }

    // Email kontrolü
    if (await _context.Users.AnyAsync(u => u.Email == user.Email))
    {
        ViewBag.Error = "Bu email zaten kayıtlı!";
        return View();
    }

    // Username kontrolü
    if (await _context.Users.AnyAsync(u => u.Username == user.Username))
    {
        ViewBag.Error = "Bu kullanıcı adı zaten alınmış!";
        return View();
    }

    // Rol belirleme
    if (string.IsNullOrEmpty(selectedRole))
    {
        user.Role = "User";
        user.IsApproved = true; // User direkt onaylı
    }
    else
    {
        user.Role = selectedRole;
        
        // Chef ve Admin için onay beklemeli
        if (selectedRole == "Chef" || selectedRole == "Admin")
        {
            user.IsApproved = false; // ONAY BEKLİYOR
        }
        else
        {
            user.IsApproved = true; // User direkt onaylı
        }
    }

    user.CreatedDate = DateTime.Now;

    _context.Users.Add(user);
    await _context.SaveChangesAsync();

    // Chef veya Admin olarak kaydolduysa bilgilendirme
    if (user.Role == "Chef" || user.Role == "Admin")
    {
        TempData["Info"] = $"{user.Role} hesabınız oluşturuldu. Admin onayı bekleniyor. Onaylandıktan sonra giriş yapabilirsiniz.";
    }

    return RedirectToAction("Login");
}
    // GET: /Account/Logout
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}