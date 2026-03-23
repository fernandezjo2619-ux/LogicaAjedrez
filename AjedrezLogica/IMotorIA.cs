using AjedrezLogica.Recursos;
using System;
using System.Collections.Generic;
using System.Text;

namespace AjedrezLogica
{
    public interface IMotorIA
    {
        void ElegirHabilidadesDePiezas(BaseJuego baseJuego, ColorPieza color);
        (Pieza pieza, int xFin, int yFin) ElegirMovimiento(BaseJuego baseJuego, ColorPieza color);
    }
}
