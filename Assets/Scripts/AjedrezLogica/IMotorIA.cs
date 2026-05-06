using AjedrezLogica.IA.Estructuras;
using AjedrezLogica.Recursos;

namespace AjedrezLogica
{
    public interface IMotorIA
    {
        void InicializarPiezasDeIA(BaseJuego baseJuego, ColorPieza color);
        Accion ElegirMovimiento(BaseJuego baseJuego, ColorPieza color);
    }
}
