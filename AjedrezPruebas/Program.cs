// See https://aka.ms/new-console-template for more information
using AjedrezLogica;
using AjedrezLogica.IA;
using AjedrezLogica.IA.Estructuras;
using AjedrezLogica.Recursos;

BaseJuego juego = new BaseJuego();
IMotorIA ia = new IAAleatoria();

ia.InicializarPiezasDeIA(juego, ColorPieza.Blanco);
ia.InicializarPiezasDeIA(juego, ColorPieza.Negro);

while (true)
{
    Accion accion = ia.ElegirMovimiento(juego, juego.TurnoActual);
    juego.RealizarMovimiento(accion.pieza.Posicion.X, accion.pieza.Posicion.Y, accion.xFin, accion.yFin);
}