using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace SupabaseAjedrez
{
    /// <summary>
    /// Llama a la función RPC registrar_movimiento de Supabase
    /// </summary>
    public class RegistrarMovimientoDb
    {
        private readonly string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
        private readonly string apiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";
        private static readonly HttpClient client = new HttpClient();

        /// <summary>
        /// Registra un movimiento en Supabase llamando a la función registrar_movimiento.
        /// Devuelve el id del movimiento creado, o 0 si falla.
        /// </summary>
        public async Task<int> PostRegistrarMovimientoAsync(
            int p_id_partida,
            int p_id_usuario,
            int p_id_pieza,
            int p_turno_numero,
            int p_x_origen,
            int p_y_origen,
            int p_x_fin,
            int p_y_fin,
            int? p_id_habilidad_usada = null,
            int? p_id_pieza_empujada = null,
            int? p_x_origen_empujada = null,
            int? p_y_origen_empujada = null,
            int? p_x_fin_empujada = null,
            int? p_y_fin_empujada = null)
        {
            string url = baseUrl + "/registrar_movimiento";

            string jsonBody = "{";
            jsonBody += "\"p_id_partida\": " + p_id_partida + ",";
            jsonBody += "\"p_id_usuario\": " + p_id_usuario + ",";
            jsonBody += "\"p_id_pieza\": " + p_id_pieza + ",";
            jsonBody += "\"p_turno_numero\": " + p_turno_numero + ",";
            jsonBody += "\"p_x_origen\": " + p_x_origen + ",";
            jsonBody += "\"p_y_origen\": " + p_y_origen + ",";
            jsonBody += "\"p_x_fin\": " + p_x_fin + ",";
            jsonBody += "\"p_y_fin\": " + p_y_fin;

            if (p_id_habilidad_usada.HasValue) jsonBody += ",\"p_id_habilidad_usada\": " + p_id_habilidad_usada.Value;
            if (p_id_pieza_empujada.HasValue) jsonBody += ",\"p_id_pieza_empujada\": " + p_id_pieza_empujada.Value;
            if (p_x_origen_empujada.HasValue) jsonBody += ",\"p_x_origen_empujada\": " + p_x_origen_empujada.Value;
            if (p_y_origen_empujada.HasValue) jsonBody += ",\"p_y_origen_empujada\": " + p_y_origen_empujada.Value;
            if (p_x_fin_empujada.HasValue) jsonBody += ",\"p_x_fin_empujada\": " + p_x_fin_empujada.Value;
            if (p_y_fin_empujada.HasValue) jsonBody += ",\"p_y_fin_empujada\": " + p_y_fin_empujada.Value;
            
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
                        Console.WriteLine("[RegistrarMovimiento] Movimiento registrado. Respuesta: " + responseText);
                        
                        if (int.TryParse(responseText, out int idMovimiento))
                        {
                            return idMovimiento;
                        }
                        else
                        {
                            Console.WriteLine("[RegistrarMovimiento] Se registró pero no se pudo parsear el ID del movimiento: " + responseText);
                            return 0;
                        }
                    }
                    else
                    {
                        Console.WriteLine("[RegistrarMovimiento] Falló el registro de movimiento. Status: " + response.StatusCode + " | Respuesta: " + responseText);
                        return 0;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[RegistrarMovimiento] Falló por excepción: " + ex.Message);
                    return 0;
                }
            }
        }
    }
}
