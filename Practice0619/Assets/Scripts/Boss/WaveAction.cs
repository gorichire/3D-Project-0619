using UnityEngine;
using RPG.BehaviourTree;
using RPG.Boss.Centipede;
using static RPG.BehaviourTree.Node;

namespace RPG.BehaviourTree.Centipede
{
    /// <summary>지네보스 웨이브 발사 패턴.</summary>
    public class WaveAction : ActionNode
    {
        private readonly CentipedeRigController rig;
        private readonly Transform player;
        private readonly GameObject wavePrefab;
        private readonly float waveSpeed;
        private readonly float maxRange;
        private readonly bool flattenY;

        private NodeState state;
        private bool fired;
        private Coroutine routine;

        public WaveAction(
            CentipedeRigController rig,
            Transform player,
            GameObject wavePrefab,
            float waveSpeed = 8f,
            float maxRange = 12f,
            bool flattenY = true)
        {
            this.rig = rig;
            this.player = player;
            this.wavePrefab = wavePrefab;
            this.waveSpeed = waveSpeed;
            this.maxRange = maxRange;
            this.flattenY = flattenY;
        }

        protected override void OnStart()
        {
            state = NodeState.Running;
            fired = false;
            if (routine == null)
                routine = rig.StartCoroutine(rig.PlaySpitThrust(FireWave)); // Spit과 동일 모션 재사용
        }

        protected override void OnEnd()
        {
            routine = null;
        }

        protected override NodeState Tick(float dt)
        {
            // 사거리 체크
            if (Vector3.Distance(rig.transform.position, player.position) > maxRange)
                return NodeState.Failure;

            return state;
        }

        private void FireWave()
        {
            if (fired) return;
            fired = true;

            Transform spawn = rig.ProjectileSpawn;
            Vector3 dir = player.position - spawn.position;
            if (flattenY) dir.y = 0f;
            if (dir.sqrMagnitude < 0.0001f) dir = spawn.forward;

            Quaternion rot = Quaternion.LookRotation(dir.normalized);
            GameObject go = Object.Instantiate(wavePrefab, spawn.position, rot);

            // 속도 적용(있으면)
            if (go.TryGetComponent<Rigidbody>(out var rb))
                rb.velocity = rot * Vector3.forward * waveSpeed;

            state = NodeState.Success;
        }
    }
}
