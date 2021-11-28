using System;
using System.Collections.Generic;

namespace Utility.Graph
{
    public abstract class GraphSearchStrategy<T>
    {
        public abstract T Search(GridGraphNode<T> node, Predicate<T> selector);

        public abstract List<T> SelectNearby(GridGraphNode<T> origin, Predicate<T> selector);
    }

    public class GraphSearchDFS<T> : GraphSearchStrategy<T>
    {
        public override T Search(GridGraphNode<T> node, Predicate<T> selector)
        {
            // foreach (var neighbour in node.Neighbours)
            // {
            //     if(node.Neighbours)
            // }
            throw new NotImplementedException();
        }

        public override List<T> SelectNearby(GridGraphNode<T> origin, Predicate<T> selector)
        {
            throw new NotImplementedException();
        }
    }
    
    // public class GraphSearchBFS<T> : GraphSearchStrategy<T>
    // {
    //     public override void Search(GraphNode<T> node, Predicate<T> selector)
    //     {
    //         throw new NotImplementedException();
    //     }
    // }
    //
    // public class GraphSearchAStar<T> : GraphSearchStrategy<T>
    // {
    //     public override void Search(GraphNode<T> node, Predicate<T> selector)
    //     {
    //         throw new NotImplementedException();
    //     }
    // }
}