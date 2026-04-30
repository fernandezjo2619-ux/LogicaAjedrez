using AjedrezLogica.IA.Estructuras;
using AjedrezLogica.Recursos;
using AjedrezLogica.TiposReglasMovimiento;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AjedrezLogica.IA.Utilidad
{
    public class IAHelp
    {
        public Random random = new Random();
        private TipoHabilidad ObtenerHabilidadAleatoria(TipoPieza tipoPieza, List<Habilidad> habilidades)
        {
            var habilidadesDeTipo = habilidades.Where(h => h.TipoPieza == tipoPieza).ToList();
            int indice = random.Next(0, habilidadesDeTipo.Count);
            return habilidadesDeTipo[indice].TipoHabilidad;
        }
        public void InicializarPiezasDeIA(BaseJuego baseJuego, ColorPieza color)
        {
            TipoHabilidad habilidadAleatoriaPeon = ObtenerHabilidadAleatoria(TipoPieza.Peon, baseJuego.ListaHabilidades);
            TipoHabilidad habilidadAleatoriaTorre = ObtenerHabilidadAleatoria(TipoPieza.Torre, baseJuego.ListaHabilidades);
            TipoHabilidad habilidadAleatoriaCaballo = ObtenerHabilidadAleatoria(TipoPieza.Caballo, baseJuego.ListaHabilidades);
            TipoHabilidad habilidadAleatoriaAlfil = ObtenerHabilidadAleatoria(TipoPieza.Alfil, baseJuego.ListaHabilidades);
            TipoHabilidad habilidadAleatoriaDama = ObtenerHabilidadAleatoria(TipoPieza.Dama, baseJuego.ListaHabilidades);
            TipoHabilidad habilidadAleatoriaRey = ObtenerHabilidadAleatoria(TipoPieza.Rey, baseJuego.ListaHabilidades);
            // Peones
            for (int col = 0; col < 8; col++)
            {
                if (color == ColorPieza.Blanco)
                {
                    baseJuego.IniciarPieza(TipoPieza.Peon, ColorPieza.Blanco, 1, col, baseJuego.Tablero, habilidadAleatoriaPeon);
                }
                else
                {
                    baseJuego.IniciarPieza(TipoPieza.Peon, ColorPieza.Negro, 6, col, baseJuego.Tablero, habilidadAleatoriaPeon);
                }

            }

            if (color == ColorPieza.Blanco)
            {
                // Torres
                baseJuego.IniciarPieza(TipoPieza.Torre, ColorPieza.Blanco, 0, 0, baseJuego.Tablero, habilidadAleatoriaTorre);
                baseJuego.IniciarPieza(TipoPieza.Torre, ColorPieza.Blanco, 0, 7, baseJuego.Tablero, habilidadAleatoriaTorre);
                // Caballos
                baseJuego.IniciarPieza(TipoPieza.Caballo, ColorPieza.Blanco, 0, 1, baseJuego.Tablero, habilidadAleatoriaCaballo);
                baseJuego.IniciarPieza(TipoPieza.Caballo, ColorPieza.Blanco, 0, 6, baseJuego.Tablero, habilidadAleatoriaCaballo);
                // Alfiles
                baseJuego.IniciarPieza(TipoPieza.Alfil, ColorPieza.Blanco, 0, 2, baseJuego.Tablero, habilidadAleatoriaAlfil);
                baseJuego.IniciarPieza(TipoPieza.Alfil, ColorPieza.Blanco, 0, 5, baseJuego.Tablero, habilidadAleatoriaAlfil);
                // Damas
                baseJuego.IniciarPieza(TipoPieza.Dama, ColorPieza.Blanco, 0, 3, baseJuego.Tablero, habilidadAleatoriaDama);
                // Reyes
                baseJuego.IniciarPieza(TipoPieza.Rey, ColorPieza.Blanco, 0, 4, baseJuego.Tablero, habilidadAleatoriaRey);
            }
            else
            {
                // Torres
                baseJuego.IniciarPieza(TipoPieza.Torre, ColorPieza.Negro, 7, 0, baseJuego.Tablero, habilidadAleatoriaTorre);
                baseJuego.IniciarPieza(TipoPieza.Torre, ColorPieza.Negro, 7, 7, baseJuego.Tablero, habilidadAleatoriaTorre);
                // Caballos
                baseJuego.IniciarPieza(TipoPieza.Caballo, ColorPieza.Negro, 7, 1, baseJuego.Tablero, habilidadAleatoriaCaballo);
                baseJuego.IniciarPieza(TipoPieza.Caballo, ColorPieza.Negro, 7, 6, baseJuego.Tablero, habilidadAleatoriaCaballo);
                // Alfiles
                baseJuego.IniciarPieza(TipoPieza.Alfil, ColorPieza.Negro, 7, 2, baseJuego.Tablero, habilidadAleatoriaAlfil);
                baseJuego.IniciarPieza(TipoPieza.Alfil, ColorPieza.Negro, 7, 5, baseJuego.Tablero, habilidadAleatoriaAlfil);
                // Damas
                baseJuego.IniciarPieza(TipoPieza.Dama, ColorPieza.Negro, 7, 3, baseJuego.Tablero, habilidadAleatoriaDama);
                // Reyes
                baseJuego.IniciarPieza(TipoPieza.Rey, ColorPieza.Negro, 7, 4, baseJuego.Tablero, habilidadAleatoriaRey);
            }
        }
        public List<Accion> ObtenerTodasLasOpciones(BaseJuego juego, ColorPieza color)
        {
            List<Accion> opciones = new List<Accion>();

            foreach (var (pieza, destinos) in juego.MovimientosPosiblesBando(color))
            {
                foreach (var (x, y) in destinos)
                {
                    opciones.Add(new Accion
                    {
                        Tipo = TipoAccion.Movimiento,
                        Pieza = pieza,
                        XFin = x,
                        YFin = y,
                        PiezaEliminada = juego.Tablero.Grid[x, y].EstaOcupado ? juego.Tablero.Grid[x, y].Ocupante.Color == color ? null : juego.Tablero.Grid[x, y].Ocupante : null
                    });
                }
            }

            // Empujones
            foreach (Pieza pieza in juego.ListaPiezas.Where(p =>
                p.Color == color &&
                p.Habilidad?.TipoHabilidad == TipoHabilidad.EmbestidaReal))
            {
                foreach (var (empujada, xd, yd) in ReglasDama
                    .EmpujonesDisponibles(pieza.Posicion, color, juego.Tablero))
                {
                    opciones.Add(new Accion
                    {
                        Tipo = TipoAccion.Empujon,
                        Pieza = pieza,
                        PiezaEmpujada = empujada,
                        XFin = xd,
                        YFin = yd
                    });
                }
            }

            return opciones;
        }
    }
}