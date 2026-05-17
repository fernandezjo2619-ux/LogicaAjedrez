using System;
using System.Collections.Generic;
using UnityEngine;

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
            // Desuscribirse del evento de movimientos remotos
            if (networkManager != null)
            {
                networkManager.OnChessMoveReceived -= HandleRemoteChessMove;
            }
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

        // Suscribirse al evento de movimientos remotos recibidos por TCP
        networkManager.OnChessMoveReceived += HandleRemoteChessMove;

        if (idPartida <= 0 || idJugadorLocal <= 0)
        {
            Debug.LogWarning("[CHESS_SYNC] Datos de partida inválidos (puede ser normal si aún no se ha iniciado la partida)");
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

        // === ENVIAR POR TCP AL OPONENTE ===
        if (networkManager != null && networkManager.IsConnected())
        {
            string tcpMessage = "CHESS_MOVE|" + SerializeMove(move);
            networkManager.SendMessage(tcpMessage);
            Debug.Log($"[CHESS_SYNC] Movimiento enviado por TCP: ({xOrigen},{yOrigen}) -> ({xFin},{yFin})");
        }
        else
        {
            Debug.LogWarning("[CHESS_SYNC] No hay conexión de red, el movimiento solo se guardó en Supabase");
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

    // ====== SINCRONIZACIÓN TCP ======

    /// <summary>
    /// Maneja un movimiento recibido del oponente por TCP.
    /// Deserializa los datos, actualiza el historial y dispara el evento OnMoveReceived
    /// para que el tablero visual aplique el movimiento.
    /// </summary>
    private void HandleRemoteChessMove(string moveData)
    {
        ChessMoveSync move = DeserializeMove(moveData);
        if (move == null)
        {
            Debug.LogError("[CHESS_SYNC] Error al deserializar movimiento remoto");
            return;
        }

        // Ignorar movimientos propios (el host hace relay, podría recibirlo de vuelta)
        if (move.IdJugador == idJugadorLocal)
        {
            Debug.Log("[CHESS_SYNC] Movimiento propio ignorado (relay)");
            return;
        }

        // Agregar al historial
        if (movesHistory.Count >= maxMovesHistory)
        {
            movesHistory.RemoveAt(0);
        }
        movesHistory.Add(move);
        lastReceivedMove = move;

        Debug.LogWarning($"[CHESS_SYNC] Movimiento remoto recibido del Jugador {move.IdJugador}: ({move.XOrigen},{move.YOrigen}) -> ({move.XFin},{move.YFin})");

        // Disparar evento para que el tablero visual aplique el movimiento
        OnMoveReceived?.Invoke(move);
    }

    /// <summary>
    /// Serializa un ChessMoveSync a string para envío TCP.
    /// Formato: idPartida|idJugador|xOrig|yOrig|xFin|yFin|timestamp|idHab|idPiezaEmp|xOrigEmp|yOrigEmp|xFinEmp|yFinEmp
    /// Los campos opcionales (nullable) usan -1 cuando no aplican.
    /// </summary>
    private string SerializeMove(ChessMoveSync move)
    {
        return $"{move.IdPartida}|{move.IdJugador}|{move.XOrigen}|{move.YOrigen}|{move.XFin}|{move.YFin}|" +
               $"{move.Timestamp}|{move.IdHabilidadUsada ?? -1}|{move.IdPiezaEmpujada ?? -1}|" +
               $"{move.XOrigenEmpujada ?? -1}|{move.YOrigenEmpujada ?? -1}|{move.XFinEmpujada ?? -1}|{move.YFinEmpujada ?? -1}";
    }

    /// <summary>
    /// Deserializa un string TCP a un objeto ChessMoveSync.
    /// Formato esperado: idPartida|idJugador|xOrig|yOrig|xFin|yFin|timestamp|idHab|idPiezaEmp|xOrigEmp|yOrigEmp|xFinEmp|yFinEmp
    /// </summary>
    private ChessMoveSync DeserializeMove(string data)
    {
        try
        {
            string[] parts = data.Split('|');
            if (parts.Length < 13)
            {
                Debug.LogError($"[CHESS_SYNC] Datos de movimiento inválidos ({parts.Length} campos, se esperan 13): {data}");
                return null;
            }

            int idHab = int.Parse(parts[7]);
            int idPiezaEmp = int.Parse(parts[8]);
            int xOrigEmp = int.Parse(parts[9]);
            int yOrigEmp = int.Parse(parts[10]);
            int xFinEmp = int.Parse(parts[11]);
            int yFinEmp = int.Parse(parts[12]);

            return new ChessMoveSync
            {
                IdPartida = int.Parse(parts[0]),
                IdJugador = int.Parse(parts[1]),
                XOrigen = int.Parse(parts[2]),
                YOrigen = int.Parse(parts[3]),
                XFin = int.Parse(parts[4]),
                YFin = int.Parse(parts[5]),
                Timestamp = long.Parse(parts[6]),
                IdHabilidadUsada = idHab >= 0 ? idHab : (int?)null,
                IdPiezaEmpujada = idPiezaEmp >= 0 ? idPiezaEmp : (int?)null,
                XOrigenEmpujada = xOrigEmp >= 0 ? xOrigEmp : (int?)null,
                YOrigenEmpujada = yOrigEmp >= 0 ? yOrigEmp : (int?)null,
                XFinEmpujada = xFinEmp >= 0 ? xFinEmp : (int?)null,
                YFinEmpujada = yFinEmp >= 0 ? yFinEmp : (int?)null
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"[CHESS_SYNC] Error deserializando movimiento: {ex.Message}");
            return null;
        }
    }
}
