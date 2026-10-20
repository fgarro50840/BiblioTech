
namespace BiblioTech.Models
{
    public class Prestamo
    {
        public int Id { get; set; }

        public int LibroId { get; set; }

        public Libro Libro { get; set; } = null!;

        public int UsuarioId { get; set; }

        public Usuario Usuario { get; set; } = null!;

        public DateTime FechaPrestamo { get; set; } = DateTime.Now;

        public DateTime FechaDevolucionPrevista { get; set; }

        public DateTime? FechaDevolucionReal { get; private set; }

        // Indica si el préstamo ya fue devuelto
        public bool Devuelto => FechaDevolucionReal.HasValue;

        // Indica si el préstamo continúa activo
        public bool EstaActivo()
        {
            return !Devuelto;
        }

        // Registra la devolución del libro
        public void RegistrarDevolucion()
        {
            if (Devuelto)
            {
                throw new InvalidOperationException(
                    "Este préstamo ya fue devuelto.");
            }

            Libro.Devolver();

            FechaDevolucionReal = DateTime.Now;
        }
    }
}
