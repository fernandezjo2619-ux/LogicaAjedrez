using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gestor de sincronizacion de estado del juego entre jugadores
/// Maneja la transmision de datos criticos entre cliente y servidor
/// </summary>
public class GameSyncManager : MonoBehaviour
{
    [System.Serializable]
    public class PlayerGameState
    {
        public int PlayerId { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        public float Health { get; set; }
        public int Score { get; set; }
        public long Timestamp { get; set; }
    }
    
    [Header("== CONFIGURACION ==")]
    [SerializeField] private float syncInterval = 0.05f;
    [SerializeField] private int maxSyncPacketsPerSecond = 20;
    [SerializeField] private bool enableCompression = true;
    
    private NetworkLobbyManager networkManager;
    private Dictionary<int, PlayerGameState> playerStates = new Dictionary<int, PlayerGameState>();
    private Dictionary<int, PlayerGameState> lastSyncedStates = new Dictionary<int, PlayerGameState>();
    
    private Coroutine syncCoroutine;
    private float timeSinceLastSync = 0f;
    private List<string> syncHistory = new List<string>();
    private const int MAX_SYNC_HISTORY = 1000;
    
    public delegate void OnPlayerStateChangedDelegate(int playerId, PlayerGameState state);
    public event OnPlayerStateChangedDelegate OnPlayerStateChanged;
    
    private static GameSyncManager instance;
    
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
            StopSynchronization();
        }
    }
    
    /// <summary>
    /// Inicializa el gestor y busca dependencias
    /// </summary>
    private void InitializeManager()
    {
        networkManager = FindObjectOfType<NetworkLobbyManager>();
        
        if (networkManager == null)
        {
            Debug.LogError("[SYNC] NetworkLobbyManager no encontrado");
            return;
        }
        
        if (networkManager.IsConnected())
        {
            InitializePlayerStates();
            StartSynchronization();
        }
        
        Debug.Log("[SYNC] === GameSyncManager inicializado ===");
    }
    
    /// <summary>
    /// Inicializa el diccionario de estados de jugadores
    /// </summary>
    private void InitializePlayerStates()
    {
        var connectedPlayers = networkManager.GetConnectedPlayers();
        
        foreach (var kvp in connectedPlayers)
        {
            int playerId = kvp.Key;
            
            playerStates[playerId] = new PlayerGameState
            {
                PlayerId = playerId,
                Position = Vector3.zero,
                Rotation = Quaternion.identity,
                Health = 100f,
                Score = 0,
                Timestamp = DateTime.Now.Ticks
            };
            
            lastSyncedStates[playerId] = new PlayerGameState
            {
                PlayerId = playerId,
                Position = Vector3.zero,
                Rotation = Quaternion.identity,
                Health = 100f,
                Score = 0,
                Timestamp = DateTime.Now.Ticks
            };
        }
        
        Debug.Log($"[SYNC] Estados de {playerStates.Count} jugador(es) inicializados");
    }
    
    /// <summary>
    /// Inicia la corrutina de sincronizacion
    /// </summary>
    public void StartSynchronization()
    {
        if (syncCoroutine != null)
        {
            StopCoroutine(syncCoroutine);
        }
        
        syncCoroutine = StartCoroutine(SynchronizationCoroutine());
        Debug.Log("[SYNC] Sincronizacion iniciada");
    }
    
    /// <summary>
    /// Detiene la sincronizacion
    /// </summary>
    public void StopSynchronization()
    {
        if (syncCoroutine != null)
        {
            StopCoroutine(syncCoroutine);
            syncCoroutine = null;
        }
        
        Debug.Log("[SYNC] Sincronizacion detenida");
    }
    
    /// <summary>
    /// Actualiza el estado de un jugador local
    /// </summary>
    public void UpdateLocalPlayerState(int playerId, Vector3 position, Quaternion rotation, float health, int score)
    {
        if (!playerStates.ContainsKey(playerId))
        {
            Debug.LogWarning($"[SYNC] Jugador {playerId} no existe en estados");
            return;
        }
        
        var state = playerStates[playerId];
        state.Position = position;
        state.Rotation = rotation;
        state.Health = health;
        state.Score = score;
        state.Timestamp = DateTime.Now.Ticks;
        
        playerStates[playerId] = state;
    }
    
    /// <summary>
    /// Obtiene el estado actual de un jugador
    /// </summary>
    public PlayerGameState GetPlayerState(int playerId)
    {
        if (playerStates.ContainsKey(playerId))
        {
            return playerStates[playerId];
        }
        
        return null;
    }
    
    /// <summary>
    /// Obtiene los estados de todos los jugadores
    /// </summary>
    public Dictionary<int, PlayerGameState> GetAllPlayerStates()
    {
        return new Dictionary<int, PlayerGameState>(playerStates);
    }
    
    /// <summary>
    /// Obtiene el historial de sincronizacion
    /// </summary>
    public List<string> GetSyncHistory()
    {
        return new List<string>(syncHistory);
    }
    
    /// <summary>
    /// Limpia el historial de sincronizacion
    /// </summary>
    public void ClearSyncHistory()
    {
        syncHistory.Clear();
        Debug.Log("[SYNC] Historial de sincronizacion limpiado");
    }
    
    // ====== METODOS PRIVADOS ======
    
    /// <summary>
    /// Corrutina principal de sincronizacion
    /// Envía datos de estado a intervalos regulares
    /// </summary>
    private IEnumerator SynchronizationCoroutine()
    {
        while (true)
        {
            timeSinceLastSync += Time.deltaTime;
            
            if (timeSinceLastSync >= syncInterval)
            {
                ProcessSynchronization();
                timeSinceLastSync = 0f;
            }
            
            yield return null;
        }
    }
    
    /// <summary>
    /// Procesa la sincronizacion de datos
    /// Detecta cambios y transmite solo lo que cambio
    /// </summary>
    private void ProcessSynchronization()
    {
        int localPlayerId = networkManager.GetLocalPlayerId();
        
        if (localPlayerId <= 0)
        {
            return;
        }
        
        if (!playerStates.ContainsKey(localPlayerId))
        {
            return;
        }
        
        var currentState = playerStates[localPlayerId];
        var lastState = lastSyncedStates[localPlayerId];
        
        if (HasStateChanged(currentState, lastState))
        {
            TransmitPlayerState(currentState);
            lastSyncedStates[localPlayerId] = CloneState(currentState);
            
            string syncLog = $"[{DateTime.Now:HH:mm:ss.fff}] Sync: Player{localPlayerId} | Pos:{currentState.Position.ToString("F2")} | Health:{currentState.Health:F1} | Score:{currentState.Score}";
            AddToSyncHistory(syncLog);
        }
    }
    
    /// <summary>
    /// Verifica si el estado de un jugador ha cambiado significativamente
    /// </summary>
    private bool HasStateChanged(PlayerGameState current, PlayerGameState last)
    {
        float positionDelta = Vector3.Distance(current.Position, last.Position);
        float rotationDelta = Quaternion.Angle(current.Rotation, last.Rotation);
        float healthDelta = Mathf.Abs(current.Health - last.Health);
        int scoreDelta = Mathf.Abs(current.Score - last.Score);
        
        const float POSITION_THRESHOLD = 0.01f;
        const float ROTATION_THRESHOLD = 1f;
        const float HEALTH_THRESHOLD = 0.5f;
        const int SCORE_THRESHOLD = 0;
        
        return positionDelta > POSITION_THRESHOLD ||
               rotationDelta > ROTATION_THRESHOLD ||
               healthDelta > HEALTH_THRESHOLD ||
               scoreDelta > SCORE_THRESHOLD;
    }
    
    /// <summary>
    /// Transmite el estado del jugador a traves de la red
    /// </summary>
    private void TransmitPlayerState(PlayerGameState state)
    {
        string serializedState = SerializePlayerState(state);
        
        Debug.Log($"[SYNC] Transmitiendo estado jugador {state.PlayerId}: {serializedState}");
    }
    
    /// <summary>
    /// Serializa el estado del jugador a string
    /// </summary>
    private string SerializePlayerState(PlayerGameState state)
    {
        return $"{state.PlayerId}|{state.Position.x:F3}|{state.Position.y:F3}|{state.Position.z:F3}|" +
               $"{state.Rotation.x:F3}|{state.Rotation.y:F3}|{state.Rotation.z:F3}|{state.Rotation.w:F3}|" +
               $"{state.Health:F1}|{state.Score}|{state.Timestamp}";
    }
    
    /// <summary>
    /// Deserializa el estado del jugador desde string
    /// </summary>
    private PlayerGameState DeserializePlayerState(string data)
    {
        try
        {
            string[] parts = data.Split('|');
            
            if (parts.Length < 11)
            {
                Debug.LogWarning("[SYNC] Datos de estado invalidos");
                return null;
            }
            
            return new PlayerGameState
            {
                PlayerId = int.Parse(parts[0]),
                Position = new Vector3(float.Parse(parts[1]), float.Parse(parts[2]), float.Parse(parts[3])),
                Rotation = new Quaternion(float.Parse(parts[4]), float.Parse(parts[5]), float.Parse(parts[6]), float.Parse(parts[7])),
                Health = float.Parse(parts[8]),
                Score = int.Parse(parts[9]),
                Timestamp = long.Parse(parts[10])
            };
        }
        catch (Exception ex)
        {
            Debug.LogError($"[SYNC] Error desserializando estado: {ex.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Crea una copia del estado del jugador
    /// </summary>
    private PlayerGameState CloneState(PlayerGameState state)
    {
        return new PlayerGameState
        {
            PlayerId = state.PlayerId,
            Position = state.Position,
            Rotation = state.Rotation,
            Health = state.Health,
            Score = state.Score,
            Timestamp = state.Timestamp
        };
    }
    
    /// <summary>
    /// Añade una entrada al historial de sincronizacion
    /// Mantiene solo las ultimas entradas para evitar uso excesivo de memoria
    /// </summary>
    private void AddToSyncHistory(string entry)
    {
        syncHistory.Add(entry);
        
        if (syncHistory.Count > MAX_SYNC_HISTORY)
        {
            syncHistory.RemoveAt(0);
        }
    }
}

/// <summary>
/// Controlador de un jugador remoto para interpolar su posicion y rotacion
/// </summary>
public class RemotePlayerController : MonoBehaviour
{
    [SerializeField] private int playerId = -1;
    [SerializeField] private float interpolationSpeed = 5f;
    [SerializeField] private float rotationLerpSpeed = 5f;
    
    private Vector3 targetPosition;
    private Quaternion targetRotation;
    private GameSyncManager syncManager;
    
    private void Start()
    {
        syncManager = FindObjectOfType<GameSyncManager>();
        
        if (syncManager == null)
        {
            Debug.LogError("[REMOTE_PLAYER] GameSyncManager no encontrado");
            return;
        }
        
        targetPosition = transform.position;
        targetRotation = transform.rotation;
        
        Debug.Log($"[REMOTE_PLAYER] Controlador remoto para jugador {playerId} inicializado");
    }
    
    private void Update()
    {
        if (syncManager == null || playerId <= 0)
            return;
        
        var playerState = syncManager.GetPlayerState(playerId);
        
        if (playerState == null)
            return;
        
        UpdateTargets(playerState);
        InterpolatePosition();
        InterpolateRotation();
    }
    
    /// <summary>
    /// Actualiza las posiciones/rotaciones objetivo basadas en el estado sincronizado
    /// </summary>
    private void UpdateTargets(GameSyncManager.PlayerGameState state)
    {
        targetPosition = state.Position;
        targetRotation = state.Rotation;
    }
    
    /// <summary>
    /// Interpola suavemente la posicion hacia el objetivo
    /// </summary>
    private void InterpolatePosition()
    {
        transform.position = Vector3.Lerp(transform.position, targetPosition, interpolationSpeed * Time.deltaTime);
    }
    
    /// <summary>
    /// Interpola suavemente la rotacion hacia el objetivo
    /// </summary>
    private void InterpolateRotation()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationLerpSpeed * Time.deltaTime);
    }
    
    public void SetPlayerId(int id)
    {
        playerId = id;
    }
    
    public int GetPlayerId()
    {
        return playerId;
    }
}
