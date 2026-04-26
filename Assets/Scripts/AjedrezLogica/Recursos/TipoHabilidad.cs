using System;
using System.Collections.Generic;
using System.Text;

namespace AjedrezLogica.Recursos
{
    public enum TipoHabilidad
    {
        vacio = 0,

        // Peon
        Mimico = 1,
        Reverso = 2,
        PasoForzado = 3,
        MovimientoCruzado = 4,

        // Torre
        JustaDefensa = 5,
        MuroImpenetrable = 6,

        // Caballo
        PuraSangre = 7,
        CozParalizante = 8,

        // Alfil
        Estratega = 9,
        AnuladorDeHabilidad = 10,

        // Dama
        DesplazamientoImperial = 11,
        EmbestidaReal = 12,

        // Rey
        DecretoReal = 13,
        SituacionDesesperada = 14
    }
}
