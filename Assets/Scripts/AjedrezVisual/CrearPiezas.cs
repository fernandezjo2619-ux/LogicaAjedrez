<<<<<<< HEAD

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
=======
using AjedrezLogica;
using AjedrezLogica.IA;
using AjedrezLogica.IA.Estructuras;
using AjedrezLogica.Recursos;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static MenuInicio;
>>>>>>> origin/Raquel

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
<<<<<<< HEAD
    public IMotorIA iaActual;

    public System.Random random = new System.Random();

    //Activar IA
    //public bool jugarContraIA = false;
    //public ColorPieza colorIA = ColorPieza.Negro;
    //private bool ejecutandoIA = false;
=======

    //Activar IA
    public bool jugarContraIA = false;
    public ColorPieza colorIA = ColorPieza.Negro;
    private bool ejecutandoIA = false;
>>>>>>> origin/Raquel

    //Guardar pieza y posicion
    //public static GameObject[,] piezasVisuales = new GameObject[8, 8];

    public Dictionary<Pieza, PiezasPrefab> mapaPiezas = new();
<<<<<<< HEAD
    public Dictionary<int, Estructuras> mapaRegistroMovimiento = new();

    // Instancias de MonoBehaviour para acceso a métodos coroutine
    private SupabaseRPC GuardarPartidaBD;
    private ObtenerHabilidadesUsuario HabilidadesUsuarioBD;
    private RegistrarMovimiento registrarMovimientoDb;
    private bool movimientoUsuarioRealizado = false;

    // Almacena temporalmente los datos del movimiento del usuario o remoto para guardarlos al final del turno
    public Estructuras ultimoMovimientoUsuarioDatos = null;
    private ChessGameSyncManager chessSyncManager;

    public void NotificarMovimientoUsuario()
    {
        movimientoUsuarioRealizado = true;
    }

    /// <summary>
    /// Guarda los datos del movimiento antes de que termine el turno para agregarlos a mapaRegistroMovimiento
    /// </summary>
    public void RegistrarMovimientoUsuario(Estructuras datos)
    {
        ultimoMovimientoUsuarioDatos = datos;
    }
=======

    private SupabaseRPC GuardarPartidaBD = new();
    private ObtenerHabilidadesUsuario HabilidadesUsuarioBD = new();
    private RegistrarMovimiento registrarMovimientoDb = new();
>>>>>>> origin/Raquel

    void Awake()
    {
        Instance = this;
<<<<<<< HEAD

        // Obtener o crear componentes necesarios
        GuardarPartidaBD = GetComponent<SupabaseRPC>() ?? gameObject.AddComponent<SupabaseRPC>();
        HabilidadesUsuarioBD = GetComponent<ObtenerHabilidadesUsuario>() ?? gameObject.AddComponent<ObtenerHabilidadesUsuario>();
        registrarMovimientoDb = GetComponent<RegistrarMovimiento>() ?? gameObject.AddComponent<RegistrarMovimiento>();
    }

    private IEnumerator IniciarPartida(int idUsuario1, int idUsuario2, int idPartidaExistente)
    {
        // 1. Guardar partida en BD y obtener el ID
        int idPartida = idPartidaExistente > 0 ? idPartidaExistente : 0;
=======
    }

    private IEnumerator IniciarPartida(int idUsuario1, int idUsuario2)
    {
        // 1. Guardar partida en BD y obtener el ID
        int idPartida = 0;
>>>>>>> origin/Raquel
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
<<<<<<< HEAD
        // Inicializar usuarios si no existen
        if (usuario1 == null) usuario1 = new Usuario();
        if (usuario2 == null) usuario2 = new Usuario();

        bool esMultijugador = !ConfigPartida.vsIA;

        if (!esMultijugador && idUsuario1 <= 4)
=======
        if (idUsuario1 <= 4)
>>>>>>> origin/Raquel
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

<<<<<<< HEAD
        if (!esMultijugador && idUsuario2 <= 4)
=======
        if (idUsuario2 <= 4)
>>>>>>> origin/Raquel
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
<<<<<<< HEAD
            usuario2.InicializarPiezasDeUsuario(juego, ColorPieza.Negro, habilidades, idUsuario2);
        }
=======
            usuario2.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, habilidades, idUsuario2);
        }

>>>>>>> origin/Raquel
    }

    private IEnumerator BucleConEspera()
    {
<<<<<<< HEAD
        int NumeroDeTurno = 1;
        ColorPieza turno;
        Accion accionIa;
        this.movimientoUsuarioRealizado = false;

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
                yield return new WaitUntil(() => this.movimientoUsuarioRealizado);
                this.movimientoUsuarioRealizado = false;

                SincronizarVisual();

                int idUsuario = turno == ColorPieza.Blanco ? juego.IdUsuario1 : juego.IdUsuario2;
                
                if (ultimoMovimientoUsuarioDatos != null)
                {
                    ultimoMovimientoUsuarioDatos.p_id_partida = juego.IdPartida;
                    ultimoMovimientoUsuarioDatos.p_id_usuario = idUsuario;
                    ultimoMovimientoUsuarioDatos.p_turno_numero = NumeroDeTurno;
                    mapaRegistroMovimiento[NumeroDeTurno] = ultimoMovimientoUsuarioDatos;
                    ultimoMovimientoUsuarioDatos = null;
                }

                Debug.Log("Movimiento del usuario completado");
            }
            NumeroDeTurno++;

            // --- COMPROBAR CONDICIÓN DE VICTORIA (REY CAPTURADO) ---
            bool reyBlancoVivo = false;
            bool reyNegroVivo = false;
            foreach (var p in juego.ListaPiezas)
            {
                if (p == null) continue;

                if (p.Tipo == TipoPieza.Rey)
                {
                    if (p.Color == ColorPieza.Blanco) reyBlancoVivo = true;
                    if (p.Color == ColorPieza.Negro) reyNegroVivo = true;
                }
            }

            if (!reyBlancoVivo || !reyNegroVivo)
            {
                int idGanador = !reyBlancoVivo ? juego.IdUsuario2 : juego.IdUsuario1;
                Debug.Log($"[PARTIDA] ¡El Rey ha sido capturado! Ganador: Jugador {idGanador}");
                
                // Guardar el ID del ganador para la siguiente escena
                PlayerPrefs.SetInt("IdGanador", idGanador);
                PlayerPrefs.Save();
                
                // Iniciar proceso de guardado y carga de la escena de victoria
                StartCoroutine(FinalizarYGuardarPartida(idGanador));
                break;
            }
        }
    }

    private IEnumerator FinalizarYGuardarPartida(int idGanador)
    {
        Debug.Log("[PARTIDA] Enviando movimientos a Supabase...");
        
        // 1. Enviar cada movimiento secuencialmente
        foreach (var kvp in mapaRegistroMovimiento)
        {
            var mov = kvp.Value;
            bool movimientoGuardado = false;
            
            yield return StartCoroutine(registrarMovimientoDb.PostRegistrarMovimiento(
                p_id_partida: mov.p_id_partida,
                p_id_usuario: mov.p_id_usuario,
                p_id_pieza: mov.p_id_pieza,
                p_turno_numero: mov.p_turno_numero,
                p_x_origen: mov.p_x_origen,
                p_y_origen: mov.p_y_origen,
                p_x_fin: mov.p_x_fin,
                p_y_fin: mov.p_y_fin,
                p_id_habilidad_usada: mov.p_id_habilidad_usada,
                p_id_pieza_empujada: mov.p_id_pieza_empujada,
                p_x_origen_empujada: mov.p_x_origen_empujada,
                p_y_origen_empujada: mov.p_y_origen_empujada,
                p_x_fin_empujada: mov.p_x_fin_empujada,
                p_y_fin_empujada: mov.p_y_fin_empujada,
                onSuccess: (id) => movimientoGuardado = true,
                onError: (err) => movimientoGuardado = true // Si falla, continuamos para no bloquear
            ));
            
            // Wait until this move is processed
            yield return new WaitUntil(() => movimientoGuardado);
        }

        Debug.Log("[PARTIDA] Todos los movimientos enviados. Actualizando estado de la partida...");

        // 2. Actualizar estado de la partida a FINALIZADA
        bool partidaActualizada = false;
        yield return StartCoroutine(SupabaseRPC.ActualizarPartida(juego.IdPartida, "FINALIZADA", idGanador, (exito) => 
        {
            partidaActualizada = true;
        }));
        
        yield return new WaitUntil(() => partidaActualizada);

        Debug.Log("[PARTIDA] Proceso completado. Cargando escena Victoria.");
        
        // 3. Cargar la escena de victoria
        UnityEngine.SceneManagement.SceneManager.LoadScene("Victoria");
    }

    void Start()
    {
        int idB = 0;
        int idN = 0;

        // --- DETECTAR SI VENIMOS DEL LOBBY MULTIJUGADOR ---
        bool esMultijugador = !ConfigPartida.vsIA;

        if (esMultijugador)
        {
            int idJ1 = PlayerPrefs.GetInt("IdJugador1", 0);
            int idJ2 = PlayerPrefs.GetInt("IdJugador2", 0);
            int idPartidaExistente = PlayerPrefs.GetInt("IdPartida", 0);

            if (idJ1 == 0 || idJ2 == 0)
            {
                Debug.LogWarning("[CrearPiezas] Faltan IDs en PlayerPrefs. Usando IDs por defecto 1 y 2.");
                idJ1 = 1;
                idJ2 = 2;
            }
            
            // IMPORTANTE: En multijugador NO podemos hacer un random independiente en cada cliente
            // porque podrían quedar con colores invertidos. El Host (idJ1) siempre será Blanco y el Cliente (idJ2) Negro.
            idB = idJ1;
            idN = idJ2;

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
        chessSyncManager = ChessGameSyncManager.Instance;
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

        // Identificar la pieza antes de que se mueva
        Pieza piezaRemota = juego.ListaPiezas.Find(p => p != null && p.Posicion.X == move.XOrigen && p.Posicion.Y == move.YOrigen);
        int idPiezaRemota = piezaRemota != null ? piezaRemota.Id : 0;
        int? idHabilidad = piezaRemota?.Habilidad != null ? (int?)piezaRemota.Habilidad.TipoHabilidad : null;

        // Ejecutar el movimiento en la lógica del juego
        bool exito = juego.RealizarMovimiento(move.XOrigen, move.YOrigen, move.XFin, move.YFin);

        if (exito)
        {
            Debug.Log($"[CrearPiezas] Movimiento remoto aplicado exitosamente");
            
            // Guardar datos para registro
            RegistrarMovimientoUsuario(new Estructuras {
                p_id_pieza = idPiezaRemota,
                p_x_origen = move.XOrigen,
                p_y_origen = move.YOrigen,
                p_x_fin = move.XFin,
                p_y_fin = move.YFin,
                p_id_habilidad_usada = idHabilidad
            });

            // Notificar que el turno del oponente (remoto) ha terminado
            NotificarMovimientoUsuario();
        }
        else
        {
            Debug.LogError($"[CrearPiezas] El movimiento remoto ({move.XOrigen},{move.YOrigen})->({move.XFin},{move.YFin}) fue rechazado por la lógica");
        }

        SincronizarVisual();
=======
        int idUsuario;
        while (true)
        {
            // Tu c�digo aqu�
            Accion accionIa = iaB.ElegirMovimiento(juego, juego.TurnoActual);
            if (accionIa.Tipo == TipoAccion.Empujon)
            {
                juego.EjecutarEmpujon(accionIa.Pieza, accionIa.PiezaEmpujada, accionIa.XFin, accionIa.YFin);
            }
            else
            {
                juego.RealizarMovimiento(accionIa.Pieza.Posicion.X, accionIa.Pieza.Posicion.Y, accionIa.XFin, accionIa.YFin);
            }
            // forma de conseguir la pieza
            MoverVisual(accionIa.Pieza);
            // idUsuario = juego.TurnoActual == ColorPieza.Blanco ? juego.IdUsuario1 : juego.IdUsuario2;
            // registrarMovimientoDb.PostRegistrarMovimiento(juego.IdPartida, idUsuario, pieza.Id, xOrigen, yOrigen, xFin, yFin, piezaEmpujada.Id, xOrigenEmpujada, yOrigenEmpujada, xFinEmpujada, yFinEmpujada);

            Debug.Log("pieza movida por la ia " + accionIa.Pieza);
            yield return new WaitForSeconds(5); 
        }
    }

    void Start()
    {

        // IDs 1 al 4, Reservados para la IA
        StartCoroutine(IniciarPartida(1,2));
        //jugarContraIA = ConfigPartida.vsIA;

        //// Inicializa la l�gica
        //juego = new BaseJuego();

        //usuario1 = new Usuario();
        //usuario2 = new Usuario();


        //if (jugarContraIA)
        //{
        //    usuario1.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, 1);

        //    // Inicializar piezas de IA
        //    ia = new IAAleatoria();
        //    ia.InicializarPiezasDeIA(juego, ColorPieza.Negro);
        //}
        //else
        //{
        //    // Inicializar piezas de usuarios
        //    usuario1.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, 1);
        //    usuario2.InicializarPiezasDeUsuario(juego, ColorPieza.Negro, 2);
        //}

        //// Crear prefabs visuales
        //CrearPrefabs();
>>>>>>> origin/Raquel
    }

    void CrearPrefabs()
    {
<<<<<<< HEAD
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
=======
        foreach (var piezaLogica in juego.ListaPiezas)
        {
            GameObject prefab = ObtenerPrefab(piezaLogica.Tipo, piezaLogica.Color);
            GameObject piezaObj = Instantiate(prefab);
>>>>>>> origin/Raquel

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

<<<<<<< HEAD
    public void EliminarPiezaVisual(Pieza pieza)
    {
        Debug.Log("pieza eliminada");

        if (mapaPiezas.TryGetValue(pieza, out var go))
        {
            Destroy(go.gameObject); // ¡Importante! Destroy(go) solo borraba el script, Destroy(go.gameObject) borra el modelo 3D entero.
            mapaPiezas.Remove(pieza);
        }
    }


    public void SincronizarVisual()
    {
        // 1. Identificar piezas que ya no existen en la lógica o superpuestas
        List<Pieza> piezasAEliminar = new List<Pieza>();
        
        // 1.5. Hack de superposición: si dos piezas están en la misma coordenada, una debe morir
        Dictionary<string, Pieza> casillasOcupadas = new Dictionary<string, Pieza>();

        foreach (var kvp in mapaPiezas)
        {
            Pieza p = kvp.Key;
            
            // Si la lógica la borró de ListaPiezas, la eliminamos
            if (!juego.ListaPiezas.Contains(p) || kvp.Value == null)
            {
                piezasAEliminar.Add(p);
                continue;
            }

            // Detección de colisión visual (dos piezas en el mismo sitio)
            string coords = $"{p.Posicion.X},{p.Posicion.Y}";
            if (casillasOcupadas.ContainsKey(coords))
            {
                Pieza otra = casillasOcupadas[coords];
                // La pieza capturada será la del color contrario al turno actual
                if (p.Color != juego.TurnoActual)
                {
                    piezasAEliminar.Add(p);
                }
                else
                {
                    piezasAEliminar.Add(otra);
                    casillasOcupadas[coords] = p;
                }
            }
            else
            {
                casillasOcupadas[coords] = p;
            }
        }

        foreach (var pieza in piezasAEliminar)
        {
            EliminarPiezaVisual(pieza);
        }

        // 2. Reposicionar las piezas restantes
        foreach (var kvp in mapaPiezas)
        {
            if (kvp.Value != null && !piezasAEliminar.Contains(kvp.Key))
            {
                kvp.Value.transform.position = new Vector3(kvp.Key.Posicion.X, 0.5f, kvp.Key.Posicion.Y);
            }
=======
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
>>>>>>> origin/Raquel
        }
    }
}