namespace BiblioTech.Models
{
    public class Usuario : Persona
    {
        public DateTime FechaRegistro { get; set; }
            = DateTime.Now;

        public ICollection<Prestamo> Prestamos { get; set; }
            = new List<Prestamo>();

        public override string ObtenerDescripcion()
        {
            return $"Usuario: {Nombre} {Apellido} - {Correo}";
        }
    }
}