using UnityEngine;
using UnityEngine.AI;
using RPG.BehaviourTree;
using static RPG.BehaviourTree.Node;
using RPG.Boss.Centipede;

namespace RPG.BehaviourTree.Centipede
{
    /// <summary>
    /// ���׺��� ���� ���� ���.
    /// - ������ ���۵Ǹ� �Ӹ�/���� ������ SlitherRig(�� ������)�� ����
    /// - NavMeshAgent �� �÷��̾ ���� ������ ����
    /// - ���� �Ÿ� �ȿ� ���ų�, �ִ� �ð��� �ʰ��ϸ� Success ��ȯ
    ///   (��ٿ� Decorator �� Success �� ��Ƽ� ��Ÿ���� ����)
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

            // ��� �÷��̾ ���󰡵��� ���� (����� �̵� ����)
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
