using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using AjedrezLogica;
using UnityEngine.SceneManagement;

/// <summary>
/// Controlador de la interfaz de usuario del lobby multijugador para ajedrez
/// Maneja la conexion entre dos jugadores y la creacion de partidas en Supabase
/// </summary>
public class LobbyUIController : MonoBehaviour
{
    [Header("== REFERENCIAS DE UI ==")]
    [SerializeField] private Button hostButton;
    [SerializeField] private Button joinButton;
    [SerializeField] private Button startGameButton;
    [SerializeField] private Button backButton;
    
    [SerializeField] private TMP_InputField roomNameInput;
    [SerializeField] private TMP_InputField ipAddressInput;
    [SerializeField] private TMP_InputField portInput;
    [SerializeField] private TMP_InputField idJugador1Input;
    [SerializeField] private TMP_InputField idJugador2Input;
    
    [SerializeField] private TextMeshProUGUI statusLabel;
    [SerializeField] private TextMeshProUGUI connectedPlayersLabel;
    [SerializeField] private Transform roomListContainer;
    [SerializeField] private GameObject roomListButtonPrefab;
    
    [Header("== CONFIGURACION ==")]
    [SerializeField] private string gameSceneName = "PartidaTablero";
    [SerializeField] private Color statusConnectedColor = Color.green;
    [SerializeField] private Color statusDisconnectedColor = Color.red;
    [SerializeField] private Color statusWaitingColor = Color.yellow;
    
    private NetworkLobbyManager networkManager;
    private SupabaseRPC supabaseManager;
    private Dictionary<string, GameObject> discoveredRoomButtons = new Dictionary<string, GameObject>();
    
    // Cache de salas descubiertas por nombre (nombre normalizado en minúsculas → datos de la sala)
    private Dictionary<string, RoomDiscoveryData> _discoveredRoomsByName = new Dictionary<string, RoomDiscoveryData>();
    
    private const int BASE_PORT = 8000;
    
    // IDs de jugadores para la partida de ajedrez
    private int idJugador1 = -1;
    private int idJugador2 = -1;
    private int idPartidaCreada = -1;
    
    private void Start()
    {
        // IMPORTANTE: InitializeNetworkManager va PRIMERO para que networkManager
        // siempre esté disponible aunque haya referencias de UI faltantes.
        InitializeNetworkManager();
        
        bool uiValid = ValidateUIReferences();
        
        if (uiValid)
        {
            SetupUIListeners();
            InitializeUIState();
        }
        else
        {
            Debug.LogError("[LOBBY_UI] Inicialización incompleta: faltan referencias de UI en el Inspector.");
        }
        
        // Iniciar descubrimiento de salas automáticamente para ver salas disponibles en la red
        if (networkManager != null)
            networkManager.StartRoomDiscovery();
        
        Debug.Log("[LOBBY_UI] === UI del Lobby inicializada ===");
    }
    
    private float _refreshTimer = 0f;
    private int   _lastPlayerCount = -1;
    
    /// <summary>
    /// Refresca el estado de la UI cada segundo como mecanismo de seguridad,
    /// independiente de que los eventos de red se hayan recibido correctamente.
    /// </summary>
    private void Update()
    {
        if (networkManager == null) return;
        
        _refreshTimer += Time.deltaTime;
        if (_refreshTimer < 1f) return;
        _refreshTimer = 0f;
        
        int count = networkManager.GetConnectedPlayerCount();
        
        // Solo actualizar la UI si el número de jugadores cambió
        if (count == _lastPlayerCount) return;
        _lastPlayerCount = count;
        
        Debug.Log($"[LOBBY_UI] [UPDATE] Jugadores detectados: {count}");
        UpdatePlayersLabel();
        
        if (count >= 2 && networkManager.IsServer())
        {
            if (startGameButton != null)
                startGameButton.gameObject.SetActive(true);
            UpdateStatusLabel("2 Jugadores conectados - Listo para jugar", statusConnectedColor);
        }
        else if (count < 2 && networkManager.IsServer())
        {
            UpdateStatusLabel($"Esperando jugador 2... ({count}/2)", statusWaitingColor);
        }
    }
    
    private void OnDestroy()
    {
        if (networkManager != null)
        {
            networkManager.OnPlayerConnected -= HandlePlayerConnected;
            networkManager.OnPlayerDisconnected -= HandlePlayerDisconnected;
            networkManager.OnRoomDiscovered -= HandleRoomDiscovered;
            networkManager.OnConnectionFailed -= HandleConnectionFailed;
            networkManager.OnConnectionEstablished -= HandleConnectionEstablished;
        }
    }
    
    /// <summary>
    /// Valida que todas las referencias de UI esten asignadas
    /// </summary>
    /// <returns>true si todas las referencias están asignadas, false si falta alguna</returns>
    private bool ValidateUIReferences()
    {
        List<string> missingReferences = new List<string>();
        
        if (hostButton == null)       missingReferences.Add("hostButton");
        if (joinButton == null)       missingReferences.Add("joinButton");
        if (startGameButton == null)  missingReferences.Add("startGameButton");
        if (backButton == null)       missingReferences.Add("backButton");
        if (roomNameInput == null)    missingReferences.Add("roomNameInput");
        if (statusLabel == null)      missingReferences.Add("statusLabel");
        // ipAddressInput y portInput son opcionales — se auto-rellenan en InitializeUIState
        
        if (missingReferences.Count > 0)
        {
            string errorMsg = "[LOBBY_UI] === REFERENCIAS FALTANTES en el Inspector ===\n";
            foreach (string reference in missingReferences)
                errorMsg += $"  >> {reference}\n";
            errorMsg += "Arrastra los objetos de la Hierarchy al Inspector del LobbyUIController.";
            Debug.LogError(errorMsg);
            return false;
        }
        
        Debug.Log("[LOBBY_UI] --- Todas las referencias de UI validadas correctamente ---");
        return true;
    }
    
    /// <summary>
    /// Obtiene e inicializa la referencia del NetworkLobbyManager y SupabaseRPC
    /// </summary>
    private void InitializeNetworkManager()
    {
        // FindObjectOfType no encuentra objetos en DontDestroyOnLoad en algunas versiones de Unity.
        // Usamos el parámetro includeInactive=true y además accedemos al singleton como fallback.
        networkManager = FindObjectOfType<NetworkLobbyManager>(true);
        
        if (networkManager == null)
        {
            // Fallback: acceder al singleton a través de un método estático
            networkManager = NetworkLobbyManager.Instance;
        }
        
        supabaseManager = FindObjectOfType<SupabaseRPC>(true);
        
        if (networkManager == null)
        {
            string errMsg = "[LOBBY_UI] === CRITICO: NetworkLobbyManager no encontrado en ningún contexto. " +
                            "Asegúrate de que existe en la escena o persiste desde una escena anterior ===";
            Debug.LogError(errMsg);
            UpdateStatusLabel("ERROR: NetworkLobbyManager no encontrado", statusDisconnectedColor);
            return;
        }
        
        Debug.Log($"[LOBBY_UI] NetworkLobbyManager encontrado: {networkManager.gameObject.name}");
        
        if (supabaseManager == null)
        {
            Debug.LogWarning("[LOBBY_UI] === SupabaseRPC no encontrado, algunas funciones pueden no estar disponibles ===");
        }
        
        networkManager.OnPlayerConnected += HandlePlayerConnected;
        networkManager.OnPlayerDisconnected += HandlePlayerDisconnected;
        networkManager.OnRoomDiscovered += HandleRoomDiscovered;
        networkManager.OnConnectionFailed += HandleConnectionFailed;
        networkManager.OnConnectionEstablished += HandleConnectionEstablished;
        
        Debug.Log("[LOBBY_UI] --- NetworkLobbyManager inicializado ---");
    }
    
    /// <summary>
    /// Asigna listeners a los botones de la UI
    /// </summary>
    private void SetupUIListeners()
    {
        hostButton.onClick.AddListener(OnHostButtonPressed);
        joinButton.onClick.AddListener(OnJoinButtonPressed);
        startGameButton.onClick.AddListener(OnStartGameButtonPressed);
        backButton.onClick.AddListener(OnBackButtonPressed);
        
        if (portInput != null)
        {
            portInput.text = BASE_PORT.ToString();
        }
        
        Debug.Log("[LOBBY_UI] --- Listeners de UI configurados ---");
    }
    
    /// <summary>
    /// Inicializa el estado visual de la UI
    /// </summary>
    private void InitializeUIState()
    {
        startGameButton.gameObject.SetActive(false);
        UpdateStatusLabel("Desconectado", statusDisconnectedColor);
        UpdatePlayersLabel();
        
        // El nombre de sala lo escribe el usuario — no se auto-rellena
        if (roomNameInput != null)
            roomNameInput.text = string.Empty;
        
        // Auto-rellenar IP local y puerto — el usuario no necesita tocarlos
        string localIp = GetLocalIpAddress();
        if (ipAddressInput != null)
        {
            ipAddressInput.text = localIp;
            Debug.Log($"[LOBBY_UI] IP local detectada: {localIp}");
        }
        
        if (portInput != null)
        {
            portInput.text = BASE_PORT.ToString();
            portInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        }
    }
    
    /// <summary>
    /// Obtiene la IP local de red LAN (192.168.x.x / 10.x.x.x).
    /// Si no se encuentra ninguna, devuelve 127.0.0.1.
    /// </summary>
    private string GetLocalIpAddress()
    {
        try
        {
            var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[LOBBY_UI] No se pudo obtener IP local: {ex.Message}");
        }
        return "127.0.0.1";
    }
    
    // ====== MANEJADORES DE EVENTOS DE BOTONES ======
    
    /// <summary>
    /// Maneja el evento de presionar el boton Host
    /// Inicia un servidor local
    /// </summary>
    public void OnHostButtonPressed()
    {
        Debug.Log("[LOBBY_UI] Boton Host presionado");
        
        if (networkManager == null)
        {
            Debug.LogError("[LOBBY_UI] networkManager es null. Verifica que NetworkLobbyManager existe en la escena.");
            UpdateStatusLabel("ERROR: NetworkLobbyManager no encontrado en la escena", statusDisconnectedColor);
            return;
        }
        
        string roomName = roomNameInput.text.Trim();
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = "Sala de " + SystemInfo.deviceName;
        }
        
        UpdateStatusLabel("Iniciando servidor...", statusWaitingColor);
        hostButton.interactable = false;
        joinButton.interactable = false;
        
        if (networkManager.StartHost(roomName))
        {
            Debug.Log("[LOBBY_UI] Servidor iniciado exitosamente");
            UpdateUIForHost();
            networkManager.StartRoomDiscovery();
        }
        else
        {
            Debug.LogError("[LOBBY_UI] Error iniciando servidor");
            UpdateStatusLabel("Error al iniciar servidor", statusDisconnectedColor);
            hostButton.interactable = true;
            joinButton.interactable = true;
        }
    }
    
    /// <summary>
    /// Maneja el evento de presionar el boton Join.
    /// Usa una Coroutine asíncrona para no bloquear el hilo principal de Unity.
    /// </summary>
    public void OnJoinButtonPressed()
    {
        Debug.Log("[LOBBY_UI] Boton Join presionado");
        
        if (networkManager == null)
        {
            Debug.LogError("[LOBBY_UI] networkManager es null.");
            UpdateStatusLabel("ERROR: NetworkLobbyManager no encontrado", statusDisconnectedColor);
            return;
        }
        
        string ip   = string.Empty;
        int    port = BASE_PORT;
        
        // --- PRIORIDAD 1: buscar por nombre de sala en el discovery UDP ---
        string searchName = roomNameInput != null ? roomNameInput.text.Trim() : string.Empty;
        if (!string.IsNullOrEmpty(searchName))
        {
            string key = searchName.ToLowerInvariant();
            if (_discoveredRoomsByName.TryGetValue(key, out RoomDiscoveryData found))
            {
                ip   = found.IpAddress;
                port = found.Port;
                Debug.LogWarning($"[LOBBY_UI] Sala '{searchName}' encontrada por discovery — IP: {ip}:{port}");
            }
            else
            {
                // No encontrada todavía: esperar unos segundos y reintentar
                UpdateStatusLabel($"Buscando sala '{searchName}'...", statusWaitingColor);
                hostButton.interactable  = false;
                joinButton.interactable  = false;
                StartCoroutine(SearchRoomByNameCoroutine(searchName));
                return;
            }
        }
        
        // --- PRIORIDAD 2: usar IP del campo ipAddressInput ---
        if (string.IsNullOrEmpty(ip))
        {
            string ipText = ipAddressInput != null ? ipAddressInput.text.Trim() : string.Empty;
            if (string.IsNullOrEmpty(ipText))
            {
                UpdateStatusLabel("Escribe el nombre de sala o una IP válida", statusDisconnectedColor);
                return;
            }
            if (ipText.Contains(":"))
            {
                string[] parts = ipText.Split(':');
                ip = parts[0].Trim();
                int.TryParse(parts[1].Trim(), out port);
            }
            else
            {
                ip = ipText;
            }
            if (portInput != null && int.TryParse(portInput.text, out int inputPort))
                port = inputPort;
        }
        
        UpdateStatusLabel($"Conectando a {ip}:{port}...", statusWaitingColor);
        hostButton.interactable = false;
        joinButton.interactable = false;
        StartCoroutine(JoinServerCoroutine(ip, port));
    }
    
    /// <summary>
    /// Espera hasta 5 segundos a que el discovery encuentre la sala por nombre.
    /// Si la encuentra, conecta; si no, muestra error.
    /// </summary>
    private IEnumerator SearchRoomByNameCoroutine(string roomName)
    {
        float elapsed = 0f;
        float timeout = 5f;
        string key    = roomName.ToLowerInvariant();
        
        while (elapsed < timeout)
        {
            yield return new WaitForSeconds(0.5f);
            elapsed += 0.5f;
            
            if (_discoveredRoomsByName.TryGetValue(key, out RoomDiscoveryData found))
            {
                Debug.LogWarning($"[LOBBY_UI] Sala '{roomName}' encontrada tras {elapsed:F1}s");
                UpdateStatusLabel($"Conectando a {found.IpAddress}:{found.Port}...", statusWaitingColor);
                yield return StartCoroutine(JoinServerCoroutine(found.IpAddress, found.Port));
                yield break;
            }
            
            UpdateStatusLabel($"Buscando '{roomName}'... ({elapsed:F0}/{timeout:F0}s)", statusWaitingColor);
        }
        
        // Timeout: sala no encontrada
        Debug.LogWarning($"[LOBBY_UI] Sala '{roomName}' no encontrada tras {timeout}s");
        UpdateStatusLabel($"Sala '{roomName}' no encontrada en la red", statusDisconnectedColor);
        hostButton.interactable = true;
        joinButton.interactable = true;
    }
    
    /// <summary>
    /// Coroutine que gestiona la conexión asíncrona al servidor.
    /// Espera el resultado de ConnectToServerAsync y actualiza la UI.
    /// </summary>
    private IEnumerator JoinServerCoroutine(string ip, int port)
    {
        // Lanzar la conexión asíncrona y esperar su resultado
        yield return StartCoroutine(networkManager.ConnectToServerAsync(ip, port));
        
        // Dar un frame para que los eventos se procesen
        yield return null;
        
        if (networkManager.IsConnected())
        {
            Debug.Log("[LOBBY_UI] Conectado al servidor exitosamente");
            UpdateUIForClient();
            networkManager.StopRoomDiscovery();
        }
        else
        {
            Debug.LogError("[LOBBY_UI] No se pudo conectar al servidor");
            // HandleConnectionFailed ya actualizó el statusLabel con el mensaje de error
            hostButton.interactable = true;
            joinButton.interactable = true;
        }
    }
    
    /// <summary>
    /// Maneja el evento de presionar el boton Iniciar Juego
    /// Crea la partida en Supabase y cambia a la escena de juego
    /// </summary>
    public void OnStartGameButtonPressed()
    {
        Debug.Log("[LOBBY_UI] Boton Iniciar Juego presionado");
        
        if (networkManager == null)
        {
            Debug.LogError("[LOBBY_UI] networkManager es null. Verifica que NetworkLobbyManager existe en la escena.");
            UpdateStatusLabel("ERROR: NetworkLobbyManager no encontrado en la escena", statusDisconnectedColor);
            return;
        }
        
        if (!networkManager.IsServer())
        {
            UpdateStatusLabel("Solo el Host puede iniciar el juego", statusDisconnectedColor);
            return;
        }
        
        // Validar IDs de jugadores — los campos pueden no estar asignados en el Inspector
        if (idJugador1Input == null)
        {
            Debug.LogWarning("[LOBBY_UI] idJugador1Input no asignado en Inspector. Usando ID por defecto: 1");
            idJugador1 = 1;
        }
        else if (!int.TryParse(idJugador1Input.text, out idJugador1) || idJugador1 <= 0)
        {
            UpdateStatusLabel("ID Jugador 1 inválido (debe ser un número > 0)", statusDisconnectedColor);
            return;
        }
        
        if (idJugador2Input == null)
        {
            Debug.LogWarning("[LOBBY_UI] idJugador2Input no asignado en Inspector. Usando ID por defecto: 2");
            idJugador2 = 2;
        }
        else if (!int.TryParse(idJugador2Input.text, out idJugador2) || idJugador2 <= 0)
        {
            UpdateStatusLabel("ID Jugador 2 inválido (debe ser un número > 0)", statusDisconnectedColor);
            return;
        }
        
        int playerCount = networkManager.GetConnectedPlayerCount();
        
        if (playerCount < 2)
        {
            UpdateStatusLabel($"Esperando jugadores ({playerCount}/2)", statusWaitingColor);
            return;
        }
        
        Debug.Log("[LOBBY_UI] === Creando partida en Supabase ===");
        UpdateStatusLabel("Creando partida...", statusWaitingColor);
        startGameButton.interactable = false;
        
        // Crear partida en Supabase y luego cambiar de escena
        StartCoroutine(CreateGameAndStartScene());
    }
    
    /// <summary>
    /// Crea la partida en Supabase y cambia de escena
    /// </summary>
    private IEnumerator CreateGameAndStartScene()
    {
        if (supabaseManager == null)
        {
            UpdateStatusLabel("Error: SupabaseRPC no disponible", statusDisconnectedColor);
            startGameButton.interactable = true;
            yield break;
        }
        
        // Crear la partida en Supabase
        yield return StartCoroutine(supabaseManager.GuardarPartida(idJugador1, idJugador2, 
            (idPartida) =>
            {
                if (idPartida > 0)
                {
                    idPartidaCreada = idPartida;
                    Debug.Log($"[LOBBY_UI] Partida creada con ID: {idPartida}");
                    networkManager.SetGameData(idJugador1, idJugador2, idPartida);
                    networkManager.GoToGameScene(gameSceneName);
                }
                else
                {
                    // La BD falló pero el multijugador está listo — continuar con ID temporal
                    Debug.LogWarning("[LOBBY_UI] No se pudo guardar en Supabase. Iniciando partida sin ID de BD.");
                    idPartidaCreada = -1;
                    networkManager.SetGameData(idJugador1, idJugador2, -1);
                    networkManager.GoToGameScene(gameSceneName);
                }
            }));
    }
    
    /// <summary>
    /// Maneja el evento de presionar el boton Volver
    /// Desconecta y regresa al menu principal
    /// </summary>
    public void OnBackButtonPressed()
    {
        Debug.Log("[LOBBY_UI] Boton Volver presionado");
        
        if (networkManager != null)
        {
            networkManager.Disconnect();
            networkManager.StopRoomDiscovery();
        }
        
        SceneManager.LoadScene("MainMenu");
    }
    
    // ====== MANEJADORES DE EVENTOS DE RED ======
    
    /// <summary>
    /// Maneja la conexion exitosa de un jugador
    /// </summary>
    private void HandlePlayerConnected(int playerId, string playerName)
    {
        Debug.Log($"[LOBBY_UI] Jugador conectado: ID={playerId}, Nombre={playerName}");
        UpdatePlayersLabel();
        
        if (networkManager.GetConnectedPlayerCount() >= 2)
        {
            if (networkManager.IsServer())
            {
                startGameButton.gameObject.SetActive(true);
                UpdateStatusLabel("2 Jugadores conectados - Listo para jugar", statusConnectedColor);
            }
        }
    }
    
    /// <summary>
    /// Maneja la desconexion de un jugador
    /// </summary>
    private void HandlePlayerDisconnected(int playerId)
    {
        Debug.Log($"[LOBBY_UI] Jugador desconectado: ID={playerId}");
        UpdatePlayersLabel();
        startGameButton.gameObject.SetActive(false);
        UpdateStatusLabel("Aguardando conexion de jugadores...", statusWaitingColor);
    }
    
    /// <summary>
    /// Maneja el descubrimiento de una sala disponible
    /// </summary>
    private void HandleRoomDiscovered(RoomDiscoveryData roomData)
    {
        Debug.Log($"[LOBBY_UI] Sala descubierta: {roomData.RoomName} ({roomData.IpAddress}:{roomData.Port})");
        
        // Guardar en caché por nombre (normalizado) para búsqueda por nombre
        string key = roomData.RoomName.Trim().ToLowerInvariant();
        _discoveredRoomsByName[key] = roomData;
        
        AddRoomToList(roomData);
    }
    
    /// <summary>
    /// Maneja fallos de conexion
    /// </summary>
    private void HandleConnectionFailed(string errorMessage)
    {
        Debug.LogError($"[LOBBY_UI] Error de conexion: {errorMessage}");
        UpdateStatusLabel($"Error: {errorMessage}", statusDisconnectedColor);
        
        hostButton.interactable = true;
        joinButton.interactable = true;
    }
    
    /// <summary>
    /// Maneja el establecimiento exitoso de conexion
    /// </summary>
    private void HandleConnectionEstablished()
    {
        Debug.Log("[LOBBY_UI] Conexion establecida");
        string localIp = NetworkLobbyManager.GetLocalIpAddress();
        UpdateStatusLabel($"Conectado: {localIp}:{networkManager.GetCurrentPort()}", statusConnectedColor);
    }
    
    // ====== METODOS DE ACTUALIZACION DE UI ======
    
    /// <summary>
    /// Actualiza el estado visual cuando se inicia como Host
    /// </summary>
    private void UpdateUIForHost()
    {
        hostButton.interactable = false;
        joinButton.interactable = false;
        roomNameInput.interactable = false;
        ipAddressInput.interactable = false;
        portInput.interactable = false;
        
        startGameButton.gameObject.SetActive(true);
        
        Debug.Log("[LOBBY_UI] UI actualizada para modo Host");
    }
    
    /// <summary>
    /// Actualiza el estado visual cuando se conecta como Cliente
    /// </summary>
    private void UpdateUIForClient()
    {
        hostButton.interactable = false;
        joinButton.interactable = false;
        roomNameInput.interactable = false;
        ipAddressInput.interactable = false;
        portInput.interactable = false;
        
        startGameButton.gameObject.SetActive(false);
        
        Debug.Log("[LOBBY_UI] UI actualizada para modo Cliente");
    }
    
    /// <summary>
    /// Actualiza la etiqueta de estado con color
    /// </summary>
    private void UpdateStatusLabel(string message, Color color)
    {
        if (statusLabel != null)
        {
            statusLabel.text = message;
            statusLabel.color = color;
        }
    }
    
    /// <summary>
    /// Actualiza la etiqueta de jugadores conectados
    /// </summary>
    private void UpdatePlayersLabel()
    {
        if (connectedPlayersLabel != null)
        {
            var connectedPlayers = networkManager.GetConnectedPlayers();
            
            string playersText = "[ JUGADORES CONECTADOS ]\n";
            playersText += "---\n";
            
            foreach (var kvp in connectedPlayers)
            {
                PlayerConnectionData player = kvp.Value;
                string connectionStatus = (player.PlayerId == networkManager.GetLocalPlayerId()) ? "(Local)" : "(Remoto)";
                playersText += $"> {player.PlayerName} {connectionStatus}\n";
                playersText += $"  IP: {player.IpAddress}\n";
                playersText += $"  Conectado hace: {(DateTime.Now - player.ConnectionTime).TotalSeconds:F0}s\n";
                playersText += "---\n";
            }
            
            connectedPlayersLabel.text = playersText;
        }
    }
    
    /// <summary>
    /// Añade una sala descubierta a la lista visual
    /// </summary>
    private void AddRoomToList(RoomDiscoveryData roomData)
    {
        string roomKey = $"{roomData.IpAddress}:{roomData.Port}";
        
        if (discoveredRoomButtons.ContainsKey(roomKey))
        {
            UpdateRoomButton(roomKey, roomData);
            return;
        }
        
        if (roomListButtonPrefab == null)
        {
            Debug.LogWarning("[LOBBY_UI] Prefab de boton de sala no asignado");
            return;
        }
        
        GameObject roomButtonObj = Instantiate(roomListButtonPrefab, roomListContainer);
        Button roomButton = roomButtonObj.GetComponent<Button>();
        TextMeshProUGUI roomButtonText = roomButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        
        if (roomButton == null || roomButtonText == null)
        {
            Debug.LogError("[LOBBY_UI] Prefab de sala no tiene estructura esperada");
            Destroy(roomButtonObj);
            return;
        }
        
        string displayText = $"{roomData.RoomName}\n[{roomData.IpAddress}:{roomData.Port}]\n({roomData.CurrentPlayers}/{roomData.MaxPlayers})";
        roomButtonText.text = displayText;
        
        roomButton.onClick.AddListener(() => OnRoomButtonClicked(roomData));
        
        discoveredRoomButtons[roomKey] = roomButtonObj;
        
        Debug.Log($"[LOBBY_UI] Sala añadida a lista: {roomKey}");
    }
    
    /// <summary>
    /// Actualiza un boton de sala existente
    /// </summary>
    private void UpdateRoomButton(string roomKey, RoomDiscoveryData roomData)
    {
        if (!discoveredRoomButtons.ContainsKey(roomKey))
            return;
        
        GameObject roomButtonObj = discoveredRoomButtons[roomKey];
        TextMeshProUGUI roomButtonText = roomButtonObj.GetComponentInChildren<TextMeshProUGUI>();
        
        if (roomButtonText != null)
        {
            string displayText = $"{roomData.RoomName}\n[{roomData.IpAddress}:{roomData.Port}]\n({roomData.CurrentPlayers}/{roomData.MaxPlayers})";
            roomButtonText.text = displayText;
        }
    }
    
    /// <summary>
    /// Maneja el evento de presionar un boton de sala descubierta
    /// </summary>
    private void OnRoomButtonClicked(RoomDiscoveryData roomData)
    {
        Debug.Log($"[LOBBY_UI] Sala seleccionada: {roomData.RoomName}");
        
        ipAddressInput.text = $"{roomData.IpAddress}:{roomData.Port}";
        portInput.text = roomData.Port.ToString();
        
        OnJoinButtonPressed();
    }
}
