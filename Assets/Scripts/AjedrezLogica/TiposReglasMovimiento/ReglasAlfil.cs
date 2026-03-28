using AjedrezLogica.Recursos;
using System;
using System.Collections.Generic;
using System.Text;

namespace AjedrezLogica.TiposReglasMovimiento
{
    public class ReglasAlfil
    {
        public static List<(int X, int Y)> Reglas((int x, int y) posicion, ColorPieza bando, TipoHabilidad tipohabilidad, Tablero tablero)
        {
            //switch (habilidad)
            //{
            //    default:
                    return Basicas(posicion, bando, tablero);
            //        break;
            //}
        }

        public static List<(int X, int Y)> Basicas((int x, int y) posicion, ColorPieza bando, Tablero tablero)
        {   
            List<(int X, int Y)> MovimientosPosibles = new List<(int X, int Y)>();
            (int dx, int dy)[] direcciones = { (1, 1), (-1, 1), (1, -1), (-1, -1) };

            foreach (var (dx, dy) in direcciones)
            {
                MovimientosHelp.AgregarDireccion(MovimientosPosibles, posicion, dx, dy, bando, tablero);
            }

            return MovimientosPosibles;
        }
    }
}
