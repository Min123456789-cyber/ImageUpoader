using ImageUps.Data;
using ImageUps.Models;
using ImageUps.Models.ImageViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Security.Claims;

namespace ImageUps.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IWebHostEnvironment _environment;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, IWebHostEnvironment environment, AppDbContext context)
        {
            _logger = logger;
            _environment = environment;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var images = await _context.Images.ToListAsync();
            return View(images);
        }
        public IActionResult Create()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Create(ImageVM vm)
        {
            if (vm.ImageUrl == null)
            {
                ModelState.AddModelError("file", "Image file is required");
            }
            if (!ModelState.IsValid)
            {
                return View(vm);
            }
            string wwwRootPath = _environment.WebRootPath;

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.ImageUrl!.FileName);
            string uploadDir = Path.Combine(wwwRootPath, @"images");
            string filePath = Path.Combine(uploadDir, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                vm.ImageUrl.CopyTo(fileStream);
            }
            var image = new Image
            {
                Title = vm.Title,
                Description = vm.Description,
                ImageUrl = fileName,
            };
            await _context.Images.AddAsync(image);
            await _context.SaveChangesAsync();
            TempData["success"] = ("Memories Added Successfully");
            return RedirectToAction("Index", "Home");
        }
        [HttpGet]
        public async Task<IActionResult> Details(int? id)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }
            return View(image);
        }
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (id == null || id == 0)
            {
                return NotFound();
            }
            var image = await _context.Images.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }
            return View(image);
        }
        [HttpPost]
        public async Task<IActionResult> Update(int? id, ImageVM vm)
        {
            var image = await _context.Images.FindAsync(id);
            if (image == null)
            {
                return NotFound();
            }
            //upload Image
            string wwwRootPath = _environment.WebRootPath;
            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(vm.ImageUrl.FileName);
            string uploadDir = Path.Combine(wwwRootPath, @"images");
            string filePath = Path.Combine(uploadDir, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                vm.ImageUrl.CopyTo(fileStream);
            }
            //delete old image
            string oldImage = _environment.WebRootPath + "image" + image.ImageUrl;
            System.IO.File.Delete(oldImage);

            //update all the details
            image.Title = vm.Title;
            image.Description = vm.Description;
            image.ImageUrl = fileName;
            //await _context.Images.Update(images);

            await _context.SaveChangesAsync();
            TempData["success"] = "Updated Successfully";

            return RedirectToAction("Index");
        }
        //[HttpGet]
        //public IActionResult Delete(int? id)
        //{
        //    if(id == null || id == 0)
        //    {
        //        return NotFound();
        //    }
        //    var image = _context.Images.Find(id);
        //    if(image == null)
        //    {
        //        return NotFound();
        //    }
        //    return View(image);
        //}
        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (ModelState.IsValid)
            {
                var img = await _context.Images.FindAsync(id);
                //delete old image
                string oldImage = _environment.WebRootPath + "/images/" + img.ImageUrl;
                System.IO.File.Delete(oldImage);

                _context.Images.Remove(img);
                await _context.SaveChangesAsync();
                TempData["success"] = ("Deleted Successfully");
                return RedirectToAction("Index");
            }
            return View(id);
        }
    }
}
