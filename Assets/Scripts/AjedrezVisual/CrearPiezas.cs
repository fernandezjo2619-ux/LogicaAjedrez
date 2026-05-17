
using LogicProject;
using LogicProject.IA;
using LogicProject.IA.Estructuras;
using LogicProject.IA.Utilidad;
using LogicProject.Recursos.Class;
using LogicProject.Recursos.Enum;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Estructuras
{
    public int p_id_partida { get; set; }
    public int p_id_usuario { get; set; }
    public int p_id_pieza { get; set; }
    public int p_turno_numero { get; set; }
    public int p_x_origen { get; set; }
    public int p_y_origen { get; set; }
    public int p_x_fin { get; set; }
    public int p_y_fin { get; set; }
    public int? p_id_habilidad_usada { get; set; } = null;
    public int? p_id_pieza_empujada { get; set; } = null;
    public int? p_x_origen_empujada { get; set; } = null;
    public int? p_y_origen_empujada { get; set; } = null;
    public int? p_x_fin_empujada { get; set; } = null;
    public int? p_y_fin_empujada { get; set; } = null;
}

public class CrearPiezas : MonoBehaviour
{
    public static CrearPiezas Instance;

    public GameObject peonPrefab;
    public GameObject torrePrefab;
    public GameObject caballoPrefab;
    public GameObject alfilPrefab;
    public GameObject damaPrefab;
    public GameObject reyPrefab;

    public BaseJuego juego; // Logica global del juego
    public Usuario usuario1;
    public Usuario usuario2;
    public IMotorIA iaB;
    public IMotorIA iaN;
    public IMotorIA iaActual;

    public System.Random random = new System.Random();

    //Activar IA
    //public bool jugarContraIA = false;
    //public ColorPieza colorIA = ColorPieza.Negro;
    //private bool ejecutandoIA = false;

    //Guardar pieza y posicion
    //public static GameObject[,] piezasVisuales = new GameObject[8, 8];

    public Dictionary<Pieza, PiezasPrefab> mapaPiezas = new();
    public Dictionary<int, Estructuras> mapaRegistroMovimiento = new();

    // Instancias de MonoBehaviour para acceso a métodos coroutine
    private SupabaseRPC GuardarPartidaBD;
    private ObtenerHabilidadesUsuario HabilidadesUsuarioBD;
    private RegistrarMovimiento registrarMovimientoDb;

    private bool movimientoUsuarioRealizado = false;
    private ChessGameSyncManager chessSyncManager;

    public void NotificarMovimientoUsuario()
    {
        movimientoUsuarioRealizado = true;
    }

    void Awake()
    {
        Instance = this;

        // Obtener o crear componentes necesarios
        GuardarPartidaBD = GetComponent<SupabaseRPC>() ?? gameObject.AddComponent<SupabaseRPC>();
        HabilidadesUsuarioBD = GetComponent<ObtenerHabilidadesUsuario>() ?? gameObject.AddComponent<ObtenerHabilidadesUsuario>();
        registrarMovimientoDb = GetComponent<RegistrarMovimiento>() ?? gameObject.AddComponent<RegistrarMovimiento>();
    }

    private IEnumerator IniciarPartida(int idUsuario1, int idUsuario2, int idPartidaExistente)
    {
        // 1. Guardar partida en BD y obtener el ID
        int idPartida = idPartidaExistente > 0 ? idPartidaExistente : 0;
        yield return StartCoroutine(GuardarPartidaBD.GuardarPartida(idUsuario1, idUsuario2,
            resultado => idPartida = resultado));

        // 2. Inicializar el juego con el ID obtenido
        juego = new BaseJuego();
        juego.JuegoBase(idUsuario1, idUsuario2, idPartida);

        yield return StartCoroutine(IniciarJuegoConHabilidades(juego, idUsuario1, idUsuario2));

        CrearPrefabs();

        StartCoroutine(BucleConEspera());
    }

    IEnumerator IniciarJuegoConHabilidades(BaseJuego juego, int idUsuario1, int idUsuario2)
    {
        // Inicializar usuarios si no existen
        if (usuario1 == null) usuario1 = new Usuario();
        if (usuario2 == null) usuario2 = new Usuario();

        if (idUsuario1 <= 4)
        {
            switch (idUsuario1)
            {
                case 1: iaB = new IAAleatoria(); iaB.InicializarPiezasDeIA(juego, ColorPieza.Blanco); break;
                case 2: iaB = new IABasica(); iaB.InicializarPiezasDeIA(juego, ColorPieza.Blanco); break;
                case 3: iaB = new IAMedia(); iaB.InicializarPiezasDeIA(juego, ColorPieza.Blanco); break;
                case 4: iaB = new IAAvanzada(); iaB.InicializarPiezasDeIA(juego, ColorPieza.Blanco); break;
                default:
                    break;
            }
        }
        else
        {
            yield return StartCoroutine(HabilidadesUsuarioBD.GetHabilidadesUsuario(idUsuario1));

            List<DatosHabilidadUsuario> habilidades = HabilidadesUsuarioBD.ObtenerListaHabilidadesUsuario();
            usuario1.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, habilidades, idUsuario1);
        }

        if (idUsuario2 <= 4)
        {
            switch (idUsuario2)
            {
                case 1: iaN = new IAAleatoria(); iaN.InicializarPiezasDeIA(juego, ColorPieza.Negro); break;
                case 2: iaN = new IABasica(); iaN.InicializarPiezasDeIA(juego, ColorPieza.Negro); break;
                case 3: iaN = new IAMedia(); iaN.InicializarPiezasDeIA(juego, ColorPieza.Negro); break;
                case 4: iaN = new IAAvanzada(); iaN.InicializarPiezasDeIA(juego, ColorPieza.Negro); break;
                default:
                    break;
            }
        }
        else
        {
            yield return StartCoroutine(HabilidadesUsuarioBD.GetHabilidadesUsuario(idUsuario2));

            List<DatosHabilidadUsuario> habilidades = HabilidadesUsuarioBD.ObtenerListaHabilidadesUsuario();
            usuario2.InicializarPiezasDeUsuario(juego, ColorPieza.Negro, habilidades, idUsuario2);
        }

    }

    private IEnumerator BucleConEspera()
    {
        bool movimientoUsuarioRealizado = false;
        int NumeroDeTurno = 1;
        ColorPieza turno;
        Accion accionIa;
        movimientoUsuarioRealizado = false;

        while (true)
        {
            turno = juego.TurnoActual;

            iaActual = turno == ColorPieza.Blanco ? iaB : iaN;

            if (iaActual != null)
            {
                // ── TURNO DE IA ──
                accionIa = iaActual.ElegirMovimiento(juego, turno);

                if (accionIa.Pieza == null)
                {
                    // Jaque Mate
                    break;
                }

                if (accionIa.PiezaEliminada != null) EliminarPiezaVisual(accionIa.PiezaEliminada);

                int xOrigen = accionIa.Pieza.Posicion.X;
                int yOrigen = accionIa.Pieza.Posicion.Y;

                if (accionIa.Tipo == TipoAccion.Empujon)
                {
                    juego.EjecutarEmpujon(accionIa.Pieza, accionIa.PiezaEmpujada, accionIa.XFin, accionIa.YFin);
                }
                else
                {
                    juego.RealizarMovimiento(xOrigen, yOrigen, accionIa.XFin, accionIa.YFin);
                }


                SincronizarVisual();

                // Registrar en BD
                int idUsuario = turno == ColorPieza.Blanco ? juego.IdUsuario1 : juego.IdUsuario2;
                mapaRegistroMovimiento[NumeroDeTurno] = new Estructuras
                {
                    p_id_partida = juego.IdPartida,
                    p_id_usuario = idUsuario,
                    p_id_pieza = accionIa.Pieza.Id,
                    p_turno_numero = NumeroDeTurno,
                    p_x_origen = xOrigen,
                    p_y_origen = yOrigen,
                    p_x_fin = accionIa.XFin,
                    p_y_fin = accionIa.YFin,
                    p_id_habilidad_usada = (int)accionIa.Pieza.Habilidad.TipoHabilidad,
                    p_id_pieza_empujada = accionIa.PiezaEmpujada?.Id,
                    p_x_origen_empujada = accionIa.PiezaEmpujada?.Posicion.X,
                    p_y_origen_empujada = accionIa.PiezaEmpujada?.Posicion.Y,
                    p_x_fin_empujada = accionIa.PiezaEmpujada?.Id != null ? accionIa.XFin : null,
                    p_y_fin_empujada = accionIa.PiezaEmpujada?.Id != null ? accionIa.YFin : null
                };

                Debug.Log("Pieza movida por IA: " + accionIa.Pieza.Tipo);
                yield return new WaitForSeconds(2);
            }
            else
            {
                // ── TURNO DE USUARIO ──

                // Esperar hasta que el usuario haga su movimiento
                yield return new WaitUntil(() => movimientoUsuarioRealizado);

                SincronizarVisual();

                int idUsuario = turno == ColorPieza.Blanco ? juego.IdUsuario1 : juego.IdUsuario2;
                //mapaRegistroMovimiento[NumeroDeTurno] = new Estructuras
                //{
                //    p_id_partida = juego.IdPartida,
                //    p_id_usuario = idUsuario,
                //    p_id_pieza = accionIa.Pieza.Id,
                //    p_turno_numero = NumeroDeTurno,
                //    p_x_origen = xOrigen,
                //    p_y_origen = yOrigen,
                //    p_x_fin = accionIa.XFin,
                //    p_y_fin = accionIa.YFin,
                //    p_id_habilidad_usada = (int)accionIa.Pieza.Habilidad.TipoHabilidad,
                //    p_id_pieza_empujada = accionIa.PiezaEmpujada?.Id,
                //    p_x_origen_empujada = accionIa.PiezaEmpujada?.Posicion.X,
                //    p_y_origen_empujada = accionIa.PiezaEmpujada?.Posicion.Y,
                //    p_x_fin_empujada = accionIa.PiezaEmpujada?.Id != null ? accionIa.XFin : null,
                //    p_y_fin_empujada = accionIa.PiezaEmpujada?.Id != null ? accionIa.YFin : null
                //};

                Debug.Log("Movimiento del usuario completado");
            }
            NumeroDeTurno++;
        }

        // Registrar resultado final en BD, mostrar mensaje de victoria, registrar movimientos realizados tipo dicionario etc.
    }

    void Start()
    {
        int idB = 0;
        int idN = 0;

        // --- DETECTAR SI VENIMOS DEL LOBBY MULTIJUGADOR ---
        // El Lobby guarda estos datos antes de cambiar de escena
        bool esMultijugador = PlayerPrefs.HasKey("LocalPlayerId") && PlayerPrefs.GetInt("LocalPlayerId", 0) > 0;

        if (esMultijugador)
        {
            int idJ1 = PlayerPrefs.GetInt("IdJugador1", 0);
            int idJ2 = PlayerPrefs.GetInt("IdJugador2", 0);
            int idPartidaExistente = PlayerPrefs.GetInt("IdPartida", -1);

            Debug.LogWarning($"[DEBUG_PIEZAS] MULTIJUGADOR DETECTADO: J1={idJ1}, J2={idJ2}, Partida={idPartidaExistente}");

            if (idJ1 == 0 || idJ2 == 0)
            {
                Debug.LogError("[DEBUG_PIEZAS] ERROR: Los IDs en PlayerPrefs son 0. ¿Se guardaron bien en el Lobby?");
            }
            if (random.Next(2) == 1) { idB = idJ1; idN = idJ2; }
            else { idN = idJ1; idB = idJ2; }

            StartCoroutine(IniciarPartida(idB, idN, idPartidaExistente));
        }
        else
        {
            int idIA = ConfigPartida.idIA;
            int idJugador = ConfigPartida.idJugador1;
            if (random.Next(2) == 1) { idB = idIA; idN = idJugador; }
            else { idN = idIA; idB = idJugador; }

            StartCoroutine(IniciarPartida(idB, idN, 0));
        }
        // --- Suscribirse a movimientos remotos TCP ---
        chessSyncManager = FindObjectOfType<ChessGameSyncManager>();
        if (chessSyncManager != null)
        {
            chessSyncManager.OnMoveReceived += AplicarMovimientoRemoto;
            Debug.Log("[CrearPiezas] Suscrito a movimientos remotos TCP");
        }
        else
        {
            Debug.LogWarning("[CrearPiezas] ChessGameSyncManager no encontrado, los movimientos remotos no se aplicarán");
        }
    }

    private void OnDestroy()
    {
        if (chessSyncManager != null)
        {
            chessSyncManager.OnMoveReceived -= AplicarMovimientoRemoto;
        }
    }

    /// <summary>
    /// Aplica un movimiento recibido del oponente remoto por TCP.
    /// Ejecuta la lógica del juego y sincroniza la vista visual.
    /// </summary>
    private void AplicarMovimientoRemoto(ChessGameSyncManager.ChessMoveSync move)
    {
        if (juego == null)
        {
            Debug.LogError("[CrearPiezas] juego es null, no se puede aplicar movimiento remoto");
            return;
        }

        Debug.LogWarning($"[CrearPiezas] Aplicando movimiento remoto: ({move.XOrigen},{move.YOrigen}) -> ({move.XFin},{move.YFin})");

        // Comprobar si hay una pieza enemiga en la casilla destino para eliminarla visualmente
        var casillaDestino = juego.Tablero.Grid[move.XFin, move.YFin];
        if (casillaDestino != null && casillaDestino.Pieza != null)
        {
            EliminarPiezaVisual(casillaDestino.Pieza);
        }

        // Ejecutar el movimiento en la lógica del juego
        bool exito = juego.RealizarMovimiento(move.XOrigen, move.YOrigen, move.XFin, move.YFin);

        if (exito)
        {
            Debug.Log($"[CrearPiezas] Movimiento remoto aplicado exitosamente");
            // Notificar que el turno del oponente (remoto) ha terminado
            NotificarMovimientoUsuario();
        }
        else
        {
            Debug.LogError($"[CrearPiezas] El movimiento remoto ({move.XOrigen},{move.YOrigen})->({move.XFin},{move.YFin}) fue rechazado por la lógica");
        }

        SincronizarVisual();
    }

    void CrearPrefabs()
    {
        Debug.Log($"[DEBUG_PIEZAS] Instanciando {juego.ListaPiezas.Count} prefabs...");

        if (peonPrefab == null) Debug.LogError("[DEBUG_PIEZAS] ERROR: peonPrefab no está asignado en el Inspector.");

        foreach (var piezaLogica in juego.ListaPiezas)
        {
            GameObject prefab = ObtenerPrefab(piezaLogica.Tipo, piezaLogica.Color);
            if (prefab == null)
            {
                Debug.LogError($"[DEBUG_PIEZAS] No se encontró prefab para {piezaLogica.Tipo}");
                continue;
            }

            GameObject piezaObj = Instantiate(prefab);
            // ... (resto igual)

            PiezasPrefab view = piezaObj.GetComponent<PiezasPrefab>();
            view.Inicializar(piezaLogica);

            mapaPiezas[piezaLogica] = view;
        }
    }

    public GameObject ObtenerPrefab(TipoPieza tipo, ColorPieza color)
    {
        switch (tipo)
        {
            case TipoPieza.Peon: return peonPrefab;
            case TipoPieza.Torre: return torrePrefab;
            case TipoPieza.Caballo: return caballoPrefab;
            case TipoPieza.Alfil: return alfilPrefab;
            case TipoPieza.Dama: return damaPrefab;
            case TipoPieza.Rey: return reyPrefab;
            default: return null;
        }
    }
    public void MoverVisual(Pieza pieza)
    {
        if (mapaPiezas.TryGetValue(pieza, out var view))
        {
            view.transform.position =
                new Vector3(pieza.Posicion.X, 0.5f, pieza.Posicion.Y);
        }
    }

    public void EliminarPiezaVisual(Pieza pieza)
    {
        if (mapaPiezas.TryGetValue(pieza, out var go))
        {
            Destroy(go);
            mapaPiezas.Remove(pieza);
        }
    }

    //public void IntentarMovimientoIA()
    //{
    //    if (!jugarContraIA) return;

    //    if (juego.TurnoActual != colorIA || ejecutandoIA) return;

    //    StartCoroutine(EjecutarIA());
    //}

    //IEnumerator EjecutarIA()
    //{
    //    ejecutandoIA = true;

    //    yield return new WaitForSeconds(0.5f);

    //    var accion = ia.ElegirMovimiento(juego, juego.TurnoActual);

    //    if (accion.Tipo == TipoAccion.Movimiento)
    //    {
    //        juego.RealizarMovimiento(
    //            accion.Pieza.Posicion.X,
    //            accion.Pieza.Posicion.Y,
    //            accion.XFin,
    //            accion.YFin
    //        );
    //    }
    //    else if (accion.Tipo == TipoAccion.Empujon)
    //    {
    //        juego.EjecutarEmpujon(
    //            accion.Pieza,
    //            accion.PiezaEmpujada,
    //            accion.XFin,
    //            accion.YFin
    //        );
    //    }

    //    SincronizarVisual();

    //    ejecutandoIA = false;

    //    // Por si hay m�s turnos IA
    //    IntentarMovimientoIA();
    //}

    public void SincronizarVisual()
    {
        foreach (var kvp in mapaPiezas)
        {
            var pieza = kvp.Key;
            var view = kvp.Value;

            view.transform.position =
                new Vector3(pieza.Posicion.X, 0.5f, pieza.Posicion.Y);
        }
    }
}