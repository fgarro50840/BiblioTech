
namespace BiblioTech.Models
{
    public class Libro
    {
        public int Id { get; set; }

        public string Titulo { get; set; } = string.Empty;

        public string ISBN { get; set; } = string.Empty;

        public int AnioPublicacion { get; set; }

        public int AutorId { get; set; }

        public Autor Autor { get; set; } = null!;

        public bool Disponible { get; private set; } = true;

        public ICollection<Prestamo> Prestamos { get; set; }
            = new List<Prestamo>();

        public void Prestar()
        {
            if (!Disponible)
            {
                throw new InvalidOperationException(
                    "El libro no está disponible.");
            }

            Disponible = false;
        }

        public void Devolver()
        {
            if (Disponible)
            {
                throw new InvalidOperationException(
                    "El libro ya está disponible.");
            }

            Disponible = true;
        }
    }
}

