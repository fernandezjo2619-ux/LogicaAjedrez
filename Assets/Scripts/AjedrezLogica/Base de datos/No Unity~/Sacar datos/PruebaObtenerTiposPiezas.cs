using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SupabaseAjedrez
{
    public class PruebaObtenerTiposPiezasDb
    {
        private readonly string apiKey = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";
        private static readonly HttpClient client = new HttpClient();
        private List<TipoPiezaInfo> tiposObtenidos = new List<TipoPiezaInfo>();

        public async Task<List<TipoPiezaInfo>> ObtenerTiposYGuardarAsync()
        {
            string url = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/tipo_pieza?select=id,nombre";

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Add("apikey", apiKey);
                
                try
                {
                    HttpResponseMessage response = await client.SendAsync(request);
                    string json = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("[PruebaObtenerTiposPiezas] JSON recibido:");
                        Console.WriteLine(json);

                        try 
                        {
                            var parsedList = JsonSerializer.Deserialize<List<TipoPiezaInfo>>(json);
                            if (parsedList != null)
                            {
                                tiposObtenidos = parsedList;
                                Console.WriteLine("[PruebaObtenerTiposPiezas] Se parsearon " + tiposObtenidos.Count + " tipos exitosamente.");
                                ImprimirTipos();
                            }
                            else 
                            {
                                Console.WriteLine("[PruebaObtenerTiposPiezas] No se encontraron tipos en la respuesta.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[PruebaObtenerTiposPiezas] Error al parsear JSON: " + ex.Message);
                        }
                        return tiposObtenidos;
                    }
                    else
                    {
                        Console.WriteLine("[PruebaObtenerTiposPiezas] Error al obtener tipos:");
                        Console.WriteLine(response.StatusCode);
                        Console.WriteLine(json);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[PruebaObtenerTiposPiezas] Excepción: " + ex.Message);
                    return null;
                }
            }
        }

        private void ImprimirTipos()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          LISTA DE TIPOS DE PIEZAS EN LA BD             ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

            if (tiposObtenidos.Count == 0)
            {
                Console.WriteLine("⚠️  No hay tipos en la lista.");
                return;
            }

            Console.WriteLine("Total de tipos: " + tiposObtenidos.Count + "\n");

            for (int i = 0; i < tiposObtenidos.Count; i++)
            {
                TipoPiezaInfo tipo = tiposObtenidos[i];
                Console.WriteLine("[" + (i + 1) + "] " + tipo.nombre + " (ID: " + tipo.id + ")");
            }

            Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    FIN DE LA LISTA                     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
        }

        public List<TipoPiezaInfo> ObtenerListaTipos()
        {
            return tiposObtenidos;
        }
    }

    public class TipoPiezaInfo
    {
        public int id { get; set; }
        public string nombre { get; set; }

        public override string ToString()
        {
            return nombre + " (ID: " + id + ")";
        }
    }
}
