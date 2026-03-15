using AjedrezLogica.Recursos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace AjedrezLogica
{
    public class BaseJuego
    {
        public Tablero Tablero { get; private set; }
        public ColorPieza TurnoActual { get; private set; } = ColorPieza.Blanco;
        public List<Pieza> ListaPiezas { get; private set; } = new List<Pieza>();

        public void CambiarTurno()
        {
            TurnoActual = TurnoActual == ColorPieza.Blanco ? ColorPieza.Negro : ColorPieza.Blanco;
        }

        public BaseJuego()
        {
            inicializaciones();
        }

        public void IniciarPieza(TipoPieza tipo, ColorPieza color, int x, int y, Tablero tablero)
        {
            Pieza pieza = new Pieza();
            pieza.Tipo = tipo;
            pieza.Color = color;
            pieza.Posicion = (x, y);
            tablero.Grid[x, y].Ocupante = pieza;

            ListaPiezas.Add(pieza);
        }

        public void inicializaciones()
        {
            Tablero = new Tablero(8, 8);

            for (int col = 0; col < 8; col++)
            {
                IniciarPieza(TipoPieza.Peon, ColorPieza.Blanco, 1, col, Tablero);
                IniciarPieza(TipoPieza.Peon, ColorPieza.Negro, 6, col, Tablero);
            }

            // Torres
            IniciarPieza(TipoPieza.Torre, ColorPieza.Blanco, 0, 0, Tablero);
            IniciarPieza(TipoPieza.Torre, ColorPieza.Blanco, 0, 7, Tablero);
            IniciarPieza(TipoPieza.Torre, ColorPieza.Negro, 7, 0, Tablero);
            IniciarPieza(TipoPieza.Torre, ColorPieza.Negro, 7, 7, Tablero);
            // Caballos
            IniciarPieza(TipoPieza.Caballo, ColorPieza.Blanco, 0, 1, Tablero);
            IniciarPieza(TipoPieza.Caballo, ColorPieza.Blanco, 0, 6, Tablero);
            IniciarPieza(TipoPieza.Caballo, ColorPieza.Negro, 7, 1, Tablero);
            IniciarPieza(TipoPieza.Caballo, ColorPieza.Negro, 7, 6, Tablero);
            // Alfiles
            IniciarPieza(TipoPieza.Alfil, ColorPieza.Blanco, 0, 2, Tablero);
            IniciarPieza(TipoPieza.Alfil, ColorPieza.Blanco, 0, 5, Tablero);
            IniciarPieza(TipoPieza.Alfil, ColorPieza.Negro, 7, 2, Tablero);
            IniciarPieza(TipoPieza.Alfil, ColorPieza.Negro, 7, 5, Tablero);
            // Damas
            IniciarPieza(TipoPieza.Dama, ColorPieza.Blanco, 0, 3, Tablero);
            IniciarPieza(TipoPieza.Dama, ColorPieza.Negro, 7, 3, Tablero);
            // Reyes
            IniciarPieza(TipoPieza.Rey, ColorPieza.Blanco, 0, 4, Tablero);
            IniciarPieza(TipoPieza.Rey, ColorPieza.Negro, 7, 4, Tablero);

        }

        public void RealizarMovimiento(int xOrigen, int yOrigen, int xFin, int yFin)
        {
            if (!Tablero.Grid[xOrigen, yOrigen].EstaOcupado)
            {
                return;
            }
            Pieza pieza = Tablero.Grid[xOrigen, yOrigen].Ocupante;

            if (!pieza.Color.Equals(TurnoActual))
            {
                return;
            }

            List<(int x, int y)> movimientos;
            movimientos = ReglasMovimiento.MovimientosValidos(pieza, Tablero);

            if (movimientos.Contains((xFin, yFin)))
            {
                if (Tablero.Grid[xFin, yFin].EstaOcupado && Tablero.Grid[xFin, yFin].Ocupante.Tipo.Equals(TipoPieza.Rey))
                {
                    Console.WriteLine("GANADOR: {0}", TurnoActual);
                    return;
                }
                Tablero.Grid[xOrigen, yOrigen].Ocupante = null;
                pieza.Posicion = (xFin, yFin);
                Tablero.Grid[xFin, yFin].Ocupante = pieza;

                CambiarTurno();
            }
        }

        public List<(Pieza pieza, List<(int X, int Y)>)> MovimientosPosiblesBando(ColorPieza Color)
        {
            List<(Pieza pieza, List<(int X, int Y)>)> movimientosPosiblesbando = new List<(Pieza pieza, List<(int X, int Y)>)>(); 
            foreach (Pieza pieza in ListaPiezas)
            {
                if (pieza.Color.Equals(Color))
                {
                    movimientosPosiblesbando.Add((pieza, ReglasMovimiento.MovimientosValidos(pieza, Tablero)));
                }
            }
            return movimientosPosiblesbando;
        }
    }
}
