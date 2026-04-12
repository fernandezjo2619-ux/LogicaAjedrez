using AjedrezLogica.Recursos;
using System;
using System.Collections.Generic;
using System.Text;

namespace AjedrezLogica.TiposReglasMovimiento
{
    public class ReglasDama
    {
        public static List<(int X, int Y)> Reglas((int x, int y) posicion, ColorPieza bando, TipoHabilidad tipohabilidad, Tablero tablero)
        {
            switch (tipohabilidad)
            {
                case TipoHabilidad.DesplazamientoImperial:
                    return Basicas(posicion, bando, tablero, true);
                // Regla especial - Tratada en base juego
                //case TipoHabilidad.EmbestidaReal:
                //    return Basicas(posicion, bando, tablero);
                default:
                    return Basicas(posicion, bando, tablero);
            }
        }

        public static List<(int X, int Y)> Basicas((int x, int y) posicion, ColorPieza bando, Tablero tablero, bool saltaPieza = false)
        {   
            List<(int X, int Y)> MovimientosPosibles = new List<(int X, int Y)>();
            (int dx, int dy)[] direcciones = { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (-1, 1), (1, -1), (-1, -1) };

            foreach (var (dx, dy) in direcciones)
            {
                MovimientosHelp.AgregarDireccion(MovimientosPosibles, posicion, dx, dy, bando, tablero, saltaPieza );
            }

            return MovimientosPosibles;
        }

        public static List<(Pieza piezaEmpujada, int xDestino, int yDestino)> EmpujonesDisponibles((int x, int y) posicion, ColorPieza bando, Tablero tablero)
        {
            var resultado = new List<(Pieza, int, int)>();

            (int dx, int dy)[] adyacentes = { (1, 0), (-1, 0), (0, 1), (0, -1), (1, 1), (-1, 1), (1, -1), (-1, -1) };

            

            foreach (var (dx, dy) in adyacentes)
            {
                // Si se encuentra pieza enemiga adyacente
                if (!tablero.EsDentroDelTablero(posicion.x + dx, posicion.y + dy) && !tablero.Grid[posicion.x + dx, posicion.y + dy].EstaOcupado && tablero.Grid[posicion.x + dx, posicion.y + dy].Ocupante.Color == bando) { continue; }
                resultado.Add((tablero.Grid[posicion.x + dx, posicion.y + dy].Ocupante, posicion.x, posicion.y));

                    // Si la casilla destino del empujón está libre
                if (tablero.EsDentroDelTablero(posicion.x + dx + dx, posicion.y + dy + dy) && !tablero.Grid[posicion.x + dx + dx, posicion.y + dy + dy].EstaOcupado)
                {
                    resultado.Add((tablero.Grid[posicion.x + dx, posicion.y + dy].Ocupante, posicion.x + dx + dx, posicion.y + dy + dy));
                }
            }

            return resultado;
        }
    }
}
