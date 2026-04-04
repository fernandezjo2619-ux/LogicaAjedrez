using AjedrezLogica.Recursos;
using System;
using System.Collections.Generic;
using System.Text;

namespace AjedrezLogica.TiposReglasMovimiento
{
    public class ReglasCaballo
    {
        public static List<(int X, int Y)> Reglas((int x, int y) posicion, ColorPieza bando, TipoHabilidad tipohabilidad, Tablero tablero)
        {
            switch (tipohabilidad)
            {
                case TipoHabilidad.PuraSangre:
                    (int dx, int dy)[] direcciones = { (3, 1), (3, -1), (-3, 1), (-3, -1), (1, 3), (-1, 3), (1, -3), (-1, -3) };
                    return Basicas(posicion, bando, tablero, direcciones);
                case TipoHabilidad.CozParalizante:
                    (int dx, int dy)[] direcciones = { (2, 1), (2, -1), (-2, 1), (-2, -1), (1, 2), (-1, 2), (1, -2), (-1, -2) };
                    return Basicas(posicion, bando, tablero, direcciones);
                default:
                    (int dx, int dy)[] direcciones = { (2, 1), (2, -1), (-2, 1), (-2, -1), (1, 2), (-1, 2), (1, -2), (-1, -2) };
                    return Basicas(posicion, bando, tablero, direcciones);
            }
        }

        public static List<(int X, int Y)> Basicas((int x, int y) posicion, ColorPieza bando, Tablero tablero, (int dx, int dy)[] direcciones)
        {
            List<(int X, int Y)> MovimientosPosibles = new List<(int X, int Y)>();

            foreach (var (dx, dy) in direcciones)
            {
                MovimientosHelp.AgregarPosicion(MovimientosPosibles, posicion, dx, dy, bando, tablero);
            }

            return MovimientosPosibles;
        }

        public static Dictionary<(int dx, int dy), (int px, int py)[]> RutasSalto = new Dictionary<(int dx, int dy), (int px, int py)[]>
        {
            { (2,  1), new[] { (1, 0), (1, 1) } },
            { (2, -1), new[] { (1, 0), (1,-1) } },
            { (-2, 1), new[] { (-1, 0), (-1, 1) } },
            { (-2,-1), new[] { (-1, 0), (-1,-1) } },
            { (1,  2), new[] { (0, 1), (1, 1) } },
            { (1, -2), new[] { (0,-1), (1,-1) } },
            { (-1, 2), new[] { (0, 1), (-1, 1) } },
            { (-1,-2), new[] { (0,-1), (-1,-1) } },
        };
    }
}
