
using BiblioTech.Data;
using BiblioTech.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BiblioTech.Controllers
{
    public class LibrosController : Controller
    {
        private readonly BibliotecaContext _context;

        public LibrosController(BibliotecaContext context)
        {
            _context = context;
        }

        // LISTADO Y BÚSQUEDA
        public async Task<IActionResult> Index(string? buscar)
        {
            var libros = _context.Libros
                .Include(l => l.Autor)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                libros = libros.Where(l =>
                    l.Titulo.Contains(buscar) ||
                    l.ISBN.Contains(buscar) ||
                    l.Autor.Nombre.Contains(buscar) ||
                    l.Autor.Apellido.Contains(buscar));
            }

            ViewBag.Buscar = buscar;

            return View(await libros
                .OrderBy(l => l.Titulo)
                .ToListAsync());
        }

        // FORMULARIO PARA REGISTRAR
        public async Task<IActionResult> Create()
        {
            await CargarAutores();
            return View(new Libro());
        }

        // GUARDAR LIBRO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Titulo,ISBN,AnioPublicacion,AutorId")] Libro libro)
        {
            ModelState.Remove(nameof(Libro.Autor));
            ModelState.Remove(nameof(Libro.Prestamos));

            if (!await _context.Autores.AnyAsync(
                a => a.Id == libro.AutorId))
            {
                ModelState.AddModelError(
                    nameof(Libro.AutorId),
                    "Debe seleccionar un autor válido.");
            }

            if (await _context.Libros.AnyAsync(
                l => l.ISBN == libro.ISBN))
            {
                ModelState.AddModelError(
                    nameof(Libro.ISBN),
                    "Ya existe un libro con este ISBN.");
            }

            if (libro.AnioPublicacion < 1450 ||
                libro.AnioPublicacion > DateTime.Now.Year)
            {
                ModelState.AddModelError(
                    nameof(Libro.AnioPublicacion),
                    "Ingrese un año de publicación válido.");
            }

            if (!ModelState.IsValid)
            {
                await CargarAutores(libro.AutorId);
                return View(libro);
            }

            _context.Libros.Add(libro);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "El libro se registró correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // FORMULARIO PARA EDITAR
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var libro = await _context.Libros.FindAsync(id);

            if (libro == null)
                return NotFound();

            await CargarAutores(libro.AutorId);

            return View(libro);
        }

        // GUARDAR CAMBIOS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Titulo,ISBN,AnioPublicacion,AutorId")] Libro datos)
        {
            var libro = await _context.Libros.FindAsync(id);

            if (libro == null)
                return NotFound();

            ModelState.Remove(nameof(Libro.Autor));
            ModelState.Remove(nameof(Libro.Prestamos));

            if (!await _context.Autores.AnyAsync(
                a => a.Id == datos.AutorId))
            {
                ModelState.AddModelError(
                    nameof(Libro.AutorId),
                    "Debe seleccionar un autor válido.");
            }

            if (await _context.Libros.AnyAsync(
                l => l.ISBN == datos.ISBN && l.Id != id))
            {
                ModelState.AddModelError(
                    nameof(Libro.ISBN),
                    "Ya existe otro libro con este ISBN.");
            }

            if (datos.AnioPublicacion < 1450 ||
                datos.AnioPublicacion > DateTime.Now.Year)
            {
                ModelState.AddModelError(
                    nameof(Libro.AnioPublicacion),
                    "Ingrese un año de publicación válido.");
            }

            if (!ModelState.IsValid)
            {
                await CargarAutores(datos.AutorId);
                return View(datos);
            }

            libro.Titulo = datos.Titulo;
            libro.ISBN = datos.ISBN;
            libro.AnioPublicacion = datos.AnioPublicacion;
            libro.AutorId = datos.AutorId;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "El libro se actualizó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR LIBRO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var libro = await _context.Libros
                .Include(l => l.Prestamos)
                .FirstOrDefaultAsync(l => l.Id == id);

            if (libro == null)
                return NotFound();

            if (libro.Prestamos.Any())
            {
                TempData["Error"] =
                    "No se puede eliminar un libro que tiene historial de préstamos.";

                return RedirectToAction(nameof(Index));
            }

            _context.Libros.Remove(libro);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "El libro se eliminó correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // CARGAR AUTORES PARA EL DESPLEGABLE
        private async Task CargarAutores(int? seleccionado = null)
        {
            var autores = await _context.Autores
                .OrderBy(a => a.Nombre)
                .ThenBy(a => a.Apellido)
                .ToListAsync();

            ViewBag.Autores = new SelectList(
                autores.Select(a => new
                {
                    a.Id,
                    NombreCompleto = a.Nombre + " " + a.Apellido
                }),
                "Id",
                "NombreCompleto",
                seleccionado);
        }
    }
}

