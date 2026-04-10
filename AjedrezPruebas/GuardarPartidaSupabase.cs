using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AjedrezPruebas
{
    /// <summary>
    /// Equivalente a Guardar-partida.cs de Unity pero usando HttpClient nativo de .NET.
    /// Llama a la función RPC guardar_partida de Supabase.
    /// </summary>
    public static class GuardarPartidaSupabase
    {
        private static readonly string BaseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
        private static readonly string ApiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";

        private static readonly HttpClient _client = new HttpClient();

        /// <summary>
        /// Guarda una nueva partida en Supabase usando la función RPC guardar_partida.
        /// Equivalente a IEnumerator GuardarPartida(...) de Unity.
        /// </summary>
        /// <param name="idJugador1">ID del primer jugador (blancas)</param>
        /// <param name="idJugador2">ID del segundo jugador (negras)</param>
        /// <param name="estado">Estado de la partida: "EN_CURSO" o "FINALIZADA"</param>
        /// <param name="idGanador">ID del ganador; null si no hay ganador aún</param>
        /// <returns>El ID de la partida creada, o -1 si hubo error</returns>
        public static async Task<int> GuardarPartidaAsync(
            int idJugador1,
            int idJugador2,
            string estado = "EN_CURSO",
            int? idGanador = null)
        {
            string url = BaseUrl + "/guardar_partida";

            // Construir el JSON con los parámetros de la función SQL
            // (misma lógica que Guardar-partida.cs de Unity)
            string jsonBody = "{" +
                "\"p_id_jugador_1\": " + idJugador1 + ", " +
                "\"p_id_jugador_2\": " + idJugador2 + ", " +
                "\"p_estado\": \"" + estado + "\"";

            if (idGanador.HasValue)
                jsonBody += ", \"p_id_ganador\": " + idGanador.Value;
            else
                jsonBody += ", \"p_id_ganador\": null";

            jsonBody += "}";

            using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");

            using var request = new HttpRequestMessage(HttpMethod.Post, url);
            request.Content = content;
            request.Headers.Add("apikey", ApiKey);
            request.Headers.Add("Authorization", "Bearer " + ApiKey);

            Console.WriteLine("[GuardarPartida] Enviando solicitud a Supabase...");
            Console.WriteLine("[GuardarPartida] Body: " + jsonBody);

            HttpResponseMessage response = await _client.SendAsync(request);
            string responseText = await response.Content.ReadAsStringAsync();

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("[GuardarPartida] Partida guardada exitosamente.");
                Console.WriteLine("[GuardarPartida] Respuesta: " + responseText);

                // La función SQL devuelve el ID de la partida como integer
                if (int.TryParse(responseText.Trim(), out int idPartida))
                    return idPartida;

                return 0;
            }
            else
            {
                Console.WriteLine("[GuardarPartida] Error al guardar partida.");
                Console.WriteLine("[GuardarPartida] HTTP " + (int)response.StatusCode + ": " + responseText);
                return -1;
            }
        }
    }
}
