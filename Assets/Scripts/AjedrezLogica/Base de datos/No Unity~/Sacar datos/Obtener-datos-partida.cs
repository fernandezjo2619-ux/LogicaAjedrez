using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SupabaseAjedrez
{
    /// <summary>
    /// Llama a las funciones RPC de Supabase relacionadas con los datos de una partida:
    ///   · obtener_datos_partida        → cabecera + jugadores de una partida
    ///   · obtener_movimientos_partida  → historial completo de movimientos
    /// </summary>
    public class ObtenerDatosPartidaDb
    {
        private readonly string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
        private readonly string apiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Llama a la función SQL obtener_datos_partida y devuelve la respuesta JSON como string.
        /// </summary>
        public async Task<string> GetDatosPartidaAsync(int idPartida)
        {
            string url = baseUrl + "/obtener_datos_partida";
            string jsonBody = "{\"p_id_partida\": " + idPartida + "}";

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
                        Console.WriteLine("[ObtenerDatosPartida] Cabecera + jugadores de la partida " + idPartida + ":");
                        Console.WriteLine(json);
                        return json;
                    }
                    else
                    {
                        Console.WriteLine("[ObtenerDatosPartida] Error al obtener datos de la partida " + idPartida + ":");
                        Console.WriteLine(response.StatusCode);
                        Console.WriteLine(json);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ObtenerDatosPartida] Excepción: " + ex.Message);
                    return null;
                }
            }
        }

        /// <summary>
        /// Llama a la función SQL obtener_movimientos_partida y devuelve la respuesta JSON como string.
        /// </summary>
        public async Task<string> GetMovimientosPartidaAsync(int idPartida, int? filtroIdUsuario = null, bool soloConHabilidad = false)
        {
            string url = baseUrl + "/obtener_movimientos_partida";
            string jsonBody = "{\"p_id_partida\": " + idPartida + "}";

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
                        string etiqueta = "[ObtenerMovimientosPartida] Movimientos de la partida " + idPartida;
                        if (filtroIdUsuario.HasValue)  etiqueta += " (usuario " + filtroIdUsuario.Value + ")";
                        if (soloConHabilidad)          etiqueta += " (solo con habilidad)";

                        Console.WriteLine(etiqueta + ":");
                        Console.WriteLine(json);
                        return json;
                    }
                    else
                    {
                        Console.WriteLine("[ObtenerMovimientosPartida] Error al obtener movimientos de la partida " + idPartida + ":");
                        Console.WriteLine(response.StatusCode);
                        Console.WriteLine(json);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ObtenerMovimientosPartida] Excepción: " + ex.Message);
                    return null;
                }
            }
        }
    }
}
