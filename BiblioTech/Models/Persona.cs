namespace BiblioTech.Models
{
    public abstract class Persona
    {
        public int Id { get; set; }

        public string Nombre { get; set; } = string.Empty;

        public string Apellido { get; set; } = string.Empty;

        public string Correo { get; set; } = string.Empty;

        public abstract string ObtenerDescripcion();
    }
}
