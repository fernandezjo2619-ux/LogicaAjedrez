using AjedrezLogica.IA.Estructuras;
using AjedrezLogica.IA.Utilidad;
using AjedrezLogica.Recursos;
using AjedrezLogica.TiposReglasMovimiento;
using System.Collections.Generic;
using System.Linq;

namespace AjedrezLogica.IA
{
    public class IAMedia : IMotorIA
    {

        private const int PROFUNDIDAD = 2;
        private Minimax Minimax = new();
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
            List<Accion> opciones = iAHelp.ObtenerTodasLasOpciones(baseJuego, color);
 
            if (!opciones.Any()) return default;

            Accion mejorAccion = default;
            int mejorPuntuacion = int.MinValue;

            foreach (Accion accion in opciones)
            {
                // Simular
                EstadoAnterior estado = Minimax.Simular(baseJuego, accion);

                // Minimax desde el punto de vista del enemigo
                int puntuacion = Minimax.MiniMax(baseJuego, PROFUNDIDAD - 1,
                    int.MinValue, int.MaxValue, false, color);

                // Deshacer
                Minimax.Deshacer(baseJuego, accion, estado);

                if (puntuacion > mejorPuntuacion)
                {
                    mejorPuntuacion = puntuacion;
                    mejorAccion = accion;
                }
            }

            return mejorAccion;
        }
    }
}