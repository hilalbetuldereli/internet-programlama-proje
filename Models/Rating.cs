namespace TarifDefteri.Models;
public class Rating
{
    public int Id { get; set; }
    public int Score { get; set; } // 1-5 arası
    
    public int RecipeId { get; set; }
    public int UserId { get; set; }
    
    public Recipe Recipe { get; set; } = null!;
    public User User { get; set; } = null!;
}