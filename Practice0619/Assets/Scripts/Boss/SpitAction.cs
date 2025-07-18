using UnityEngine;
using RPG.BehaviourTree;
using RPG.Boss.Centipede;
using static RPG.BehaviourTree.Node;

namespace RPG.BehaviourTree.Centipede
{
    public class SpitAction : ActionNode
    {
        private readonly CentipedeRigController rig;
        private readonly Transform player;
        private readonly GameObject projectilePrefab;
        private readonly float projectileSpeed;
        private readonly float maxRange;

        private bool complete;
        private NodeState state;

        public SpitAction(
            CentipedeRigController rig,
            Transform player,
            GameObject projectilePrefab,
            float projectileSpeed = 12f,
            float maxRange = 10f)
        {
            this.rig = rig;
            this.player = player;
            this.projectilePrefab = projectilePrefab;
            this.projectileSpeed = projectileSpeed;
            this.maxRange = maxRange;
        }

        private Coroutine spitRoutine;

        protected override void OnStart()
        {
            state = NodeState.Running;
            complete = false;

            // 이미 돌고 있으면 새로 시작 X
            if (spitRoutine == null)
                spitRoutine = rig.StartCoroutine(rig.PlaySpitThrust(FireProjectile));
        }

        protected override void OnEnd()
        {
            spitRoutine = null; // rig 코루틴 자체는 내부에서 끝남
        }

        protected override NodeState Tick(float dt)
        {
            // 사거리 체크: 너무 멀면 실패.
            if (Vector3.Distance(rig.transform.position, player.position) > maxRange)
                return NodeState.Failure;

            return state; // Running or Success
        }

        private void FireProjectile()
        {
            if (complete) return;
            complete = true;

            Transform spawn = rig.ProjectileSpawn; // 공개 프로퍼티로 노출한다고 가정
            Quaternion rot = Quaternion.LookRotation(player.position - spawn.position);

            GameObject go = Object.Instantiate(projectilePrefab, spawn.position, rot);

            if (go.TryGetComponent<Rigidbody>(out var rb))
            {
                rb.velocity = go.transform.forward * projectileSpeed;
            }

            state = NodeState.Success;
        }

    }
}
