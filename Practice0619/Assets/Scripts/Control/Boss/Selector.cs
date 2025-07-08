using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Boss
{
    public class Selector : Node
    {
        private List<Node> nodes;

        public Selector(List<Node> nodes)
        {
            this.nodes = nodes;
        }

        public override NodeState Evaluate()
        {
            foreach (var node in nodes)
            {
                NodeState result = node.Evaluate();
                if (result == NodeState.Success || result == NodeState.Running)
                    return result;
            }
            return NodeState.Failure;
        }
    }
}