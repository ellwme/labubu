namespace backend.Models;
public class InventoryItem
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int FigureId { get; set; }
    public LabubuFigure? Figure { get; set; }
    public DateTime ObtainedAt { get; set; }
}