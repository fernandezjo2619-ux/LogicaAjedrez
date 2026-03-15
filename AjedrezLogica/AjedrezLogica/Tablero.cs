namespace AjedrezLogica
{
    public class Tablero
    {
        public CasillaTablero[,] Grid;
        public int Ancho {  get; private set; }
        public int Alto { get; private set; }

        public bool EsDentroDelTablero(int x, int y)
        {
            return x >= 0 && x < Ancho && y >= 0 && y < Alto;
        }

        public Tablero(int ancho, int alto)
        {
            this.Ancho = ancho;
            this.Alto = alto;

            Grid = new CasillaTablero[ancho, alto];

            for (int x = 0; x < ancho; x++)
            {
                for (int y = 0; y < alto; y++)
                {
                    Grid[x, y] = new CasillaTablero { Posicion = (x, y) };
                }
            }
        }
    }
}
