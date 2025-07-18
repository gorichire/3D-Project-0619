using UnityEngine;
using UnityEngine.AI;

namespace RPG.Combat
{
    public class PlayerCombat : MonoBehaviour
    {
        Animator animator;
        SwordHitbox swordHitbox;
        Weapon currentWeapon;

        public GameObject attackEffectPrefab;
        public GameObject combo4EffectPrefab;
        int comboIndex = 0;
        bool canCombo = false;
        bool inputBuffered = false;

        private void Start()
        {
            animator = GetComponent<Animator>();
            swordHitbox = GetComponentInChildren<SwordHitbox>();
            if (swordHitbox != null)
            {
                swordHitbox.SetOwner(gameObject);
            }
        }

        private void Update()
        {
            currentWeapon = GetComponent<Fighter>().GetCurrentWeapon();
        }


        public void TryComboAttack()
        {
            currentWeapon = GetComponent<Fighter>().GetCurrentWeapon();

            if (currentWeapon != null && currentWeapon.HasTag("Sword"))
            {
                if (canCombo)
                {
                    inputBuffered = true;
                }
                else if (comboIndex == 0)
                {
                    comboIndex = 1;
                    animator.SetInteger("comboIndex", comboIndex);
                    animator.SetTrigger("comboAttack");
                }
            }
        }
        void OnAnimatorMove()
        {
            if (IsComboAttacking())
            {
                transform.position += animator.deltaPosition;
                transform.rotation = animator.rootRotation; // 회전도 Root Motion에 맞추고 싶으면
            }
        }
        public bool IsComboAttacking()
        {
            // 애니메이터에서 콤보 공격에 "ComboAttack" 태그를 반드시 지정할 것!
            AnimatorStateInfo info = animator.GetCurrentAnimatorStateInfo(0);
            return info.IsTag("ComboAttack") && !animator.IsInTransition(0);
        }
        void PlayComboAnimation(int index)
        {
            animator.SetTrigger("comboAttack");
            animator.SetInteger("comboIndex", index); // Blend Tree or 상태 분기용
        }
        public void SpawnAttackEffect(float yRotation)
        {
            if (attackEffectPrefab == null) return;
            Quaternion rot = transform.rotation * Quaternion.Euler(0, 0, yRotation);
            Instantiate(attackEffectPrefab, transform.position, rot);
        }

        public void SpawnCombo4Effect(float offset = 1f)
        {
            if (combo4EffectPrefab == null) return;
            // 플레이어 앞쪽(정면) 방향으로 offset만큼 이동한 위치
            Vector3 spawnPos = transform.position + transform.forward * offset + transform.up * offset;
            Quaternion rot = transform.rotation; // 그냥 플레이어 회전값만 적용

            Instantiate(combo4EffectPrefab, spawnPos, rot);
        }

        // 애니메이션 이벤트로 호출
        public void EnableHitbox()
        {
            swordHitbox?.Activate();
        }
        public void DisableHitbox()
        {
            swordHitbox?.Deactivate();
        }

        // 애니메이션 이벤트: 콤보 입력을 받을 수 있는 시점
        public void AllowCombo()
        {
            canCombo = true;
        }

        // 애니메이션 이벤트: 콤보 입력 종료
        public void EndComboWindow()
        {
            canCombo = false;

            if (inputBuffered && comboIndex < 4)
            {
                comboIndex++;
                inputBuffered = false;
                PlayComboAnimation(comboIndex);
            }
            else
            {
                comboIndex = 0;
                inputBuffered = false;
            }
        }
        public void SetSwordHitbox(SwordHitbox hitbox)
        {
            swordHitbox = hitbox;
        }
    }

}
