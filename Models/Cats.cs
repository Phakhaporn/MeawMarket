namespace MeawMarket.Models
{
    public class Cat
    {
        public int Id { get; set; }
        public string? Breed { get; set; }
        public string? Gender { get; set; }
        public int Age { get; set; }
        public decimal Price { get; set; }
        public string? Image { get; set; }
        public int OwnerId { get; set; }
        public string Status { get; set; } = "Available";
    }
}
