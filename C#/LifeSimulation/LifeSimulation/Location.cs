namespace LifeSimulation
{
    public class Location
    {
        public Location(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public Location DeepClone()
        {
            var result = (Location)MemberwiseClone();
            return result;
        }
    }
}