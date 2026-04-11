using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AjedrezLogica;
using AjedrezLogica.Recursos;
using AjedrezLogica.IA;
using AjedrezLogica.IA.Estructuras;

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

    //Guardar pieza y posicion
    //public static GameObject[,] piezasVisuales = new GameObject[8, 8];

    public Dictionary<Pieza, PiezasPrefab> mapaPiezas = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Inicializa la lógica
        juego = new BaseJuego();

        usuario1 = new Usuario();
        usuario2 = new Usuario();

        // Inicializar piezas de usuarios
        usuario1.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, 1);
        usuario2.InicializarPiezasDeUsuario(juego, ColorPieza.Negro, 2);

        /*
        //Incializar IA
        ia = new IAAleatoria();
        ia.InicializarPiezasDeIA(juego, ColorPieza.Blanco);
        ia.InicializarPiezasDeIA(juego, ColorPieza.Negro);

        while (true)
        {
            Accion accionIa = ia.ElegirMovimiento(juego, juego.TurnoActual);
            juego.RealizarMovimiento(accionIa.Pieza.Posicion.X, accionIa.Pieza.Posicion.Y, accionIa.XFin, accionIa.YFin);
        }
        */

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
}