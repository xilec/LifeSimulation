using LifeSimulation;
using Newtonsoft.Json;

namespace Visualizer
{
    public static class LandscapeSerializer
    {
        public static Landscape Deserialize(string serializedLandscape)
        {
            var result = JsonConvert.DeserializeObject<Landscape>(serializedLandscape);
            return result;
        }

        public static string Serialize(Landscape landscape)
        {
            var result = JsonConvert.SerializeObject(landscape);
            return result;
        }
    }
}