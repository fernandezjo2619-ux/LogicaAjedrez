using AjedrezLogica.Recursos;
using System;
using System.Linq;

namespace AjedrezLogica.IA
{
    public class IAAleatoria : IMotorIA
    {
        private Random random = new Random();
        public (Pieza pieza, int xFin, int yFin) ElegirMovimiento(BaseJuego baseJuego, ColorPieza color)
        {
            var MovimientosAElegir = baseJuego.MovimientosPosiblesBando(color).Where(m => m.Item2.Count > 0).ToList();


            int indicePieza = random.Next(0, MovimientosAElegir.Count);
            var (pieza, destinos) = MovimientosAElegir[indicePieza];

            int indiceDestinos = random.Next(0, destinos.Count);
            var (xfin, yfin) = destinos[indiceDestinos];

            return (pieza, xfin, yfin);
        }
    }
}
