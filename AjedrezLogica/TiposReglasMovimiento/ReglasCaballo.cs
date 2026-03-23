using AjedrezLogica.Recursos;
using System;
using System.Collections.Generic;
using System.Text;

namespace AjedrezLogica.TiposReglasMovimiento
{
    internal class ReglasCaballo
    {
        public List<(int X, int Y)> Reglas((int x, int y) posicion, ColorPieza bando, Habilidad habilidad, Tablero tablero)
        {
            switch (habilidad)
            {
                case :
                    break;
                default:
                    return Basicas(posicion, bando, tablero);
                    break;
            }
        }

        public static List<(int X, int Y)> Basicas((int x, int y) posicion, ColorPieza bando, Tablero tablero)
        {
            List<(int X, int Y)> MovimientosPosibles = new List<(int X, int Y)>();

            (int dx, int dy)[] direcciones = { (2, 1), (2, -1), (-2, 1), (-2, -1), (1, 2), (-1, 2), (1, -2), (-1, -2) };

            foreach (var (dx, dy) in direcciones)
            {
                MovimientosHelp.AgregarPosicion(MovimientosPosibles, posicion, dx, dy, bando, tablero);
            }

            return MovimientosPosibles;
        }
    }
}
