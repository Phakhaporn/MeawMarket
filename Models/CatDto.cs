using Microsoft.AspNetCore.Http;

namespace MeawMarket.Models
{
    public class CatDto
    {
        public string Breed { get; set; }
        public string Gender { get; set; }
        public int Age { get; set; }
        public decimal Price { get; set; }
        public IFormFile? Image { get; set; } // ใช้ IFormFile เพื่อรองรับไฟล์ที่อัปโหลด
        public string Status { get; set; } = "Available";
    }
}
