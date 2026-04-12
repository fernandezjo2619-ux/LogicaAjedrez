using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Llama a las funciones RPC de Supabase relacionadas con los datos de una partida:
///   · obtener_datos_partida        → cabecera + jugadores de una partida
///   · obtener_movimientos_partida  → historial completo de movimientos
/// </summary>
public class ObtenerDatosPartida : MonoBehaviour
{
    private string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
    private string apiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";

    void Start()
    {
        // ── Ejemplos de uso ──────────────────────────────────────────────
        // Cabecera y jugadores de la partida 3:
        StartCoroutine(GetDatosPartida(3));

        // Historial completo de la partida 3:
        StartCoroutine(GetMovimientosPartida(3));

        // Movimientos de un jugador concreto dentro de la partida 3:
        // StartCoroutine(GetMovimientosPartida(3, filtroIdUsuario: 7));

        // Solo movimientos en los que se usó una habilidad:
        // StartCoroutine(GetMovimientosPartida(3, soloConHabilidad: true));
        // ────────────────────────────────────────────────────────────────
    }

    // ================================================================
    //  obtener_datos_partida
    //  Devuelve la cabecera de la partida (estado, fechas, ganador)
    //  y una fila por cada jugador con su rol.
    //
    //  Equivalente SQL:
    //      SELECT * FROM obtener_datos_partida(3);
    // ================================================================
    /// <summary>
    /// Llama a la función SQL obtener_datos_partida y loguea el JSON resultante.
    /// </summary>
    /// <param name="idPartida">ID de la partida a consultar.</param>
    public IEnumerator GetDatosPartida(int idPartida)
    {
        string url      = baseUrl + "/obtener_datos_partida";
        string jsonBody = "{\"p_id_partida\": " + idPartida + "}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey",        apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("[ObtenerDatosPartida] Cabecera + jugadores de la partida " + idPartida + ":");
            Debug.Log(json);

            // Aquí puedes parsear el JSON con JsonUtility o Newtonsoft.Json
        }
        else
        {
            Debug.LogError("[ObtenerDatosPartida] Error al obtener datos de la partida " + idPartida + ":");
            Debug.LogError(request.error);
            Debug.LogError(request.downloadHandler.text);
        }
    }

    // ================================================================
    //  obtener_movimientos_partida
    //  Devuelve el historial completo de movimientos de una partida,
    //  ordenados por turno y timestamp.
    //
    //  Equivalente SQL:
    //      SELECT * FROM obtener_movimientos_partida(3);
    //      SELECT * FROM obtener_movimientos_partida(3) WHERE id_usuario = 7;
    //      SELECT * FROM obtener_movimientos_partida(3) WHERE id_habilidad_usada IS NOT NULL;
    // ================================================================
    /// <summary>
    /// Llama a la función SQL obtener_movimientos_partida y loguea el JSON resultante.
    /// </summary>
    /// <param name="idPartida">ID de la partida cuyos movimientos se quieren consultar.</param>
    /// <param name="filtroIdUsuario">Si se indica, el resultado se filtra por el ID de usuario en cliente. Null = todos los jugadores.</param>
    /// <param name="soloConHabilidad">Si true, solo se muestran movimientos en los que se usó una habilidad.</param>
    public IEnumerator GetMovimientosPartida(int idPartida, int? filtroIdUsuario = null, bool soloConHabilidad = false)
    {
        string url      = baseUrl + "/obtener_movimientos_partida";
        string jsonBody = "{\"p_id_partida\": " + idPartida + "}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler   = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey",        apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;

            string etiqueta = "[ObtenerMovimientosPartida] Movimientos de la partida " + idPartida;
            if (filtroIdUsuario.HasValue)  etiqueta += " (usuario " + filtroIdUsuario.Value + ")";
            if (soloConHabilidad)          etiqueta += " (solo con habilidad)";

            Debug.Log(etiqueta + ":");
            Debug.Log(json);

            // Aquí puedes parsear el JSON con JsonUtility o Newtonsoft.Json
            // y aplicar los filtros filtroIdUsuario / soloConHabilidad en cliente.
        }
        else
        {
            Debug.LogError("[ObtenerMovimientosPartida] Error al obtener movimientos de la partida " + idPartida + ":");
            Debug.LogError(request.error);
            Debug.LogError(request.downloadHandler.text);
        }
    }
}
