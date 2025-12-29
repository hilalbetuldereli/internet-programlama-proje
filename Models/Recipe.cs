namespace TarifDefteri.Models;

public class Recipe
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Ingredients { get; set; } = string.Empty;
    public string Instructions { get; set; } = string.Empty;
    public int PreparationTime { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; } = DateTime.Now;
    public int ViewCount { get; set; } = 0;
    
    // Foreign Keys
    public int CategoryId { get; set; }
    public int UserId { get; set; }
    
    // Navigation Properties
    public Category? Category { get; set; }
    public User? User { get; set; }
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<Rating> Ratings { get; set; } = new List<Rating>();
    public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
}