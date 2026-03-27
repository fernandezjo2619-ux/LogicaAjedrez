using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AjedrezLogica;
using AjedrezLogica.Recursos;

public class CrearPiezas : MonoBehaviour
{
    public GameObject peonPrefab;
    public GameObject torrePrefab;
    public GameObject caballoPrefab;
    public GameObject alfilPrefab;
    public GameObject damaPrefab;
    public GameObject reyPrefab;

    public static BaseJuego juego; // Logica global del juego

    //Guardar pieza y posicion
    public static GameObject[,] piezasVisuales = new GameObject[8, 8];

    void Start()
    {
        // Inicializa la lógica
        juego = new BaseJuego();
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

    GameObject ObtenerPrefab(TipoPieza tipo, ColorPieza color)
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