namespace LifeSimulation
{
    public class Plant : ISimulationObject
    {
        internal Plant()
        {
            Location = new Location(-1, -1);
        }

        public Location Location { get; set; }
    }
}