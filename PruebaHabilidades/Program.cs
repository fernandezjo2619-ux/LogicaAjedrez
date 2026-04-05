using System.Text.Json;
using PruebaHabilidades;

class Program
{
    private const string BaseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
    private const string ApiKey = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";

    static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════╗");
        Console.WriteLine("║   PRUEBA DE OBTENCIÓN DE HABILIDADES DE SUPABASE       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

        // Obtener habilidades para diferentes tipos de piezas
        var tiposPiezas = new Dictionary<int, string>
        {
            { 1, "Peón" },
            { 2, "Torre" },
            { 3, "Caballo" },
            { 4, "Alfil" },
            { 5, "Dama" },
            { 6, "Rey" }
        };

        foreach (var kvp in tiposPiezas)
        {
            Console.WriteLine($"\n--- Obteniendo habilidades para {kvp.Value} (ID: {kvp.Key}) ---\n");
            await ObtenerYMostrarHabilidades(kvp.Key);
        }

        Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                  PRUEBA COMPLETADA                     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
    }

    static async Task ObtenerYMostrarHabilidades(int idTipo)
    {
        try
        {
            using (HttpClient client = new HttpClient())
            {
                string url = $"{BaseUrl}/obtener_habilidades_tipo_pieza";
                var jsonBody = new { p_id_tipo = idTipo };
                var jsonString = JsonSerializer.Serialize(jsonBody);

                var content = new StringContent(jsonString, System.Text.Encoding.UTF8, "application/json");

                client.DefaultRequestHeaders.Add("apikey", ApiKey);
                client.DefaultRequestHeaders.Add("Authorization", $"Bearer {ApiKey}");

                HttpResponseMessage response = await client.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("✅ Respuesta recibida correctamente.\n");

                    List<DatosHabilidad> habilidades = ParsearHabilidades(responseBody);
                    ImprimirHabilidades(habilidades, idTipo);
                }
                else
                {
                    Console.WriteLine($"❌ Error: {response.StatusCode}");
                    Console.WriteLine($"   Detalles: {await response.Content.ReadAsStringAsync()}\n");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Excepción: {ex.Message}\n");
        }
    }

    static List<DatosHabilidad> ParsearHabilidades(string json)
    {
        List<DatosHabilidad> habilidades = new List<DatosHabilidad>();

        try
        {
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            DatosHabilidad[]? arrayHabilidades = JsonSerializer.Deserialize<DatosHabilidad[]>(json, options);

            if (arrayHabilidades != null)
            {
                habilidades.AddRange(arrayHabilidades);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Error parseando JSON: {ex.Message}");
        }

        return habilidades;
    }

    static void ImprimirHabilidades(List<DatosHabilidad> habilidades, int idTipo)
    {
        if (habilidades.Count == 0)
        {
            Console.WriteLine("⚠️  No se encontraron habilidades.\n");
            return;
        }

        Console.WriteLine($"📋 Total de habilidades para el tipo {idTipo}: {habilidades.Count}\n");

        foreach (var (habilidad, index) in habilidades.Select((h, i) => (h, i + 1)))
        {
            Console.WriteLine($"   [{index}] {habilidad.nombre}");
            Console.WriteLine($"       ID: {habilidad.id} | Poder Base: {habilidad.poder_base}");
            Console.WriteLine($"       {habilidad.descripcion}\n");
        }
    }
}
