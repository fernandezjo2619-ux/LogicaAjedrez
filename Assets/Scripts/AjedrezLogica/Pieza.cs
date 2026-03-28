using AjedrezLogica.Recursos;

namespace AjedrezLogica
{
    public class Pieza
    {
        public TipoPieza Tipo { get; set; }
        public ColorPieza Color { get; set; }
        private Habilidad? _habilidad;
        public Habilidad? Habilidad { get { return _habilidad; } set { _habilidad = value?.TipoPieza == Tipo ? value : null; } }

        public (int X, int Y) Posicion { get; set; }

    }
}
