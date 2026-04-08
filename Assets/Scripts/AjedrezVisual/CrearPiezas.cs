using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AjedrezLogica;
using AjedrezLogica.Recursos;
using AjedrezLogica.IA;
using AjedrezLogica.IA.Estructuras;

public class CrearPiezas : MonoBehaviour
{
    public GameObject peonPrefab;
    public GameObject torrePrefab;
    public GameObject caballoPrefab;
    public GameObject alfilPrefab;
    public GameObject damaPrefab;
    public GameObject reyPrefab;

    public static BaseJuego juego; // Logica global del juego
    public Usuario usuario1;
    public Usuario usuario2;
    public IMotorIA ia;

    //Guardar pieza y posicion
    public static GameObject[,] piezasVisuales = new GameObject[8, 8];

    public static CrearPiezas Instance;

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

            // Posición sobre la casilla
            piezaObj.transform.position = new Vector3(
                piezaLogica.Posicion.X,
                0.5f, //Altura del tablero
                piezaLogica.Posicion.Y
            );

            // Obtener posición en tablero (X, Y) de la pieza lógica
            Vector2Int pos = new Vector2Int(piezaLogica.Posicion.X, piezaLogica.Posicion.Y);

            // Guardar la pieza en la matriz visual
            piezasVisuales[pos.x, pos.y] = piezaObj;

            // Inicializar script visual si lo tienes
            PiezasPrefab pv = piezaObj.GetComponent<PiezasPrefab>();
            if (pv != null)
            {
                pv.Inicializar(piezaLogica);   // Inicializar la pieza visual con la lógica

                // Asigna color visual según el bando
                Renderer rend = piezaObj.GetComponent<Renderer>();
                if (rend != null)
                    rend.material.color = piezaLogica.Color == ColorPieza.Blanco ? Color.white : Color.black;
            }
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
}