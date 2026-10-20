
using BiblioTech.Data;
using BiblioTech.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace BiblioTech.Controllers
{
    public class HomeController : Controller
    {
        private readonly BibliotecaContext _context;

        public HomeController(BibliotecaContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            ViewBag.TotalLibros = await _context.Libros.CountAsync();
            ViewBag.LibrosDisponibles = await _context.Libros.CountAsync( l => l.Disponible);
            ViewBag.TotalUsuarios = await _context.Usuarios.CountAsync();
            ViewBag.PrestamosActivos = await _context.Prestamos.CountAsync( p => p.FechaDevolucionReal == null);
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}

