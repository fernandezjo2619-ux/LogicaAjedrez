using AjedrezLogica;
using AjedrezLogica.Recursos;
using System;

namespace AjedrezLogica
{
    public class RegistroMovimiento
    {
        public Pieza Pieza { get; set; }
        public int XOrigen { get; set; }
        public int YOrigen { get; set; }
        public int XFin { get; set; }
        public int YFin { get; set; }
        public Pieza? PiezaEmpujada { get; set; }
        public int? XOrigenEmpujada { get; set; }
        public int? YOrigenEmpujada { get; set; }
        public int? XFinEmpujada { get; set; }
        public int? YFinEmpujada { get; set; }
    }
}
