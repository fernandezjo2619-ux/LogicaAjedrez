using AjedrezLogica.Recursos;
using AjedrezLogica.TiposReglasMovimiento;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace AjedrezLogica
{
    public class BaseJuego
    {
        public Tablero Tablero { get; private set; }
        public ColorPieza TurnoActual { get; private set; } = ColorPieza.Blanco;
        public List<Pieza> ListaPiezas { get; private set; } = new List<Pieza>();
        public List<Habilidad> ListaHabilidades { get; private set; } = new List<Habilidad>();
        public (Pieza pieza, int xOrigen, int yOrigen, int xFin, int yFin)? UltimoMovimiento { get; private set; }
        public (Pieza pieza, Pieza piezaEmpujada, int xFinEmpujada, int yFinEmpujada)? UltimoEmpujon { get; private set; }

        public Func<Pieza, TipoPieza> AlCoronar { get; set; } = pieza => TipoPieza.Dama;
        

        public void CambiarTurno()
        {
            TurnoActual = TurnoActual == ColorPieza.Blanco ? ColorPieza.Negro : ColorPieza.Blanco;
        }

        public BaseJuego()
        {
            inicializaciones();
        }

        public void IniciarPieza(TipoPieza tipo, ColorPieza color, int x, int y, Tablero tablero, TipoHabilidad tipoHabilidad = TipoHabilidad.vacio)
        {
            Pieza pieza = new Pieza();
            pieza.Tipo = tipo;
            pieza.Color = color;
            pieza.Habilidad = ListaHabilidades.Where(e => e.TipoHabilidad == tipoHabilidad && e.TipoPieza == tipo).FirstOrDefault();

            pieza.Posicion = (x, y);
            tablero.Grid[x, y].Ocupante = pieza;

            ListaPiezas.Add(pieza);
        }

        private void InicializarHabilidades(TipoPieza tipoPieza, TipoHabilidad tipoHabilidad)
        {
            Habilidad habilidad = new Habilidad();
            habilidad.TipoHabilidad = tipoHabilidad;
            habilidad.TipoPieza = tipoPieza;
            ListaHabilidades.Add(habilidad);
        }

        public void inicializaciones()
        {
            Tablero = new Tablero(8, 8);
            foreach (TipoPieza tipoPieza in Enum.GetValues(typeof(TipoPieza)))
            {
                InicializarHabilidades(tipoPieza, TipoHabilidad.vacio);
            }
            foreach (TipoHabilidad tipoHabilidad in Enum.GetValues(typeof(TipoHabilidad)))
            {
                int valor = (int)tipoHabilidad;
                if (valor <= 0) { }
                else if (valor <= 4)
                    InicializarHabilidades(TipoPieza.Peon, tipoHabilidad);
                else if (valor <= 6)
                    InicializarHabilidades(TipoPieza.Torre, tipoHabilidad);
                else if (valor <= 8)
                    InicializarHabilidades(TipoPieza.Alfil, tipoHabilidad);
                else if (valor <= 10)
                    InicializarHabilidades(TipoPieza.Caballo, tipoHabilidad);
                else if (valor <= 12)
                    InicializarHabilidades(TipoPieza.Dama, tipoHabilidad);
                else if (valor <= 14)
                    InicializarHabilidades(TipoPieza.Rey, tipoHabilidad);

            }

        }

        public List<(int x, int y)> movimientos (Pieza pieza) 
        {
            return ReglasMovimiento.MovimientosValidos(pieza, Tablero, this).Where(movimientosPosibles => !MovimientoDejaEnJaque(pieza, movimientosPosibles.X, movimientosPosibles.Y, pieza.Color)).ToList();
        }

        public void RealizarMovimiento(int xOrigen, int yOrigen, int xFin, int yFin)
        {
            if (!Tablero.Grid[xOrigen, yOrigen].EstaOcupado) { return; }
            Pieza pieza = Tablero.Grid[xOrigen, yOrigen].Ocupante;

            if (!pieza.Color.Equals(TurnoActual) || pieza.EstaParalizada) { return; }

            List<(int x, int y)> movimientosPosibles = movimientos(pieza);

            if (movimientosPosibles.Contains((xFin, yFin)))
            {
                // Estado antes del movimiento
                Pieza? piezaCapturada = Tablero.Grid[xFin, yFin].Ocupante;
                //(int X, int Y) posicionOriginal = pieza.Posicion;

                ColorPieza ColorEnemigo = TurnoActual == ColorPieza.Blanco ? ColorPieza.Negro : ColorPieza.Blanco;
                if (EstaEnJaque(ColorEnemigo)) 
                {
                    Console.WriteLine("Jaque a: {0}", ColorEnemigo);
                }

                if (Tablero.Grid[xFin, yFin].EstaOcupado && Tablero.Grid[xFin, yFin].Ocupante.Tipo.Equals(TipoPieza.Rey))
                {
                    Console.WriteLine("GANADOR: {0}", TurnoActual);
                    return;
                }
                Tablero.Grid[xOrigen, yOrigen].Ocupante = null;
                pieza.Posicion = (xFin, yFin);
                Tablero.Grid[xFin, yFin].Ocupante = pieza;

                //// Valorar que el jugador no mueva una pieza que descubre el jaque
                //if (EstaEnJaque(TurnoActual))
                //{
                //    pieza.Posicion = posicionOriginal;
                //    Tablero.Grid[posicionOriginal.X, posicionOriginal.Y].Ocupante = pieza;
                //    Tablero.Grid[xFin, yFin].Ocupante = piezaCapturada;
                //    if (piezaCapturada != null) { piezaCapturada.Posicion = (xFin, yFin); }
                //    return;
                //}

                if (!pieza.SeHaMovido) { pieza.SeHaMovido = true; }
                // Añadir esto a la base de datos
                UltimoMovimiento = (pieza, xOrigen, yOrigen, xFin, yFin);

                ReglasEspeciales(pieza, xOrigen, yOrigen, xFin, yFin, piezaCapturada);
                if (piezaCapturada != null) { ListaPiezas.Remove(piezaCapturada); }
                CambiarTurno();
            }
        }

        public void ReglasEspeciales(Pieza pieza, int xOrigen, int yOrigen, int xFin, int yFin, Pieza piezaEnemiga)
        {
            foreach (Pieza torre in ListaPiezas.Where(p =>
                p.Color != pieza.Color &&
                p.Tipo == TipoPieza.Torre &&
                p.Habilidad?.TipoHabilidad == TipoHabilidad.JustaDefensa))
            {
                (int dx, int dy)[] direcciones = { (1, 0), (-1, 0), (0, 1), (0, -1) };

                foreach (var (dx, dy) in direcciones)
                {
                    if (torre.Posicion.X + dx == xFin && torre.Posicion.Y + dy == yFin && !MovimientoDejaEnJaque(torre, xFin, yFin, torre.Color))
                    {
                        Tablero.Grid[torre.Posicion.X, torre.Posicion.Y].Ocupante = null;
                        torre.Posicion = (xFin, yFin);
                        Tablero.Grid[xFin, yFin].Ocupante = torre;

                        if (EstaEnJaque(pieza.Color))
                        {
                            Pieza reyPropio = ListaPiezas.FirstOrDefault(p => p.Tipo == TipoPieza.Rey && p.Color == pieza.Color);
                            int xBloqueo = (torre.Posicion.X + reyPropio.Posicion.X) / 2;
                            int yBloqueo = (torre.Posicion.Y + reyPropio.Posicion.Y) / 2;

                            pieza.Posicion = (xBloqueo, yBloqueo);
                            Tablero.Grid[xBloqueo, yBloqueo].Ocupante = pieza;
                        }

                        break;
                    }
                }
                if (Tablero.Grid[xFin, yFin].Ocupante == TipoPieza.Torre && Tablero.Grid[xFin, yFin].Ocupante.Color != pieza.Color)
                {
                    break;
                }
            }

            if (piezaEnemiga != null & piezaEnemiga.Habilidad.TipoHabilidad == TipoHabilidad.Reverso)
            {
                pieza.Tipo = piezaEnemiga.Tipo;
                // Tiene que cambiar su habilidad por la habilidad seleccionada para el peon del usuario
            }

            switch (pieza.Tipo)
            {
                case TipoPieza.Peon:
                    // Coronacion
                    if ((pieza.Color == ColorPieza.Blanco && xFin == 7) ||
                    (pieza.Color == ColorPieza.Negro && xFin == 0))
                    {
                        pieza.Tipo = AlCoronar(pieza);
                        // Tiene que cambiar su habilidad por la habilidad seleccionada para el tipo de pieza elegido del usuario
                    }

                    if (pieza.Habilidad.TipoHabilidad == TipoHabilidad.Mimico && piezaEnemiga != null && piezaEnemiga.Tipo != TipoPieza.Dama && piezaEnemiga.Tipo != TipoPieza.Rey)
                    {
                        pieza.Tipo = piezaEnemiga.Tipo;
                        // Tiene que cambiar su habilidad por la habilidad seleccionada para tipo de pieza del usuario
                    }
                    break;

                case TipoPieza.Rey:
                    // Enroque
                    if (Math.Abs(yFin - yOrigen) == 2)
                    {
                        int fila = pieza.Color == ColorPieza.Blanco ? 0 : 7;

                        if (yFin == 6 && !pieza.SeHaMovido) // enroque corto
                        {
                            Pieza torre = Tablero.Grid[fila, 7].Ocupante;
                            if (torre.SeHaMovido) { return; }
                            Tablero.Grid[fila, 7].Ocupante = null;
                            Tablero.Grid[fila, 5].Ocupante = torre;
                            torre.Posicion = (fila, 5);
                            torre.SeHaMovido = true;
                        }
                        else if (yFin == 2 && !pieza.SeHaMovido) // enroque largo
                        {
                            Pieza torre = Tablero.Grid[fila, 0].Ocupante;
                            if (torre.SeHaMovido) { return; }
                            Tablero.Grid[fila, 0].Ocupante = null;
                            Tablero.Grid[fila, 3].Ocupante = torre;
                            torre.Posicion = (fila, 3);
                            torre.SeHaMovido = true;
                        }
                    }
                    break;

                case TipoPieza.Alfil:
                    if (pieza.Habilidad.TipoHabilidad == TipoHabilidad.AnuladorDeHabilidad)
                    {
                        foreach (var piezaenemiga in ListaPiezas)
                        {
                            if (piezaEnemiga.Tipo != TipoPieza.Peon && piezaEnemiga.Color == piezaEnemiga.Color && piezaenemiga.Tipo == piezaenemiga.Tipo)
                            {
                                piezaenemiga.Habilidad = null;
                            }
                        }
                    }
                    break;

                case TipoPieza.Caballo:
                    if (pieza.Habilidad.TipoHabilidad == TipoHabilidad.CozParalizante)
                    {
                        int dx = xFin - xOrigen;
                        int dy = yFin - yOrigen;

                        if (ReglasCaballo.RutasSalto.TryGetValue((dx, dy), out var intermedias))
                        {
                            foreach (var (px, py) in intermedias)
                            {
                                if (!Tablero.EsDentroDelTablero(xOrigen + px, yOrigen + py) || !Tablero.Grid[xOrigen + px, yOrigen + py].EstaOcupado) { continue; }

                                Pieza paralizada = Tablero.Grid[xOrigen + px, yOrigen + py].Ocupante;

                                if (paralizada.Color == pieza.Color || paralizada.Tipo == TipoPieza.Rey || paralizada.Tipo == TipoPieza.Dama) { continue; }

                                paralizada.EstaParalizada = true;
                                break;
                            }
                        }
                    }
                    break; 
            }

            // Eliminar paralisis de las piezas del turno
            foreach (Pieza p in ListaPiezas.Where(p => p.Color == TurnoActual)) { p.EstaParalizada = false; }
        }

        
        // agregar forma de vista de lista para unity
        // Reglas Movimientos por habilidades especiales - Tratadas con metodos diferentes a RealizarMovimiento
        public void EjecutarEmpujon(Pieza dama, Pieza piezaEmpujada, int xDestino, int yDestino)
        {
            if (dama.Habilidad?.TipoHabilidad != TipoHabilidad.EmbestidaReal) { return; }
            if (dama.Color != TurnoActual) { return; }

            List<(Pieza piezaEmpujada, int xDestino, int yDestino)> Empujones = ReglasDama.EmpujonesDisponibles(dama.Posicion, dama.Color, Tablero);
            Empujones = Empujones.Where(empujon => empujon.piezaEmpujada == piezaEmpujada && !MovimientoDejaEnJaque(empujon.piezaEmpujada, empujon.xDestino, empujon.yDestino, dama.Color)).ToList();

            if (!Empujones.Any()
            || (xDestino == piezaEmpujada.Posicion.X && yDestino == piezaEmpujada.Posicion.Y))
            {
                RealizarMovimiento(dama.Posicion.X, dama.Posicion.Y, piezaEmpujada.Posicion.X, piezaEmpujada.Posicion.Y);
                return;
            }

            // Validar que el destino sigue siendo válido
            if (!Tablero.EsDentroDelTablero(xDestino, yDestino) && Tablero.Grid[xDestino, yDestino].EstaOcupado) { return; }

            (int x, int y) posAnterior = piezaEmpujada.Posicion;
            Tablero.Grid[posAnterior.x, posAnterior.y].Ocupante = null;
            Tablero.Grid[xDestino, yDestino].Ocupante = piezaEmpujada;
            piezaEmpujada.Posicion = (xDestino, yDestino);

            UltimoMovimiento = (dama, dama.Posicion.X, dama.Posicion.Y, dama.Posicion.X, dama.Posicion.Y);
            UltimoEmpujon = (dama, piezaEmpujada, xDestino, yDestino);
            CambiarTurno();
        }

        // Valoracion de Jaque
        public bool EstaEnJaque(ColorPieza color)
        {
            Pieza Rey = ListaPiezas.Where(pieza => pieza.Tipo == TipoPieza.Rey && pieza.Color == color).FirstOrDefault();
            ColorPieza ColorEnemigo = color == ColorPieza.Blanco ? ColorPieza.Negro : ColorPieza.Blanco;
            List<(Pieza pieza, List<(int X, int Y)>)> MovimientosDeJaque = MovimientosPosiblesBando(ColorEnemigo, false);
            return MovimientosDeJaque.Any(jaque => jaque.Item2.Any(destino => destino.X == Rey.Posicion.X && destino.Y == Rey.Posicion.Y));
        }

        public List<(Pieza pieza, List<(int X, int Y)>)> MovimientosPosiblesBando(ColorPieza color, bool validarJaque = true)
        {
            List<(Pieza pieza, List<(int X, int Y)>)> movimientosPosiblesbando = new List<(Pieza pieza, List<(int X, int Y)>)>();
            foreach (Pieza pieza in ListaPiezas.Where(p => p.Color == color))
            {
                var destinos = ReglasMovimiento.MovimientosValidos(pieza, Tablero, this);

                if (validarJaque) { destinos = destinos.Where(d => !MovimientoDejaEnJaque(pieza, d.X, d.Y, pieza.Color)).ToList(); }

                if (destinos.Count > 0) { movimientosPosiblesbando.Add((pieza, destinos)); }
            }
            return movimientosPosiblesbando;
        }

        // Pensado para descartar esos movimientos de la lista que se pasara a la IA
        public bool MovimientoDejaEnJaque(Pieza pieza, int xFin, int yFin, ColorPieza colorpieza)
        {
            int xOrigen = pieza.Posicion.X;
            int yOrigen = pieza.Posicion.Y;
            Pieza? piezaCapturada = Tablero.Grid[xFin, yFin].Ocupante;

            // Simular
            Tablero.Grid[xOrigen, yOrigen].Ocupante = null;
            Tablero.Grid[xFin, yFin].Ocupante = pieza;
            pieza.Posicion = (xFin, yFin);

            bool enJaque = EstaEnJaque(colorpieza);

            // Deshacer
            pieza.Posicion = (xOrigen, yOrigen);
            Tablero.Grid[xOrigen, yOrigen].Ocupante = pieza;
            Tablero.Grid[xFin, yFin].Ocupante = piezaCapturada;
            if (piezaCapturada != null) { piezaCapturada.Posicion = (xFin, yFin); }

            return enJaque;
        }
    }
}
