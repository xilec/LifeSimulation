namespace LifeSimulation
{
    /// <summary>
    /// Смещения координат на поле с агентами
    /// </summary>
    public struct Offset
    {
        public Offset(int dx, int dy)
        {
            Dx = dx;
            Dy = dy;
        }

        public int Dx { get; set; }
        public int Dy { get; set; }
    }
}