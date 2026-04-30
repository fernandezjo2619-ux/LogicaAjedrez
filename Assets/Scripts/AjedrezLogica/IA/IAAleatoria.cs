using AjedrezLogica.IA.Estructuras;
using AjedrezLogica.IA.Utilidad;
using AjedrezLogica.Recursos;
using AjedrezLogica.TiposReglasMovimiento;
using System.Collections.Generic;
using System.Linq;

namespace AjedrezLogica.IA
{
    public class IAAleatoria : IMotorIA
    {
        private IAHelp iAHelp = new();

        public void InicializarPiezasDeIA(BaseJuego baseJuego, ColorPieza color)
        {
            iAHelp.InicializarPiezasDeIA(baseJuego, color);
        }

        /**
         * Antes de usar, es necesario hacer una valoracion sobre TipoAccion:
         *      Si == Movimiento llamar a juego.RealizarMovimiento
         *      Si == Empujon llamar a juego.EjecutarEmpujon
         */
        public Accion ElegirMovimiento(BaseJuego baseJuego, ColorPieza color)
        {
            List<Accion> opciones = new List<Accion>();

            // Añadir movimientos normales
            foreach (var (pieza, destinos) in baseJuego.MovimientosPosiblesBando(color))
            {
                foreach (var (x, y) in destinos)
                {
                    opciones.Add(new Accion
                    {
                        Tipo = TipoAccion.Movimiento,
                        Pieza = pieza,
                        XFin = x,
                        YFin = y,
                        PiezaEliminada = baseJuego.Tablero.Grid[x, y].EstaOcupado ? baseJuego.Tablero.Grid[x, y].Ocupante.Color == color ? null : baseJuego.Tablero.Grid[x, y].Ocupante : null
                    });
                }
            }

            // Añadir empujones de piezas con habilidad
            foreach (Pieza pieza in baseJuego.ListaPiezas
                .Where(p => p.Color == color &&
                       p.Habilidad?.TipoHabilidad == TipoHabilidad.EmbestidaReal))
            {
                foreach (var (empujada, xd, yd) in ReglasDama.EmpujonesDisponibles(pieza.Posicion, color, baseJuego.Tablero))
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

            // Elegir al azar
            return opciones.Count == 0 ? new() : opciones[iAHelp.random.Next(opciones.Count)];
        }
    }
}
