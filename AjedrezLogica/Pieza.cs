using AjedrezLogica.Recursos;

namespace AjedrezLogica
{
    public class Pieza
    {
        public TipoPieza Tipo { get; set; }
        public ColorPieza Color { get; set; }

        public (int X, int Y) Posicion { get; set; }

    }
}
