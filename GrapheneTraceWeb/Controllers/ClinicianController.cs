using GrapheneTraceWeb.Data;
using GrapheneTraceWeb.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace GrapheneTraceWeb.Controllers
{
    public class ClinicianController : Controller
    {
        private readonly AppDbContext _context;

        public ClinicianController(AppDbContext context)
        {
            _context = context;
        }

        public IActionResult Dashboard()
        {
            // Obtenemos todos los usuarios con rol "User"
            List<User> patients = _context.Users
                .Where(u => u.Role == "User")
                .OrderBy(u => u.Name)
                .ToList();

            // Para depurar: cuántos pacientes hay
            ViewBag.PatientCount = patients.Count;

            return View(patients);
        }
    }
}
