using System.Collections.Generic;
using UnityEngine;

public class SeleccionarPieza : MonoBehaviour
{
    private PiezasPrefab piezaSeleccionada;
    public static SeleccionarPieza Instance;

    //Movimientos válidos de la pieza seleccionada
    private List<(int x, int y)> movimientosActuales = new();

    public Material casillaBrillo;
    private List<CasillaPrefab> casillasResaltadas = new();

    void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        //Detectar click
        if (Input.GetMouseButtonDown(0))
        {
            DetectarClick();
        }
    }

    void DetectarClick()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Si el raycast no golpea nada, salimos
        if (!Physics.Raycast(ray, out RaycastHit hit)) return;

        //CLICK EN PIEZA
        if (hit.collider.TryGetComponent(out PiezasPrefab pieza))
        {
            Seleccionar(pieza);
            return;
        }

        //CLICK EN CASILLA
        if (hit.collider.TryGetComponent(out CasillaPrefab casilla))
        {
            Mover(casilla);
            return;
        }
    }

    //SELECCIONAR PIEZA
    void Seleccionar(PiezasPrefab pieza)
    {
        var juego = CrearPiezas.Instance.juego;

        //Validar turno
        if (pieza.piezaLogica.Color != juego.TurnoActual)
            return;

        //Guardar pieza seleccionada
        piezaSeleccionada = pieza;
        movimientosActuales.Clear();

        //Limpiar resaltados anteriores
        LimpiarResaltado();

        //Obtener movimientos válidos desde la lógica
        movimientosActuales = juego.movimientos(pieza.piezaLogica);

        //Resaltar casillas válidas
        foreach (var mov in movimientosActuales)
        {
            CasillaPrefab casilla = CrearCasillas.casillas[mov.x, mov.y];

            casilla.Resaltar(casillaBrillo);
            casillasResaltadas.Add(casilla);
        }
    }

    //MOVER PIEZA A UNA CASILLA
    public void Mover(CasillaPrefab casilla)
    {
        //No hay pieza seleccionada
        if (piezaSeleccionada == null) return;

        var juego = CrearPiezas.Instance.juego;

        //Origen
        int xOrigen = piezaSeleccionada.piezaLogica.Posicion.X;
        int yOrigen = piezaSeleccionada.piezaLogica.Posicion.Y;

        //Destino
        int xDestino = casilla.columna;
        int yDestino = casilla.fila;

        //Validar movimiento
        if (!movimientosActuales.Contains((xDestino, yDestino)))
        {
            Debug.Log("Movimiento inválido");
            return;
        }

        //Ejecutar lógica del juego
        juego.RealizarMovimiento(xOrigen, yOrigen, xDestino, yDestino);

        //CrearPiezas.Instance.MoverVisual(piezaSeleccionada.piezaLogica);
        CrearPiezas.Instance.SincronizarVisual();
        //CrearPiezas.Instance.IntentarMovimientoIA();

        //Limpiar selección y resaltados
        LimpiarResaltado();
        piezaSeleccionada = null;
        movimientosActuales.Clear();
    }
    public void SeleccionarDesdePieza(PiezasPrefab pieza)
    {
        Seleccionar(pieza);
    }

    //LIMPIAR CASILLAS RESALTADAS
    void LimpiarResaltado()
    {
        foreach (var casilla in casillasResaltadas)
        {
            casilla.ResetColor();
        }

        casillasResaltadas.Clear();
    }
}