using AjedrezLogica.Recursos;
using System.Collections.Generic;

namespace AjedrezLogica.TiposReglasMovimiento
{
    internal class ReglasPeon
    {
        public static List<(int X, int Y)> Basicas((int x, int y) posicion, ColorPieza bando, Tablero tablero)
        {
            List<(int X, int Y)> MovimientosPosibles = new List<(int X, int Y)>();
            int direccion = bando == ColorPieza.Blanco ? 1 : -1;
            int filaInicial = bando == ColorPieza.Blanco ? 1 : tablero.Alto - 2;

            if (tablero.EsDentroDelTablero(posicion.x + direccion, posicion.y))
            {
                MovimientosPosibles.Add((posicion.x + direccion, posicion.y));
            }

            if (posicion.x.Equals(filaInicial))
            {
                MovimientosPosibles.Add((posicion.x + direccion * 2, posicion.y));
            }
            AgregarSiPuedeCapturar(MovimientosPosibles, posicion.x + direccion, posicion.y - 1, bando, tablero);
            AgregarSiPuedeCapturar(MovimientosPosibles, posicion.x + direccion, posicion.y + 1, bando, tablero);

            return MovimientosPosibles;
        }

        private static void AgregarSiPuedeCapturar(
            List<(int X, int Y)> movimientos,
            int x, int y,
            ColorPieza bando,
            Tablero tablero)
        {
            if (tablero.EsDentroDelTablero(x, y)
                && tablero.Grid[x, y].EstaOcupado
                && !tablero.Grid[x, y].Ocupante.Color.Equals(bando))
            {
                movimientos.Add((x, y));
            }
        }
    }
}
