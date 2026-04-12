using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class SupabaseRPC : MonoBehaviour
{
    private string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
    private string apiKey = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";

    void Start()
    {
        StartCoroutine(GetPartidasUsuario(7)); // ejemplo con id = 7
        // StartCoroutine(GuardarPartida(1, 2)); // ejemplo: guardar partida entre jugador 1 y 2
    }

    IEnumerator GetPartidasUsuario(int idUsuario)
    {
        // JSON con el par�metro de la funci�n
        string jsonBody = "{\"p_id_usuario\": " + idUsuario + "}";

        string url = baseUrl + "/obtener_partidas_usuario";
        UnityWebRequest request = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Respuesta:");
            Debug.Log(request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error:");
            Debug.LogError(request.error);
            Debug.LogError(request.downloadHandler.text);
        }
    }

    /// <summary>
    /// Guarda una nueva partida en la base de datos usando la funcion guardar_partida
    /// </summary>
    /// <param name="idJugador1">ID del primer jugador</param>
    /// <param name="idJugador2">ID del segundo jugador</param>
    /// <param name="estado">Estado de la partida (EN_CURSO o FINALIZADA). Por defecto: EN_CURSO</param>
    /// <param name="idGanador">ID del ganador (opcional, NULL si no hay ganador)</param>
    IEnumerator GuardarPartida(int idJugador1, int idJugador2, string estado = "EN_CURSO", int? idGanador = null)
    {
        string url = baseUrl + "/guardar_partida";

        // Construir JSON con los parametros de la funcion SQL
        string jsonBody = "{" +
            "\"p_id_jugador_1\": " + idJugador1 + ", " +
            "\"p_id_jugador_2\": " + idJugador2 + ", " +
            "\"p_estado\": \"" + estado + "\"";

        if (idGanador.HasValue)
        {
            jsonBody += ", \"p_id_ganador\": " + idGanador.Value;
        }
        else
        {
            jsonBody += ", \"p_id_ganador\": null";
        }

        jsonBody += "}";

        UnityWebRequest request = new UnityWebRequest(url, "POST");

        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("apikey", apiKey);
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            Debug.Log("Partida guardada exitosamente. ID de partida: " + request.downloadHandler.text);
        }
        else
        {
            Debug.LogError("Error al guardar partida:");
            Debug.LogError(request.error);
            Debug.LogError(request.downloadHandler.text);
        }
    }
}