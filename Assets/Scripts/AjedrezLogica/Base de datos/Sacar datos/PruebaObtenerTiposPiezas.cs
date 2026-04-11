using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Script de prueba que obtiene todos los tipos de piezas de Supabase,
/// los guarda en una lista y los imprime por consola.
/// </summary>
public class PruebaObtenerTiposPiezas : MonoBehaviour
{
    private string apiKey = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";
    private List<TipoPiezaInfo> tiposObtenidos = new List<TipoPiezaInfo>();

    void Start()
    {
        StartCoroutine(ObtenerTiposYGuardar());
    }

    /// <summary>
    /// Obtiene todos los tipos de piezas de la BD Supabase,
    /// los guarda en una lista y los imprime por consola.
    /// </summary>
    public IEnumerator ObtenerTiposYGuardar()
    {
        string url = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/tipo_pieza?select=id,nombre";

        UnityWebRequest request = new UnityWebRequest(url, "GET");
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("apikey", apiKey);

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            string json = request.downloadHandler.text;
            Debug.Log("[PruebaObtenerTiposPiezas] JSON recibido:");
            Debug.Log(json);

            ParsearYGuardarTipos(json);
            ImprimirTipos();
        }
        else
        {
            Debug.LogError("[PruebaObtenerTiposPiezas] Error al obtener tipos:");
            Debug.LogError(request.error);
            Debug.LogError(request.downloadHandler.text);
        }
    }

    /// <summary>
    /// Parsea el JSON de respuesta y guarda los tipos en la lista
    /// </summary>
    private void ParsearYGuardarTipos(string json)
    {
        tiposObtenidos.Clear();

        try
        {
            // El JSON viene como array directamente, lo envolvemos para deserializar
            string jsonEnvuelto = "{\"tipos\": " + json + "}";
            TipoPiezasWrapper wrapper = JsonUtility.FromJson<TipoPiezasWrapper>(jsonEnvuelto);

            if (wrapper.tipos != null)
            {
                foreach (TipoPiezaInfo tipo in wrapper.tipos)
                {
                    tiposObtenidos.Add(tipo);
                }
                Debug.Log("[PruebaObtenerTiposPiezas] Se parsearon " + tiposObtenidos.Count + " tipos exitosamente.");
            }
            else
            {
                Debug.LogWarning("[PruebaObtenerTiposPiezas] No se encontraron tipos en la respuesta.");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[PruebaObtenerTiposPiezas] Error al parsear JSON: " + ex.Message);
        }
    }

    /// <summary>
    /// Imprime en consola todos los tipos guardados en la lista
    /// </summary>
    private void ImprimirTipos()
    {
        Debug.Log("\n╔════════════════════════════════════════════════════════╗");
        Debug.Log("║          LISTA DE TIPOS DE PIEZAS EN LA BD             ║");
        Debug.Log("╚════════════════════════════════════════════════════════╝\n");

        if (tiposObtenidos.Count == 0)
        {
            Debug.Log("⚠️  No hay tipos en la lista.");
            return;
        }

        Debug.Log("Total de tipos: " + tiposObtenidos.Count + "\n");

        for (int i = 0; i < tiposObtenidos.Count; i++)
        {
            TipoPiezaInfo tipo = tiposObtenidos[i];
            Debug.Log("[" + (i + 1) + "] " + tipo.nombre + " (ID: " + tipo.id + ")");
        }

        Debug.Log("\n╔════════════════════════════════════════════════════════╗");
        Debug.Log("║                    FIN DE LA LISTA                     ║");
        Debug.Log("╚════════════════════════════════════════════════════════╝\n");
    }

    /// <summary>
    /// Método público para acceder a la lista de tipos desde otros scripts
    /// </summary>
    public List<TipoPiezaInfo> ObtenerListaTipos()
    {
        return tiposObtenidos;
    }
}

/// <summary>
/// Estructura para parsear cada tipo de pieza
/// </summary>
[System.Serializable]
public class TipoPiezaInfo
{
    public int id;
    public string nombre;

    public override string ToString()
    {
        return nombre + " (ID: " + id + ")";
    }
}

/// <summary>
/// Wrapper para parsear array de tipos
/// </summary>
[System.Serializable]
public class TipoPiezasWrapper
{
    public TipoPiezaInfo[] tipos;
}
