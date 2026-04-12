using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SupabaseAjedrez
{
    /// <summary>
    /// Llama a las funciones RPC de Supabase relacionadas con las partidas de un usuario:
    ///   · obtener_partidas_usuario  → todas las partidas de un usuario
    /// </summary>
    public class ObtenerPartidasUsuarioDb
    {
        private readonly string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
        private readonly string apiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Llama a la función SQL obtener_partidas_usuario y devuelve el JSON resultante.
        /// </summary>
        public async Task<string> GetPartidasUsuarioAsync(int idUsuario, string filtroEstado = null, string filtroResultado = null)
        {
            string url = baseUrl + "/obtener_partidas_usuario";
            string jsonBody = "{\"p_id_usuario\": " + idUsuario + "}";

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
                        Console.WriteLine("[ObtenerPartidasUsuario] Respuesta recibida para usuario " + idUsuario + ":");
                        Console.WriteLine(json);
                        return json;
                    }
                    else
                    {
                        Console.WriteLine("[ObtenerPartidasUsuario] Error al obtener partidas del usuario " + idUsuario + ":");
                        Console.WriteLine(response.StatusCode);
                        Console.WriteLine(json);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ObtenerPartidasUsuario] Excepción: " + ex.Message);
                    return null;
                }
            }
        }
    }
}
