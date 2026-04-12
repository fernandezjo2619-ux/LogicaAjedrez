using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SupabaseAjedrez
{
    public class ObtenerHabilidadesTipoPiezaDb
    {
        private readonly string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
        private readonly string apiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";
        private static readonly HttpClient client = new HttpClient();

        public async Task<string> GetHabilidadesTipoPiezaAsync(int idTipo)
        {
            string url = baseUrl + "/obtener_habilidades_tipo_pieza";
            string jsonBody = "{\"p_id_tipo\": " + idTipo + "}";

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Add("apikey", apiKey);
                request.Headers.Add("Authorization", "Bearer " + apiKey);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.SendAsync(request);
                    string json = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("[ObtenerHabilidadesTipoPieza] Habilidades del tipo de pieza " + idTipo + ":");
                        Console.WriteLine(json);
                        return json;
                    }
                    else
                    {
                        Console.WriteLine("[ObtenerHabilidadesTipoPieza] Error al obtener habilidades del tipo " + idTipo + ":");
                        Console.WriteLine(response.StatusCode);
                        Console.WriteLine(json);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ObtenerHabilidadesTipoPieza] Excepción: " + ex.Message);
                    return null;
                }
            }
        }
    }
}
