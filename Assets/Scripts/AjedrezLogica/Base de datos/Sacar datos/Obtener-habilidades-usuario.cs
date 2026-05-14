using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using LogicProject.Recursos.Class;

/// <summary>
/// Llama a la función RPC obtener_habilidades_usuario de Supabase
/// </summary>
public class ObtenerHabilidadesUsuario : MonoBehaviour
{
    private string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
    private string apiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";

    private List<DatosHabilidadUsuario> habilidadesUsuario = new List<DatosHabilidadUsuario>();

    void Start()
    {
        // Deshabilitado para evitar ejecución automática
        // StartCoroutine(GetHabilidadesUsuario(7));
    }

    /// <summary>
    /// Llama a la función SQL obtener_habilidades_usuario y guarda la lista.
    /// </summary>
    public IEnumerator GetHabilidadesUsuario(int idUsuario)
    {
        string url      = baseUrl + "/obtener_habilidades_usuario";
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
            Debug.Log("[ObtenerHabilidadesUsuario] Habilidades del usuario " + idUsuario + ":");
            Debug.Log(json);

            ParsearYGuardarHabilidades(json);
        }
        else
        {
            Debug.LogError("[ObtenerHabilidadesUsuario] Error al obtener habilidades del usuario " + idUsuario + ":");
            Debug.LogError(request.error);
            Debug.LogError(request.downloadHandler.text);
        }
    }

    /// <summary>
    /// Parsea el JSON devuelto y lo almacena localmente
    /// </summary>
    private void ParsearYGuardarHabilidades(string json)
    {
        habilidadesUsuario.Clear();

        try
        {
            string jsonEnvuelto = "{\"habilidades\": " + json + "}";
            ListaHabilidadesUsuario listaHabilidades = JsonUtility.FromJson<ListaHabilidadesUsuario>(jsonEnvuelto);

            if (listaHabilidades.habilidades != null)
            {
                foreach (DatosHabilidadUsuario habilidad in listaHabilidades.habilidades)
                {
                    habilidadesUsuario.Add(habilidad);
                }
                Debug.Log("[ObtenerHabilidadesUsuario] Se parsearon " + habilidadesUsuario.Count + " habilidades de usuario exitosamente.");
                ImprimirHabilidades();
            }
            else
            {
                Debug.LogWarning("[ObtenerHabilidadesUsuario] No se encontraron habilidades en la respuesta.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[ObtenerHabilidadesUsuario] Error al parsear JSON: " + ex.Message);
        }
    }

    /// <summary>
    /// Imprime la lista de habilidades formateada
    /// </summary>
    private void ImprimirHabilidades()
    {
        Debug.Log("\n╔════════════════════════════════════════════════════════╗");
        Debug.Log("║       LISTA DE HABILIDADES DE USUARIO OBTENIDAS        ║");
        Debug.Log("╚════════════════════════════════════════════════════════╝\n");

        if (habilidadesUsuario.Count == 0)
        {
            Debug.Log("⚠️  No hay habilidades en la lista.");
            return;
        }

        Debug.Log("Total de habilidades de este usuario: " + habilidadesUsuario.Count + "\n");

        for (int i = 0; i < habilidadesUsuario.Count; i++)
        {
            DatosHabilidadUsuario h = habilidadesUsuario[i];
            Debug.Log("[" + (i + 1) + "] Pieza: " + h.nombre_pieza + " (" + h.nombre_tipo + ") | Habilidad: " + h.nombre_habilidad);
            Debug.Log("    Poder Base: " + h.poder_base + " | Activa: " + (h.es_habilidad_activa ? "Sí" : "No"));
            Debug.Log("    Desbloqueada: " + h.fecha_desbloqueo);
            Debug.Log("    Descripción: " + h.descripcion + "\n");
        }

        Debug.Log("╔════════════════════════════════════════════════════════╗");
        Debug.Log("║                    FIN DE LA LISTA                     ║");
        Debug.Log("╚════════════════════════════════════════════════════════╝\n");
    }

    /// <summary>
    /// Método público para acceder a la lista desde otros scripts
    /// </summary>
    public List<DatosHabilidadUsuario> ObtenerListaHabilidadesUsuario()
    {
        return habilidadesUsuario;
    }
}

///// <summary>
///// Estructura para parsear cada fila devuelta por obtener_habilidades_usuario
///// </summary>
//[System.Serializable]
//public class DatosHabilidadUsuario
//{
//    public int id_pieza;
//    public string nombre_pieza;
//    public int id_habilidad;
//    public string nombre_habilidad;
//    public string descripcion;
//    public int poder_base;
//    public string nombre_tipo;
//    public bool es_habilidad_activa;
//    public string fecha_desbloqueo;
//}

///// <summary>
///// Wrapper para el JSON
///// </summary>
//[System.Serializable]
//public class ListaHabilidadesUsuario
//{
//    public DatosHabilidadUsuario[] habilidades;
//}
