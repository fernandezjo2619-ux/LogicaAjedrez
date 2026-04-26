using AjedrezLogica.Recursos;
using System.Collections.Generic;
using System.Linq;

namespace AjedrezLogica.TiposReglasMovimiento
{
    public class ReglasRey
    {
        public static List<(int X, int Y)> Reglas((int x, int y) posicion, ColorPieza bando, TipoHabilidad tipohabilidad, Tablero tablero, BaseJuego baseJuego)
        {
            List<(int X, int Y)> MovimientosPosibles = new List<(int X, int Y)>();
            switch (tipohabilidad)
            {
                case TipoHabilidad.DecretoReal:
                    DecretoReal(posicion, bando, tablero, baseJuego, MovimientosPosibles);
                    return MovimientosPosibles;
                case TipoHabilidad.SituacionDesesperada:
                    if (baseJuego.ListaPiezas.Any(dama => dama.Tipo == TipoPieza.Dama))
                    {
                        Basicas(posicion, bando, tablero, baseJuego, MovimientosPosibles);
                    }
                    else
                    {
                        SituacionDesesperada(posicion, bando, tablero, baseJuego, MovimientosPosibles);
                    }
                    return MovimientosPosibles;
                default:
                    Basicas(posicion, bando, tablero, baseJuego, MovimientosPosibles);
                    return MovimientosPosibles;
            }
        }

        public static void SituacionDesesperada((int x, int y) posicion, ColorPieza bando, Tablero tablero, BaseJuego baseJuego, List<(int X, int Y)> MovimientosPosibles)
        {
            (int dx, int dy)[] direcciones = { (1, 0), (-1, 0), (0, 1), (0, -1) };

            foreach (var (dx, dy) in direcciones)
            {
                if (tablero.EsDentroDelTablero(posicion.x + dx, posicion.y + dy) && !baseJuego.MovimientoDejaEnJaque(tablero.Grid[posicion.x, posicion.y].Ocupante, posicion.x + dx, posicion.y + dy, tablero.Grid[posicion.x, posicion.y].Ocupante.Color))
                {
                    MovimientosHelp.AgregarDireccion(MovimientosPosibles, posicion, dx, dy, bando, tablero);
                }
            }
        }

        public static void DecretoReal((int x, int y) posicion, ColorPieza bando, Tablero tablero, BaseJuego baseJuego, List<(int X, int Y)> MovimientosPosibles)
        {
            (int dx, int dy)[] direcciones = { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (-1, 1), (1, -1), (-1, -1) };

            foreach (var (dx, dy) in direcciones)
            {
                if (tablero.EsDentroDelTablero(posicion.x + dx, posicion.y + dy) && !baseJuego.MovimientoDejaEnJaque(tablero.Grid[posicion.x, posicion.y].Ocupante, posicion.x + dx, posicion.y + dy, tablero.Grid[posicion.x, posicion.y].Ocupante.Color))
                {
                    MovimientosHelp.AgregarDireccion(MovimientosPosibles, posicion, dx, dy, bando, tablero, true, false, 1 , 3);
                }
            }
        }

        public static void Basicas((int x, int y) posicion, ColorPieza bando, Tablero tablero, BaseJuego baseJuego, List<(int X, int Y)> MovimientosPosibles)
        {
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
        }

        public static void Enrroque((int x, int y) posicion, List<(int X, int Y)> MovimientosPosibles, ColorPieza bando, Tablero tablero, BaseJuego baseJuego)
        {
            Pieza rey = tablero.Grid[posicion.x, posicion.y].Ocupante;

            if (!rey.SeHaMovido && !baseJuego.EstaEnJaque(bando))
            {
                int fila = bando == ColorPieza.Blanco ? 0 : 7;

                // Enroque corto (columna 6) — Torre en columna 7
                Pieza torreCorta = tablero.Grid[fila, 7].Ocupante;
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
                Pieza torreLarga = tablero.Grid[fila, 0].Ocupante;
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
                if (tablero.Grid[fila, col].EstaOcupado) { return false; }
            }
            return true;
        }
    }
}
