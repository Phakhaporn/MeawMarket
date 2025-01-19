using Microsoft.AspNetCore.Mvc;
using MeawMarket.Data;
using MeawMarket.Models;
using System.Linq;

namespace MeawMarket.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        // Constructor
        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        // Action for Home Page (Index)
        public IActionResult Index()
        {
            // ดึงข้อมูลแมวทั้งหมดจากฐานข้อมูล
            var cats = _context.Cats.ToList();  // ดึงข้อมูลแมวจากฐานข้อมูล
            return View(cats);  // ส่งข้อมูลไปยังหน้า View "Index.cshtml"
        }

        // สามารถเพิ่ม action อื่นๆ เช่น About หรือ Contact ได้
        public IActionResult About()
        {
            return View(); // ส่งข้อมูลไปยังหน้า About.cshtml
        }
    }
}
