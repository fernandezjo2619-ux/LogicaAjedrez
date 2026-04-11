using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script de prueba que obtiene habilidades de la base de datos Supabase,
/// las guarda en una lista y las imprime por consola.
/// </summary>
public class PruebaObtenerHabilidades : MonoBehaviour
{
    private string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
    private string apiKey = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";

    private List<DatosHabilidad> habilidadesObtenidas = new List<DatosHabilidad>();
    private List<TipoPiezaData> tiposObtenidos = new List<TipoPiezaData>();

    void Start()
    {
        // Primero, obtener todos los tipos de piezas disponibles
        StartCoroutine(ObtenerTiposPiezas());
    }

    /// <summary>
    /// Obtiene las habilidades de un tipo de pieza de la BD Supabase,
    /// las guarda en una lista y las imprime por consola.
    /// </summary>
    /// <param name="idTipo">ID del tipo de pieza</param>
    public IEnumerator ObtenerYGuardarHabilidades(int idTipo)
    {
        // Consulta REST para obtener habilidades por id_tipo_pieza
        string url = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/habilidades?id_tipo_pieza=eq." + idTipo + "&select=id_habilidad,nombre_habilidad,descripcion,poder_base";

        UnityWebRequest request = new UnityWebRequest(url, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("apikey", apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("[PruebaObtenerHabilidades] Habilidades del tipo " + idTipo + ":");
            Debug.Log(json);

            // Parsear el JSON a lista de habilidades
            ParsearYGuardarHabilidades(json);
        }
        else
        {
            Debug.LogError("[PruebaObtenerHabilidades] Error al obtener habilidades del tipo " + idTipo + ":");
            Debug.LogError("Status: " + request.responseCode);
            Debug.LogError("Error: " + request.error);
            Debug.LogError("Respuesta: " + request.downloadHandler.text);
        }
    }

    /// <summary>
    /// Parsea el JSON de respuesta y guarda las habilidades en la lista
    /// </summary>
    private void ParsearYGuardarHabilidades(string json)
    {
        habilidadesObtenidas.Clear();

        try
        {
            // El JSON viene como un array directamente, lo envolvemos para deserializar
            string jsonEnvuelto = "{\"habilidades\": " + json + "}";
            ListaHabilidades listaHabilidades = JsonUtility.FromJson<ListaHabilidades>(jsonEnvuelto);

            if (listaHabilidades.habilidades != null)
            {
                foreach (DatosHabilidad habilidad in listaHabilidades.habilidades)
                {
                    habilidadesObtenidas.Add(habilidad);
                }
                Debug.Log("[PruebaObtenerHabilidades] Se parsearon " + habilidadesObtenidas.Count + " habilidades exitosamente.");
            }
            else
            {
                Debug.LogWarning("[PruebaObtenerHabilidades] No se encontraron habilidades en la respuesta.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[PruebaObtenerHabilidades] Error al parsear JSON: " + ex.Message);
        }
    }

    /// <summary>
    /// Imprime en consola todas las habilidades guardadas en la lista
    /// </summary>
    private void ImprimirHabilidades()
    {
        Debug.Log("\n╔════════════════════════════════════════════════════════╗");
        Debug.Log("║        LISTA DE HABILIDADES OBTENIDAS DE LA BD         ║");
        Debug.Log("╚════════════════════════════════════════════════════════╝\n");

        if (habilidadesObtenidas.Count == 0)
        {
            Debug.Log("⚠️  No hay habilidades en la lista.");
            return;
        }

        Debug.Log("Total de habilidades: " + habilidadesObtenidas.Count + "\n");

        for (int i = 0; i < habilidadesObtenidas.Count; i++)
        {
            DatosHabilidad h = habilidadesObtenidas[i];
            Debug.Log("[" + (i + 1) + "] " + h.nombre_habilidad);
            Debug.Log("    ID: " + h.id_habilidad + " | Poder Base: " + h.poder_base);
            Debug.Log("    Descripción: " + h.descripcion + "\n");
        }

        Debug.Log("╔════════════════════════════════════════════════════════╗");
        Debug.Log("║                    FIN DE LA LISTA                     ║");
        Debug.Log("╚════════════════════════════════════════════════════════╝\n");
    }

    /// <summary>
    /// Método público para acceder a la lista de habilidades desde otros scripts
    /// </summary>
    public List<DatosHabilidad> ObtenerListaHabilidades()
    {
        return habilidadesObtenidas;
    }

    /// <summary>
    /// Obtiene todos los tipos de piezas disponibles en la BD
    /// </summary>
    public IEnumerator ObtenerTiposPiezas()
    {
        string url = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/tipos_pieza?select=id_tipo,nombre_tipo";

        UnityWebRequest request = new UnityWebRequest(url, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("apikey", apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("[PruebaObtenerHabilidades] Tipos de piezas recuperados:");
            Debug.Log(json);

            // Intentar parsear y obtener habilidades para cada tipo
            TipoPiezaData[] tipos = null;
            try
            {
                // Parsear el array de tipos
                tipos = JsonUtility.FromJson<TipoPiezaWrapper>("{\"tipos\": " + json + "}").tipos;
            }
            catch (System.Exception ex)
            {
                Debug.LogError("[PruebaObtenerHabilidades] Error parseando tipos: " + ex.Message);
            }

            if (tipos != null && tipos.Length > 0)
            {
                // Guardar los tipos en la lista
                tiposObtenidos.Clear();
                foreach (TipoPiezaData tipo in tipos)
                {
                    tiposObtenidos.Add(tipo);
                }

                // Imprimir los tipos obtenidos
                ImprimirTipos();

                Debug.Log("\n✅ Se encontraron " + tipos.Length + " tipos de piezas. Obteniendo habilidades...\n");

                foreach (TipoPiezaData tipo in tipos)
                {
                    Debug.Log("→ Obteniendo habilidades para: " + tipo.nombre_tipo + " (ID: " + tipo.id_tipo + ")");
                    yield return StartCoroutine(ObtenerYGuardarHabilidades(tipo.id_tipo));
                }
            }
        }
        else
        {
            Debug.LogError("[PruebaObtenerHabilidades] Error al obtener tipos de piezas:");
            Debug.LogError("Status Code: " + request.responseCode);
            Debug.LogError("Error: " + request.error);
            Debug.LogError("Respuesta: " + request.downloadHandler.text);
        }
    }

    /// <summary>
    /// Imprime en consola todos los tipos de piezas guardados en la lista
    /// </summary>
    private void ImprimirTipos()
    {
        Debug.Log("\n╔════════════════════════════════════════════════════════╗");
        Debug.Log("║          LISTA DE TIPOS DE PIEZAS OBTENIDOS            ║");
        Debug.Log("╚════════════════════════════════════════════════════════╝\n");

        if (tiposObtenidos.Count == 0)
        {
            Debug.Log("⚠️  No hay tipos de piezas en la lista.");
            return;
        }

        Debug.Log("Total de tipos: " + tiposObtenidos.Count + "\n");

        for (int i = 0; i < tiposObtenidos.Count; i++)
        {
            TipoPiezaData tipo = tiposObtenidos[i];
            Debug.Log("[" + (i + 1) + "] " + tipo.nombre_tipo + " (ID: " + tipo.id_tipo + ")");
        }

        Debug.Log("\n╔════════════════════════════════════════════════════════╗");
        Debug.Log("║                    FIN DE LA LISTA                     ║");
        Debug.Log("╚════════════════════════════════════════════════════════╝\n");
    }

    /// <summary>
    /// Método público para acceder a la lista de tipos de piezas desde otros scripts
    /// </summary>
    public List<TipoPiezaData> ObtenerListaTipos()
    {
        return tiposObtenidos;
    }
}

/// <summary>
/// Estructura para parsear tipos de piezas
/// </summary>
[System.Serializable]
public class TipoPiezaData
{
    public int id_tipo;
    public string nombre_tipo;
}

/// <summary>
/// Wrapper para parsear array de tipos
/// </summary>
[System.Serializable]
public class TipoPiezaWrapper
{
    public TipoPiezaData[] tipos;
}
