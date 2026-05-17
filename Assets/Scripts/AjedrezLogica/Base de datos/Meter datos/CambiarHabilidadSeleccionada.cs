using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

/// <summary>
/// Permite llamar a la función RPC cambiar_habilidad_seleccionada en Supabase.
/// Actualiza en la base de datos la habilidad seleccionada para un tipo de pieza de un usuario.
/// </summary>
public class CambiarHabilidadSeleccionada : MonoBehaviour
{
    // Credenciales de conexión a Supabase (deben coincidir con el resto de scripts)
    private string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
    private string apiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";

    /// <summary>
    /// Llama a la función SQL cambiar_habilidad_seleccionada.
    /// Devuelve el resultado mediante los callbacks onSuccess y onError.
    /// </summary>
    /// <param name="idUsuario">ID del usuario.</param>
    /// <param name="idTipoPieza">ID del tipo de pieza (Ej: 1=Peón, 2=Caballo, etc.).</param>
    /// <param name="idHabilidad">ID de la habilidad a seleccionar.</param>
    /// <param name="onSuccess">Callback que se ejecuta si la actualización fue exitosa.</param>
    /// <param name="onError">Callback que se ejecuta si hubo un error (con el mensaje del error).</param>
    public IEnumerator CambiarHabilidad(int idUsuario, int idTipoPieza, int idHabilidad, Action onSuccess, Action<string> onError)
    {
        string url = baseUrl + "/cambiar_habilidad_seleccionada";
        
        // Construimos el JSON con los nombres exactos de los parámetros de la función SQL
        string jsonBody = "{\"p_id_usuario\": " + idUsuario + 
                          ", \"p_id_tipo_pieza\": " + idTipoPieza + 
                          ", \"p_id_habilidad\": " + idHabilidad + "}";

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
            Debug.Log($"[CambiarHabilidadSeleccionada] Habilidad {idHabilidad} seleccionada correctamente para el tipo de pieza {idTipoPieza} (Usuario: {idUsuario}).");
            onSuccess?.Invoke();
        }
        else
        {
            // Supabase devuelve el error (generado por RAISE EXCEPTION) en el cuerpo de la respuesta
            string errorDetallado = request.downloadHandler.text;
            string errorMessage = $"Error al cambiar habilidad: {request.error} | Detalle: {errorDetallado}";
            
            Debug.LogError($"[CambiarHabilidadSeleccionada] {errorMessage}");
            onError?.Invoke(errorMessage);
        }
    }
}
