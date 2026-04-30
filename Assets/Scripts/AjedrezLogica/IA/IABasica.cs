using AjedrezLogica.IA.Estructuras;
using AjedrezLogica.IA.Utilidad;
using AjedrezLogica.Recursos;
using System.Collections.Generic;
using System.Linq;

namespace AjedrezLogica.IA
{
    public class IABasica : IMotorIA
    {
        private IAHelp iAHelp = new();
        private BaseJuego basePruebas = new();

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
            List<Accion> opciones = iAHelp.ObtenerTodasLasOpciones(baseJuego, color);

            if (!opciones.Any()) return default;

            Accion mejorAccion = default;
            int mejorPuntuacion = int.MinValue;

            foreach (Accion accion in opciones)
            {
                // Simular el movimiento
                int puntuacion = SimularYEvaluar(baseJuego, accion, color);

                if (puntuacion > mejorPuntuacion)
                {
                    mejorPuntuacion = puntuacion;
                    mejorAccion = accion;
                }
            }

            return mejorAccion;
        }
        private int SimularYEvaluar(BaseJuego baseJuego, Accion accion, ColorPieza color)
        {
            // Crear insatncia de juego nueva para no modificar la del juego            
            basePruebas = baseJuego;
            Pieza? ocupante = basePruebas.Tablero.Grid[accion.Pieza.Posicion.X, accion.Pieza.Posicion.Y].Ocupante;
            var Xorigen = accion.Pieza.Posicion.X;
            var Yorigen = accion.Pieza.Posicion.Y;

            // Simular
            basePruebas.Tablero.Grid[accion.Pieza.Posicion.X, accion.Pieza.Posicion.Y].Ocupante = null;
            basePruebas.Tablero.Grid[accion.XFin, accion.YFin].Ocupante = accion.Pieza;
            accion.Pieza.Posicion = (accion.XFin, accion.YFin);

            int puntuacion = Evaluador.EvaluarBasico(basePruebas, color);

            // Retroceder
            basePruebas.Tablero.Grid[accion.Pieza.Posicion.X, accion.Pieza.Posicion.Y].Ocupante = ocupante;
            basePruebas.Tablero.Grid[Xorigen, Yorigen].Ocupante = accion.Pieza;
            accion.Pieza.Posicion = (Xorigen, Yorigen);

            return puntuacion;
        }
    }
}