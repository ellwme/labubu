namespace backend.Models;
public class LabubuFigure
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Rarity { get; set; } = "Common"; // Common, Rare, Secret
    public string ImageUrl { get; set; } = string.Empty;
    public int DropWeight { get; set; } = 1;
}