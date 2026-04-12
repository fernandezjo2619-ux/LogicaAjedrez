using AjedrezLogica.Recursos;
using System;
using System.Collections.Generic;

namespace AjedrezLogica.TiposReglasMovimiento
{
    public class ReglasPeon
    {
        public static List<(int X, int Y)> Reglas((int x, int y) posicion, ColorPieza bando, TipoHabilidad tipohabilidad, Tablero tablero)
        {
            switch (tipohabilidad)
            {
                case TipoHabilidad.Mimico:
                    // Esta habilidad se tiene que valorar en el tablero
                    // Se transforma en la pieza que come
                    // Reglas de movimiento iguales
                    return Basicas(posicion, bando, tablero);
                case TipoHabilidad.Reverso:
                    // Esta habilidad se tiene que valorar en el tablero
                    // La pieza que se lo come se transforma el peon, exceptuando la dama y el rey
                    // Reglas de movimiento iguales
                    return Basicas(posicion, bando, tablero);
                case TipoHabilidad.PasoForzado:
                    // Doble Movimiento
                    return Basicas(posicion, bando, tablero, true);
                case TipoHabilidad.MovimientoCruzado:
                    // Se puede mover lateralmente incluso si no hay pieza que pueda eliminar
                    return Basicas(posicion, bando, tablero, false , true);
                default:
                    return Basicas(posicion, bando, tablero);
            }
        }

        public static List<(int X, int Y)> Basicas((int x, int y) posicion, ColorPieza bando, Tablero tablero, bool doblepaso = false, bool movimientocruzado = false)
        {
            List<(int X, int Y)> MovimientosPosibles = new List<(int X, int Y)>();
            int direccion = bando == ColorPieza.Blanco ? 1 : -1;
            int filaInicial = bando == ColorPieza.Blanco ? 1 : tablero.Alto - 2;

            if (tablero.EsDentroDelTablero(posicion.x + direccion, posicion.y) && !tablero.Grid[posicion.x + direccion, posicion.y].EstaOcupado)
            {
                MovimientosPosibles.Add((posicion.x + direccion, posicion.y));
            }
            AgregarPasoInicial(MovimientosPosibles, posicion.x, posicion.y, 2, direccion, filaInicial, tablero);

            if (doblepaso)
            {
                if (!posicion.x.Equals(filaInicial) && tablero.EsDentroDelTablero(posicion.x + direccion * 2, posicion.y) && !tablero.Grid[posicion.x + direccion * 2, posicion.y].EstaOcupado)
                {
                    MovimientosPosibles.Add((posicion.x + direccion * 2, posicion.y));
                }
                AgregarPasoInicial(MovimientosPosibles, posicion.x, posicion.y, 3, direccion, filaInicial, tablero);
            }

            AgregarSiPuedeCapturar(MovimientosPosibles, posicion.x + direccion, posicion.y - 1, bando, tablero, movimientocruzado);
            AgregarSiPuedeCapturar(MovimientosPosibles, posicion.x + direccion, posicion.y + 1, bando, tablero, movimientocruzado);

            return MovimientosPosibles;
        }

        private static void AgregarPasoInicial(
            List<(int X, int Y)> movimientos,
            int x, int y, int multiplicador,
            int direccion,
            int filaInicial,
            Tablero tablero)
        {
            if (x.Equals(filaInicial) && !tablero.Grid[x + direccion * multiplicador, y].EstaOcupado)
            {
                movimientos.Add((x + direccion * multiplicador, y));
            }
        }

        private static void AgregarSiPuedeCapturar(
            List<(int X, int Y)> movimientos,
            int x, int y,
            ColorPieza bando,
            Tablero tablero,
            bool movimientocruzado)
        {
            if (movimientocruzado)
            {
                if (tablero.EsDentroDelTablero(x, y)
                && (!tablero.Grid[x, y].EstaOcupado
                || !tablero.Grid[x, y].Ocupante.Color.Equals(bando)))
                {
                    movimientos.Add((x, y));
                }
            }
            else
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
}
