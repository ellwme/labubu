namespace backend.Models;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "user";
    public int Balance { get; set; } = 1000;
    public List<InventoryItem> Inventory { get; set; } = new();
}
