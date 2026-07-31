namespace Backend.Models
{
    public class Curso
    {
        public int Id { get; set; }
        public int ColegioId { get; set; }
        public Colegio Colegio { get; set; } = null!;
        public string Nombre { get; set; } = string.Empty; // Ejemplo: "11-A", "11-B"
    }
}
