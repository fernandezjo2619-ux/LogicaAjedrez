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
            switch (tipohabilidad)
            {
                case TipoHabilidad.Estratega:
                    return Basicas(posicion, bando, tablero, true);
                case TipoHabilidad.AnuladorDeHabilidad:
                    return Basicas(posicion, bando, tablero);
                default:
                    return Basicas(posicion, bando, tablero);
            }
        }

        public static List<(int X, int Y)> Basicas((int x, int y) posicion, ColorPieza bando, Tablero tablero, bool reposicionamiento = false)
        {
            List<(int X, int Y)> MovimientosPosibles = new List<(int X, int Y)>();
            (int dx, int dy)[] direcciones = { (1, 1), (-1, 1), (1, -1), (-1, -1) };

            foreach (var (dx, dy) in direcciones)
            {
                MovimientosHelp.AgregarDireccion(MovimientosPosibles, posicion, dx, dy, bando, tablero);
            }

            if (reposicionamiento)
            {
                (int dx, int dy)[] direccion = { (1, 0), (-1, 0), (0, 1), (0, -1) };
                foreach (var (dx, dy) in direccion)
                {
                    if (tablero.EsDentroDelTablero(posicion.x + dx, posicion.y + dy) && !tablero.Grid[posicion.x + dx , posicion.y + dy].EstaOcupado)
                    {
                        MovimientosPosibles.Add((posicion.x + dx, posicion.y + dy));
                    }
                }
            }

            return MovimientosPosibles;
        }
    }
}
