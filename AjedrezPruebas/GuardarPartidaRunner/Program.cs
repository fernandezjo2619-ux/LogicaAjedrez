// GuardarPartidaRunner - Ejecuta una partida de ajedrez con la IA
// y guarda el resultado en Supabase al terminar.
// NO modifica AjedrezPruebas/Program.cs.

using AjedrezLogica;
using AjedrezLogica.IA;
using AjedrezLogica.Recursos;
using System.Net.Http;
using System.Text;

// ──────────────────────────────────────────────
//  Configuración de Supabase (igual que Guardar-partida.cs de Unity)
// ──────────────────────────────────────────────
const string BaseUrl = "https://qutkopstyyqzcjmytylq.supabase.co/rest/v1/rpc";
const string ApiKey  = "sb_publishable__2OpD7oxYv91E64NozZR0w_z9CKUPMu";

// IDs de jugador para la partida (ajusta según tus usuarios en la BD)
const int IdJugadorBlancas = 1;  // jugador que lleva las blancas
const int IdJugadorNegras  = 2;  // jugador que lleva las negras

// ──────────────────────────────────────────────
//  Ejecutar el juego (misma lógica que AjedrezPruebas/Program.cs)
// ──────────────────────────────────────────────
BaseJuego juego = new BaseJuego();
IMotorIA ia = new IAAleatoria();

ia.InicializarPiezasDeIA(juego, ColorPieza.Blanco);
ia.InicializarPiezasDeIA(juego, ColorPieza.Negro);

int turno = 0;
int? idGanador = null;
bool partidaTerminada = false;
int maxTurnos = 1000;  // límite de seguridad para evitar bucle infinito

Console.WriteLine("=== Partida de Ajedrez iniciada ===");
Console.WriteLine($"Blancas: Jugador {IdJugadorBlancas} | Negras: Jugador {IdJugadorNegras}");
Console.WriteLine();

// Interceptamos la salida de consola para detectar "GANADOR"
var consolaOriginal = Console.Out;
var interceptor = new GanadorInterceptor(consolaOriginal);
Console.SetOut(interceptor);

while (!partidaTerminada && turno < maxTurnos)
{
    Accion accion = ia.ElegirMovimiento(juego, juego.TurnoActual);
    ColorPieza turnoAntes = juego.TurnoActual;
    juego.RealizarMovimiento(accion.pieza.Posicion.X, accion.pieza.Posicion.Y, accion.xFin, accion.yFin);
    turno++;

    if (interceptor.SeDetectoGanador)
    {
        partidaTerminada = true;
        // El ganador es el color del turno EN QUE se realizó el movimiento ganador
        idGanador = turnoAntes == ColorPieza.Blanco ? IdJugadorBlancas : IdJugadorNegras;
    }
}

// Restaurar salida de consola original
Console.SetOut(consolaOriginal);

Console.WriteLine();
Console.WriteLine($"=== Partida terminada en {turno} turnos ===");

if (idGanador.HasValue)
    Console.WriteLine($"Ganador: Jugador {idGanador.Value} ({(idGanador.Value == IdJugadorBlancas ? "Blancas" : "Negras")})");
else
    Console.WriteLine("Partida sin ganador (límite de turnos alcanzado).");

// ──────────────────────────────────────────────
//  Guardar la partida en Supabase
//  (lógica equivalente a IEnumerator GuardarPartida() de Unity)
// ──────────────────────────────────────────────
Console.WriteLine();
Console.WriteLine("[Supabase] Guardando partida en la base de datos...");

await GuardarPartidaAsync(
    idJugador1: IdJugadorBlancas,
    idJugador2: IdJugadorNegras,
    estado: "FINALIZADA",
    idGanador: idGanador
);

// ──────────────────────────────────────────────
//  Función de guardado - equivalente a Guardar-partida.cs de Unity
// ──────────────────────────────────────────────
static async Task GuardarPartidaAsync(int idJugador1, int idJugador2, string estado = "EN_CURSO", int? idGanador = null)
{
    using var client = new HttpClient();
    string url = BaseUrl + "/guardar_partida";

    // Mismo JSON que construye Guardar-partida.cs en Unity
    string jsonBody = "{" +
        "\"p_id_jugador_1\": " + idJugador1 + ", " +
        "\"p_id_jugador_2\": " + idJugador2 + ", " +
        "\"p_estado\": \"" + estado + "\"";

    if (idGanador.HasValue)
        jsonBody += ", \"p_id_ganador\": " + idGanador.Value;
    else
        jsonBody += ", \"p_id_ganador\": null";

    jsonBody += "}";

    Console.WriteLine("[Supabase] Body enviado: " + jsonBody);

    using var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
    using var request = new HttpRequestMessage(HttpMethod.Post, url);
    request.Content = content;
    request.Headers.Add("apikey", ApiKey);
    request.Headers.Add("Authorization", "Bearer " + ApiKey);

    HttpResponseMessage response = await client.SendAsync(request);
    string responseText = await response.Content.ReadAsStringAsync();

    if (response.IsSuccessStatusCode)
    {
        Console.WriteLine("[Supabase] ✓ Partida guardada correctamente.");
        Console.WriteLine("[Supabase] Respuesta: " + responseText);
    }
    else
    {
        Console.WriteLine("[Supabase] ✗ Error al guardar partida.");
        Console.WriteLine("[Supabase] HTTP " + (int)response.StatusCode + ": " + responseText);
    }
}

// ──────────────────────────────────────────────
//  Clase auxiliar: intercepta Console.Out para detectar "GANADOR"
//  sin modificar BaseJuego.cs ni Program.cs
// ──────────────────────────────────────────────
class GanadorInterceptor : TextWriter
{
    private readonly TextWriter _inner;
    public bool SeDetectoGanador { get; private set; }

    public GanadorInterceptor(TextWriter inner)
    {
        _inner = inner;
    }

    public override Encoding Encoding => _inner.Encoding;

    public override void WriteLine(string? value)
    {
        _inner.WriteLine(value);
        if (value != null && value.StartsWith("GANADOR:"))
            SeDetectoGanador = true;
    }

    public override void Write(char value) => _inner.Write(value);
    public override void Write(string? value) => _inner.Write(value);
}
