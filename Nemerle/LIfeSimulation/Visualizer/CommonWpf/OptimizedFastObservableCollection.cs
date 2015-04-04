using System.Collections.Generic;
using Catel.Collections;

namespace Visualizer.CommonWpf
{
    public class OptimizedFastObservableCollection<T> : FastObservableCollection<T>
    {
        public OptimizedFastObservableCollection() : base()
        {            
        }

        public OptimizedFastObservableCollection(int capacity)
        {
            var list = Items as List<T>;
            list.Capacity = capacity;
        }

        public OptimizedFastObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }
    }
}