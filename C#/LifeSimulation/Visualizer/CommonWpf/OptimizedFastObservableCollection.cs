using System.Collections.Generic;
using Catel.Collections;

namespace Visualizer.CommonWpf
{
    internal class OptimizedFastObservableCollection<T> : FastObservableCollection<T>
    {
        internal OptimizedFastObservableCollection() : base()
        {            
        }

        internal OptimizedFastObservableCollection(int capacity)
        {
            var list = Items as List<T>;
            list.Capacity = capacity;
        }

        internal OptimizedFastObservableCollection(IEnumerable<T> collection) : base(collection)
        {
        }
    }
}