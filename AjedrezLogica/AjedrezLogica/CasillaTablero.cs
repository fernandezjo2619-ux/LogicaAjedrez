namespace AjedrezLogica
{
    public struct CasillaTablero
    {
        public (int X, int Y) Posicion { get; set; }
        public Pieza Ocupante { get; set; }

        public bool EstaOcupado => Ocupante != null;

    }
}
