using AjedrezLogica.Recursos;
using System;
using System.Collections.Generic;
using System.Text;

namespace AjedrezLogica.TiposReglasMovimiento
{
    internal class MovimientosHelp
    {
        public static void AgregarDireccion(
            List<(int X, int Y)> movimientos,
            (int x, int y) posicion,
            int dx, int dy,
            ColorPieza bando,
            Tablero tablero)
        {
            int i = 1;

            while (tablero.EsDentroDelTablero(posicion.x + dx * i, posicion.y + dy * i)
             && (!tablero.Grid[posicion.x + dx * i, posicion.y + dy * i].EstaOcupado
             || !tablero.Grid[posicion.x + dx * i, posicion.y + dy * i].Ocupante.Color.Equals(bando)))
            {
                movimientos.Add((posicion.x + dx * i, posicion.y + dy * i));
                i++;
            }

        }

        public static void AgregarPosicion(
           List<(int X, int Y)> movimientos,
           (int x, int y) posicion,
           int dx, int dy,
           ColorPieza bando,
           Tablero tablero)
        {

            if (tablero.EsDentroDelTablero(posicion.x + dx, posicion.y + dy)
             && (!tablero.Grid[posicion.x + dx, posicion.y + dy].EstaOcupado
             || !tablero.Grid[posicion.x + dx, posicion.y + dy].Ocupante.Color.Equals(bando)))
            {
                movimientos.Add((posicion.x + dx, posicion.y + dy));
            }

        }
    }
}
