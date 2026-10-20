
using BiblioTech.Data;
using BiblioTech.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

namespace BiblioTech.Controllers
{
    public class PrestamosController : Controller
    {
        private readonly BibliotecaContext _context;

        public PrestamosController(BibliotecaContext context)
        {
            _context = context;
        }

        // MOSTRAR TODOS LOS PRÉSTAMOS
        public async Task<IActionResult> Index(string? estado)
        {
            var prestamos = _context.Prestamos
                .Include(p => p.Libro)
                .Include(p => p.Usuario)
                .AsQueryable();

            if (estado == "activos")
            {
                prestamos = prestamos.Where(
                    p => p.FechaDevolucionReal == null);
            }
            else if (estado == "devueltos")
            {
                prestamos = prestamos.Where(
                    p => p.FechaDevolucionReal != null);
            }

            ViewBag.Estado = estado;

            return View(await prestamos
                .OrderByDescending(p => p.FechaPrestamo)
                .ToListAsync());
        }

        // MOSTRAR FORMULARIO DE PRÉSTAMO
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await CargarListas();

            return View(new Prestamo
            {
                FechaDevolucionPrevista =
                    DateTime.Today.AddDays(7)
            });
        }

        // REGISTRAR NUEVO PRÉSTAMO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            int LibroId,
            int UsuarioId,
            DateTime FechaDevolucionPrevista)
        {
            var libro = await _context.Libros
                .FirstOrDefaultAsync(l => l.Id == LibroId);

            var usuario = await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == UsuarioId);

            if (libro == null)
            {
                ModelState.AddModelError(
                    "LibroId",
                    "Seleccione un libro válido.");
            }
            else if (!libro.Disponible)
            {
                ModelState.AddModelError(
                    "LibroId",
                    "Este libro ya se encuentra prestado.");
            }

            if (usuario == null)
            {
                ModelState.AddModelError(
                    "UsuarioId",
                    "Seleccione un usuario válido.");
            }

            if (FechaDevolucionPrevista.Date <= DateTime.Today)
            {
                ModelState.AddModelError(
                    "FechaDevolucionPrevista",
                    "La devolución debe ser posterior a hoy.");
            }

            if (!ModelState.IsValid)
            {
                await CargarListas(LibroId, UsuarioId);

                return View(new Prestamo
                {
                    LibroId = LibroId,
                    UsuarioId = UsuarioId,
                    FechaDevolucionPrevista =
                        FechaDevolucionPrevista
                });
            }

            try
            {
                // El objeto Libro controla su disponibilidad
                libro!.Prestar();

                var prestamo = new Prestamo
                {
                    LibroId = libro.Id,
                    UsuarioId = usuario!.Id,
                    FechaPrestamo = DateTime.Now,
                    FechaDevolucionPrevista =
                        FechaDevolucionPrevista
                };

                _context.Prestamos.Add(prestamo);

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "El préstamo se registró correctamente.";

                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);

                await CargarListas(LibroId, UsuarioId);

                return View(new Prestamo
                {
                    LibroId = LibroId,
                    UsuarioId = UsuarioId,
                    FechaDevolucionPrevista =
                        FechaDevolucionPrevista
                });
            }
        }

        // DEVOLVER UN LIBRO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Devolver(int id)
        {
            var prestamo = await _context.Prestamos
                .Include(p => p.Libro)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (prestamo == null)
                return NotFound();

            if (prestamo.Devuelto)
            {
                TempData["Error"] =
                    "Este préstamo ya fue devuelto.";

                return RedirectToAction(nameof(Index));
            }

            try
            {
                prestamo.RegistrarDevolucion();

                await _context.SaveChangesAsync();

                TempData["Success"] =
                    "La devolución se registró correctamente.";
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // CARGAR LIBROS DISPONIBLES Y USUARIOS
        private async Task CargarListas(
            int? libroSeleccionado = null,
            int? usuarioSeleccionado = null)
        {
            var libros = await _context.Libros
                .Where(l => l.Disponible)
                .OrderBy(l => l.Titulo)
                .ToListAsync();

            var usuarios = await _context.Usuarios
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Apellido)
                .ToListAsync();

            ViewBag.Libros = new SelectList(
                libros,
                "Id",
                "Titulo",
                libroSeleccionado);

            ViewBag.Usuarios = new SelectList(
                usuarios.Select(u => new
                {
                    u.Id,
                    NombreCompleto =
                        u.Nombre + " " + u.Apellido
                }),
                "Id",
                "NombreCompleto",
                usuarioSeleccionado);
        }
    }
}

