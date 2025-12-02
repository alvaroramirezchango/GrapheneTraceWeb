using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using GrapheneTraceWeb.Data;
using GrapheneTraceWeb.Models;
using System.Linq;

namespace GrapheneTraceWeb.Controllers
{
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;


        // Helper method to verify Admin access
        private bool IsAdmin()
        {
            var role = HttpContext.Session.GetString(SessionKeys.UserRole);
            return role == "Admin";
        }

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // List all users
        public IActionResult Dashboard()
        {
            if (!IsAdmin())
            {
                return RedirectToAction("Login", "Home");
            }

            var users = _context.Users
                .OrderBy(u => u.Role)
                .ThenBy(u => u.Name)
                .ToList();

            return View(users);
        }

        // GET: Admin/Edit/5
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);

            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // POST: Admin/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, User model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            // Update basic fields
            user.Name = model.Name;
            user.Email = model.Email;
            user.Password = model.Password; // In real apps, use hashing
            user.Role = model.Role;

            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }

        // POST: Admin/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(int id)
        {
            var user = _context.Users.FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            _context.SaveChanges();

            return RedirectToAction("Dashboard");
        }
    }
}
