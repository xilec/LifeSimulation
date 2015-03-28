using Visualizer.CommonWpf;

namespace Visualizer.ViewModels
{
    public class AgentStatisticsViewModel : ViewModelBase
    {
        private int _count;
        private int _reproductions;
        private int _deathes;
        private int _maxAges;

        public AgentStatisticsViewModel(int count, int reproductions, int deathes, int maxAges)
        {
            _count = count;
            _reproductions = reproductions;
            _deathes = deathes;
            _maxAges = maxAges;
        }

        public int Count
        {
            get { return _count; }
        }

        public int Reproductions
        {
            get { return _reproductions; }
        }

        public int Deathes
        {
            get { return _deathes; }
        }

        public int MaxAges
        {
            get { return _maxAges; }
        }
    }
}