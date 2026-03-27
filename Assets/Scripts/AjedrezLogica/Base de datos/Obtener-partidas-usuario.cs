using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Llama a las funciones RPC de Supabase relacionadas con las partidas de un usuario:
///   · obtener_partidas_usuario  → todas las partidas de un usuario
/// </summary>
public class ObtenerPartidasUsuario : MonoBehaviour
{
    private string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
    private string apiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";

    void Start()
    {
        // ── Ejemplos de uso ──────────────────────────────────────────────
        // Todas las partidas del usuario 7:
        StartCoroutine(GetPartidasUsuario(7));

        // Solo partidas finalizadas (filtro en cliente tras recibir JSON):
        // StartCoroutine(GetPartidasUsuario(7, filtroEstado: "FINALIZADA"));

        // Solo victorias:
        // StartCoroutine(GetPartidasUsuario(7, filtroResultado: "VICTORIA"));
        // ────────────────────────────────────────────────────────────────
    }

    // ================================================================
    //  obtener_partidas_usuario
    //  Devuelve todas las partidas en las que participó el usuario,
    //  con su rol, resultado (VICTORIA / DERROTA / EN_CURSO) y duración.
    //
    //  Equivalente SQL:
    //      SELECT * FROM obtener_partidas_usuario(7);
    //      SELECT * FROM obtener_partidas_usuario(7) WHERE estado = 'FINALIZADA';
    //      SELECT * FROM obtener_partidas_usuario(7) WHERE resultado = 'VICTORIA';
    // ================================================================
    /// <summary>
    /// Llama a la función SQL obtener_partidas_usuario y loguea el JSON resultante.
    /// </summary>
    /// <param name="idUsuario">ID del usuario cuyas partidas se quieren consultar.</param>
    /// <param name="filtroEstado">Si se indica, el resultado se filtra por estado (EN_CURSO / FINALIZADA). Null = sin filtro.</param>
    /// <param name="filtroResultado">Si se indica, el resultado se filtra por resultado (VICTORIA / DERROTA / EN_CURSO). Null = sin filtro.</param>
    public IEnumerator GetPartidasUsuario(int idUsuario, string filtroEstado = null, string filtroResultado = null)
    {
        string url      = baseUrl + "/obtener_partidas_usuario";
        string jsonBody = "{\"p_id_usuario\": " + idUsuario + "}";

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
            Debug.Log("[ObtenerPartidasUsuario] Respuesta recibida para usuario " + idUsuario + ":");
            Debug.Log(json);

            // Aquí puedes parsear el JSON con JsonUtility o Newtonsoft.Json
            // y filtrar por filtroEstado / filtroResultado si lo necesitas en cliente.
        }
        else
        {
            Debug.LogError("[ObtenerPartidasUsuario] Error al obtener partidas del usuario " + idUsuario + ":");
            Debug.LogError(request.error);
            Debug.LogError(request.downloadHandler.text);
        }
    }
}
