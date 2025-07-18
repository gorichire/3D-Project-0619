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
        private bool _started = false;   

        public NodeState Update(float deltaTime)
        {
            if (!_started)
            {
                OnStart();
                _started = true;
            }

            NodeState result = Tick(deltaTime);

            if (result != NodeState.Running)
            {
                OnEnd();
                _started = false;
            }

            return result;
        }

        protected virtual void OnStart() { }

        protected virtual void OnEnd() { }

        protected abstract NodeState Tick(float deltaTime);

        public abstract class CompositeNode : Node
        {
            protected readonly List<Node> children = new List<Node>();

            protected CompositeNode(params Node[] nodes)
            {
                children.AddRange(nodes);
            }

            public void AddChild(Node node) => children.Add(node);
        }

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
                    _current++;
                }
                _current = 0;
                return NodeState.Success;
            }
        }

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
                    _current++;
                }
                _current = 0;
                return NodeState.Failure;
            }
        }

        public abstract class DecoratorNode : Node
        {
            protected Node child;

            protected DecoratorNode(Node child)
            {
                this.child = child;
            }
        }

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
                if (timer > 0f) timer -= deltaTime;

                if (timer > 0f) return NodeState.Failure;

                NodeState state = child.Update(deltaTime);

                if (state == NodeState.Success)
                {
                    timer = cooldown;
                }

                return state;
            }
        }

        public abstract class ActionNode : Node { }

        public abstract class ConditionNode : Node
        {
            protected override NodeState Tick(float deltaTime)
            {
                return Evaluate() ? NodeState.Success : NodeState.Failure;
            }
            protected abstract bool Evaluate();
        }

    }
}
