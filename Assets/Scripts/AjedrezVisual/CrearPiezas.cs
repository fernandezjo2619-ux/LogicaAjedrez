using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AjedrezLogica;
using AjedrezLogica.Recursos;
using AjedrezLogica.IA;
using AjedrezLogica.IA.Estructuras;

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
    public IMotorIA? iaB;
    public IMotorIA? iaN;

    //Guardar pieza y posicion
    //public static GameObject[,] piezasVisuales = new GameObject[8, 8];

    public Dictionary<Pieza, PiezasPrefab> mapaPiezas = new();
    private ObtenerHabilidadesUsuario ObHabilidadesBD = new();
    private SupabaseRPC GuardarPartidaBD = new();

    void Awake()
    {
        Instance = this;
    }

    private IEnumerator IniciarPartidaAsync(int idUsuario1, int idUsuario2)
    {
        // 1. Guardar partida en BD y obtener el ID
        int idPartida = 0;
        yield return StartCoroutine(GuardarPartidaBD.GuardarPartida(idUsuario1, idUsuario2,
            resultado => idPartida = resultado));

        // 2. Inicializar el juego con el ID obtenido
        juego = new BaseJuego();
        juego.JuegoBase(idUsuario1, idUsuario2, idPartida);

        yield return StartCoroutine(IniciarJuegoConHabilidades(juego, idUsuario1, idUsuario2));

        CrearPrefabs();

        yield return StartCoroutine(BucleConEspera());
    }

    private IEnumerator IniciarJuegoConHabilidades(BaseJuego juego, int idUsuario1, int idUsuario2)
    {
        if (idUsuario1 >= 1 && idUsuario1 <= 3) // Es una IA
        {
            switch (idUsuario1)
            {
                case 1:
                    iaB = new IAAleatoria(); // Aleatoria Nivel 1
                    iaB.InicializarPiezasDeIA(juego, ColorPieza.Blanco);
                    break;
                case 2:
                    iaB = new IABasica(); // Basica Nivel 2
                    iaB.InicializarPiezasDeIA(juego, ColorPieza.Blanco);
                    break;
                case 3:
                    iaB = new IAMedia(); // Media Nivel 3
                    iaB.InicializarPiezasDeIA(juego, ColorPieza.Blanco);
                    break;
                case 4:
                    iaB = new IAAvanzada(); // Avanzada Nivel 4
                    iaB.InicializarPiezasDeIA(juego, ColorPieza.Blanco);
                    break;
            }
        }
        else
        {
            // Esperar a que se carguen las habilidades
            yield return StartCoroutine(ObHabilidadesBD.GetHabilidadesUsuario(idUsuario1));

            List<DatosHabilidadUsuario> habilidades = ObHabilidadesBD.ObtenerListaHabilidadesUsuario();
            usuario1 = new Usuario();
            usuario1.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, habilidades, idUsuario1);
        }

        if (idUsuario2 >= 1 && idUsuario2 <= 3) // Es una IA
        {
            switch (idUsuario1)
            {
                case 1:
                    iaN = new IAAleatoria(); // Aleatoria Nivel 1
                    iaN.InicializarPiezasDeIA(juego, ColorPieza.Negro);
                    break;
                case 2:
                    iaN = new IABasica(); // Basica Nivel 2
                    iaN.InicializarPiezasDeIA(juego, ColorPieza.Negro);
                    break;
                case 3:
                    iaN = new IAMedia(); // Media Nivel 3
                    iaN.InicializarPiezasDeIA(juego, ColorPieza.Negro);
                    break;
                case 4:
                    iaN = new IAAvanzada(); // Avanzada Nivel 4
                    iaN.InicializarPiezasDeIA(juego, ColorPieza.Negro);
                    break;
            }
        }
        else
        {
            // Esperar a que se carguen las habilidades
            yield return StartCoroutine(ObHabilidadesBD.GetHabilidadesUsuario(idUsuario2));

            List<DatosHabilidadUsuario> habilidades = ObHabilidadesBD.ObtenerListaHabilidadesUsuario();
            usuario2 = new Usuario();
            usuario2.InicializarPiezasDeUsuario(juego, ColorPieza.Negro, habilidades, idUsuario2);
        }
    }

    private IEnumerator BucleConEspera()
    {
        while (true)
        {
            // Tu código aquí
            Accion accionIa = iaB.ElegirMovimiento(juego, juego.TurnoActual);
            if (accionIa.Tipo == TipoAccion.Empujon)
            {
                juego.EjecutarEmpujon(accionIa.Pieza, accionIa.PiezaEmpujada, accionIa.XFin, accionIa.YFin);
            }
            else
            {
                juego.RealizarMovimiento(accionIa.Pieza.Posicion.X, accionIa.Pieza.Posicion.Y, accionIa.XFin, accionIa.YFin);
            }
            yield return new WaitForSeconds(5); // Espera X segundos
        }
    }

    void Start()
    {
        // Inicializa la lógica
        // Inicializar piezas de usuarios
        // IDs 1, 2 y 3 ... Reservadas para IA

        // Agregar logica para decidir que IA usar segun el nivel del usuario, y si se usa IA
        StartCoroutine(IniciarPartidaAsync(1, 2));

        //usuario1.InicializarPiezasDeUsuario(juego, ColorPieza.Blanco, 1);
        //usuario2.InicializarPiezasDeUsuario(juego, ColorPieza.Negro, 2);

        /*
        //Incializar IA
        ia = new IAAleatoria();
        ia.InicializarPiezasDeIA(juego, ColorPieza.Blanco);
        ia.InicializarPiezasDeIA(juego, ColorPieza.Negro);
        */
        // StartCoroutine(BucleConEspera());

        // Crear prefabs visuales
        // CrearPrefabs();
    }

    void CrearPrefabs()
    {
        foreach (var piezaLogica in juego.ListaPiezas)
        {
            GameObject prefab = ObtenerPrefab(piezaLogica.Tipo, piezaLogica.Color);
            GameObject piezaObj = Instantiate(prefab);

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
}