using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Boss
{
    public class Sequence : Node
    {
        private List<Node> nodes;

        public Sequence(List<Node> nodes)
        {
            this.nodes = nodes;
        }

        public override NodeState Evaluate()
        {
            foreach (var node in nodes)
            {
                NodeState result = node.Evaluate();
                if (result != NodeState.Success)
                    return result;
            }
            return NodeState.Success;
        }
    }
}