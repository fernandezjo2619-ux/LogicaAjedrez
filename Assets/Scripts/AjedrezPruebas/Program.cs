// See https://aka.ms/new-console-template for more information
using AjedrezLogica;
using AjedrezLogica.IA;
using AjedrezLogica.Recursos;

BaseJuego juego = new BaseJuego();
IMotorIA ia = new IAAleatoria();

ia.ElegirHabilidadesDePiezas(juego, ColorPieza.Blanco);
ia.ElegirHabilidadesDePiezas(juego, ColorPieza.Negro);

while (true)
{
    var (pieza, xFin, yFin) = ia.ElegirMovimiento(juego, juego.TurnoActual);
    juego.RealizarMovimiento(pieza.Posicion.X, pieza.Posicion.Y, xFin, yFin);
}