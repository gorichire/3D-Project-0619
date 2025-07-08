using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RPG.Boss
{
    public class ZigZagMover : MonoBehaviour
    {
        [Header("타겟 오브젝트")]
        public Transform positionTarget;
        public Transform aimTarget;

        [Header("패트롤 설정")]
        public Vector3 center = Vector3.zero;
        public float roamRadius = 10f;
        public float moveSpeed = 2f;
        public float waitTime = 1f;

        [Header("지그재그 설정")]
        public float zigzagAmplitude = 0.5f;
        public float zigzagFrequency = 2f;

        private Vector3 startPos;
        private Vector3 targetPos;
        private float moveTimer = 0f;
        private bool isMoving = false;
        private float waitTimer = 0f;

        public bool IsMoving => isMoving;
        public bool IsWaiting => !isMoving && waitTimer > 0f;
        public bool IsDone => !isMoving && waitTimer <= 0f;

        void Start()
        {
            PickNewTarget();
        }

        void Update()
        {
            if (!isMoving)
            {
                waitTimer -= Time.deltaTime;
                return;
            }

            float totalDist = Vector3.Distance(startPos, targetPos);
            if (totalDist < 0.1f) totalDist = 0.1f;

            moveTimer += Time.deltaTime;
            float t = Mathf.Clamp01(moveTimer * moveSpeed / totalDist);

            Vector3 linearPos = Vector3.Lerp(startPos, targetPos, t);
            Vector3 moveDir = (targetPos - startPos).normalized;
            Vector3 flatDir = new Vector3(moveDir.x, 0, moveDir.z);
            Vector3 sideDir = Vector3.Cross(flatDir, Vector3.up);
            Vector3 offset = sideDir * Mathf.Sin(t * Mathf.PI * zigzagFrequency) * zigzagAmplitude;

            Vector3 finalPos = linearPos + offset;
            positionTarget.position = finalPos;
            aimTarget.position = finalPos;

            if (t >= 1f)
            {
                isMoving = false;
                waitTimer = waitTime;
            }
        }

        public void PickNewTarget()
        {
            Vector2 rand = Random.insideUnitCircle * roamRadius;
            targetPos = center + new Vector3(rand.x, 0f, rand.y);
            startPos = positionTarget.position;
            moveTimer = 0f;
            isMoving = true;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(center == Vector3.zero ? transform.position : center, roamRadius);
        }
    }
}