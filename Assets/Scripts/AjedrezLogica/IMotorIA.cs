using AjedrezLogica.Recursos;
using AjedrezLogica.IA.Estructuras;
using System;
using System.Collections.Generic;
using System.Text;

namespace AjedrezLogica
{
    public interface IMotorIA
    {
        void InicializarPiezasDeIA(BaseJuego baseJuego, ColorPieza color);
        Accion ElegirMovimiento(BaseJuego baseJuego, ColorPieza color);
    }
}
