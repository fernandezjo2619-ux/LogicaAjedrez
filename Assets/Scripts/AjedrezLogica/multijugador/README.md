# README - SISTEMA MULTIJUGADOR

## COMPONENTES PRINCIPALES

### 1. NetworkLobbyManager
- Gestiona conexiones TCP entre clientes y servidor
- Host: Abre puerto y espera conexión
- Cliente: Se conecta a IP:Puerto del host
- Almacena IDs de jugadores y partida en PlayerPrefs antes de cambiar escena

### 2. ChessGameSyncManager
- Sincroniza movimientos de piezas entre jugadores
- Registra cada movimiento en Supabase automáticamente
- Mantiene historial de movimientos
- Lee datos de partida desde PlayerPrefs

### 3. LobbyUIController
- Interfaz para crear sala (Host) o conectarse (Cliente)
- Solicita IDs de jugadores antes de iniciar
- Crea partida en Supabase cuando Host inicia el juego
- Pasa datos a través de NetworkLobbyManager

### 4. CrearPiezas (modificado)
- Lee IDs desde PlayerPrefs al iniciar
- Inicializa ChessGameSyncManager
- Llama a SyncChessMove() después de cada movimiento

---

## FLUJO DE CONEXION

```
Host                              Cliente
|                                    |
+ Inicia Lobby                       |
|                                    |
+ Crea sala                          + Abre Lobby
| (StartHost)                        | (ConnectToServer)
|                                    |
+ Espera conexion                    + Conecta a IP:Puerto
|                                    |
+ Ambos ingresan IDs                 |
|                                    |
+ Host hace click "Iniciar"          |
| (crea partida en Supabase)         |
|                                    |
+ SetGameData(J1, J2, idPartida)    |
|                                    |
+ Cambia escena a PartidaTablero     |
|_____________________________________|
         Ambos en la escena
         ChessGameSyncManager carga
         IDs desde PlayerPrefs
```

---

## SINCRONIZACION DE MOVIMIENTOS

```
Jugador 1 mueve pieza
    |
    v
CrearPiezas.RealizarMovimiento()
    |
    v
ChessGameSyncManager.SyncChessMove()
    |
    v
RegistrarMovimiento.PostRegistrarMovimiento()
    |
    v
Supabase (BD)
```

---

## ESTRUCTURA DE DATOS

### PlayerPrefs (almacena entre escenas)
```
IdJugador1      : int
IdJugador2      : int
IdPartida       : int
LocalPlayerId   : int
```

### ChessMoveSync (movimiento sincronizado)
```
IdPartida              : int
IdJugador              : int
XOrigen, YOrigen       : int (posicion inicial)
XFin, YFin             : int (posicion final)
Timestamp              : long
IdHabilidadUsada       : int?
IdPiezaEmpujada        : int?
XOrigenEmpujada        : int?
YOrigenEmpujada        : int?
XFinEmpujada           : int?
YFinEmpujada           : int?
```

---

## EVENTOS DE RED

```
NetworkLobbyManager dispara eventos:

OnPlayerConnected         -> Un jugador se conectó
OnPlayerDisconnected      -> Un jugador se desconectó
OnConnectionEstablished   -> Conexión exitosa
OnConnectionFailed        -> Error de conexión
OnRoomDiscovered         -> Se encontró una sala en red
```

---

## PASO 1: PREPARAR EL LOBBY

En la escena del Menu_Lobby/Lobby:

```
Inspector:
  LobbyUIController
    Host Button -> Boton
    Join Button -> Boton
    Start Game Button -> Boton
    Back Button -> Boton
    
    Room Name Input -> Input Field (opcional)
    IP Address Input -> Input Field
    Port Input -> Input Field (default 8000)
    
    ID Jugador 1 Input -> Input Field (REQUERIDO)
    ID Jugador 2 Input -> Input Field (REQUERIDO)
    
    Status Label -> TextMeshProUGUI
    Connected Players Label -> TextMeshProUGUI
    
    Game Scene Name = "PartidaTablero"
```

Requisitos:
- SupabaseRPC debe estar en la escena
- NetworkLobbyManager se crea automáticamente (Singleton)

---

## PASO 2: MODIFICAR CREAR PIEZAS

En CrearPiezas.cs, modificar Start():

```csharp
private void Start()
{
    // Obtener datos desde PlayerPrefs (set por NetworkLobbyManager)
    NetworkLobbyManager.GetGameDataFromPrefs(out int idJ1, out int idJ2, out int idPartida);
    
    // Si no hay datos, es modo local/debug
    if (idPartida <= 0)
    {
        Debug.Log("Modo local detectado");
        idJ1 = 1;
        idJ2 = 2;
    }
    
    // Iniciar partida con los IDs
    StartCoroutine(IniciarPartida(idJ1, idJ2));
}

private IEnumerator IniciarPartida(int idUsuario1, int idUsuario2)
{
    int idPartida = 0;
    
    // Guardar partida en BD si no existe
    yield return StartCoroutine(GuardarPartidaBD.GuardarPartida(idUsuario1, idUsuario2,
        resultado => idPartida = resultado));
    
    // Inicializar juego
    juego = new BaseJuego();
    juego.JuegoBase(idUsuario1, idUsuario2, idPartida);
    
    yield return StartCoroutine(IniciarJuegoConHabilidades(juego, idUsuario1, idUsuario2));
    
    CrearPrefabs();
    StartCoroutine(BucleConEspera());
}
```

---

## PASO 3: SINCRONIZAR MOVIMIENTOS

En CrearPiezas, modificar donde se realiza un movimiento:

```csharp
// Opcion 1: En BucleConEspera (movimientos de IA)
private IEnumerator BucleConEspera()
{
    while (true)
    {
        Accion accionIa = iaB.ElegirMovimiento(juego, juego.TurnoActual);
        
        if (accionIa.Tipo == TipoAccion.Empujon)
        {
            juego.EjecutarEmpujon(accionIa.Pieza, accionIa.PiezaEmpujada, 
                                 accionIa.XFin, accionIa.YFin);
        }
        else
        {
            juego.RealizarMovimiento(accionIa.Pieza.Posicion.X, 
                                    accionIa.Pieza.Posicion.Y, 
                                    accionIa.XFin, accionIa.YFin);
        }
        
        // SINCRONIZAR
        var syncManager = FindObjectOfType<ChessGameSyncManager>();
        if (syncManager != null)
        {
            syncManager.SyncChessMove(
                accionIa.Pieza.Posicion.X, accionIa.Pieza.Posicion.Y,
                accionIa.XFin, accionIa.YFin);
        }
        
        MoverVisual(accionIa.Pieza);
        yield return new WaitForSeconds(5);
    }
}

// Opcion 2: En SeleccionarPieza (movimientos del usuario)
// Cuando el usuario hace click para mover
public void OnPiezaMoved(int xOrigen, int yOrigen, int xFin, int yFin)
{
    juego.RealizarMovimiento(xOrigen, yOrigen, xFin, yFin);
    
    // SINCRONIZAR
    var syncManager = FindObjectOfType<ChessGameSyncManager>();
    if (syncManager != null)
    {
        syncManager.SyncChessMove(xOrigen, yOrigen, xFin, yFin);
    }
}
```

---

## PASO 4: DESACTIVAR IA EN MULTIJUGADOR

En CrearPiezas.IniciarJuegoConHabilidades():

```csharp
IEnumerator IniciarJuegoConHabilidades(BaseJuego juego, int idUsuario1, int idUsuario2)
{
    bool esMultijugador = idUsuario1 > 4 && idUsuario2 > 4;
    
    if (esMultijugador)
    {
        Debug.Log("Modo multijugador: sin IA");
        
        // Ambos son jugadores reales - NO inicializar IA
        usuario1 = new Usuario();
        usuario2 = new Usuario();
        
        usuario1.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, null, idUsuario1);
        usuario2.InicializarPiezasDeUsuario(juego, ColorPieza.Negro, null, idUsuario2);
    }
    else
    {
        Debug.Log("Modo local: con IA");
        
        // Inicializar IA como antes
        if (idUsuario1 <= 4)
        {
            iaB = new IAAleatoria();
            iaB.InicializarPiezasDeIA(juego, ColorPieza.Blanco);
        }
        
        if (idUsuario2 <= 4)
        {
            iaN = new IAAleatoria();
            iaN.InicializarPiezasDeIA(juego, ColorPieza.Negro);
        }
    }
}
```

---

## METODOS CLAVE

### NetworkLobbyManager

```csharp
bool StartHost(string roomName = "")
    -> Abre puerto y espera cliente

bool ConnectToServer(string ipAddress, int port)
    -> Conecta a servidor remoto

void SetGameData(int j1, int j2, int idPartida)
    -> Almacena datos antes de cambiar escena

void GoToGameScene(string sceneName)
    -> Cambia escena y pasa datos vía PlayerPrefs

// Estaticos
int GetLocalPlayerIdFromPrefs()
    -> Lee ID del jugador local

void GetGameDataFromPrefs(out int j1, out int j2, out int partida)
    -> Lee IDs de ambos jugadores y partida
```

### ChessGameSyncManager

```csharp
void SyncChessMove(int xOrigen, int yOrigen, int xFin, int yFin, 
                   int? idHabilidad, int? idPiezaEmpujada, ...)
    -> Registra movimiento en Supabase

ChessMoveSync GetLastReceivedMove()
    -> Obtiene ultimo movimiento del oponente

List<ChessMoveSync> GetMovesHistory()
    -> Historial de todos los movimientos

int GetGameId()
    -> ID de la partida actual

int GetLocalPlayerId()
    -> ID del jugador local
```

---

## FLUJO COMPLETO

1. Abrir Menu_Inicio
2. Seleccionar "Jugar Multijugador"
3. Cargar Lobby
4. Ingresar IDs de jugadores (ej: 7, 8)
5. Host crea sala / Cliente se conecta
6. Host hace click "Iniciar Juego"
   - Se crea partida en Supabase
   - Se guardaan IDs en PlayerPrefs
   - Se cambia a PartidaTablero
7. Se carga PartidaTablero
   - CrearPiezas lee IDs desde PlayerPrefs
   - ChessGameSyncManager se inicializa
   - BaseJuego se crea con los IDs
   - Juego inicia
8. Cada movimiento:
   - Se llama SyncChessMove()
   - Se registra en Supabase
9. Fin de partida
   - Regresa a Menu_Inicio

---

## ERRORES COMUNES

### "IDs no se pasan"
- Verificar que idJugador1Input y idJugador2Input estan asignados en inspector
- Verificar que contienen valores numéricos válidos

### "ChessGameSyncManager no sincroniza"
- Verificar que RegistrarMovimiento existe en la escena
- Verificar que SupabaseRPC tiene credenciales correctas
- Revisar consola para errores de conexión

### "Juego no inicia en PartidaTablero"
- Verificar que el nombre de escena es exacto ("PartidaTablero")
- Verificar que la escena existe en Build Settings
- Revisar que PlayerPrefs tiene datos válidos

### "Conexion falla"
- Verificar que ambas máquinas están en la misma red
- Verificar que puerto 8000 no está bloqueado por firewall
- Revisar que IP del servidor es correcta

---

## CONFIGURACION EN UNITY

### Escena Menu_Lobby

```
Crear objetos:
  - Canvas (para UI)
  - InputFields (Room Name, IP, Port, ID1, ID2)
  - Buttons (Host, Join, Start, Back)
  - TextMeshProUGUI (Status, Players)

Scripts a agregar:
  - LobbyUIController (asignar referencias)
  - SupabaseRPC (credenciales Supabase)
  - NetworkLobbyManager (se crea automático)
```

### Escena PartidaTablero

```
Ya debe tener:
  - CrearPiezas
  - Tablero visual
  - RegistrarMovimiento
  
Agregara automáticamente:
  - ChessGameSyncManager (si no existe)
```

---

## TESTING

Para probar en la misma máquina:

```
Opcion 1: Dos instancias de Unity
  - Instance 1: Host en puerto 8000
  - Instance 2: Cliente conecta a localhost:8000

Opcion 2: Dos builds
  - Build 1: Host en IP local
  - Build 2: Cliente conecta a esa IP

IDs de prueba:
  - Jugador 1: 7 (usuario en Supabase)
  - Jugador 2: 8 (usuario en Supabase)
```

---

## VARIABLES DE ENTORNO

En .env o credenciales de Supabase:

```
SUPABASE_URL = https://qutkopstyyqzcjmytylq.supabase.co
SUPABASE_KEY = sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu
SUPABASE_RPC_GUARDAR_PARTIDA = guardar_partida
SUPABASE_RPC_REGISTRAR_MOVIMIENTO = registrar_movimiento
```

---

## NOTAS IMPORTANTES

- Los IDs 1-4 estan reservados para IA local
- Los IDs 5+ son para jugadores reales (en multijugador)
- Cada movimiento se registra con timestamp en Supabase
- El historial de movimientos se mantiene en memoria
- Al cerrar el juego, PlayerPrefs se limpian

---

## RESUMEN

El sistema multijugador funciona en 3 fases:

1. CONEXION: NetworkLobbyManager (TCP entre máquinas)
2. INICIO: LobbyUIController (crea partida en Supabase)
3. JUEGO: ChessGameSyncManager (sincroniza movimientos)

Todo pasa a través de PlayerPrefs entre escenas.
