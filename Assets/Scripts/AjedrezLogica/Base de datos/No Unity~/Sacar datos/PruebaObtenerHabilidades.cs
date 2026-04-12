using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SupabaseAjedrez
{
    public class PruebaObtenerHabilidadesDb
    {
        private readonly string baseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
        private readonly string apiKey = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";
        private static readonly HttpClient client = new HttpClient();

        private List<DatosHabilidad> habilidadesObtenidas = new List<DatosHabilidad>();
        private List<TipoPiezaData> tiposObtenidos = new List<TipoPiezaData>();

        public async Task ObtenerTiposPiezasYHabilidadesAsync()
        {
            string url = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/tipos_pieza?select=id_tipo,nombre_tipo";

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Add("apikey", apiKey);
                
                try
                {
                    HttpResponseMessage response = await client.SendAsync(request);
                    string json = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("[PruebaObtenerHabilidades] Tipos de piezas recuperados:");
                        Console.WriteLine(json);

                        try
                        {
                            var parsedTipos = JsonSerializer.Deserialize<List<TipoPiezaData>>(json);
                            if (parsedTipos != null && parsedTipos.Count > 0)
                            {
                                tiposObtenidos = parsedTipos;
                                ImprimirTipos();

                                Console.WriteLine("\n✅ Se encontraron " + tiposObtenidos.Count + " tipos de piezas. Obteniendo habilidades...\n");

                                foreach (var tipo in tiposObtenidos)
                                {
                                    Console.WriteLine("→ Obteniendo habilidades para: " + tipo.nombre_tipo + " (ID: " + tipo.id_tipo + ")");
                                    await ObtenerYGuardarHabilidadesAsync(tipo.id_tipo);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[PruebaObtenerHabilidades] Error parseando tipos: " + ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("[PruebaObtenerHabilidades] Error al obtener tipos de piezas:");
                        Console.WriteLine("Status Code: " + response.StatusCode);
                        Console.WriteLine("Respuesta: " + json);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[PruebaObtenerHabilidades] Excepcion de red: " + ex.Message);
                }
            }
        }

        public async Task ObtenerYGuardarHabilidadesAsync(int idTipo)
        {
            string url = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/habilidades?id_tipo_pieza=eq." + idTipo + "&select=id_habilidad,nombre_habilidad,descripcion,poder_base";

            using (var request = new HttpRequestMessage(HttpMethod.Get, url))
            {
                request.Headers.Add("apikey", apiKey);
                
                try
                {
                    HttpResponseMessage response = await client.SendAsync(request);
                    string json = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("[PruebaObtenerHabilidades] Habilidades del tipo " + idTipo + ":");
                        Console.WriteLine(json);

                        try
                        {
                            var parsedList = JsonSerializer.Deserialize<List<DatosHabilidad>>(json);
                            if (parsedList != null)
                            {
                                habilidadesObtenidas.AddRange(parsedList);
                                Console.WriteLine("[PruebaObtenerHabilidades] Se parsearon " + parsedList.Count + " habilidades exitosamente.");
                            }
                            else
                            {
                                Console.WriteLine("[PruebaObtenerHabilidades] No se encontraron habilidades en la respuesta.");
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("[PruebaObtenerHabilidades] Error al parsear JSON de habilidades: " + ex.Message);
                        }
                    }
                    else
                    {
                        Console.WriteLine("[PruebaObtenerHabilidades] Error al obtener habilidades del tipo " + idTipo + ":");
                        Console.WriteLine("Status: " + response.StatusCode);
                        Console.WriteLine("Respuesta: " + json);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("[PruebaObtenerHabilidades] Excepción de red: " + ex.Message);
                }
            }
        }

        public void ImprimirHabilidades()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║        LISTA DE HABILIDADES OBTENIDAS DE LA BD         ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

            if (habilidadesObtenidas.Count == 0)
            {
                Console.WriteLine("⚠️  No hay habilidades en la lista.");
                return;
            }

            Console.WriteLine("Total de habilidades: " + habilidadesObtenidas.Count + "\n");

            for (int i = 0; i < habilidadesObtenidas.Count; i++)
            {
                DatosHabilidad h = habilidadesObtenidas[i];
                Console.WriteLine("[" + (i + 1) + "] " + h.nombre_habilidad);
                Console.WriteLine("    ID: " + h.id_habilidad + " | Poder Base: " + h.poder_base);
                Console.WriteLine("    Descripción: " + h.descripcion + "\n");
            }

            Console.WriteLine("╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    FIN DE LA LISTA                     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
        }

        public List<DatosHabilidad> ObtenerListaHabilidades()
        {
            return habilidadesObtenidas;
        }

        private void ImprimirTipos()
        {
            Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║          LISTA DE TIPOS DE PIEZAS OBTENIDOS            ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

            if (tiposObtenidos.Count == 0)
            {
                Console.WriteLine("⚠️  No hay tipos de piezas en la lista.");
                return;
            }

            Console.WriteLine("Total de tipos: " + tiposObtenidos.Count + "\n");

            for (int i = 0; i < tiposObtenidos.Count; i++)
            {
                TipoPiezaData tipo = tiposObtenidos[i];
                Console.WriteLine("[" + (i + 1) + "] " + tipo.nombre_tipo + " (ID: " + tipo.id_tipo + ")");
            }

            Console.WriteLine("\n╔════════════════════════════════════════════════════════╗");
            Console.WriteLine("║                    FIN DE LA LISTA                     ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");
        }

        public List<TipoPiezaData> ObtenerListaTipos()
        {
            return tiposObtenidos;
        }
    }

    public class TipoPiezaData
    {
        public int id_tipo { get; set; }
        public string nombre_tipo { get; set; }
    }
}
