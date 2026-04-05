namespace PruebaHabilidades;

/// <summary>
/// Estructura para parsear las habilidades devueltas por Supabase
/// </summary>
public class DatosHabilidad
{
    public int id { get; set; }
    public string nombre { get; set; } = string.Empty;
    public string descripcion { get; set; } = string.Empty;
    public int poder_base { get; set; }
    public int id_tipo { get; set; }

    public override string ToString()
    {
        return $"[{id}] {nombre} (Poder: {poder_base}) - {descripcion}";
    }
}
