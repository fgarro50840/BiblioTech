namespace BiblioTech.Models
{
    public class Autor : Persona
    {
        public string Nacionalidad { get; set; } = string.Empty;

        public ICollection<Libro> Libros { get; set; }
            = new List<Libro>();

        public override string ObtenerDescripcion()
        {
            return $"Autor: {Nombre} {Apellido} - {Nacionalidad}";
        }
    }
}
