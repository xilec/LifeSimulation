namespace LifeSimulation
{
    /// <summary>
    /// Смещения координат на поле с агентами
    /// </summary>
    internal struct Offset
    {
        internal Offset(int dx, int dy)
        {
            Dx = dx;
            Dy = dy;
        }

        internal int Dx { get; private set; }
        internal int Dy { get; private set; }
    }
}