using AjedrezLogica;
using AjedrezLogica.Recursos;
using AjedrezLogica.TiposReglasMovimiento;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeleccionarPieza : MonoBehaviour
{
    private GameObject piezaSeleccionada;

    public Material casillaBrillo;  //Color brillo de la casilla
    private Renderer renderObject;
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
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

            GameObject objeto = hit.collider.gameObject;

            // Seleccionar pieza
            if (objeto.CompareTag("Pieza"))
            {
                piezaSeleccionada = objeto;
                SeleccionarNuevaPieza(objeto);
                Debug.Log("Pieza seleccionada: " + objeto.name);
                return;
            }

            // clicar casilla, mover pieza
            else if (objeto.CompareTag("Casilla") && piezaSeleccionada != null)
            {
                //Posicion de origen y destino
                Vector2Int origen = ObtenerPosicionTablero(piezaSeleccionada.transform.position);
                Vector2Int destino = ObtenerPosicionTablero(objeto.transform.position);

                //basejuego.RealizarMovimiento(origen.x, origen.y, destino.x, destino.y);
                CrearPiezas.juego.RealizarMovimiento(origen.x, origen.y, destino.x, destino.y);

                // Sincronizar la posición visual de la pieza
                Pieza piezaLogica = piezaSeleccionada.GetComponent<PiezasPrefab>().piezaLogica;
                SincronizarPosicionVisual(piezaLogica);

                SincronizarVisual();

                LimpiarResaltado();
                piezaSeleccionada = null;

            }
    }

    void SeleccionarNuevaPieza(GameObject pieza)
    {
        LimpiarResaltado();

        piezaSeleccionada = pieza;

        Pieza piezaLogica = pieza.GetComponent<PiezasPrefab>().piezaLogica;

        List<(int x, int y)> movimientos = CrearPiezas.juego.movimientos(piezaLogica);

        foreach (var mov in movimientos)
        {
            CasillaPrefab casilla = CrearCasillas.casillas[mov.x, mov.y];
            casilla.Resaltar(casillaBrillo);
            casillasResaltadas.Add(casilla);
        }
    }

    //Limpiar casillas resaltadas
    void LimpiarResaltado()
    {
        foreach (var casilla in casillasResaltadas)
            casilla.ResetColor();

        casillasResaltadas.Clear();
    }

    //Sincronizar el tablero visual con el logico
    void SincronizarVisual()
    {
        var juego = CrearPiezas.juego;

        // Limpiar visual
        foreach (var obj in CrearPiezas.piezasVisuales)
        {
            if (obj != null)
                Destroy(obj);
        }

        CrearPiezas.piezasVisuales = new GameObject[8, 8];

        // Volver a crear TODO desde la lógica
        foreach (var pieza in juego.ListaPiezas)
        {
            GameObject prefab = CrearPiezas.Instance.ObtenerPrefab(pieza.Tipo, pieza.Color);

            GameObject piezaObj = Instantiate(prefab);

            piezaObj.transform.position = new Vector3(pieza.Posicion.X, 0.5f, pieza.Posicion.Y);

            PiezasPrefab pv = piezaObj.GetComponent<PiezasPrefab>();
            pv.Inicializar(pieza);

            CrearPiezas.piezasVisuales[pieza.Posicion.X, pieza.Posicion.Y] = piezaObj;
        }
    }

    //Obj Visual
    GameObject BuscarObjetoVisual(Pieza piezaLogica)
    {
        foreach (var obj in FindObjectsOfType<PiezasPrefab>())
        {
            if (obj.piezaLogica == piezaLogica)
                return obj.gameObject;
        }
        return null;
    }

    //Posicion de la pieza en 2D, obteniendolas del espacio 3D
    Vector2Int ObtenerPosicionTablero(Vector3 posicion)
    {
        return new Vector2Int(
            Mathf.RoundToInt(posicion.x),
            Mathf.RoundToInt(posicion.z)
        );
    }

    // Método para sincronizar la posición visual de una pieza con su posición lógica
    void SincronizarPosicionVisual(Pieza piezaLogica)
    {
        // Buscar la pieza visual correspondiente
        GameObject piezaVisual = BuscarObjetoVisual(piezaLogica);

        // Actualizar la posición de la pieza visual en Unity
        piezaVisual.transform.position = new Vector3(piezaLogica.Posicion.X, 0.5f, piezaLogica.Posicion.Y);
    }

    /*
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
    }
    */
}
