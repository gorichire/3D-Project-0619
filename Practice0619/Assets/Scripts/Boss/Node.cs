using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.BehaviourTree
{
    public enum NodeState
    {
        Success,
        Failure,
        Running
    }

    public abstract class Node
    {
        private bool _started = false;   // OnStart�� ȣ��Ǿ����� �÷���

        public NodeState Update(float deltaTime)
        {
            if (!_started)
            {
                OnStart();
                _started = true;
            }

            // ���� ���� ����
            NodeState result = Tick(deltaTime);

            // Running�� �ƴϸ� �̹� ����Ŭ ���� �� OnEnd ȣ�� �� �ʱ�ȭ
            if (result != NodeState.Running)
            {
                OnEnd();
                _started = false;
            }

            return result;
        }
        /// <summary>
        /// ��尡 ó�� ����� �� 1ȸ ȣ��.
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// ��尡 Success �Ǵ� Failure�� ���� �� 1ȸ ȣ��.
        /// </summary>
        protected virtual void OnEnd() { }

        /// <summary>
        /// ���� �ൿ�� �����ϴ� �κ�. �� ������ ȣ��ȴ�.
        /// </summary>
        protected abstract NodeState Tick(float deltaTime);

        /// <summary>
        /// �ڽ� ��带 ���� �� ���� �� �ִ� ���� ���.
        /// </summary>
        public abstract class CompositeNode : Node
        {
            protected readonly List<Node> children = new List<Node>();

            protected CompositeNode(params Node[] nodes)
            {
                children.AddRange(nodes);
            }

            public void AddChild(Node node) => children.Add(node);
        }
        /// <summary>
        /// Sequence(��) : �ڽĵ��� ���ʷ� ����.
        ///   - �ϳ��� Failure �� ��� Failure
        ///   - ��� �ڽ��� Success �� Success
        ///   - ���� ���� �ڽ��� Running �� Running ����
        /// </summary>
        public class Sequence : CompositeNode
        {
            private int _current = 0;

            public Sequence(params Node[] nodes) : base(nodes) { }

            protected override void OnEnd() { _current = 0; }

            protected override NodeState Tick(float deltaTime)
            {
                while (_current < children.Count)
                {
                    NodeState state = children[_current].Update(deltaTime);

                    if (state == NodeState.Running) return NodeState.Running;
                    if (state == NodeState.Failure)
                    {
                        _current = 0;
                        return NodeState.Failure;
                    }
                    // Success�� ���� �ڽ����� ����
                    _current++;
                }

                // ���� ����
                _current = 0;
                return NodeState.Success;
            }
        }

        /// <summary>
        /// Selector(?) : �ڽĵ��� ������ �Ʒ��� �˻�.
        ///   - �ϳ��� Success �� ��� Success
        ///   - ��� �ڽ��� Failure �� Failure
        ///   - ���� ���� �ڽ��� Running �� Running ����
        /// </summary>
        public class Selector : CompositeNode
        {
            private int _current = 0;

            public Selector(params Node[] nodes) : base(nodes) { }

            protected override void OnEnd() { _current = 0; }

            protected override NodeState Tick(float deltaTime)
            {
                while (_current < children.Count)
                {
                    NodeState state = children[_current].Update(deltaTime);

                    if (state == NodeState.Running) return NodeState.Running;
                    if (state == NodeState.Success)
                    {
                        _current = 0;
                        return NodeState.Success;
                    }
                    // Failure�� ���� �ڽ����� ����
                    _current++;
                }

                // ���� ����
                _current = 0;
                return NodeState.Failure;
            }
        }

        /* -------------------------------
         *  Decorator ���
         * -------------------------------*/

        /// <summary>
        /// �ڽ� ��� 1���� ���� ������ �����������ϴ� ���̽� Ŭ����.
        /// </summary>
        public abstract class DecoratorNode : Node
        {
            protected Node child;

            protected DecoratorNode(Node child)
            {
                this.child = child;
            }
        }

        /// <summary>
        /// Cooldown : �ڽ� ��尡 �������� �� ��Ÿ���� ����.
        /// ��Ÿ���� ���� ������ ������ Failure�� ��ȯ�� ������� �ʴ´�.
        /// </summary>
        public class Cooldown : DecoratorNode
        {
            private readonly float cooldown;
            private float timer;

            public Cooldown(Node child, float seconds) : base(child)
            {
                cooldown = seconds;
                timer = 0f;
            }

            protected override NodeState Tick(float deltaTime)
            {
                // Ÿ�̸� ����
                if (timer > 0f) timer -= deltaTime;

                // ���� ��Ÿ���̸� ���� ó��
                if (timer > 0f) return NodeState.Failure;

                // �ڽ� ����
                NodeState state = child.Update(deltaTime);

                // �����̸� ��Ÿ�� ����
                if (state == NodeState.Success)
                {
                    timer = cooldown;
                }

                return state;
            }
        }

        /* -------------------------------
         *  Leaf ��� (�׼ǡ�����) ���̽�
         * -------------------------------*/

        /// <summary>
        /// ���� �ൿ(�̵�, ���� ��)�� ������ �� ���.
        /// Tick �ȿ��� Running ���� ���θ� ���� �Ǵ��Ѵ�.
        /// </summary>
        public abstract class ActionNode : Node { }

        /// <summary>
        /// ���� ���. Evaluate()�� true�� Success, false�� Failure ��ȯ.
        /// </summary>
        public abstract class ConditionNode : Node
        {
            protected override NodeState Tick(float deltaTime)
            {
                return Evaluate() ? NodeState.Success : NodeState.Failure;
            }

            /// <summary>
            /// ���� ���� ������ �����ϴ� �޼���.
            /// </summary>
            protected abstract bool Evaluate();
        }

    }
}
