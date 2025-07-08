using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


namespace RPG.Boss
{
    public class BossAIController : MonoBehaviour
    {
        private Node rootNode;

        [Header("컴포넌트 연결")]
        public ZigZagMover zigZagMover;

        void Start()
        {
            ZigZagPatrolNode patrolNode = new ZigZagPatrolNode(zigZagMover);

            Selector attackSelector = new Selector(new List<Node>
            {
                // new FireProjectileNode(...),
                // new ChargeAttackNode(...),
            });

            rootNode = new Sequence(new List<Node>
            {
                patrolNode,
                attackSelector
            });
        }

        void Update()
        {
            if (rootNode != null)
                rootNode.Evaluate();
        }
    }
}