using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System;

[Serializable]
public class SesionResponse
{
    public int id_usuario;
    public string nombre_usuario;
}

/// <summary>
/// Permite iniciar sesión llamando a la función RPC iniciar_sesion en Supabase.
/// Proporciona tres métodos distintos según lo que se desee recuperar (id, nombre, o ambos).
/// </summary>
public class IniciarSesion : MonoBehaviour
{
    // Asegúrate de poner tus credenciales correctas aquí
    private string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
    private string apiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";

    /// <summary>
    /// Inicia sesión y devuelve únicamente el ID del usuario a través del callback.
    /// </summary>
    public IEnumerator IniciarSesion_DevolverId(string email, string password, Action<int> onSuccess, Action<string> onError)
    {
        yield return IniciarSesion_DevolverAmbos(email, password, (response) => {
            onSuccess?.Invoke(response.id_usuario);
        }, onError);
    }

    /// <summary>
    /// Inicia sesión y devuelve únicamente el nombre de usuario a través del callback.
    /// </summary>
    public IEnumerator IniciarSesion_DevolverNombre(string email, string password, Action<string> onSuccess, Action<string> onError)
    {
        yield return IniciarSesion_DevolverAmbos(email, password, (response) => {
            onSuccess?.Invoke(response.nombre_usuario);
        }, onError);
    }

    /// <summary>
    /// Inicia sesión y devuelve tanto el ID como el nombre de usuario empaquetados en un objeto SesionResponse.
    /// </summary>
    public IEnumerator IniciarSesion_DevolverAmbos(string email, string password, Action<SesionResponse> onSuccess, Action<string> onError)
    {
        string url = baseUrl + "/iniciar_sesion";
        
        // Cuidado con las comillas en el JSON. Se asume que email y password no contienen comillas dobles sin escapar.
        string jsonBody = "{\"p_email\": \"" + email + "\", \"p_password\": \"" + password + "\"}";

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
            string json = request.downloadHandler.text;
            
            // Como Supabase devuelve una tabla (arreglo de objetos JSON), puede venir como:
            // [{"id_usuario": 1, "nombre_usuario": "Jugador1"}] o vacio [] si falla.
            
            if (json == "[]" || string.IsNullOrEmpty(json))
            {
                onError?.Invoke("Credenciales incorrectas o usuario no encontrado.");
                yield break;
            }

            // Quitamos los corchetes del arreglo para poder parsear el primer objeto directamente con JsonUtility
            string cleanJson = json.Trim();
            if (cleanJson.StartsWith("[") && cleanJson.EndsWith("]"))
            {
                cleanJson = cleanJson.Substring(1, cleanJson.Length - 2);
            }

            try
            {
                SesionResponse response = JsonUtility.FromJson<SesionResponse>(cleanJson);
                if (response != null)
                {
                    onSuccess?.Invoke(response);
                }
                else
                {
                    onError?.Invoke("No se pudieron parsear los datos devueltos.");
                }
            }
            catch (Exception e)
            {
                onError?.Invoke("Error al leer formato de respuesta: " + e.Message);
            }
        }
        else
        {
            onError?.Invoke("Error de conexión: " + request.error + " | " + request.downloadHandler.text);
        }
    }
}
