using AjedrezLogica;
using AjedrezLogica.IA;
using AjedrezLogica.IA.Estructuras;
using AjedrezLogica.Recursos;
using System.Collections;
using System.Collections.Generic;
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
    public IMotorIA ia;

    //Activar IA
    public bool jugarContraIA = false;
    public ColorPieza colorIA = ColorPieza.Negro;
    private bool ejecutandoIA = false;

    //Guardar pieza y posicion
    //public static GameObject[,] piezasVisuales = new GameObject[8, 8];

    public Dictionary<Pieza, PiezasPrefab> mapaPiezas = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        jugarContraIA = ConfigPartida.vsIA;

        // Inicializa la lógica
        juego = new BaseJuego();

        usuario1 = new Usuario();
        usuario2 = new Usuario();

        
        if (jugarContraIA)
        {
            usuario1.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, 1);

            // Inicializar piezas de IA
            ia = new IAAleatoria();
            ia.InicializarPiezasDeIA(juego, ColorPieza.Negro);
        }
        else
        {
            // Inicializar piezas de usuarios
            usuario1.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, 1);
            usuario2.InicializarPiezasDeUsuario(juego, ColorPieza.Negro, 2);
        }

        // Crear prefabs visuales
        CrearPrefabs();
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

    public void IntentarMovimientoIA()
    {
        if (!jugarContraIA) return;

        if (juego.TurnoActual != colorIA || ejecutandoIA) return;

        StartCoroutine(EjecutarIA());
    }

    IEnumerator EjecutarIA()
    {
        ejecutandoIA = true;

        yield return new WaitForSeconds(0.5f);

        var accion = ia.ElegirMovimiento(juego, juego.TurnoActual);

        if (accion.Tipo == TipoAccion.Movimiento)
        {
            juego.RealizarMovimiento(
                accion.Pieza.Posicion.X,
                accion.Pieza.Posicion.Y,
                accion.XFin,
                accion.YFin
            );
        }
        else if (accion.Tipo == TipoAccion.Empujon)
        {
            juego.EjecutarEmpujon(
                accion.Pieza,
                accion.PiezaEmpujada,
                accion.XFin,
                accion.YFin
            );
        }

        SincronizarVisual();

        ejecutandoIA = false;

        // Por si hay más turnos IA
        IntentarMovimientoIA();
    }

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