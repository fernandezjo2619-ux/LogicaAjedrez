using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    
    private const int BASE_PORT = 8000;
    
    // IDs de jugadores para la partida de ajedrez
    private int idJugador1 = -1;
    private int idJugador2 = -1;
    private int idPartidaCreada = -1;
    
    private void Start()
    {
        ValidateUIReferences();
        InitializeNetworkManager();
        SetupUIListeners();
        InitializeUIState();
        
        Debug.Log("[LOBBY_UI] === UI del Lobby inicializada ===");
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
    private void ValidateUIReferences()
    {
        List<string> missingReferences = new List<string>();
        
        if (hostButton == null) missingReferences.Add("hostButton");
        if (joinButton == null) missingReferences.Add("joinButton");
        if (startGameButton == null) missingReferences.Add("startGameButton");
        if (backButton == null) missingReferences.Add("backButton");
        if (roomNameInput == null) missingReferences.Add("roomNameInput");
        if (ipAddressInput == null) missingReferences.Add("ipAddressInput");
        if (portInput == null) missingReferences.Add("portInput");
        if (idJugador1Input == null) missingReferences.Add("idJugador1Input");
        if (idJugador2Input == null) missingReferences.Add("idJugador2Input");
        if (statusLabel == null) missingReferences.Add("statusLabel");
        if (connectedPlayersLabel == null) missingReferences.Add("connectedPlayersLabel");
        
        if (missingReferences.Count > 0)
        {
            string errorMsg = "[LOBBY_UI] === REFERENCIAS FALTANTES === \n";
            foreach (string reference in missingReferences)
            {
                errorMsg += $"  >> {reference}\n";
            }
            Debug.LogError(errorMsg);
            throw new MissingComponentException(errorMsg);
        }
        
        Debug.Log("[LOBBY_UI] --- Todas las referencias de UI validadas correctamente ---");
    }
    
    /// <summary>
    /// Obtiene e inicializa la referencia del NetworkLobbyManager y SupabaseRPC
    /// </summary>
    private void InitializeNetworkManager()
    {
        networkManager = FindObjectOfType<NetworkLobbyManager>();
        supabaseManager = FindObjectOfType<SupabaseRPC>();
        
        if (networkManager == null)
        {
            Debug.LogError("[LOBBY_UI] === NetworkLobbyManager no encontrado en la escena ===");
            return;
        }
        
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
        roomNameInput.text = SystemInfo.deviceName;
        
        if (portInput != null)
        {
            portInput.contentType = TMP_InputField.ContentType.IntegerNumber;
        }
    }
    
    // ====== MANEJADORES DE EVENTOS DE BOTONES ======
    
    /// <summary>
    /// Maneja el evento de presionar el boton Host
    /// Inicia un servidor local
    /// </summary>
    private void OnHostButtonPressed()
    {
        Debug.Log("[LOBBY_UI] Boton Host presionado");
        
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
    /// Maneja el evento de presionar el boton Join
    /// Conecta con un servidor remoto
    /// </summary>
    private void OnJoinButtonPressed()
    {
        Debug.Log("[LOBBY_UI] Boton Join presionado");
        
        string ipText = ipAddressInput.text.Trim();
        
        if (string.IsNullOrEmpty(ipText))
        {
            UpdateStatusLabel("Ingresa una IP valida", statusDisconnectedColor);
            return;
        }
        
        string ip = ipText;
        int port = BASE_PORT;
        
        if (ipText.Contains(":"))
        {
            string[] parts = ipText.Split(':');
            ip = parts[0].Trim();
            
            if (!int.TryParse(parts[1].Trim(), out port))
            {
                port = BASE_PORT;
            }
        }
        
        if (portInput.text.Length > 0 && int.TryParse(portInput.text, out int inputPort))
        {
            port = inputPort;
        }
        
        UpdateStatusLabel($"Conectando a {ip}:{port}...", statusWaitingColor);
        hostButton.interactable = false;
        joinButton.interactable = false;
        
        if (networkManager.ConnectToServer(ip, port))
        {
            Debug.Log("[LOBBY_UI] Conectado al servidor");
            UpdateUIForClient();
            networkManager.StopRoomDiscovery();
        }
        else
        {
            Debug.LogError("[LOBBY_UI] Error conectando al servidor");
            UpdateStatusLabel("Error al conectar", statusDisconnectedColor);
            hostButton.interactable = true;
            joinButton.interactable = true;
        }
    }
    
    /// <summary>
    /// Maneja el evento de presionar el boton Iniciar Juego
    /// Crea la partida en Supabase y cambia a la escena de juego
    /// </summary>
    private void OnStartGameButtonPressed()
    {
        Debug.Log("[LOBBY_UI] Boton Iniciar Juego presionado");
        
        if (!networkManager.IsServer())
        {
            UpdateStatusLabel("Solo el Host puede iniciar el juego", statusDisconnectedColor);
            return;
        }
        
        // Validar IDs de jugadores
        if (!int.TryParse(idJugador1Input.text, out idJugador1) || idJugador1 <= 0)
        {
            UpdateStatusLabel("ID Jugador 1 inválido", statusDisconnectedColor);
            return;
        }
        
        if (!int.TryParse(idJugador2Input.text, out idJugador2) || idJugador2 <= 0)
        {
            UpdateStatusLabel("ID Jugador 2 inválido", statusDisconnectedColor);
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
                    
                    // Establecer los datos en el NetworkLobbyManager
                    networkManager.SetGameData(idJugador1, idJugador2, idPartida);
                    
                    // Cambiar de escena
                    networkManager.GoToGameScene(gameSceneName);
                }
                else
                {
                    UpdateStatusLabel("Error creando partida en Supabase", statusDisconnectedColor);
                    startGameButton.interactable = true;
                }
            }));
    }
    
    /// <summary>
    /// Maneja el evento de presionar el boton Volver
    /// Desconecta y regresa al menu principal
    /// </summary>
    private void OnBackButtonPressed()
    {
        Debug.Log("[LOBBY_UI] Boton Volver presionado");
        
        networkManager.Disconnect();
        networkManager.StopRoomDiscovery();
        
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
