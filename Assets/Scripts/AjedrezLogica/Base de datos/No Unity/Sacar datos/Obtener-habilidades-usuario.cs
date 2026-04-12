using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SupabaseAjedrez
{
    public class ObtenerHabilidadesUsuarioDb
    {
        private readonly string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
        private readonly string apiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";
        private static readonly HttpClient client = new HttpClient();

        private List<DatosHabilidadUsuario> habilidadesUsuario = new List<DatosHabilidadUsuario>();

        public async Task<List<DatosHabilidadUsuario>> GetHabilidadesUsuarioAsync(int idUsuario)
        {
            string url = baseUrl + "/obtener_habilidades_usuario";
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
                        Console.WriteLine("[ObtenerHabilidadesUsuario] Habilidades del usuario " + idUsuario + ":");
                        Console.WriteLine(json);

                        try 
                        {
                            // System.Text.Json maneja deserializacion de listas directamente
                            var parsedList = JsonSerializer.Deserialize<List<DatosHabilidadUsuario>>(json);
                            if (parsedList != null)
                            {
                                habilidadesUsuario = parsedList;
                                Console.WriteLine("[ObtenerHabilidadesUsuario] Se parsearon " + habilidadesUsuario.Count + " habilidades de usuario exitosamente.");
                                ImprimirHabilidades();
                            }
                            else 
                            {
                                Console.WriteLine("[ObtenerHabilidadesUsuario] La lista deserializada resultó nula.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[ObtenerHabilidadesUsuario] Error al parsear JSON: " + ex.Message);
                        }

                        return habilidadesUsuario;
                    }
                    else
                    {
                        Console.WriteLine("[ObtenerHabilidadesUsuario] Error al obtener habilidades del usuario " + idUsuario + ":");
                        Console.WriteLine(response.StatusCode);
                        Console.WriteLine(json);
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[ObtenerHabilidadesUsuario] Excepción de red: " + ex.Message);
                    return null;
                }
            }
        }

        private void ImprimirHabilidades()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║       LISTA DE HABILIDADES DE USUARIO OBTENIDAS        ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

            if (habilidadesUsuario.Count == 0)
            {
                Console.WriteLine("⚠️  No hay habilidades en la lista.");
                return;
            }

            Console.WriteLine("Total de habilidades de este usuario: " + habilidadesUsuario.Count + "\n");

            for (int i = 0; i < habilidadesUsuario.Count; i++)
            {
                DatosHabilidadUsuario h = habilidadesUsuario[i];
                Console.WriteLine("[" + (i + 1) + "] Pieza: " + h.nombre_pieza + " (" + h.nombre_tipo + ") | Habilidad: " + h.nombre_habilidad);
                Console.WriteLine("    Poder Base: " + h.poder_base + " | Activa: " + (h.es_habilidad_activa ? "Sí" : "No"));
                Console.WriteLine("    Desbloqueada: " + h.fecha_desbloqueo);
                Console.WriteLine("    Descripción: " + h.descripcion + "\n");
            }

            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    FIN DE LA LISTA                     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
        }

        public List<DatosHabilidadUsuario> ObtenerListaHabilidadesUsuario()
        {
            return habilidadesUsuario;
        }
    }

    public class DatosHabilidadUsuario
    {
        public int id_pieza { get; set; }
        public string nombre_pieza { get; set; }
        public int id_habilidad { get; set; }
        public string nombre_habilidad { get; set; }
        public string descripcion { get; set; }
        public int poder_base { get; set; }
        public string nombre_tipo { get; set; }
        public bool es_habilidad_activa { get; set; }
        public string fecha_desbloqueo { get; set; }
    }
}
