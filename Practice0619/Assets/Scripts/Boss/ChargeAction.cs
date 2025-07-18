using UnityEngine;
using UnityEngine.AI;
using RPG.BehaviourTree;
using static RPG.BehaviourTree.Node;
using RPG.Boss.Centipede;

namespace RPG.BehaviourTree.Centipede
{
    /// <summary>
    /// 지네보스 돌진 패턴 노드.
    /// - 패턴이 시작되면 머리/몸을 내리고 SlitherRig(뱀 움직임)만 유지
    /// - NavMeshAgent 로 플레이어를 향해 빠르게 돌진
    /// - 지정 거리 안에 들어가거나, 최대 시간을 초과하면 Success 반환
    ///   (쿨다운 Decorator 가 Success 를 잡아서 쿨타임을 시작)
    /// </summary>
    public class ChargeAction : ActionNode
    {
        private readonly Transform bossTransform;
        private readonly NavMeshAgent agent;
        private readonly Transform player;
        private readonly float chargeSpeed;
        private readonly float stopDistance;
        private readonly float maxDuration;

        private float timer;

        private readonly CentipedeRigController rigCtrl;

        public ChargeAction(
            Transform bossTransform,
            NavMeshAgent agent,
            Transform player,
            CentipedeRigController rigCtrl,
            float chargeSpeed = 6f,
            float stopDistance = 1.5f,
            float maxDuration = 3f)
        {
            this.bossTransform = bossTransform;
            this.agent = agent;
            this.player = player;
            this.chargeSpeed = chargeSpeed;
            this.stopDistance = stopDistance;
            this.maxDuration = maxDuration;
            this.rigCtrl = rigCtrl;
        }

        protected override void OnStart()
        {
            timer = 0f;
            agent.isStopped = false;
            agent.speed = chargeSpeed;
            agent.stoppingDistance = stopDistance;
            agent.SetDestination(player.position);

            rigCtrl?.SetChargePose();
        }

        protected override NodeState Tick(float deltaTime)
        {
            timer += deltaTime;

            // 계속 플레이어를 따라가도록 갱신 (사소한 이동 보정)
            if (!agent.pathPending)
            {
                agent.SetDestination(player.position);
            }

            bool reached = !agent.pathPending && agent.remainingDistance <= stopDistance;
            bool timeout = timer >= maxDuration;

            if (reached || timeout)
            {
                return NodeState.Success;
            }

            return NodeState.Running;
        }

        protected override void OnEnd()
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
    }
}
