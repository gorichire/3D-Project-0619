using System.Collections;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace RPG.Boss.Centipede
{
    /// <summary>
    /// ���� ���� Rig ���̾� ���� ��Ʈ�ѷ�.
    /// - SlitherRig  : �⺻ �� ������ (Weight 1 ����)
    /// - BodyLiftRig : �Ӹ����� ���ø��� + ���� ���� (���� 2��3)
    /// - HeadAimRig  : �Ӹ� Aim (���� 2��3)
    ///
    /// �׼� ���/AI ��������:
    ///    ���� ����  �� SetChargePose();       (Lift/Aim 0)
    ///    Lift ���� �� StartLiftPose();       (Lift/Aim 0��1)
    ///    ���� ���� �� EndLiftPose();         (Lift/Aim 1��0)
    /// </summary>
    public class CentipedeRigController : MonoBehaviour
    {
        [Header("Rig Layers (RigBuilder ���� ����)")]
        [SerializeField] private Rig slitherRig;
        [SerializeField] private Rig bodyLiftRig;
        [SerializeField] private Rig headAimRig;

        [Header("Blend Settings")]
        [SerializeField] private float blendDuration = 0.35f;
        [SerializeField] private Transform mouthRoot;      // �� ��ü�� ��ǥ�ϴ� Transform (�޸Ӹ� ���� �ڽĵ� ������)
        [SerializeField] private Transform projectileSpawn; // ����ü ���� ����Ʈ (mouthRoot�� �ڽ�, ���� ����)
        [SerializeField] private float windUpDist = 0.35f;  // �ڷ� �糢�� �Ÿ�
        [SerializeField] private float windUpTime = 0.18f;  // �ڷ� ���� �ð�
        [SerializeField] private float forwardTime = 0.10f; // ������ �и� �߻��ϴ� �ð�
        [SerializeField] private float fireNormTime = 0.25f;// ������ �̵� ���� �� �� % �������� �߻����� (0~1)

        [SerializeField] GameObject debugProjectile;

        private Coroutine blendRoutine;

        public Transform ProjectileSpawn => projectileSpawn;

        private void Awake()
        {
            if (slitherRig) slitherRig.weight = 1f; // �׻� ����
        }
#if UNITY_EDITOR
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.P))
            {
                StartCoroutine(PlaySpitThrust(() =>
                {
                    Debug.Log("<color=green>Spit FIRE (manual test)</color>");
                    TestSpawnProjectile(); // �Ʒ� �Լ�
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
        /// ���� 1(����)�� : Lift/Aim�� 0���� ���� �⺻ �����̴����� ����.
        /// </summary>
        public void SetChargePose()
        {
            StartBlend(0f, 0f);
        }

        /// <summary>
        /// ���� 2��3 ���� : Lift/Aim�� 1�� �÷� �Ӹ��� ���ø��� �÷��̾ Aim.
        /// </summary>
        public void StartLiftPose()
        {
            StartBlend(1f, 1f);
        }

        /// <summary>
        /// ���� 2��3 ���� �� Lift/Aim �ٽ� 0����.
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
            StartLiftPose();  // �Ӹ� ��� �÷��̾� ���� (HeadAimRig weight��)

            // ���� ���� ��ǥ ����
            Vector3 start = mouthRoot.localPosition;

            // �ڷ� �糢�� ��ǥ (mouthRoot.forward �� "���� �ٶ󺸴� ����"�̶�� ����)
            Vector3 back = start - mouthRoot.forward * windUpDist;

            // 1) �ڷ� �糢��
            yield return LerpLocal(mouthRoot, start, back, windUpTime);

            // 2) ������ ���ƿ��鼭 Ư�� Ÿ�ֿ̹� �߻�
            float t = 0f;
            bool fired = false;
            while (t < forwardTime)
            {
                t += Time.deltaTime;
                float norm = Mathf.Clamp01(t / forwardTime);

                // ��ġ ����: back -> start
                mouthRoot.localPosition = Vector3.Lerp(back, start, norm);

                // ���� �������� �߻� (��: forward ������ 25% �α�)
                if (!fired && norm >= fireNormTime)
                {
                    fired = true;
                    onFire?.Invoke();
                }
                yield return null;
            }
            mouthRoot.localPosition = start;

            EndLiftPose(); // �ʿ� �� ���� (���Ϻ� ������ �Ÿ� �ּ�)
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
