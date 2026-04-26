using AjedrezLogica;
using AjedrezLogica.IA.Estructuras;
using AjedrezLogica.IA.Utilidad;
using AjedrezLogica.Recursos;
using AjedrezLogica.TiposReglasMovimiento;
using System.Collections.Generic;
using System.Linq;

namespace AjedrezLogica.IA.Utilidad
{
    public class Minimax
    {
        public int MiniMax(BaseJuego juego, int profundidad,
            int alfa, int beta, bool maximizando, ColorPieza colorIA)
        {
            // Caso base
            if (profundidad == 0)
                return Evaluador.EvaluarAvanzado(juego, colorIA);

            ColorPieza turno = maximizando ? colorIA
                : (colorIA == ColorPieza.Blanco ? ColorPieza.Negro : ColorPieza.Blanco);

            List<Accion> opciones = ObtenerTodasLasOpciones(juego, turno);

            if (!opciones.Any())
                return Evaluador.EvaluarAvanzado(juego, colorIA);

            if (maximizando)
            {
                int maxEval = int.MinValue;

                foreach (Accion accion in opciones)
                {
                    EstadoAnterior estado = Simular(juego, accion);
                    int eval = MiniMax(juego, profundidad - 1, alfa, beta, false, colorIA);
                    Deshacer(juego, accion, estado);

                    maxEval = System.Math.Max(maxEval, eval);
                    alfa = System.Math.Max(alfa, eval);

                    if (beta <= alfa) break; // Poda alfa-beta
                }

                return maxEval;
            }
            else
            {
                int minEval = int.MaxValue;

                foreach (Accion accion in opciones)
                {
                    EstadoAnterior estado = Simular(juego, accion);
                    int eval = MiniMax(juego, profundidad - 1, alfa, beta, true, colorIA);
                    Deshacer(juego, accion, estado);

                    minEval = System.Math.Min(minEval, eval);
                    beta = System.Math.Min(beta, eval);

                    if (beta <= alfa) break; // Poda alfa-beta
                }

                return minEval;
            }
        }

        public EstadoAnterior Simular(BaseJuego juego, Accion accion)
        {
            EstadoAnterior estado = new EstadoAnterior
            {
                PiezaCapturada = juego.Tablero.Grid[accion.XFin, accion.YFin].Ocupante,
                PosicionOriginal = accion.Pieza.Posicion
            };

            juego.Tablero.Grid[estado.PosicionOriginal.X, estado.PosicionOriginal.Y].Ocupante = null;
            juego.Tablero.Grid[accion.XFin, accion.YFin].Ocupante = accion.Pieza;
            accion.Pieza.Posicion = (accion.XFin, accion.YFin);

            return estado;
        }

        public void Deshacer(BaseJuego juego, Accion accion, EstadoAnterior estado)
        {
            accion.Pieza.Posicion = estado.PosicionOriginal;
            juego.Tablero.Grid[estado.PosicionOriginal.X, estado.PosicionOriginal.Y].Ocupante = accion.Pieza;
            juego.Tablero.Grid[accion.XFin, accion.YFin].Ocupante = estado.PiezaCapturada;
            if (estado.PiezaCapturada != null)
                estado.PiezaCapturada.Posicion = (accion.XFin, accion.YFin);
        }

        private List<Accion> ObtenerTodasLasOpciones(BaseJuego juego, ColorPieza color)
        {
            List<Accion> opciones = new List<Accion>();

            foreach (var (pieza, destinos) in juego.MovimientosPosiblesBando(color))
            {
                foreach (var (x, y) in destinos)
                {
                    opciones.Add(new Accion
                    {
                        Tipo = TipoAccion.Movimiento,
                        Pieza = pieza,
                        XFin = x,
                        YFin = y
                    });
                }
            }

            // Empujones
            foreach (Pieza pieza in juego.ListaPiezas.Where(p =>
                p.Color == color &&
                p.Habilidad?.TipoHabilidad == TipoHabilidad.EmbestidaReal))
            {
                foreach (var (empujada, xd, yd) in ReglasDama
                    .EmpujonesDisponibles(pieza.Posicion, color, juego.Tablero))
                {
                    opciones.Add(new Accion
                    {
                        Tipo = TipoAccion.Empujon,
                        Pieza = pieza,
                        PiezaEmpujada = empujada,
                        XFin = xd,
                        YFin = yd
                    });
                }
            }

            return opciones;
        }
    }

    // Clase auxiliar para guardar el estado antes de simular
    public class EstadoAnterior
    {
        public Pieza? PiezaCapturada { get; set; }
        public (int X, int Y) PosicionOriginal { get; set; }
    }

}