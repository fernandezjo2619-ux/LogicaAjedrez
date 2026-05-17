using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Gestor central de lobby para conexiones multijugador entre dos jugadores
/// Proporciona funcionalidades de host, cliente, descubrimiento de salas y sincronizacion
/// </summary>
public class NetworkLobbyManager : MonoBehaviour
//hola
{
    private const int BASE_PORT = 8000;
    private const int MAX_PLAYERS = 2;
    private const int MAX_PORT_ATTEMPTS = 10;
    private const int SERVER_BROADCAST_PORT = 47777;
    private const int DISCOVERY_TIMEOUT = 3000;
    
    // TODO: BORRAR estas constantes cuando los IDs reales estén disponibles desde la UI/BD
    private const int TODO_TEST_ID_JUGADOR1 = 6;
    private const int TODO_TEST_ID_JUGADOR2 = 7;
    
    [SerializeField] private int currentPort = BASE_PORT;
    [SerializeField] private bool isServer = false;
    [SerializeField] private bool isConnected = false;
    
    private int connectedPlayerId = -1;
    private int idJugador1 = -1;  // Para ajedrez: ID del primer jugador
    private int idJugador2 = -1;  // Para ajedrez: ID del segundo jugador
    private int idPartida = -1;   // Para ajedrez: ID de la partida en Supabase
    
    private Dictionary<int, PlayerConnectionData> connectedPlayers = new Dictionary<int, PlayerConnectionData>();
    private Dictionary<string, RoomDiscoveryData> discoveredRooms = new Dictionary<string, RoomDiscoveryData>();
    
    private UdpClient discoveryClient;
    private System.Net.Sockets.TcpListener tcpListener;
    private List<System.Net.Sockets.TcpClient> connectedClients = new List<System.Net.Sockets.TcpClient>();
    
    // Cola thread-safe para conexiones aceptadas desde el hilo de fondo
    private readonly Queue<System.Net.Sockets.TcpClient> _pendingClients = new Queue<System.Net.Sockets.TcpClient>();
    private readonly object _pendingLock = new object();
    private bool _acceptingConnections = false;
    
    // Cola thread-safe para mensajes TCP recibidos desde el hilo de fondo
    private readonly Queue<string> _incomingMessages = new Queue<string>();
    private readonly object _msgLock = new object();
    
    public delegate void OnPlayerConnectedDelegate(int playerId, string playerName);
    public delegate void OnPlayerDisconnectedDelegate(int playerId);
    public delegate void OnRoomDiscoveredDelegate(RoomDiscoveryData roomData);
    public delegate void OnConnectionFailedDelegate(string errorMessage);
    public delegate void OnConnectionEstablishedDelegate();
    public delegate void OnChessMoveReceivedDelegate(string moveData);
    
    public event OnPlayerConnectedDelegate OnPlayerConnected;
    public event OnPlayerDisconnectedDelegate OnPlayerDisconnected;
    public event OnRoomDiscoveredDelegate OnRoomDiscovered;
    public event OnConnectionFailedDelegate OnConnectionFailed;
    public event OnConnectionEstablishedDelegate OnConnectionEstablished;
    public event OnChessMoveReceivedDelegate OnChessMoveReceived;
    
    private static NetworkLobbyManager instance;
    
    /// <summary>
    /// Acceso al singleton (útil cuando el objeto está en DontDestroyOnLoad)
    /// </summary>
    public static NetworkLobbyManager Instance => instance;
    
    private bool isDiscoveringRooms = false;
    private Coroutine discoveryCoroutine;
    private Coroutine _broadcastCoroutine;
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            // Escuchar cambios de escena para cerrar el puerto al volver al menú
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Se ejecuta cada vez que una escena termina de cargarse.
    /// Si es la escena del menú, cierra el puerto TCP y todos los recursos de red.
    /// </summary>
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Nombres de la escena del menú principal — ajusta si cambia el nombre
        bool esMenuPrincipal = scene.name == "Menu_Inicio"
                            || scene.name == "MenuInicio"
                            || scene.name == "MainMenu";

        if (esMenuPrincipal)
        {
            Debug.Log($"[NETWORK] Escena '{scene.name}' detectada — cerrando puerto y recursos de red.");

            // Parar flags primero para evitar que corrutinas sigan en fondo
            _acceptingConnections = false;
            isConnected           = false;
            isServer              = false;

            // Limpiar todos los recursos (puerto TCP, UDP, corrutinas)
            CleanupNetworkResources();

            // Limpiar estado de jugadores
            connectedPlayers.Clear();
            discoveredRooms.Clear();
            connectedPlayerId = -1;

            Debug.Log("[NETWORK] Puerto 8000 cerrado. Recursos de red liberados al volver al menú.");
        }
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            CleanupNetworkResources();
        }
    }
    
    /// <summary>
    /// Inicia el servidor en el puerto especificado
    /// Realiza multiples intentos en puertos consecutivos si es necesario
    /// </summary>
    public bool StartHost(string roomName = "")
    {
        Debug.LogWarning("[NETWORK] === StartHost() LLAMADO ===");
        
        if (string.IsNullOrEmpty(roomName))
        {
            roomName = SystemInfo.deviceName + " Sala";
        }
        
        for (int attempt = 0; attempt < MAX_PORT_ATTEMPTS; attempt++)
        {
            currentPort = BASE_PORT + attempt;
            
            try
            {
                if (TryCreateServerOnPort(currentPort))
                {
                    isServer = true;
                    isConnected = true;
                    
                    string localIp = GetLocalIpAddress();
                    connectedPlayerId = 1;
                    
                    connectedPlayers[1] = new PlayerConnectionData
                    {
                        PlayerId = 1,
                        PlayerName = "Jugador 1 (Host)",
                        IpAddress = localIp,
                        Port = currentPort,
                        ConnectionTime = DateTime.Now
                    };
                    
                    Debug.LogWarning($"[NETWORK] === SERVIDOR CREADO en puerto {currentPort}, IP={localIp}, Sala={roomName} ===");
                    
                    StartBroadcastDiscovery(roomName, localIp, currentPort);
                    OnConnectionEstablished?.Invoke();
                    
                    return true;
                }
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NETWORK] Puerto {currentPort} no disponible: {ex.Message}");
            }
        }
        
        Debug.LogError("[NETWORK] === No se pudo crear servidor en ningun puerto ===");
        OnConnectionFailed?.Invoke($"No se encontro puerto disponible entre {BASE_PORT} y {BASE_PORT + MAX_PORT_ATTEMPTS - 1}");
        
        return false;
    }
    
    /// <summary>
    /// Inicia la conexión al servidor de forma asíncrona (no bloquea el hilo principal).
    /// Usa StartCoroutine(ConnectToServerAsync(ip, port)) desde LobbyUIController.
    /// </summary>
    public IEnumerator ConnectToServerAsync(string ipAddress, int port)
    {
        Debug.Log("[NETWORK] === Iniciando conexion asincrona a servidor remoto ===");
        Debug.Log($"[NETWORK] IP: {ipAddress}, Puerto: {port}");
        
        System.Net.Sockets.TcpClient client = new System.Net.Sockets.TcpClient();
        
        // Lanzar ConnectAsync en un hilo de fondo
        System.Threading.Tasks.Task connectTask = client.ConnectAsync(ipAddress, port);
        
        float elapsed = 0f;
        const float TIMEOUT = 3f;
        
        // Esperar hasta que conecte o expire el timeout
        while (!connectTask.IsCompleted && elapsed < TIMEOUT)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (!connectTask.IsCompleted)
        {
            // Timeout: cancelar y limpiar
            client.Close();
            client.Dispose();
            string msg = $"Timeout: el servidor {ipAddress}:{port} no respondió en {TIMEOUT}s";
            Debug.LogError($"[NETWORK] {msg}");
            OnConnectionFailed?.Invoke(msg);
            yield break;
        }
        
        if (connectTask.IsFaulted || connectTask.Exception != null)
        {
            client.Close();
            client.Dispose();
            string msg = connectTask.Exception?.GetBaseException().Message ?? "Error desconocido";
            Debug.LogError($"[NETWORK] Error conectando: {msg}");
            OnConnectionFailed?.Invoke($"Error de conexion: {msg}");
            yield break;
        }
        
        // Conexión exitosa
        connectedClients.Add(client);
        isServer = false;
        isConnected = true;
        currentPort = port;
        connectedPlayerId = 2;
        
        connectedPlayers[2] = new PlayerConnectionData
        {
            PlayerId = 2,
            PlayerName = "Jugador 2 (Cliente)",
            IpAddress = ipAddress,
            Port = port,
            ConnectionTime = DateTime.Now
        };
        
        // Iniciar recepción de mensajes del HOST (START_GAME, etc.)
        StartReceivingMessages(client);
        
        Debug.Log($"[NETWORK] === Conexion establecida con {ipAddress}:{port} ===");
        OnConnectionEstablished?.Invoke();
        // IEnumerator termina aquí — no se necesita return
    }
    
    /// <summary>
    /// Inicia la busqueda de salas disponibles mediante UDP broadcast
    /// </summary>
    public void StartRoomDiscovery()
    {
        if (isDiscoveringRooms)
        {
            Debug.LogWarning("[NETWORK] Descubrimiento ya en progreso");
            return;
        }
        
        Debug.Log("[NETWORK] === Iniciando descubrimiento de salas ===");
        isDiscoveringRooms = true;
        
        try
        {
            discoveryClient = new UdpClient();
            discoveryClient.EnableBroadcast = true;
            discoveryClient.Client.ReceiveTimeout = DISCOVERY_TIMEOUT;
            
            discoveryCoroutine = StartCoroutine(DiscoveryCoroutine());
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NETWORK] Error iniciando descubrimiento: {ex.Message}");
            isDiscoveringRooms = false;
        }
    }
    
    /// <summary>
    /// Detiene el descubrimiento de salas
    /// </summary>
    public void StopRoomDiscovery()
    {
        Debug.Log("[NETWORK] === Deteniendo descubrimiento de salas ===");
        isDiscoveringRooms = false;
        
        if (discoveryCoroutine != null)
        {
            StopCoroutine(discoveryCoroutine);
            discoveryCoroutine = null;
        }
        
        if (discoveryClient != null)
        {
            try
            {
                discoveryClient.Close();
                discoveryClient.Dispose();
            }
            catch { }
            discoveryClient = null;
        }
    }
    
    /// <summary>
    /// Corrutina que maneja el descubrimiento continuo de salas
    /// </summary>
    private IEnumerator DiscoveryCoroutine()
    {
        while (isDiscoveringRooms)
        {
            try
            {
                SendDiscoveryBroadcast();
            }
            catch (Exception ex)
            {
                Debug.LogWarning($"[NETWORK] Error en descubrimiento: {ex.Message}");
            }
            
            float timeoutCounter = 0f;
            while (timeoutCounter < 2f && isDiscoveringRooms)
            {
                try
                {
                    if (discoveryClient.Available > 0)
                    {
                        ProcessDiscoveryResponse();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[NETWORK] Error procesando respuesta: {ex.Message}");
                }
                
                timeoutCounter += 0.1f;
                yield return new WaitForSeconds(0.1f);
            }
            
            yield return new WaitForSeconds(1f);
        }
    }
    
    /// <summary>
    /// Obtiene la direccion IP local valida de la red
    /// Prioriza las redes privadas sobre localhost
    /// </summary>
    public static string GetLocalIpAddress()
    {
        Debug.Log("[NETWORK] === Obteniendo direccion IP local ===");
        
        try
        {
            List<string> validAddresses = new List<string>();
            
            foreach (NetworkInterface networkInterface in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    continue;
                
                if (networkInterface.OperationalStatus != OperationalStatus.Up)
                    continue;
                
                foreach (UnicastIPAddressInformation ipAddress in networkInterface.GetIPProperties().UnicastAddresses)
                {
                    if (ipAddress.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;
                    
                    string address = ipAddress.Address.ToString();
                    Debug.Log($"[NETWORK] IP encontrada: {address} ({networkInterface.Name})");
                    validAddresses.Add(address);
                    
                    if (address.StartsWith("192.168.") || address.StartsWith("10."))
                    {
                        Debug.Log($"[NETWORK] >>> IP seleccionada (WiFi/Red privada): {address}");
                        return address;
                    }
                }
            }
            
            if (validAddresses.Count > 0)
            {
                Debug.Log($"[NETWORK] >>> IP seleccionada (fallback): {validAddresses[0]}");
                return validAddresses[0];
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"[NETWORK] Error obteniendo IP: {ex.Message}");
        }
        
        Debug.LogWarning("[NETWORK] >>> IP por defecto: localhost");
        return "127.0.0.1";
    }
    
    /// <summary>
    /// Desconecta el cliente o cierra el servidor
    /// </summary>
    public void Disconnect()
    {
        Debug.Log("[NETWORK] === Desconectando ===");
        
        // Parar flags ANTES de limpiar recursos para que AcceptClientsAsync
        // y BroadcastDiscoveryCoroutine terminen limpiamente
        _acceptingConnections = false;
        isConnected = false;
        isServer = false;
        
        CleanupNetworkResources();
        
        connectedPlayers.Clear();
        discoveredRooms.Clear();
        connectedPlayerId = -1;
        
        Debug.Log("[NETWORK] === Puerto cerrado y recursos liberados ===");
    }
    
    /// <summary>
    /// Cambia a la escena del juego (pasando los IDs de jugadores y partida)
    /// </summary>
    public void GoToGameScene(string sceneName)
    {
        if (!isConnected || connectedPlayerId <= 0)
        {
            Debug.LogError("[NETWORK] No conectado - No se puede cambiar de escena");
            return;
        }
        
        if (idJugador1 <= 0 || idJugador2 <= 0)
        {
            // TODO: BORRAR este bloque cuando los IDs reales estén disponibles
            Debug.LogWarning($"[NETWORK] IDs no inicializados — usando IDs de prueba ({TODO_TEST_ID_JUGADOR1}/{TODO_TEST_ID_JUGADOR2})");
            idJugador1 = TODO_TEST_ID_JUGADOR1;
            idJugador2 = TODO_TEST_ID_JUGADOR2;
        }
        
        // idPartida puede ser -1 si Supabase falló — se permite continuar igualmente
        if (idPartida <= 0)
        {
            Debug.LogWarning("[NETWORK] idPartida no válido, usando -1 (sin registro en BD)");
            idPartida = -1;
        }
        
        // Pasar los datos a través de PlayerPrefs para que estén disponibles en la siguiente escena
        PlayerPrefs.SetInt("IdJugador1", idJugador1);
        PlayerPrefs.SetInt("IdJugador2", idJugador2);
        PlayerPrefs.SetInt("IdPartida", idPartida);
        PlayerPrefs.SetInt("LocalPlayerId", connectedPlayerId);
        PlayerPrefs.Save();
        
        // Notificar a todos los clientes para que también cambien de escena
        if (isServer && connectedClients.Count > 0)
        {
            string msg = $"START_GAME|{sceneName}|{idJugador1}|{idJugador2}|{idPartida}";
            SendToAllClients(msg);
            Debug.LogWarning($"[NETWORK] Mensaje START_GAME enviado a {connectedClients.Count} cliente(s)");
            // Pequeña pausa para asegurar que el mensaje se envía antes de cambiar de escena
            System.Threading.Thread.Sleep(200);
        }
        
        Debug.Log($"[NETWORK] === Cambiando a escena: {sceneName} ===");
        Debug.Log($"[NETWORK] Datos: J1={idJugador1}, J2={idJugador2}, Partida={idPartida}, Local={connectedPlayerId}");
        
        SceneManager.LoadScene(sceneName);
    }
    
    /// <summary>
    /// Envía un mensaje de texto a todos los clientes conectados (HOST → CLIENTs).
    /// </summary>
    private void SendToAllClients(string message)
    {
        byte[] data = System.Text.Encoding.UTF8.GetBytes(message + "\n");
        foreach (var client in connectedClients)
        {
            try
            {
                if (client != null && client.Connected)
                    client.GetStream().Write(data, 0, data.Length);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[NETWORK] Error enviando mensaje: {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Envía un mensaje TCP al otro jugador (bidireccional).
    /// - Si eres HOST: envía a todos los clientes conectados.
    /// - Si eres CLIENTE: envía al host a través de la conexión TCP existente.
    /// Usar para sincronizar movimientos de ajedrez durante la partida.
    /// </summary>
    public void SendMessage(string message)
    {
        if (!isConnected)
        {
            Debug.LogWarning("[NETWORK] No se puede enviar mensaje: no hay conexión activa");
            return;
        }
        
        if (isServer)
        {
            // HOST → enviar a todos los clientes
            SendToAllClients(message);
        }
        else
        {
            // CLIENTE → enviar al host (primer TcpClient en connectedClients)
            if (connectedClients.Count > 0 && connectedClients[0] != null && connectedClients[0].Connected)
            {
                try
                {
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(message + "\n");
                    connectedClients[0].GetStream().Write(data, 0, data.Length);
                }
                catch (Exception ex)
                {
                    Debug.LogError($"[NETWORK] Error enviando mensaje al host: {ex.Message}");
                }
            }
            else
            {
                Debug.LogWarning("[NETWORK] No hay conexión TCP al host para enviar mensaje");
            }
        }
    }
    
    /// <summary>
    /// Inicia la escucha de mensajes entrantes en un hilo de fondo.
    /// Llamado tanto en el cliente (tras conectar) como en el host (por cada cliente aceptado).
    /// </summary>
    private async void StartReceivingMessages(System.Net.Sockets.TcpClient client)
    {
        Debug.LogWarning("[NETWORK] Iniciando recepcion de mensajes TCP...");
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[4096];
            var sb = new System.Text.StringBuilder();
            
            while (client.Connected)
            {
                int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;
                
                sb.Append(System.Text.Encoding.UTF8.GetString(buffer, 0, bytesRead));
                
                // Procesar mensajes completos (delimitados por \n)
                string data = sb.ToString();
                int newlineIdx;
                while ((newlineIdx = data.IndexOf('\n')) >= 0)
                {
                    string msg = data.Substring(0, newlineIdx).Trim();
                    data = data.Substring(newlineIdx + 1);
                    if (!string.IsNullOrEmpty(msg))
                    {
                        lock (_msgLock) { _incomingMessages.Enqueue(msg); }
                        Debug.LogWarning($"[NETWORK] Mensaje recibido: {msg}");
                    }
                }
                sb.Clear();
                sb.Append(data);
            }
        }
        catch (Exception ex)
        {
            if (isConnected)
                Debug.LogWarning($"[NETWORK] Recepcion terminada: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Establece los IDs de jugadores y partida (debe ser llamado desde el lobby)
    /// </summary>
    public void SetGameData(int jugador1, int jugador2, int partida)
    {
        idJugador1 = jugador1;
        idJugador2 = jugador2;
        idPartida = partida;
        
        Debug.Log($"[NETWORK] Datos de juego establecidos: J1={jugador1}, J2={jugador2}, Partida={partida}");
    }
    
    /// <summary>
    /// Obtiene el ID de la partida actual
    /// </summary>
    public int GetGameId()
    {
        return idPartida;
    }
    
    /// <summary>
    /// Obtiene los IDs de los jugadores
    /// </summary>
    public void GetPlayerIds(out int jugador1, out int jugador2)
    {
        jugador1 = idJugador1;
        jugador2 = idJugador2;
    }
    
    /// <summary>
    /// Obtiene la informacion de los jugadores conectados
    /// </summary>
    public Dictionary<int, PlayerConnectionData> GetConnectedPlayers()
    {
        return new Dictionary<int, PlayerConnectionData>(connectedPlayers);
    }
    
    /// <summary>
    /// Obtiene las salas descubiertas
    /// </summary>
    public List<RoomDiscoveryData> GetDiscoveredRooms()
    {
        return discoveredRooms.Values.ToList();
    }
    
    /// <summary>
    /// Obtiene el ID del jugador local
    /// </summary>
    public int GetLocalPlayerId()
    {
        return connectedPlayerId;
    }
    
    /// <summary>
    /// Verifica si el cliente local es el servidor
    /// </summary>
    public bool IsServer()
    {
        return isServer;
    }
    
    /// <summary>
    /// Verifica si hay conexion activa
    /// </summary>
    public bool IsConnected()
    {
        return isConnected;
    }
    
    public int GetCurrentPort()
    {
        return currentPort;
    }
    
    public int GetConnectedPlayerCount()
    {
        return connectedPlayers.Count;
    }
    
    /// <summary>
    /// Obtiene el ID del jugador local desde PlayerPrefs (después de cambiar de escena)
    /// </summary>
    public static int GetLocalPlayerIdFromPrefs()
    {
        return PlayerPrefs.GetInt("LocalPlayerId", -1);
    }
    
    /// <summary>
    /// Obtiene los IDs de los jugadores desde PlayerPrefs (después de cambiar de escena)
    /// </summary>
    public static void GetGameDataFromPrefs(out int jugador1, out int jugador2, out int partida)
    {
        jugador1 = PlayerPrefs.GetInt("IdJugador1", -1);
        jugador2 = PlayerPrefs.GetInt("IdJugador2", -1);
        partida = PlayerPrefs.GetInt("IdPartida", -1);
    }
    
    // ====== METODOS PRIVADOS ======
    
    private bool TryCreateServerOnPort(int port)
    {
        try
        {
            tcpListener = new System.Net.Sockets.TcpListener(IPAddress.Any, port);
            tcpListener.Start();
            
            // Usar async/await en vez de polling — más fiable para conexiones LAN
            _acceptingConnections = true;
            AcceptClientsAsync();
            
            Debug.LogWarning($"[NETWORK] Servidor TCP escuchando en puerto {port}");
            return true;
        }
        catch (SocketException ex)
        {
            Debug.LogError($"[NETWORK] ERROR en puerto {port}: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Acepta conexiones entrantes de forma asíncrona en un hilo de fondo.
    /// Las conexiones se encolan y se procesan en el hilo principal via Update().
    /// </summary>
    private async void AcceptClientsAsync()
    {
        Debug.LogWarning("[NETWORK] AcceptClientsAsync INICIADO — esperando clientes...");
        
        while (_acceptingConnections && tcpListener != null)
        {
            try
            {
                System.Net.Sockets.TcpClient client = await tcpListener.AcceptTcpClientAsync();
                Debug.LogWarning("[NETWORK] >>> CLIENTE ACEPTADO (hilo de fondo)");
                
                lock (_pendingLock)
                {
                    _pendingClients.Enqueue(client);
                }
            }
            catch (Exception ex)
            {
                if (_acceptingConnections)
                    Debug.LogError($"[NETWORK] Error aceptando cliente: {ex.Message}");
                break;
            }
        }
        
        Debug.LogWarning("[NETWORK] AcceptClientsAsync TERMINADO");
    }
    
    private void Update()
    {
        // 1. Procesar conexiones entrantes (HOST)
        lock (_pendingLock)
        {
            while (_pendingClients.Count > 0)
            {
                System.Net.Sockets.TcpClient incomingClient = _pendingClients.Dequeue();
                connectedClients.Add(incomingClient);
                
                string clientIp = "desconocida";
                try { clientIp = ((IPEndPoint)incomingClient.Client.RemoteEndPoint).Address.ToString(); } catch { }
                
                int newPlayerId = connectedPlayers.Count + 1;
                string newPlayerName = $"Jugador {newPlayerId}";
                
                connectedPlayers[newPlayerId] = new PlayerConnectionData
                {
                    PlayerId       = newPlayerId,
                    PlayerName     = newPlayerName,
                    IpAddress      = clientIp,
                    Port           = currentPort,
                    ConnectionTime = DateTime.Now
                };
                
                // Iniciar recepción de mensajes de este cliente (para mensajes futuros cliente → host)
                StartReceivingMessages(incomingClient);
                
                Debug.LogWarning($"[NETWORK] Jugador {newPlayerId} registrado desde {clientIp} — Total: {connectedPlayers.Count}");
                OnPlayerConnected?.Invoke(newPlayerId, newPlayerName);
            }
        }
        
        // 2. Procesar mensajes TCP recibidos (CLIENT y HOST)
        lock (_msgLock)
        {
            while (_incomingMessages.Count > 0)
            {
                string msg = _incomingMessages.Dequeue();
                ProcessNetworkMessage(msg);
            }
        }
    }
    
    /// <summary>
    /// Procesa un mensaje TCP recibido. Ejecutado en el hilo principal de Unity.
    /// </summary>
    private void ProcessNetworkMessage(string message)
    {
        Debug.LogWarning($"[NETWORK] Procesando mensaje: {message}");
        
        string[] parts = message.Split('|');
        if (parts.Length == 0) return;
        
        switch (parts[0])
        {
            case "START_GAME":
                // Formato: START_GAME|sceneName|idJ1|idJ2|idPartida
                if (parts.Length >= 5)
                {
                    string sceneName  = parts[1];
                    int.TryParse(parts[2], out int j1);
                    int.TryParse(parts[3], out int j2);
                    int.TryParse(parts[4], out int partida);
                    
                    idJugador1 = j1 > 0 ? j1 : TODO_TEST_ID_JUGADOR1;
                    idJugador2 = j2 > 0 ? j2 : TODO_TEST_ID_JUGADOR2;
                    idPartida  = partida;
                    
                    PlayerPrefs.SetInt("IdJugador1",   idJugador1);
                    PlayerPrefs.SetInt("IdJugador2",   idJugador2);
                    PlayerPrefs.SetInt("IdPartida",    idPartida);
                    PlayerPrefs.SetInt("LocalPlayerId", connectedPlayerId);
                    PlayerPrefs.Save();
                    
                    Debug.LogWarning($"[NETWORK] START_GAME recibido — cargando escena: {sceneName}");
                    SceneManager.LoadScene(sceneName);
                }
                break;
            
            case "CHESS_MOVE":
                // Formato: CHESS_MOVE|idPartida|idJugador|xOrig|yOrig|xFin|yFin|timestamp|idHab|idPiezaEmp|xOrigEmp|yOrigEmp|xFinEmp|yFinEmp
                if (parts.Length >= 14)
                {
                    // Si soy host, retransmitir el movimiento a todos los demás clientes (relay)
                    if (isServer)
                    {
                        SendToAllClients(message);
                    }
                    
                    // Reconstruir los datos del movimiento (sin el prefijo CHESS_MOVE)
                    string moveData = message.Substring("CHESS_MOVE|".Length);
                    Debug.LogWarning($"[NETWORK] CHESS_MOVE recibido: {moveData}");
                    OnChessMoveReceived?.Invoke(moveData);
                }
                else
                {
                    Debug.LogWarning($"[NETWORK] CHESS_MOVE con formato inválido ({parts.Length} campos, se esperan 14): {message}");
                }
                break;
                
            default:
                Debug.Log($"[NETWORK] Mensaje desconocido: {parts[0]}");
                break;
        }
    }
    
    // AcceptConnectionsCoroutine reemplazada por AcceptClientsAsync + Update()
    
    private void StartBroadcastDiscovery(string roomName, string ipAddress, int port)
    {
        // Parar broadcast previo si existiera
        if (_broadcastCoroutine != null)
        {
            StopCoroutine(_broadcastCoroutine);
            _broadcastCoroutine = null;
        }
        _broadcastCoroutine = StartCoroutine(BroadcastDiscoveryCoroutine(roomName, ipAddress, port));
    }
    
    private IEnumerator BroadcastDiscoveryCoroutine(string roomName, string ipAddress, int port)
    {
        using (UdpClient broadcastClient = new UdpClient())
        {
            broadcastClient.EnableBroadcast = true;
            IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, SERVER_BROADCAST_PORT);
            
            while (isServer && isConnected)
            {
                try
                {
                    string broadcastMessage = $"ROOM_DISCOVERY|{roomName}|{ipAddress}|{port}|{connectedPlayers.Count}|{MAX_PLAYERS}";
                    byte[] data = System.Text.Encoding.UTF8.GetBytes(broadcastMessage);
                    
                    broadcastClient.Send(data, data.Length, broadcastEndpoint);
                    
                    Debug.Log($"[NETWORK] Broadcasting sala: {roomName} en puerto {port}");
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[NETWORK] Error en broadcast: {ex.Message}");
                }
                
                yield return new WaitForSeconds(2f);
            }
        }
    }
    
    private void SendDiscoveryBroadcast()
    {
        try
        {
            string discoveryRequest = "ROOM_DISCOVERY_REQUEST";
            byte[] data = System.Text.Encoding.UTF8.GetBytes(discoveryRequest);
            
            IPEndPoint broadcastEndpoint = new IPEndPoint(IPAddress.Broadcast, SERVER_BROADCAST_PORT);
            discoveryClient.Send(data, data.Length, broadcastEndpoint);
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[NETWORK] Error enviando solicitud de descubrimiento: {ex.Message}");
        }
    }
    
    private void ProcessDiscoveryResponse()
    {
        try
        {
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 0);
            byte[] receivedData = discoveryClient.Receive(ref remoteIpEndPoint);
            string message = System.Text.Encoding.UTF8.GetString(receivedData);
            
            if (message.StartsWith("ROOM_DISCOVERY|"))
            {
                string[] parts = message.Split('|');
                
                if (parts.Length >= 6)
                {
                    var roomData = new RoomDiscoveryData
                    {
                        RoomName = parts[1],
                        IpAddress = parts[2],
                        Port = int.Parse(parts[3]),
                        CurrentPlayers = int.Parse(parts[4]),
                        MaxPlayers = int.Parse(parts[5]),
                        DiscoveryTime = DateTime.Now
                    };
                    
                    string roomKey = $"{roomData.IpAddress}:{roomData.Port}";
                    
                    if (!discoveredRooms.ContainsKey(roomKey))
                    {
                        Debug.Log($"[NETWORK] >>> Nueva sala descubierta: {roomData.RoomName}");
                        OnRoomDiscovered?.Invoke(roomData);
                    }
                    
                    discoveredRooms[roomKey] = roomData;
                }
            }
        }
        catch (SocketException)
        {
            // Timeout normal
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[NETWORK] Error procesando respuesta de descubrimiento: {ex.Message}");
        }
    }
    
    private void CleanupNetworkResources()
    {
        // 1. Parar corrutina de broadcast UDP explícitamente
        if (_broadcastCoroutine != null)
        {
            StopCoroutine(_broadcastCoroutine);
            _broadcastCoroutine = null;
            Debug.Log("[NETWORK] Broadcast UDP detenido");
        }

        // 2. Parar descubrimiento de salas
        StopRoomDiscovery();

        // 3. Cerrar el servidor TCP (libera el puerto)
        try
        {
            if (tcpListener != null)
            {
                tcpListener.Stop();
                tcpListener = null;
                Debug.Log("[NETWORK] Puerto TCP del servidor cerrado");
            }
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"[NETWORK] Error cerrando tcpListener: {ex.Message}");
        }

        // 4. Cerrar todas las conexiones de clientes
        foreach (var client in connectedClients)
        {
            try
            {
                client?.Close();
                client?.Dispose();
            }
            catch { }
        }
        connectedClients.Clear();

        // 5. Cerrar cliente UDP de descubrimiento si sigue abierto
        if (discoveryClient != null)
        {
            try
            {
                discoveryClient.Close();
                discoveryClient.Dispose();
            }
            catch { }
            discoveryClient = null;
        }

        Debug.Log("[NETWORK] Todos los recursos de red han sido liberados");
    }
}

/// <summary>
/// Estructura de datos para almacenar informacion de jugadores conectados
/// </summary>
public class PlayerConnectionData
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public DateTime ConnectionTime { get; set; }
}

/// <summary>
/// Estructura de datos para informacion de salas descubiertas
/// </summary>
public class RoomDiscoveryData
{
    public string RoomName { get; set; }
    public string IpAddress { get; set; }
    public int Port { get; set; }
    public int CurrentPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public DateTime DiscoveryTime { get; set; }
}
