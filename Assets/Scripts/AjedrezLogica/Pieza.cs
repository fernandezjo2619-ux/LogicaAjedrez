using AjedrezLogica.Recursos;

namespace AjedrezLogica
{
    public class Pieza
    {
        public TipoPieza Tipo { get; set; }
        public ColorPieza Color { get; set; }
        public Habilidad Habilidad { get { return field.TipoPieza == Tipo ? field : null; } set { field = value; } }

        public (int X, int Y) Posicion { get; set; }

    }
}
