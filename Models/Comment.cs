namespace TarifDefteri.Models;
public class Comment
{
    public int Id { get; set; }
    public string Text { get; set; }
    public DateTime CreatedDate { get; set; }
    
    public int RecipeId { get; set; }
    public int UserId { get; set; }
    
    public Recipe Recipe { get; set; }
    public User User { get; set; }
}