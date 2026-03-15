using AjedrezLogica.Recursos;
using AjedrezLogica.TiposReglasMovimiento;
using System.Collections.Generic;

namespace AjedrezLogica
{
    public class ReglasMovimiento
    {
        public static List<(int X, int Y)> MovimientosValidos( Pieza pieza, Tablero tablero)
        {
            switch (pieza.Tipo)
            {
                case TipoPieza.Peon:
                    return ReglasPeon.Basicas(pieza.Posicion, pieza.Color, tablero);
                case TipoPieza.Torre:
                    return ReglasTorre.Basicas(pieza.Posicion, pieza.Color, tablero);
                case TipoPieza.Alfil:
                    return ReglasAlfil.Basicas(pieza.Posicion, pieza.Color, tablero);
                case TipoPieza.Caballo:
                    return ReglasCaballo.Basicas(pieza.Posicion, pieza.Color, tablero);
                case TipoPieza.Dama:
                    return ReglasDama.Basicas(pieza.Posicion, pieza.Color, tablero);
                case TipoPieza.Rey:
                    return ReglasRey.Basicas(pieza.Posicion, pieza.Color, tablero);
                default:
                    return new List<(int X, int Y)> { (0, 0) };
            }
        }
    }
}
