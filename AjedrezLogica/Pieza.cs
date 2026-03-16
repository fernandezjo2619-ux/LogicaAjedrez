using AjedrezLogica.Recursos;

namespace AjedrezLogica
{
    public class Pieza
    {
        public TipoPieza Tipo { get; set; }
        public ColorPieza Color { get; set; }
        public Habilidad Habilidad { get { return Habilidad.TipoHabilidad == Tipo ? Habilidad : null; } set; }

        public (int X, int Y) Posicion { get; set; }


        
    }
}
