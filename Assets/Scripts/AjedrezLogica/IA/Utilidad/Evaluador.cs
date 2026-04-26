using AjedrezLogica.Recursos;
using System.Collections.Generic;
using System.Linq;

namespace AjedrezLogica.IA.Utilidad
{
    public static class Evaluador
    {
        private static readonly Dictionary<TipoPieza, int> ValorPieza =
            new Dictionary<TipoPieza, int>
        {
            { TipoPieza.Peon,    1   },
            { TipoPieza.Caballo, 3   },
            { TipoPieza.Alfil,   3   },
            { TipoPieza.Torre,   5   },
            { TipoPieza.Dama,    9   },
            { TipoPieza.Rey,     1000}
        };

        public static int EvaluarBasico(BaseJuego juego, ColorPieza color)
        {
            int puntuacion = 0;

            foreach (Pieza pieza in juego.ListaPiezas)
            {
                int valor = ValorPieza[pieza.Tipo];

                if (pieza.Color == color)
                    puntuacion += valor;
                else
                    puntuacion -= valor;
            }

            return puntuacion;
        }

        public static int EvaluarAvanzado(BaseJuego juego, ColorPieza color)
        {
            int puntuacion = 0;
            ColorPieza enemigo = color == ColorPieza.Blanco ? ColorPieza.Negro : ColorPieza.Blanco;

            foreach (Pieza pieza in juego.ListaPiezas)
            {
                int valor = ValorPieza[pieza.Tipo];

                // Bonus por tener habilidad equipada
                if (pieza.Habilidad != null) valor += 1;

                // Bonus por posición central (columnas 3-4, filas 3-4)
                if (pieza.Posicion.X >= 3 && pieza.Posicion.X <= 4 &&
                    pieza.Posicion.Y >= 3 && pieza.Posicion.Y <= 4)
                    valor += 1;

                if (pieza.Color == color)
                    puntuacion += valor;
                else
                    puntuacion -= valor;
            }

            // Bonus por movilidad (más movimientos = mejor posición)
            puntuacion += juego.MovimientosPosiblesBando(color, false).Sum(m => m.Item2.Count);
            puntuacion -= juego.MovimientosPosiblesBando(enemigo, false).Sum(m => m.Item2.Count);

            // Bonus por tener al enemigo en jaque
            if (juego.EstaEnJaque(enemigo)) puntuacion += 5;

            return puntuacion;
        }
    }
}