using AjedrezLogica;
using AjedrezLogica.IA;
using AjedrezLogica.IA.Estructuras;
using AjedrezLogica.Recursos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.Rendering;
using UnityEngine;
using static MenuInicio;

public class CrearPiezas : MonoBehaviour
{
    public static CrearPiezas Instance;

    public GameObject peonPrefab;
    public GameObject torrePrefab;
    public GameObject caballoPrefab;
    public GameObject alfilPrefab;
    public GameObject damaPrefab;
    public GameObject reyPrefab;

    public BaseJuego juego; // Logica global del juego
    public Usuario usuario1;
    public Usuario usuario2;
    public IMotorIA iaB;
    public IMotorIA iaN;
    public IMotorIA iaActual;

    public UnityEngine.Random random = new();

    //Activar IA
    //public bool jugarContraIA = false;
    //public ColorPieza colorIA = ColorPieza.Negro;
    //private bool ejecutandoIA = false;

    //Guardar pieza y posicion
    //public static GameObject[,] piezasVisuales = new GameObject[8, 8];

    public Dictionary<Pieza, PiezasPrefab> mapaPiezas = new();

    private SupabaseRPC GuardarPartidaBD = new();
    private ObtenerHabilidadesUsuario HabilidadesUsuarioBD = new();
    private RegistrarMovimiento registrarMovimientoDb = new();

    void Awake()
    {
        Instance = this;
    }

    private IEnumerator IniciarPartida(int idUsuario1, int idUsuario2)
    {
        // 1. Guardar partida en BD y obtener el ID
        int idPartida = 0;
        yield return StartCoroutine(GuardarPartidaBD.GuardarPartida(idUsuario1, idUsuario2,
            resultado => idPartida = resultado));

        // 2. Inicializar el juego con el ID obtenido
        juego = new BaseJuego();
        juego.JuegoBase(idUsuario1, idUsuario2, idPartida);

        yield return StartCoroutine(IniciarJuegoConHabilidades(juego, idUsuario1, idUsuario2));

        CrearPrefabs();

        StartCoroutine(BucleConEspera());
    }

    IEnumerator IniciarJuegoConHabilidades(BaseJuego juego, int idUsuario1, int idUsuario2)
    {
        if (idUsuario1 <= 4)
        {
            switch (idUsuario1)
            {
                case 1: iaB = new IAAleatoria(); iaB.InicializarPiezasDeIA(juego, ColorPieza.Blanco); break;
                case 2: iaB = new IABasica(); iaB.InicializarPiezasDeIA(juego, ColorPieza.Blanco); break;
                case 3: iaB = new IAMedia(); iaB.InicializarPiezasDeIA(juego, ColorPieza.Blanco); break;
                case 4: iaB = new IAAvanzada(); iaB.InicializarPiezasDeIA(juego, ColorPieza.Blanco); break;
                default:
                    break;
            }
        }
        else
        {
            yield return StartCoroutine(HabilidadesUsuarioBD.GetHabilidadesUsuario(idUsuario1));

            List<DatosHabilidadUsuario> habilidades = HabilidadesUsuarioBD.ObtenerListaHabilidadesUsuario();
            usuario1.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, habilidades, idUsuario1);
        }

        if (idUsuario2 <= 4)
        {
            switch (idUsuario2)
            {
                case 1: iaN = new IAAleatoria(); iaN.InicializarPiezasDeIA(juego, ColorPieza.Negro); break;
                case 2: iaN = new IABasica(); iaN.InicializarPiezasDeIA(juego, ColorPieza.Negro); break;
                case 3: iaN = new IAMedia(); iaN.InicializarPiezasDeIA(juego, ColorPieza.Negro); break;
                case 4: iaN = new IAAvanzada(); iaN.InicializarPiezasDeIA(juego, ColorPieza.Negro); break;
                default:
                    break;
            }
        }
        else
        {
            yield return StartCoroutine(HabilidadesUsuarioBD.GetHabilidadesUsuario(idUsuario2));

            List<DatosHabilidadUsuario> habilidades = HabilidadesUsuarioBD.ObtenerListaHabilidadesUsuario();
            usuario2.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, habilidades, idUsuario2);
        }

    }

    private IEnumerator BucleConEspera()
    {
        bool movimientoUsuarioRealizado = false;
        int NumeroDeTurno = 1;
        ColorPieza turno;
        Accion accionIa;

        while (true)
        {
            turno = juego.TurnoActual;

            iaActual = turno == ColorPieza.Blanco ? iaB : iaN;

            if (iaActual != null)
            {
                // ── TURNO DE IA ──
                accionIa = iaActual.ElegirMovimiento(juego, turno);

                if (accionIa.Pieza == null)
                {
                    // Jaque Mate
                    break;
                }

                if (accionIa.PiezaEliminada != null) EliminarPiezaVisual(accionIa.PiezaEliminada);

                int xOrigen = accionIa.Pieza.Posicion.X;
                int yOrigen = accionIa.Pieza.Posicion.Y;

                if (accionIa.Tipo == TipoAccion.Empujon)
                {
                    juego.EjecutarEmpujon(accionIa.Pieza, accionIa.PiezaEmpujada, accionIa.XFin, accionIa.YFin);
                }
                else
                {
                    juego.RealizarMovimiento(xOrigen, yOrigen, accionIa.XFin, accionIa.YFin);
                }


                SincronizarVisual();

                // Registrar en BD
                int idUsuario = turno == ColorPieza.Blanco ? juego.IdUsuario1 : juego.IdUsuario2;
                StartCoroutine(registrarMovimientoDb.PostRegistrarMovimiento(
                    juego.IdPartida, idUsuario, accionIa.Pieza.Id, NumeroDeTurno,
                    xOrigen, yOrigen, accionIa.XFin, accionIa.YFin,
                    (int)accionIa.Pieza.Habilidad.TipoHabilidad,
                    accionIa.PiezaEmpujada?.Id ?? null,
                    accionIa.PiezaEmpujada?.Posicion.X ?? null,
                    accionIa.PiezaEmpujada?.Posicion.Y ?? null,
                    accionIa.PiezaEmpujada?.Id != null ? accionIa.XFin : null,
                    accionIa.PiezaEmpujada?.Id != null ? accionIa.YFin : null));

                Debug.Log("Pieza movida por IA: " + accionIa.Pieza.Tipo);
                yield return new WaitForSeconds(2);
            }
            else
            {
                // ── TURNO DE USUARIO ──
                movimientoUsuarioRealizado = false;

                // Esperar hasta que el usuario haga su movimiento
                yield return new WaitUntil(() => movimientoUsuarioRealizado);

                Debug.Log("Movimiento del usuario completado");
            }
            NumeroDeTurno++;
        }
    }

    void Start()
    {
        int idB;
        int idN;

        if (ConfigPartida.vsIA)
        {
            int idIA = ConfigPartida.idIA;
            int idJugador = ConfigPartida.idJugador;

            random.Next(2) == 1 ? idB = idIA; idN = idJugador; : idN = idIA; idB = idJugador; ;
        }

        // IDs 1 al 4, Reservados para la IA
        StartCoroutine(IniciarPartida(2, 2));
        //jugarContraIA = ConfigPartida.vsIA;

        //// Inicializa la lógica
        //juego = new BaseJuego();

        //usuario1 = new Usuario();
        //usuario2 = new Usuario();


        //if (jugarContraIA)
        //{
        //    usuario1.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, 1);

        //    // Inicializar piezas de IA
        //    ia = new IAAleatoria();
        //    ia.InicializarPiezasDeIA(juego, ColorPieza.Negro);
        //}
        //else
        //{
        //    // Inicializar piezas de usuarios
        //    usuario1.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, 1);
        //    usuario2.InicializarPiezasDeUsuario(juego, ColorPieza.Negro, 2);
        //}

        //// Crear prefabs visuales
        //CrearPrefabs();
    }

    void CrearPrefabs()
    {
        foreach (var piezaLogica in juego.ListaPiezas)
        {
            GameObject prefab = ObtenerPrefab(piezaLogica.Tipo, piezaLogica.Color);
            GameObject piezaObj = Instantiate(prefab);

            PiezasPrefab view = piezaObj.GetComponent<PiezasPrefab>();
            view.Inicializar(piezaLogica);

            mapaPiezas[piezaLogica] = view;
        }
    }

    public GameObject ObtenerPrefab(TipoPieza tipo, ColorPieza color)
    {
        switch (tipo)
        {
            case TipoPieza.Peon: return peonPrefab;
            case TipoPieza.Torre: return torrePrefab;
            case TipoPieza.Caballo: return caballoPrefab;
            case TipoPieza.Alfil: return alfilPrefab;
            case TipoPieza.Dama: return damaPrefab;
            case TipoPieza.Rey: return reyPrefab;
            default: return null;
        }
    }
    public void MoverVisual(Pieza pieza)
    {
        if (mapaPiezas.TryGetValue(pieza, out var view))
        {
            view.transform.position =
                new Vector3(pieza.Posicion.X, 0.5f, pieza.Posicion.Y);
        }
    }
    void EliminarPiezaVisual(Pieza pieza)
    {
        if (mapaPiezas.TryGetValue(pieza, out var go))
        {
            Destroy(go);
            mapaPiezas.Remove(pieza);
        }
    }

    //public void IntentarMovimientoIA()
    //{
    //    if (!jugarContraIA) return;

    //    if (juego.TurnoActual != colorIA || ejecutandoIA) return;

    //    StartCoroutine(EjecutarIA());
    //}

    //IEnumerator EjecutarIA()
    //{
    //    ejecutandoIA = true;

    //    yield return new WaitForSeconds(0.5f);

    //    var accion = ia.ElegirMovimiento(juego, juego.TurnoActual);

    //    if (accion.Tipo == TipoAccion.Movimiento)
    //    {
    //        juego.RealizarMovimiento(
    //            accion.Pieza.Posicion.X,
    //            accion.Pieza.Posicion.Y,
    //            accion.XFin,
    //            accion.YFin
    //        );
    //    }
    //    else if (accion.Tipo == TipoAccion.Empujon)
    //    {
    //        juego.EjecutarEmpujon(
    //            accion.Pieza,
    //            accion.PiezaEmpujada,
    //            accion.XFin,
    //            accion.YFin
    //        );
    //    }

    //    SincronizarVisual();

    //    ejecutandoIA = false;

    //    // Por si hay más turnos IA
    //    IntentarMovimientoIA();
    //}

    public void SincronizarVisual()
    {
        foreach (var kvp in mapaPiezas)
        {
            var pieza = kvp.Key;
            var view = kvp.Value;

            view.transform.position =
                new Vector3(pieza.Posicion.X, 0.5f, pieza.Posicion.Y);
        }
    }
}