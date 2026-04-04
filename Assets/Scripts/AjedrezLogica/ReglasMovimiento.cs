using AjedrezLogica.Recursos;
using AjedrezLogica.TiposReglasMovimiento;
using System.Collections.Generic;

namespace AjedrezLogica
{
    public class ReglasMovimiento
    {
        public static List<(int X, int Y)> MovimientosValidos( Pieza pieza, Tablero tablero, BaseJuego baseJuego)
        {
            switch (pieza.Tipo)
            {
                case TipoPieza.Peon:
                    return ReglasPeon.Reglas(pieza.Posicion, pieza.Color, pieza.Habilidad.TipoHabilidad, tablero);
                case TipoPieza.Torre:
                    return ReglasTorre.Reglas(pieza.Posicion, pieza.Color, pieza.Habilidad.TipoHabilidad, tablero);
                case TipoPieza.Alfil:
                    return ReglasAlfil.Reglas(pieza.Posicion, pieza.Color, pieza.Habilidad.TipoHabilidad, tablero);
                case TipoPieza.Caballo:
                    return ReglasCaballo.Reglas(pieza.Posicion, pieza.Color, pieza.Habilidad.TipoHabilidad, tablero);
                case TipoPieza.Dama:
                    return ReglasDama.Reglas(pieza.Posicion, pieza.Color, pieza.Habilidad.TipoHabilidad, tablero);
                case TipoPieza.Rey:
                    return ReglasRey.Reglas(pieza.Posicion, pieza.Color, pieza.Habilidad.TipoHabilidad, tablero, baseJuego);
                default:
                    return new List<(int X, int Y)> { (0, 0) };
            }
        }
    }
}
