using System;

namespace AjedrezLogica
{
    public class Usuario
    {
        public void RecogerHabilidadesDePiezas(BaseJuego baseJuego, ColorPieza color)
        {
            // Recoger las habilidades de cada pieza por la base de datos, del usuario
            TipoHabilidad habilidadAleatoriaPeon = default;
            TipoHabilidad habilidadAleatoriaTorre = default;
            TipoHabilidad habilidadAleatoriaCaballo = default;
            TipoHabilidad habilidadAleatoriaAlfil = default;
            TipoHabilidad habilidadAleatoriaDama = default;
            TipoHabilidad habilidadAleatoriaRey = default;
            
            // Peones
            for (int col = 0; col < 8; col++)
            {
                if (color == ColorPieza.Blanco)
                {
                    BaseJuego.IniciarPieza(TipoPieza.Peon, ColorPieza.Blanco, habilidadAleatoriaPeon, 1, col, Tablero);
                }
                else
                {
                    BaseJuego.IniciarPieza(TipoPieza.Peon, ColorPieza.Negro, habilidadAleatoriaPeon, 6, col, Tablero);
                }

            }

            if (color == ColorPieza.Blanco)
            {
                // Torres
                BaseJuego.IniciarPieza(TipoPieza.Torre, ColorPieza.Blanco, habilidadAleatoriaTorre, 0, 0, Tablero);
                BaseJuego.IniciarPieza(TipoPieza.Torre, ColorPieza.Blanco, habilidadAleatoriaTorre, 0, 7, Tablero);
                // Caballos
                BaseJuego.IniciarPieza(TipoPieza.Caballo, ColorPieza.Blanco, habilidadAleatoriaCaballo, 0, 1, Tablero);
                BaseJuego.IniciarPieza(TipoPieza.Caballo, ColorPieza.Blanco, habilidadAleatoriaCaballo, 0, 6, Tablero);
                // Alfiles
                BaseJuego.IniciarPieza(TipoPieza.Alfil, ColorPieza.Blanco, habilidadAleatoriaAlfil, 0, 2, Tablero);
                BaseJuego.IniciarPieza(TipoPieza.Alfil, ColorPieza.Blanco, habilidadAleatoriaAlfil, 0, 5, Tablero);
                // Damas
                BaseJuego.IniciarPieza(TipoPieza.Dama, ColorPieza.Blanco, habilidadAleatoriaDama, 0, 3, Tablero);
                // Reyes
                BaseJuego.IniciarPieza(TipoPieza.Rey, ColorPieza.Blanco, habilidadAleatoriaRey, 0, 4, Tablero);
            }
            else
            {
                // Torres
                BaseJuego.IniciarPieza(TipoPieza.Torre, ColorPieza.Negro, habilidadAleatoriaTorre, 7, 0, Tablero);
                BaseJuego.IniciarPieza(TipoPieza.Torre, ColorPieza.Negro, habilidadAleatoriaTorre, 7, 7, Tablero);
                // Caballos
                BaseJuego.IniciarPieza(TipoPieza.Caballo, ColorPieza.Negro, habilidadAleatoriaCaballo, 7, 1, Tablero);
                BaseJuego.IniciarPieza(TipoPieza.Caballo, ColorPieza.Negro, habilidadAleatoriaCaballo, 7, 6, Tablero);
                // Alfiles
                BaseJuego.IniciarPieza(TipoPieza.Alfil, ColorPieza.Negro, habilidadAleatoriaAlfil, 7, 2, Tablero);
                BaseJuego.IniciarPieza(TipoPieza.Alfil, ColorPieza.Negro, habilidadAleatoriaAlfil, 7, 5, Tablero);
                // Damas
                BaseJuego.IniciarPieza(TipoPieza.Dama, ColorPieza.Negro, habilidadAleatoriaDama, 7, 3, Tablero);
                // Reyes
                BaseJuego.IniciarPieza(TipoPieza.Rey, ColorPieza.Negro, habilidadAleatoriaRey, 7, 4, Tablero);
            }
        }
    }
}
