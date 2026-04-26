using AjedrezLogica.Recursos;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace AjedrezLogica
{
    public class Usuario
    {
        public TipoHabilidad habilidadPeon;
        public TipoHabilidad habilidadTorre;
        public TipoHabilidad habilidadCaballo;
        public TipoHabilidad habilidadAlfil;
        public TipoHabilidad habilidadDama;
        public TipoHabilidad habilidadRey;

        public int IdUsuario;

        /**
         * Crear la base de juego anteriormente
         * Color = El primer id de usuario metido en el la creacion de la base de datos tendra que ser el blanco CUIDADO
         * habilidadesUsuario = pasar la lista de habilidades de ese usuario 
         * idusuario correspondiente
         */
        public void InicializarPiezasDeUsuario(BaseJuego baseJuego, ColorPieza color, List<DatosHabilidadUsuario> habilidadesUsuario, int idUsuario)
        {
            IdUsuario = idUsuario;

            TipoHabilidad ObtenerHabilidad(TipoPieza tipo)
            {
                DatosHabilidadUsuario datos = habilidadesUsuario
                    .FirstOrDefault(h =>
                        h.es_habilidad_activa &&
                        Enum.TryParse<TipoPieza>(h.nombre_tipo, true, out TipoPieza t) &&
                        t == tipo);

                if (datos != null && Enum.TryParse<TipoHabilidad>(datos.nombre_habilidad, true, out TipoHabilidad hab))
                    return hab;

                // Si no hay habilidad en BD
                return TipoHabilidad.vacio;
            }

            habilidadPeon = ObtenerHabilidad(TipoPieza.Peon);
            habilidadTorre = ObtenerHabilidad(TipoPieza.Torre);
            habilidadCaballo = ObtenerHabilidad(TipoPieza.Caballo);
            habilidadAlfil = ObtenerHabilidad(TipoPieza.Alfil);
            habilidadDama = ObtenerHabilidad(TipoPieza.Dama);
            habilidadRey = ObtenerHabilidad(TipoPieza.Rey);

            // Peones
            for (int col = 0; col < 8; col++)
            {
                if (color == ColorPieza.Blanco)
                {
                    baseJuego.IniciarPieza(TipoPieza.Peon, ColorPieza.Blanco, 1, col, baseJuego.Tablero, habilidadPeon);
                }
                else
                {
                    baseJuego.IniciarPieza(TipoPieza.Peon, ColorPieza.Negro, 6, col, baseJuego.Tablero, habilidadPeon);
                }

            }

            if (color == ColorPieza.Blanco)
            {
                // Torres
                baseJuego.IniciarPieza(TipoPieza.Torre, ColorPieza.Blanco, 0, 0, baseJuego.Tablero, habilidadTorre);
                baseJuego.IniciarPieza(TipoPieza.Torre, ColorPieza.Blanco, 0, 7, baseJuego.Tablero, habilidadTorre);
                // Caballos
                baseJuego.IniciarPieza(TipoPieza.Caballo, ColorPieza.Blanco, 0, 1, baseJuego.Tablero, habilidadCaballo);
                baseJuego.IniciarPieza(TipoPieza.Caballo, ColorPieza.Blanco, 0, 6, baseJuego.Tablero, habilidadCaballo);
                // Alfiles
                baseJuego.IniciarPieza(TipoPieza.Alfil, ColorPieza.Blanco, 0, 2, baseJuego.Tablero, habilidadAlfil);
                baseJuego.IniciarPieza(TipoPieza.Alfil, ColorPieza.Blanco, 0, 5, baseJuego.Tablero, habilidadAlfil);
                // Damas
                baseJuego.IniciarPieza(TipoPieza.Dama, ColorPieza.Blanco, 0, 3, baseJuego.Tablero, habilidadDama);
                // Reyes
                baseJuego.IniciarPieza(TipoPieza.Rey, ColorPieza.Blanco, 0, 4, baseJuego.Tablero, habilidadRey);
            }
            else
            {
                // Torres
                baseJuego.IniciarPieza(TipoPieza.Torre, ColorPieza.Negro, 7, 0, baseJuego.Tablero, habilidadTorre);
                baseJuego.IniciarPieza(TipoPieza.Torre, ColorPieza.Negro, 7, 7, baseJuego.Tablero, habilidadTorre);
                // Caballos
                baseJuego.IniciarPieza(TipoPieza.Caballo, ColorPieza.Negro, 7, 1, baseJuego.Tablero, habilidadCaballo);
                baseJuego.IniciarPieza(TipoPieza.Caballo, ColorPieza.Negro, 7, 6, baseJuego.Tablero, habilidadCaballo);
                // Alfiles
                baseJuego.IniciarPieza(TipoPieza.Alfil, ColorPieza.Negro, 7, 2, baseJuego.Tablero, habilidadAlfil);
                baseJuego.IniciarPieza(TipoPieza.Alfil, ColorPieza.Negro, 7, 5, baseJuego.Tablero, habilidadAlfil);
                // Damas
                baseJuego.IniciarPieza(TipoPieza.Dama, ColorPieza.Negro, 7, 3, baseJuego.Tablero, habilidadDama);
                // Reyes
                baseJuego.IniciarPieza(TipoPieza.Rey, ColorPieza.Negro, 7, 4, baseJuego.Tablero, habilidadRey);
            }
        }
    }
}
