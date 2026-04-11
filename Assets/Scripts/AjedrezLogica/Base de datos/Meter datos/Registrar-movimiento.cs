using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

/// <summary>
/// Llama a la función RPC registrar_movimiento de Supabase
/// </summary>
public class RegistrarMovimiento : MonoBehaviour
{
    private string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
    private string apiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";

    void Start()
    {
        // ── Ejemplo de uso ──────────────────────────────────────────────
        // StartCoroutine(PostRegistrarMovimiento(
        //     p_id_partida: 3, 
        //     p_id_usuario: 7, 
        //     p_id_pieza: 12, 
        //     p_turno_numero: 1, 
        //     p_x_origen: 0, p_y_origen: 1, 
        //     p_x_fin: 0, p_y_fin: 2,
        //     onSuccess: (id) => Debug.Log("Movimiento OK con ID: " + id),
        //     onError: (err) => Debug.LogError("Fallo: " + err)
        // ));
        // ────────────────────────────────────────────────────────────────
    }

    /// <summary>
    /// Registra un movimiento en Supabase llamando a la función registrar_movimiento.
    /// Devuelve el id del movimiento creado en el callback onSuccess.
    /// </summary>
    public IEnumerator PostRegistrarMovimiento(
        int p_id_partida,
        int p_id_usuario,
        int p_id_pieza,
        int p_turno_numero,
        int p_x_origen,
        int p_y_origen,
        int p_x_fin,
        int p_y_fin,
        int? p_id_habilidad_usada = null,
        int? p_id_pieza_empujada = null,
        int? p_x_origen_empujada = null,
        int? p_y_origen_empujada = null,
        int? p_x_fin_empujada = null,
        int? p_y_fin_empujada = null,
        System.Action<int> onSuccess = null,
        System.Action<string> onError = null)
    {
        string url = baseUrl + "/registrar_movimiento";

        // Construimos el JSON manualmente para manejar los valores opcionales (nulls) de forma correcta
        string jsonBody = "{";
        jsonBody += "\"p_id_partida\": " + p_id_partida + ",";
        jsonBody += "\"p_id_usuario\": " + p_id_usuario + ",";
        jsonBody += "\"p_id_pieza\": " + p_id_pieza + ",";
        jsonBody += "\"p_turno_numero\": " + p_turno_numero + ",";
        jsonBody += "\"p_x_origen\": " + p_x_origen + ",";
        jsonBody += "\"p_y_origen\": " + p_y_origen + ",";
        jsonBody += "\"p_x_fin\": " + p_x_fin + ",";
        jsonBody += "\"p_y_fin\": " + p_y_fin;

        if (p_id_habilidad_usada.HasValue) jsonBody += ",\"p_id_habilidad_usada\": " + p_id_habilidad_usada.Value;
        if (p_id_pieza_empujada.HasValue) jsonBody += ",\"p_id_pieza_empujada\": " + p_id_pieza_empujada.Value;
        if (p_x_origen_empujada.HasValue) jsonBody += ",\"p_x_origen_empujada\": " + p_x_origen_empujada.Value;
        if (p_y_origen_empujada.HasValue) jsonBody += ",\"p_y_origen_empujada\": " + p_y_origen_empujada.Value;
        if (p_x_fin_empujada.HasValue) jsonBody += ",\"p_x_fin_empujada\": " + p_x_fin_empujada.Value;
        if (p_y_fin_empujada.HasValue) jsonBody += ",\"p_y_fin_empujada\": " + p_y_fin_empujada.Value;
        
        jsonBody += "}";

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
            string responseText = request.downloadHandler.text;
            Debug.Log("[RegistrarMovimiento] Movimiento registrado. Respuesta: " + responseText);
            
            // La función SQL devuelve el id_movimiento que se acaba de crear (un INT)
            if (int.TryParse(responseText, out int idMovimiento))
            {
                onSuccess?.Invoke(idMovimiento);
            }
            else
            {
                Debug.LogWarning("[RegistrarMovimiento] Se registró pero no se pudo parsear el ID del movimiento: " + responseText);
                onSuccess?.Invoke(0);
            }
        }
        else
        {
            string errorText = "Error de red: " + request.error + "\nRespuesta: " + request.downloadHandler.text;
            Debug.LogError("[RegistrarMovimiento] Falló el registro de movimiento: " + errorText);
            onError?.Invoke(errorText);
        }
    }
}
