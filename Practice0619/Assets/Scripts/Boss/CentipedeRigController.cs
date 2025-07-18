using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace RPG.Boss.Centipede
{
    /// <summary>
    /// 지네 보스 Rig 레이어 블렌딩 컨트롤러.
    /// - SlitherRig  : 기본 뱀 움직임 (Weight 1 고정)
    /// - BodyLiftRig : 머리·몸 들어올리기 + 꼬리 고정 (패턴 2·3)
    /// - HeadAimRig  : 머리 Aim (패턴 2·3)
    ///
    /// 액션 노드/AI 로직에서:
    ///    돌진 시작  → SetChargePose();       (Lift/Aim 0)
    ///    Lift 패턴 → StartLiftPose();       (Lift/Aim 0→1)
    ///    패턴 종료 → EndLiftPose();         (Lift/Aim 1→0)
    /// </summary>
    public class CentipedeRigController : MonoBehaviour
    {
        [Header("Rig Layers (RigBuilder 순서 기준)")]
        [SerializeField] private Rig slitherRig;
        [SerializeField] private Rig bodyLiftRig;
        [SerializeField] private Rig headAimRig;

        [Header("Blend Settings")]
        [SerializeField] private float blendDuration = 0.35f;
        [SerializeField] private Transform mouthRoot;      // 입 전체를 대표하는 Transform (뒷머리 포함 자식들 끌려옴)
        [SerializeField] private Transform projectileSpawn; // 투사체 스폰 포인트 (mouthRoot의 자식, 정면 앞쪽)
        [SerializeField] private float windUpDist = 0.35f;  // 뒤로 재끼는 거리
        [SerializeField] private float windUpTime = 0.18f;  // 뒤로 가는 시간
        [SerializeField] private float forwardTime = 0.10f; // 앞으로 밀며 발사하는 시간
        [SerializeField] private float fireNormTime = 0.25f;// 앞으로 이동 구간 중 몇 % 지점에서 발사할지 (0~1)

        [SerializeField] GameObject debugProjectile;

        private Coroutine blendRoutine;

        public Transform ProjectileSpawn => projectileSpawn;

        private void Awake()
        {
            if (slitherRig) slitherRig.weight = 1f; // 항상 켜짐
        }
#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                StartCoroutine(PlaySpitThrust(() =>
                {
                    Debug.Log("<color=green>Spit FIRE (manual test)</color>");
                    TestSpawnProjectile(); // 아래 함수
                }));
            }
        }

        private void TestSpawnProjectile()
        {
            if (!debugProjectile) return; // SerializeField GameObject debugProjectile
            Transform spawn = projectileSpawn;
            var go = Instantiate(debugProjectile, spawn.position, spawn.rotation);
            if (go.TryGetComponent<Rigidbody>(out var rb))
                rb.velocity = spawn.forward * 12f;
        }
#endif

        /// <summary>
        /// 패턴 1(돌진)용 : Lift/Aim을 0으로 내려 기본 슬라이더링만 유지.
        /// </summary>
        public void SetChargePose()
        {
            StartBlend(0f, 0f);
        }

        /// <summary>
        /// 패턴 2·3 진입 : Lift/Aim을 1로 올려 머리를 들어올리고 플레이어를 Aim.
        /// </summary>
        public void StartLiftPose()
        {
            StartBlend(1f, 1f);
        }

        /// <summary>
        /// 패턴 2·3 종료 → Lift/Aim 다시 0으로.
        /// </summary>
        public void EndLiftPose()
        {
            StartBlend(0f, 0f);
        }

        private void StartBlend(float targetLift, float targetAim)
        {
            if (blendRoutine != null) StopCoroutine(blendRoutine);
            blendRoutine = StartCoroutine(BlendRoutine(targetLift, targetAim));
        }

        private IEnumerator BlendRoutine(float targetLift, float targetAim)
        {
            float t = 0f;
            float startLift = bodyLiftRig ? bodyLiftRig.weight : 0f;
            float startAim = headAimRig ? headAimRig.weight : 0f;

            while (t < blendDuration)
            {
                t += Time.deltaTime;
                float w = t / blendDuration;

                if (bodyLiftRig)
                    bodyLiftRig.weight = Mathf.Lerp(startLift, targetLift, w);

                if (headAimRig)
                    headAimRig.weight = Mathf.Lerp(startAim, targetAim, w);

                yield return null;
            }

            if (bodyLiftRig) bodyLiftRig.weight = targetLift;
            if (headAimRig) headAimRig.weight = targetAim;
            blendRoutine = null;
        }

        public IEnumerator PlaySpitThrust(System.Action onFire)
        {
            StartLiftPose();  // 머리 들고 플레이어 조준 (HeadAimRig weight↑)

            // 시작 로컬 좌표 저장
            Vector3 start = mouthRoot.localPosition;

            // 뒤로 재끼는 목표 (mouthRoot.forward 가 "입이 바라보는 방향"이라고 가정)
            Vector3 back = start - mouthRoot.forward * windUpDist;

            // 1) 뒤로 재끼기
            yield return LerpLocal(mouthRoot, start, back, windUpTime);

            // 2) 앞으로 돌아오면서 특정 타이밍에 발사
            float t = 0f;
            bool fired = false;
            while (t < forwardTime)
            {
                t += Time.deltaTime;
                float norm = Mathf.Clamp01(t / forwardTime);

                // 위치 보간: back -> start
                mouthRoot.localPosition = Vector3.Lerp(back, start, norm);

                // 지정 시점에서 발사 (예: forward 구간의 25% 부근)
                if (!fired && norm >= fireNormTime)
                {
                    fired = true;
                    onFire?.Invoke();
                }
                yield return null;
            }
            mouthRoot.localPosition = start;

            EndLiftPose(); // 필요 시 끄기 (패턴별 유지할 거면 주석)
        }
        private IEnumerator LerpLocal(Transform tr, Vector3 from, Vector3 to, float dur)
        {
            float t = 0f;
            while (t < dur)
            {
                t += Time.deltaTime;
                tr.localPosition = Vector3.Lerp(from, to, t / dur);
                yield return null;
            }
            tr.localPosition = to;
        }
    }

}
