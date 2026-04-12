using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SupabaseAjedrez
{
    public class SupabaseGuardarPartida
    {
        private readonly string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
        private readonly string apiKey = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";
        private static readonly HttpClient client = new HttpClient();

        public async Task GetPartidasUsuarioAsync(int idUsuario)
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
                    string responseText = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Respuesta:");
                        Console.WriteLine(responseText);
                    }
                    else
                    {
                        Console.WriteLine("Error:");
                        Console.WriteLine(response.StatusCode);
                        Console.WriteLine(responseText);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Excepción de red: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// Guarda una nueva partida en la base de datos usando la funcion guardar_partida
        /// </summary>
        public async Task<int> GuardarPartidaAsync(int idJugador1, int idJugador2, string estado = "EN_CURSO", int? idGanador = null)
        {
            string url = baseUrl + "/guardar_partida";

            string jsonBody = "{" +
                "\"p_id_jugador_1\": " + idJugador1 + ", " +
                "\"p_id_jugador_2\": " + idJugador2 + ", " +
                "\"p_estado\": \"" + estado + "\"";

            if (idGanador.HasValue)
            {
                jsonBody += ", \"p_id_ganador\": " + idGanador.Value;
            }
            else
            {
                jsonBody += ", \"p_id_ganador\": null";
            }

            jsonBody += "}";

            using (var request = new HttpRequestMessage(HttpMethod.Post, url))
            {
                request.Headers.Add("apikey", apiKey);
                request.Headers.Add("Authorization", "Bearer " + apiKey);
                request.Content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

                try
                {
                    HttpResponseMessage response = await client.SendAsync(request);
                    string responseText = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Partida guardada exitosamente. ID de partida: " + responseText);
                        if (int.TryParse(responseText, out int idPartida))
                        {
                            return idPartida;
                        }
                        return 0;
                    }
                    else
                    {
                        Console.WriteLine("Error al guardar partida:");
                        Console.WriteLine(response.StatusCode);
                        Console.WriteLine(responseText);
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Excepción de red: " + ex.Message);
                    return 0;
                }
            }
        }
    }
}
