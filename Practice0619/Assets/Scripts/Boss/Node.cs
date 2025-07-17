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
        private bool _started = false;   // OnStart가 호출되었는지 플래그

        public NodeState Update(float deltaTime)
        {
            if (!_started)
            {
                OnStart();
                _started = true;
            }

            // 실제 실행 로직
            NodeState result = Tick(deltaTime);

            // Running이 아니면 이번 사이클 종료 → OnEnd 호출 후 초기화
            if (result != NodeState.Running)
            {
                OnEnd();
                _started = false;
            }

            return result;
        }
        /// <summary>
        /// 노드가 처음 실행될 때 1회 호출.
        /// </summary>
        protected virtual void OnStart() { }

        /// <summary>
        /// 노드가 Success 또는 Failure로 끝날 때 1회 호출.
        /// </summary>
        protected virtual void OnEnd() { }

        /// <summary>
        /// 실제 행동을 구현하는 부분. 매 프레임 호출된다.
        /// </summary>
        protected abstract NodeState Tick(float deltaTime);

        /// <summary>
        /// 자식 노드를 여러 개 가질 수 있는 공통 기반.
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
        /// Sequence(→) : 자식들을 차례로 실행.
        ///   - 하나라도 Failure → 즉시 Failure
        ///   - 모든 자식이 Success → Success
        ///   - 실행 중인 자식이 Running → Running 유지
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
                    // Success면 다음 자식으로 진행
                    _current++;
                }

                // 전부 성공
                _current = 0;
                return NodeState.Success;
            }
        }

        /// <summary>
        /// Selector(?) : 자식들을 위에서 아래로 검사.
        ///   - 하나라도 Success → 즉시 Success
        ///   - 모든 자식이 Failure → Failure
        ///   - 실행 중인 자식이 Running → Running 유지
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
                    // Failure면 다음 자식으로 진행
                    _current++;
                }

                // 전부 실패
                _current = 0;
                return NodeState.Failure;
            }
        }

        /* -------------------------------
         *  Decorator 노드
         * -------------------------------*/

        /// <summary>
        /// 자식 노드 1개를 감싸 동작을 수정·제어하는 베이스 클래스.
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
        /// Cooldown : 자식 노드가 성공했을 때 쿨타임을 시작.
        /// 쿨타임이 남아 있으면 무조건 Failure를 반환해 실행되지 않는다.
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
                // 타이머 감소
                if (timer > 0f) timer -= deltaTime;

                // 아직 쿨타임이면 실패 처리
                if (timer > 0f) return NodeState.Failure;

                // 자식 실행
                NodeState state = child.Update(deltaTime);

                // 성공이면 쿨타임 갱신
                if (state == NodeState.Success)
                {
                    timer = cooldown;
                }

                return state;
            }
        }

        /* -------------------------------
         *  Leaf 노드 (액션·조건) 베이스
         * -------------------------------*/

        /// <summary>
        /// 실제 행동(이동, 공격 등)을 구현할 때 상속.
        /// Tick 안에서 Running 유지 여부를 직접 판단한다.
        /// </summary>
        public abstract class ActionNode : Node { }

        /// <summary>
        /// 조건 노드. Evaluate()가 true면 Success, false면 Failure 반환.
        /// </summary>
        public abstract class ConditionNode : Node
        {
            protected override NodeState Tick(float deltaTime)
            {
                return Evaluate() ? NodeState.Success : NodeState.Failure;
            }

            /// <summary>
            /// 실제 조건 판정을 구현하는 메서드.
            /// </summary>
            protected abstract bool Evaluate();
        }

    }
}
