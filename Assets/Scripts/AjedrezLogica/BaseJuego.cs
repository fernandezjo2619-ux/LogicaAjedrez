using AjedrezLogica.Recursos;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AjedrezLogica
{
    public class BaseJuego
    {
        public Tablero Tablero { get; private set; }
        public ColorPieza TurnoActual { get; private set; } = ColorPieza.Blanco;
        public List<Pieza> ListaPiezas { get; private set; } = new List<Pieza>();
        public List<Habilidad> ListaHabilidades { get; private set; } = new List<Habilidad>();

        public void CambiarTurno()
        {
            TurnoActual = TurnoActual == ColorPieza.Blanco ? ColorPieza.Negro : ColorPieza.Blanco;
        }

        public BaseJuego()
        {
            inicializaciones();
        }

        public void IniciarPieza(TipoPieza tipo, ColorPieza color, int x, int y, Tablero tablero, TipoHabilidad tipoHabilidad = TipoHabilidad.vacio)
        {
            Pieza pieza = new Pieza();
            pieza.Tipo = tipo;
            pieza.Color = color;
            pieza.Habilidad = ListaHabilidades.Where(e => e.TipoHabilidad == tipoHabilidad && e.TipoPieza == tipo).FirstOrDefault();

            pieza.Posicion = (x, y);
            tablero.Grid[x, y].Ocupante = pieza;

            ListaPiezas.Add(pieza);
        }

        private void InicializarHabilidades(TipoPieza tipoPieza, TipoHabilidad tipoHabilidad)
        {
            Habilidad habilidad = new Habilidad();
            habilidad.TipoHabilidad = tipoHabilidad;
            habilidad.TipoPieza = tipoPieza;
            ListaHabilidades.Add(habilidad);
        }

        public void inicializaciones()
        {
            Tablero = new Tablero(8, 8);
            foreach (TipoPieza tipoPieza in Enum.GetValues(typeof(TipoPieza)))
            {
                InicializarHabilidades(tipoPieza, TipoHabilidad.vacio);
            }
            foreach (TipoHabilidad tipoHabilidad in Enum.GetValues(typeof(TipoHabilidad)))
            {
                int valor = (int)tipoHabilidad;
                if (valor <= 0) { }
                else if (valor <= 4)
                    InicializarHabilidades(TipoPieza.Peon, tipoHabilidad);
                else if (valor <= 6)
                    InicializarHabilidades(TipoPieza.Torre, tipoHabilidad);
                else if (valor <= 8)
                    InicializarHabilidades(TipoPieza.Alfil, tipoHabilidad);
                else if (valor <= 10)
                    InicializarHabilidades(TipoPieza.Caballo, tipoHabilidad);
                else if (valor <= 12)
                    InicializarHabilidades(TipoPieza.Dama, tipoHabilidad);
                else if (valor <= 14)
                    InicializarHabilidades(TipoPieza.Rey, tipoHabilidad);

            }

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
