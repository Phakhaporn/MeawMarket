using Microsoft.AspNetCore.Mvc;
using MeawMarket.Data; // Import DbContext
using MeawMarket.Models; // Import Models
using System.Linq; // ใช้สำหรับการค้นหาในฐานข้อมูล
using System.Text;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace MeawMarket.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : Controller
    {
        private readonly AppDbContext _context;
        

        // Constructor: เชื่อมต่อ DbContext
        public UserController(AppDbContext context)
        {
            _context = context;
        }
        [HttpGet("register")]
        public IActionResult Register()
        {
            ViewData["Title"] = "Register";
            var model = new User(); // Provide an empty model instance
            return View(model);
        }
        //  Register
        [HttpPost("register")]
        public IActionResult Register([FromForm] User user)
        {
            //  Username ซ้ำมั้ย
            if (_context.Users.Any(u => u.Username == user.Username))
            {
                ViewData["ErrorMessage"] = "Username already exists.";
                return View("Register");
            }
            if (_context.Users.Any(u => u.Email == user.Email))
            {
                ViewData["ErrorMessage"] = "Email is already associated with an account.";
                return View("Register");
            }
            //user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            // บันทึกข้อมูลผู้ใช้ลงฐานข้อมูล
            _context.Users.Add(user);
            _context.SaveChanges();
            ViewData["SuccessMessage"] = "Registration successful.";
            return View("Register");
        }

        [HttpGet("login")]
        public IActionResult Login()
        {
            ViewData["Title"] = "Login";
            var model = new LoginReq(); // Provide an empty model instance
            return View();
        }
        // API สำหรับ Login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginReq loginDetails)
        {
            if (string.IsNullOrEmpty(loginDetails.Username) || string.IsNullOrEmpty(loginDetails.Password))
            {
                ViewData["ErrorMessage"] = "Invalid username or password.";
                return BadRequest(new { message = "Please provide both username and password." }); // ✅ ส่ง JSON
            }

            var user = _context.Users.FirstOrDefault(u => u.Username == loginDetails.Username);

            if (user == null || user.Password != loginDetails.Password)
            {
                return Unauthorized(new { message = "Invalid username or password." }); // ✅ ส่ง JSON
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Username),
            new Claim("UserId", user.Id.ToString())
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = DateTime.UtcNow.AddHours(2)
            };

            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties
            );

            return Ok(new { message = "Login successful", userId = user.Id }); // ✅ ส่ง JSON
        }



        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Ok(new { message = "Logged out successfully" }); // ✅ ส่ง JSON
        }

        [HttpGet("check-auth")]
        public IActionResult CheckAuth()
        {
            var userId = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { message = "Unauthorized: Please log in." });
            }
            return Ok(new { message = "User is authenticated.", userId = userId });
        }


        [HttpGet]
        public IActionResult Index()
        {
            // ดึง Username จาก Claims
            var usernameClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Name);
            var username = usernameClaim?.Value;

            // ตรวจสอบว่าผู้ใช้ล็อกอินหรือไม่
            if (username == null)
            {
                return RedirectToAction("Login", "User");
            }

            // ส่งข้อมูล Cats ไปยัง View
            return View(_context.Cats.ToList());
        }
    }
}
