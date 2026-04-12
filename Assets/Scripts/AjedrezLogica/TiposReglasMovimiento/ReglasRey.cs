using AjedrezLogica.Recursos;
using System;
using System.Collections.Generic;
using System.Text;

namespace AjedrezLogica.TiposReglasMovimiento
{
    public class ReglasRey
    {
        public static List<(int X, int Y)> Reglas((int x, int y) posicion, ColorPieza bando, TipoHabilidad tipohabilidad, Tablero tablero, BaseJuego baseJuego)
        {
            switch (tipohabilidad)
            {
                case TipoHabilidad.DecretoReal:
                    return Basicas(posicion, bando, tablero, baseJuego);
                case TipoHabilidad.ReclutamientoForzado:
                    return Basicas(posicion, bando, tablero, baseJuego);
                default:
                    return Basicas(posicion, bando, tablero, baseJuego);
            }
        }

        public static List<(int X, int Y)> Basicas((int x, int y) posicion, ColorPieza bando, Tablero tablero, BaseJuego baseJuego)
        {
            List<(int X, int Y)> MovimientosPosibles = new List<(int X, int Y)>();
            (int dx, int dy)[] direcciones = { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (-1, 1), (1, -1), (-1, -1) };

            foreach (var (dx, dy) in direcciones)
            {
                if (tablero.EsDentroDelTablero(posicion.x + dx, posicion.y + dy) && !baseJuego.MovimientoDejaEnJaque(tablero.Grid[posicion.x, posicion.y].Ocupante, posicion.x + dx, posicion.y + dy, tablero.Grid[posicion.x, posicion.y].Ocupante.Color))
                {
                    MovimientosHelp.AgregarPosicion(MovimientosPosibles, posicion, dx, dy, bando, tablero);
                }
            }

            // Enroque
            Enrroque(posicion, MovimientosPosibles, bando, tablero, baseJuego);


            return MovimientosPosibles;
        }

        public static void Enrroque((int x, int y) posicion, List<(int X, int Y)> MovimientosPosibles, ColorPieza bando, Tablero tablero, BaseJuego baseJuego)
        {
            Pieza rey = tablero.Grid[posicion.x, posicion.y].Ocupante;

            if (!rey.SeHaMovido && !baseJuego.EstaEnJaque(bando))
            {
                int fila = bando == ColorPieza.Blanco ? 0 : 7;

                // Enroque corto (columna 6) — Torre en columna 7
                Pieza? torreCorta = tablero.Grid[fila, 7].Ocupante;
                if (torreCorta != null &&
                    torreCorta.Tipo == TipoPieza.Torre &&
                    !torreCorta.SeHaMovido &&
                    CasillasCaminoVacias(tablero, fila, 5, 7) &&
                    !baseJuego.MovimientoDejaEnJaque(rey, fila, 5, rey.Color) &&
                    !baseJuego.MovimientoDejaEnJaque(rey, fila, 6, rey.Color))
                {
                    MovimientosPosibles.Add((fila, 6));
                }

                // Enroque largo (columna 2) — Torre en columna 0
                Pieza? torreLarga = tablero.Grid[fila, 0].Ocupante;
                if (torreLarga != null &&
                    torreLarga.Tipo == TipoPieza.Torre &&
                    !torreLarga.SeHaMovido &&
                    CasillasCaminoVacias(tablero, fila, 1, 4) &&
                    !baseJuego.MovimientoDejaEnJaque(rey, fila, 3, rey.Color) &&
                    !baseJuego.MovimientoDejaEnJaque(rey, fila, 2, rey.Color))
                {
                    MovimientosPosibles.Add((fila, 2));
                }
            }
        }

        public static bool CasillasCaminoVacias(Tablero tablero, int fila, int colDesde, int colHasta)
        {
            int paso = colDesde < colHasta ? 1 : -1;
            for (int col = colDesde; col != colHasta; col += paso)
            {
                if (tablero.Grid[fila, col].EstaOcupado)
                    return false;
            }
            return true;
        }

        internal static List<(int X, int Y)> Basicas((int x, int y) value, ColorPieza color, Tablero tableroLogico)
        {
            throw new NotImplementedException();
        }
    }
}
