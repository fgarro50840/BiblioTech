
using BiblioTech.Data;
using BiblioTech.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BiblioTech.Controllers
{
    public class UsuariosController : Controller
    {
        private readonly BibliotecaContext _context;

        public UsuariosController(BibliotecaContext context)
        {
            _context = context;
        }

        // LISTAR Y BUSCAR USUARIOS
        public async Task<IActionResult> Index(string? buscar)
        {
            var usuarios = _context.Usuarios.AsQueryable();

            if (!string.IsNullOrWhiteSpace(buscar))
            {
                buscar = buscar.Trim();

                usuarios = usuarios.Where(u =>
                    u.Nombre.Contains(buscar) ||
                    u.Apellido.Contains(buscar) ||
                    u.Correo.Contains(buscar));
            }

            ViewBag.Buscar = buscar;

            return View(await usuarios
                .OrderBy(u => u.Nombre)
                .ThenBy(u => u.Apellido)
                .ToListAsync());
        }

        // FORMULARIO PARA REGISTRAR
        public IActionResult Create()
        {
            return View(new Usuario());
        }

        // GUARDAR NUEVO USUARIO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Nombre,Apellido,Correo")] Usuario usuario)
        {
            ModelState.Remove(nameof(Usuario.Prestamos));

            if (await _context.Usuarios.AnyAsync(
                u => u.Correo == usuario.Correo))
            {
                ModelState.AddModelError(
                    nameof(Usuario.Correo),
                    "Ya existe un usuario con este correo electrónico.");
            }

            if (!ModelState.IsValid)
                return View(usuario);

            usuario.FechaRegistro = DateTime.Now;

            _context.Usuarios.Add(usuario);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "El usuario se registró correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // FORMULARIO PARA EDITAR
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
                return NotFound();

            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            return View(usuario);
        }

        // GUARDAR CAMBIOS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind("Nombre,Apellido,Correo")] Usuario datos)
        {
            var usuario = await _context.Usuarios.FindAsync(id);

            if (usuario == null)
                return NotFound();

            ModelState.Remove(nameof(Usuario.Prestamos));

            if (await _context.Usuarios.AnyAsync(
                u => u.Correo == datos.Correo && u.Id != id))
            {
                ModelState.AddModelError(
                    nameof(Usuario.Correo),
                    "Ya existe otro usuario con este correo electrónico.");
            }

            if (!ModelState.IsValid)
                return View(datos);

            usuario.Nombre = datos.Nombre;
            usuario.Apellido = datos.Apellido;
            usuario.Correo = datos.Correo;

            await _context.SaveChangesAsync();

            TempData["Success"] =
                "Los datos del usuario se actualizaron correctamente.";

            return RedirectToAction(nameof(Index));
        }

        // ELIMINAR USUARIO
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.Prestamos)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (usuario == null)
                return NotFound();

            if (usuario.Prestamos.Any())
            {
                TempData["Error"] =
                    "No se puede eliminar un usuario con historial de préstamos.";

                return RedirectToAction(nameof(Index));
            }

            _context.Usuarios.Remove(usuario);
            await _context.SaveChangesAsync();

            TempData["Success"] =
                "El usuario se eliminó correctamente.";

            return RedirectToAction(nameof(Index));
        }
    }
}
