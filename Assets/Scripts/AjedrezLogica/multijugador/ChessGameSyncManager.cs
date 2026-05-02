using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AjedrezLogica;

/// <summary>
/// Gestor de sincronizacion de movimientos de ajedrez entre jugadores
/// Maneja la transmision de movimientos de piezas en tiempo real
/// </summary>
public class ChessGameSyncManager : MonoBehaviour
{
    [System.Serializable]
    public class ChessMoveSync
    {
        public int IdPartida { get; set; }
        public int IdJugador { get; set; }
        public int XOrigen { get; set; }
        public int YOrigen { get; set; }
        public int XFin { get; set; }
        public int YFin { get; set; }
        public long Timestamp { get; set; }
        public int? IdHabilidadUsada { get; set; }
        public int? IdPiezaEmpujada { get; set; }
        public int? XOrigenEmpujada { get; set; }
        public int? YOrigenEmpujada { get; set; }
        public int? XFinEmpujada { get; set; }
        public int? YFinEmpujada { get; set; }
    }
    
    [Header("== CONFIGURACION ==")]
    [SerializeField] private float syncInterval = 0.5f;
    [SerializeField] private int maxMovesHistory = 100;
    
    private NetworkLobbyManager networkManager;
    private List<ChessMoveSync> movesHistory = new List<ChessMoveSync>();
    private ChessMoveSync lastReceivedMove = null;
    
    private int idPartida = -1;
    private int idJugadorLocal = -1;
    private RegistrarMovimiento registrarMovimientoDb;
    
    public delegate void OnMoveReceivedDelegate(ChessMoveSync move);
    public event OnMoveReceivedDelegate OnMoveReceived;
    
    private static ChessGameSyncManager instance;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Start()
    {
        InitializeManager();
    }
    
    private void OnDestroy()
    {
        if (instance == this)
        {
            // Cleanup
        }
    }
    
    /// <summary>
    /// Inicializa el gestor de sincronizacion
    /// </summary>
    private void InitializeManager()
    {
        networkManager = FindObjectOfType<NetworkLobbyManager>();
        registrarMovimientoDb = FindObjectOfType<RegistrarMovimiento>();
        
        // Obtener datos desde PlayerPrefs (establecidos cuando se cambió de escena)
        NetworkLobbyManager.GetGameDataFromPrefs(out int j1, out int j2, out int partida);
        idPartida = partida;
        idJugadorLocal = NetworkLobbyManager.GetLocalPlayerIdFromPrefs();
        
        if (networkManager == null)
        {
            Debug.LogError("[CHESS_SYNC] NetworkLobbyManager no encontrado");
            return;
        }
        
        if (idPartida <= 0 || idJugadorLocal <= 0)
        {
            Debug.LogError("[CHESS_SYNC] Datos de partida inválidos");
            return;
        }
        
        Debug.Log("[CHESS_SYNC] === ChessGameSyncManager inicializado ===");
        Debug.Log($"[CHESS_SYNC] Partida: {idPartida}, Jugador Local: {idJugadorLocal}");
    }
    
    /// <summary>
    /// Registra y sincroniza un movimiento de pieza
    /// </summary>
    public void SyncChessMove(int xOrigen, int yOrigen, int xFin, int yFin, 
        int? idHabilidadUsada = null, int? idPiezaEmpujada = null,
        int? xOrigenEmpujada = null, int? yOrigenEmpujada = null,
        int? xFinEmpujada = null, int? yFinEmpujada = null)
    {
        if (idPartida <= 0 || idJugadorLocal <= 0)
        {
            Debug.LogError("[CHESS_SYNC] No se puede sincronizar sin datos de partida");
            return;
        }
        
        var move = new ChessMoveSync
        {
            IdPartida = idPartida,
            IdJugador = idJugadorLocal,
            XOrigen = xOrigen,
            YOrigen = yOrigen,
            XFin = xFin,
            YFin = yFin,
            Timestamp = DateTime.Now.Ticks,
            IdHabilidadUsada = idHabilidadUsada,
            IdPiezaEmpujada = idPiezaEmpujada,
            XOrigenEmpujada = xOrigenEmpujada,
            YOrigenEmpujada = yOrigenEmpujada,
            XFinEmpujada = xFinEmpujada,
            YFinEmpujada = yFinEmpujada
        };
        
        // Agregar al historial
        if (movesHistory.Count >= maxMovesHistory)
        {
            movesHistory.RemoveAt(0);
        }
        movesHistory.Add(move);
        
        // Registrar en la base de datos
        if (registrarMovimientoDb != null)
        {
            StartCoroutine(registrarMovimientoDb.PostRegistrarMovimiento(
                move.IdPartida,
                move.IdJugador,
                0,  // ID pieza (se obtendría del contexto de CrearPiezas)
                movesHistory.Count,  // Número de turno
                move.XOrigen,
                move.YOrigen,
                move.XFin,
                move.YFin,
                move.IdHabilidadUsada,
                move.IdPiezaEmpujada,
                move.XOrigenEmpujada,
                move.YOrigenEmpujada,
                move.XFinEmpujada,
                move.YFinEmpujada,
                (id) => Debug.Log($"[CHESS_SYNC] Movimiento registrado con ID: {id}"),
                (error) => Debug.LogError($"[CHESS_SYNC] Error registrando: {error}")
            ));
        }
        
        Debug.Log($"[CHESS_SYNC] Movimiento sincronizado: ({xOrigen},{yOrigen}) -> ({xFin},{yFin})");
    }
    
    /// <summary>
    /// Obtiene el último movimiento recibido del oponente
    /// </summary>
    public ChessMoveSync GetLastReceivedMove()
    {
        return lastReceivedMove;
    }
    
    /// <summary>
    /// Obtiene el historial de movimientos
    /// </summary>
    public List<ChessMoveSync> GetMovesHistory()
    {
        return new List<ChessMoveSync>(movesHistory);
    }
    
    /// <summary>
    /// Obtiene el ID de la partida actual
    /// </summary>
    public int GetGameId()
    {
        return idPartida;
    }
    
    /// <summary>
    /// Obtiene el ID del jugador local
    /// </summary>
    public int GetLocalPlayerId()
    {
        return idJugadorLocal;
    }
    
    /// <summary>
    /// Limpia el historial de movimientos
    /// </summary>
    public void ClearMovesHistory()
    {
        movesHistory.Clear();
        lastReceivedMove = null;
        Debug.Log("[CHESS_SYNC] Historial de movimientos limpiado");
    }
}
