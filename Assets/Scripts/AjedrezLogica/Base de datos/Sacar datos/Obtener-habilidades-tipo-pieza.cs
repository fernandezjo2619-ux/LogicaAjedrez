using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Llama a la función RPC de Supabase:
///   · obtener_habilidades_tipo_pieza → devuelve todas las habilidades
///     asociadas a un tipo de pieza, ordenadas por poder_base DESC.
/// </summary>
public class ObtenerHabilidadesTipoPieza : MonoBehaviour
{
    private string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
    private string apiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";

    void Start()
    {
        // ── Ejemplos de uso ──────────────────────────────────────────────
        // Habilidades del tipo de pieza con id = 1:
        StartCoroutine(GetHabilidadesTipoPieza(1));

        // Habilidades del tipo de pieza con id = 3:
        // StartCoroutine(GetHabilidadesTipoPieza(3));
        // ────────────────────────────────────────────────────────────────
    }

    // ================================================================
    //  obtener_habilidades_tipo_pieza
    //  Devuelve todas las habilidades asociadas al tipo de pieza dado,
    //  ordenadas por poder_base de mayor a menor.
    //  Lanza excepción en Supabase si el id_tipo no existe.
    //
    //  Equivalente SQL:
    //      SELECT * FROM obtener_habilidades_tipo_pieza(1);
    // ================================================================
    /// <summary>
    /// Llama a la función SQL obtener_habilidades_tipo_pieza y loguea el JSON resultante.
    /// </summary>
    /// <param name="idTipo">ID del tipo de pieza cuyas habilidades se quieren consultar.</param>
    public IEnumerator GetHabilidadesTipoPieza(int idTipo)
    {
        string url      = baseUrl + "/obtener_habilidades_tipo_pieza";
        string jsonBody = "{\"p_id_tipo\": " + idTipo + "}";

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
            Debug.Log("[ObtenerHabilidadesTipoPieza] Habilidades del tipo de pieza " + idTipo + ":");
            Debug.Log(json);

            // Aquí puedes parsear el JSON con JsonUtility o Newtonsoft.Json.
            // Nota: si el id_tipo no existe, Supabase devolverá un error 400/500
            // con el mensaje de la excepción definida en la función SQL.
        }
        else
        {
            Debug.LogError("[ObtenerHabilidadesTipoPieza] Error al obtener habilidades del tipo " + idTipo + ":");
            Debug.LogError(request.error);
            Debug.LogError(request.downloadHandler.text);
        }
    }
}
