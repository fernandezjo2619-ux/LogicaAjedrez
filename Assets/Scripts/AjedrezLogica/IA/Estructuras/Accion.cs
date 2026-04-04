using System;

namespace AjedrezLogica.IA.Estructuras
{
    public struct Accion()
	{
        public TipoAccion Tipo { get; set; }
        public Pieza Pieza { get; set; }
        public int XFin { get; set; }
        public int YFin { get; set; }

        // Solo para empujones (Habilidad de Dama)
        public Pieza? PiezaEmpujada { get; set; }
    }
}
