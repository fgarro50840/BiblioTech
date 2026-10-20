
using BiblioTech.Data;
using BiblioTech.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BiblioTech.Controllers
{
    public class AutoresController : Controller
    {
        private readonly BibliotecaContext _context;

        public AutoresController(BibliotecaContext context)
        {
            _context = context;
        }

        // LISTAR AUTORES
        public async Task<IActionResult> Index(string? buscar)
        {
            var autores = _context.Autores.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                autores = autores.Where(a =>
                    a.Nombre.Contains(buscar) ||
                    a.Apellido.Contains(buscar) ||
                    a.Nacionalidad.Contains(buscar));
            }

            ViewBag.Buscar = buscar;

            return View(await autores
                .OrderBy(a => a.Nombre)
                .ThenBy(a => a.Apellido)
                .ToListAsync());
        }

        // FORMULARIO DE REGISTRO DE NUEVOS AUTORES
        public IActionResult Create()
        {
            return View();
        }

        // GUARDAR AUTOR
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nombre,Apellido,Correo,Nacionalidad")] Autor autor)
        {
            if (!ModelState.IsValid)
                return View(autor);

            _context.Autores.Add(autor);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "El autor se registró correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // FORMULARIO DE EDICIÓN DE AUTOR EXISTENTE
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var autor = await _context.Autores.FindAsync(id);

            if (autor == null)
                return NotFound();

            return View(autor);
        }

        // GUARDAR CAMBIOS EN AUTOR EXISTENTE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Nombre,Apellido,Correo,Nacionalidad")] Autor datos)
        {
            var autor = await _context.Autores.FindAsync(id);

            if (autor == null)
                return NotFound();

            if (!ModelState.IsValid)
                return View(datos);

            autor.Nombre = datos.Nombre;
            autor.Apellido = datos.Apellido;
            autor.Correo = datos.Correo;
            autor.Nacionalidad = datos.Nacionalidad;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Los datos del autor se actualizaron correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR AUTOR EXISTENTE
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var autor = await _context.Autores
                .Include(a => a.Libros)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (autor == null)
                return NotFound();

            if (autor.Libros.Any())
            {
                TempData["Error"] =
                    "No se puede eliminar un autor que tiene libros registrados.";

                return RedirectToAction(nameof(Index));
            }

            _context.Autores.Remove(autor);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "El autor se eliminó correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
