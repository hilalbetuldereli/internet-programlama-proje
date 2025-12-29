namespace TarifDefteri.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public bool IsApproved { get; set; } = true; 
    
    // Navigation Properties
    public ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}