using AjedrezLogica;
using AjedrezLogica.Recursos;
using AjedrezLogica.TiposReglasMovimiento;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeleccionarPieza : MonoBehaviour
{
    private GameObject piezaSeleccionada;

    public Material materialHighlight;
    private List<CasillaPrefab> casillasResaltadas = new List<CasillaPrefab>();

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            DetectarClick();
        }
    }

    void DetectarClick()
    {
        //La posicion del mouse crea un rayo para detectar colisiones
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject objeto = hit.collider.gameObject;

            // Si clicas una pieza
            if (objeto.CompareTag("Pieza"))
            {
                piezaSeleccionada = objeto;
                Debug.Log("Pieza seleccionada: " + objeto.name);
            }
            // Si clicas una casilla
            else if (objeto.CompareTag("Casilla") && piezaSeleccionada != null)
            {
                //Validamos el movimiento de la pieza
                if (MovimientoValido(objeto))
                {
                    MoverPieza(objeto.transform.position);
                }
                else
                {
                    Debug.Log("Movimiento no válido");
                }
            }
        }
    }

    //Posicion de la pieza en 2D, obteniendolas del espacio 3D
    Vector2Int ObtenerPosicionTablero(Vector3 posicion)
    {
        return new Vector2Int(
            Mathf.RoundToInt(posicion.x),
            Mathf.RoundToInt(posicion.z)
        );
    }

    //Validamos el movimiento de la pieza
    bool MovimientoValido(GameObject casillaDestino)
    {
        Pieza pieza = piezaSeleccionada.GetComponent<PiezasPrefab>().piezaLogica;

        LimpiarResaltado();

        //Posicion de origen y destino
        Vector2Int origen = ObtenerPosicionTablero(piezaSeleccionada.transform.position);
        Vector2Int destino = ObtenerPosicionTablero(casillaDestino.transform.position);

        //Conectamos con el tablero de logica, mediante CrearPiezas
        Tablero tableroLogico = CrearPiezas.juego.Tablero;

        List<(int X, int Y)> movimientos = null;

        //Depende de la pieza escogemos una de las logicas de movimiento
        switch (pieza.Tipo)
        {
            case TipoPieza.Peon:
                movimientos = ReglasPeon.Basicas(
                    (origen.x, origen.y),
                    pieza.Color,
                    tableroLogico
                );
                break;
            case TipoPieza.Torre:
                movimientos = ReglasTorre.Basicas(
                    (origen.x, origen.y),
                    pieza.Color,
                    tableroLogico
                );
                break;
            case TipoPieza.Caballo:
                movimientos = ReglasCaballo.Basicas(
                    (origen.x, origen.y),
                    pieza.Color,
                    tableroLogico
                );
                break;
            case TipoPieza.Alfil:
                movimientos = ReglasAlfil.Basicas(
                    (origen.x, origen.y),
                    pieza.Color,
                    tableroLogico
                );
                break;
            case TipoPieza.Dama:
                movimientos = ReglasDama.Basicas(
                    (origen.x, origen.y),
                    pieza.Color,
                    tableroLogico
                );
                break;
            case TipoPieza.Rey:
                movimientos = ReglasRey.Basicas(
                    (origen.x, origen.y),
                    pieza.Color,
                    tableroLogico
                );
                break;
        }


        // comprobar donde se puede mover
        /*foreach (var mov in movimientos)
        {
            //Resaltar las casillas en las que se puede mover
            string nombreCasilla = NombreCasilla(mov.X, mov.Y);
            GameObject casillaObj = GameObject.Find(nombreCasilla);
            if (casillaObj != null)
            {
                CasillaPrefab casilla = casillaObj.GetComponent<CasillaPrefab>();
                if (casilla != null)
                {
                casilla.Resaltar(materialHighlight);
                casillasResaltadas.Add(casilla);
                }
            }
        }*/

        // comprobar donde se puede moverse
        foreach (var mov in movimientos)
        {
            //Comprobar si se puede mover a la posición
            if (mov.X == destino.x && mov.Y == destino.y)
            {
                return true;
            }
        }

        return false;
    }

    private string NombreCasilla(int x, int y)
    {
        char letra = (char)('A' + x); // columna → letra
        int num = y + 1;              // fila → número
        return letra.ToString() + num.ToString();
    }

    void MoverPieza(Vector3 destino)
    {
        Vector2Int origen = ObtenerPosicionTablero(piezaSeleccionada.transform.position);
        Vector2Int destinoTablero = ObtenerPosicionTablero(destino);

        Tablero tablero = CrearPiezas.juego.Tablero;

        // Mover en lógica
        piezaSeleccionada.transform.position = destino + Vector3.up * 0.5f;


        //Mover pieza en visual
        piezaSeleccionada.transform.position = destino + Vector3.up * 0.5f;
        piezaSeleccionada = null;

        LimpiarResaltado();
    }

    //Quitar el resaltado de casilla
    void LimpiarResaltado()
    {
        foreach (var casilla in casillasResaltadas)
        {
            casilla.ResetColor();
        }
        casillasResaltadas.Clear();
    }

}
