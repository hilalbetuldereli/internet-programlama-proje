namespace TarifDefteri.Models;
public class Favorite
{
    public int Id { get; set; }
    public DateTime AddedDate { get; set; }
    
    public int UserId { get; set; }
    public int RecipeId { get; set; }
    
    public User User { get; set; }
    public Recipe Recipe { get; set; }
}