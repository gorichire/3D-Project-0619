using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
namespace RPG.Boss
{
    public class ZigZagPatrolNode : Node
    {
        private ZigZagMover mover;

        public ZigZagPatrolNode(ZigZagMover mover)
        {
            this.mover = mover;
        }

        public override NodeState Evaluate()
        {
            if (mover == null)
            {
                UnityEngine.Debug.LogWarning("ZigZagMover�� ������� �ʾҽ��ϴ�.");
                return NodeState.Failure;
            }

            if (mover.IsDone)
            {
                return NodeState.Success;
            }

            return NodeState.Running;
        }
    }
}