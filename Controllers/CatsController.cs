using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Text;
using MeawMarket.Models;
using MeawMarket.Data;
using Microsoft.Extensions.Logging;

[Route("api/[controller]")]
[ApiController]
public class CatsController : Controller
{
    private readonly AppDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CatsController> _logger;

    public CatsController(AppDbContext context, IWebHostEnvironment environment, ILogger<CatsController> logger)
    {
        _context = context;
        _environment = environment;
        _logger = logger;
    }

    private int? GetUserId()
    {
        var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == "UserId");
        if (userIdClaim == null) return null;
        return int.Parse(userIdClaim.Value);
    }

    private bool IsCatSold(Cat cat) => cat.Status == "Sold";

    public IActionResult Index()
    {
        var cats = _context.Cats.ToList();
        return View(cats);
    }

    [HttpGet("all")]
    public IActionResult GetAllCats()
    {
        var cats = _context.Cats
            .Select(c => new
            {
                c.Id,
                c.Breed,
                c.Gender,
                c.Age,
                c.Price,
                ImageUrl = c.Image,
                c.Status
            }).ToList();

        return Ok(cats);
    }

    [HttpGet("view/{id}")]
    public IActionResult ViewCats(int id)
    {
        var userIdString = User.FindFirst("UserId")?.Value;
        if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
        {
            TempData["ErrorMessage"] = "Please log in to continue.";
            //return RedirectToAction("Login", "User"); // 🔹 Redirect ไปหน้า Login
            return Redirect("/api/user/login");

        }

        var cat = _context.Cats.FirstOrDefault(c => c.Id == id);
        if (cat == null)
        {
            return NotFound("Meaw not found.");
        }
        ViewData["UserId"] = userId;

        return View("ViewCats", cat); // 🔹 คืนค่า View
    }

    // ✅ **API คืนค่า JSON**
    [HttpGet("cat/{id}")]
    public IActionResult GetCat(int id)
    {
        var cat = _context.Cats
            .Where(c => c.Id == id)
            .Select(c => new
            {
                c.Id,
                c.Breed,
                c.Gender,
                c.Age,
                c.Price,
                c.Image,
                c.Status
            })
            .FirstOrDefault();

        if (cat == null)
        {
            return NotFound(new { message = "Meaw not found." });
        }

        return Ok(cat); // 🔹 คืนค่า JSON
    }





[HttpGet("search")]
    public IActionResult SearchCats([FromQuery] string? breed, [FromQuery] string? gender, [FromQuery] decimal? maxPrice)
    {
        try
        {
            Console.WriteLine($"🔍 Searching Cats - Breed: {breed}, Gender: {gender}, MaxPrice: {maxPrice}");
            var query = _context.Cats.AsQueryable();

            if (!string.IsNullOrEmpty(breed))
            {
                Console.WriteLine($"🟢 Filtering by Breed: {breed}");
                query = query.Where(c => c.Breed.ToLower().Contains(breed.ToLower()));
            }

            if (!string.IsNullOrEmpty(gender))
            {
                Console.WriteLine($"🟢 Filtering by Gender: {gender}");
                query = query.Where(c => c.Gender.ToLower() == gender.ToLower());
            }

            if (maxPrice.HasValue)
            {
                Console.WriteLine($"🟢 Filtering by MaxPrice: {maxPrice}");
                query = query.Where(c => c.Price <= maxPrice.Value);
            }

            var result = query.Select(c => new
            {
                c.Id,
                c.Breed,
                c.Gender,
                c.Age,
                c.Price,
                ImageUrl = c.Image,
                c.Status
            }).ToList();
            Console.WriteLine($"✅ Found {result.Count} cats");

            if (!result.Any())
            {
                return Ok(Array.Empty<object>());
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while searching cats: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [Authorize(AuthenticationSchemes = CookieAuthenticationDefaults.AuthenticationScheme)]
    [HttpGet("my-cats")]
    public IActionResult GetMyCats()
    {
        try
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { message = "Please log in." });
            }

            var myCats = _context.Cats
                .Where(c => c.OwnerId == userId)
                .Select(c => new
                {
                    c.Id,
                    c.Breed,
                    c.Gender,
                    c.Age,
                    c.Price,
                    c.Image,
                    c.Status
                }).ToList();

            if (!myCats.Any())
            {
                return Ok(new { message = "You don't own any cats yet." });
            }

            return Ok(myCats);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while fetching user cats: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [Authorize]
    [HttpPost("add")]
    public async Task<IActionResult> AddCat([FromForm] CatDto catDto)
    {
        try
        {
            var userIdString = User.FindFirst("UserId")?.Value; // ✅ ดึงค่า UserId จาก Cookie
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { message = "Please log in to continue." });
            }

            string? imagePath = null;
            if (catDto.Image != null)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(catDto.Image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Only .jpg, .jpeg, and .png files are allowed.");
                }

                if (catDto.Image.Length > 2 * 1024 * 1024)
                {
                    return BadRequest("File size cannot exceed 2MB.");
                }

                var uploadFolder = Path.Combine(_environment.WebRootPath, "images", "cats", DateTime.UtcNow.ToString("yyyy-MM"));
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{catDto.Image.FileName}";
                var filePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await catDto.Image.CopyToAsync(stream);
                }

                imagePath = $"/images/cats/{DateTime.UtcNow.ToString("yyyy-MM")}/{uniqueFileName}";
            }

            var cat = new Cat
            {
                Breed = catDto.Breed,
                Gender = catDto.Gender,
                Age = catDto.Age,
                Price = catDto.Price,
                OwnerId = userId,
                Image = imagePath,
                Status = "Available"
            };

            _context.Cats.Add(cat);
            await _context.SaveChangesAsync();

            return Ok("Cat added successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while adding cat: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }

    [Authorize]
    [HttpDelete("delete/{id}")]
    public IActionResult DeleteCat(int id)
    {
        try
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { success = false, message = "Please log in to continue." });
            }

            var cat = _context.Cats.FirstOrDefault(c => c.Id == id);
            if (cat == null)
            {
                return NotFound(new { success = false, message = "Meaw not found." });
            }

            if (IsCatSold(cat))
            {
                return BadRequest(new { success = false, message = "This meaw has already been sold. You can't delete this meaw." });
            }

            _context.Cats.Remove(cat);
            _context.SaveChanges();

            return Json(new { success = true, message = "Meaw deleted successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while deleting cat: {ex.Message}");
            return StatusCode(500, new { success = false, message = "An unexpected error occurred while processing your request." });
        }
    }


    [Authorize]
    [HttpPut("update/{id}")]
    public async Task<IActionResult> UpdateCat(int id, [FromForm] CatDto catDto)
    {
        try
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { message = "Please log in to continue." });
            }

            var cat = await _context.Cats.FindAsync(id);
            if (cat == null)
            {
                return NotFound("Meaw not found.");
            }

            if (IsCatSold(cat))
            {
                return BadRequest("This meaw has already been sold. You can't update this meaw.");
            }

            // ✅ อัปเดตเฉพาะค่าที่ได้รับจาก FormData
            if (!string.IsNullOrEmpty(catDto.Breed)) cat.Breed = catDto.Breed;
            if (!string.IsNullOrEmpty(catDto.Gender)) cat.Gender = catDto.Gender;
            if (catDto.Age > 0) cat.Age = catDto.Age;
            if (catDto.Price > 0) cat.Price = catDto.Price;

            // ✅ ตรวจสอบและอัปโหลดรูปภาพใหม่
            if (catDto.Image != null)
            {
                if (!string.IsNullOrEmpty(cat.Image))
                {
                    var oldImagePath = Path.Combine(_environment.WebRootPath, cat.Image.TrimStart('/').Replace("/", "\\"));
                    if (System.IO.File.Exists(oldImagePath))
                    {
                        System.IO.File.Delete(oldImagePath);
                    }
                }

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var fileExtension = Path.GetExtension(catDto.Image.FileName).ToLower();

                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Only .jpg, .jpeg, and .png files are allowed.");
                }

                var uploadFolder = Path.Combine(_environment.WebRootPath, "images", "cats", DateTime.UtcNow.ToString("yyyy-MM"));
                if (!Directory.Exists(uploadFolder))
                {
                    Directory.CreateDirectory(uploadFolder);
                }

                var uniqueFileName = $"{Guid.NewGuid()}_{catDto.Image.FileName}";
                var newImagePath = Path.Combine(uploadFolder, uniqueFileName);

                using (var stream = new FileStream(newImagePath, FileMode.Create))
                {
                    await catDto.Image.CopyToAsync(stream);
                }

                cat.Image = $"/images/cats/{DateTime.UtcNow:yyyy-MM}/{uniqueFileName}";
            }

            _context.Cats.Update(cat);
            await _context.SaveChangesAsync();

            return Ok("Cat updated successfully!");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while updating cat: {ex.Message}");
            return StatusCode(500, "An unexpected error occurred.");
        }
    }


    [Authorize]
    [HttpPost("buy/{id}")]
    public async Task<IActionResult> BuyCat(int id)
    {
        try
        {
            var userIdString = User.FindFirst("UserId")?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized(new { success = false, message = "Please log in to continue." });
            }

            var cat = await _context.Cats.FindAsync(id);
            if (cat == null)
            {
                return NotFound(new { success = false, message = "Meaw not found." });
            }
            if (IsCatSold(cat))
            {
                return BadRequest(new { success = false, message = "This meaw has already been sold." });
            }

            cat.Status = "Sold";
            var soldCat = new SoldCat
            {
                CatId = cat.Id,
                BuyerId = userId,
                SoldDate = DateTime.UtcNow
            };

            _context.SoldCats.Add(soldCat);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Meaw purchased successfully." });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error while buying cat: {ex.Message}");
            return StatusCode(500, new { success = false, message = "An unexpected error occurred while processing your request." });
        }
    }

}
