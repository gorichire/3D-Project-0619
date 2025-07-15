using UnityEngine;

namespace RPG.Combat
{
    public class PlayerCombat : MonoBehaviour
    {
        Animator animator;
        SwordHitbox swordHitbox;
        Weapon currentWeapon;

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
                    Debug.Log("콤보 시작: comboIndex 1 트리거 발동");
                    comboIndex = 1;
                    animator.SetInteger("comboIndex", comboIndex);
                    animator.SetTrigger("comboAttack");
                }
            }
        }
        void PlayComboAnimation(int index)
        {
            animator.SetTrigger("comboAttack");
            animator.SetInteger("comboIndex", index); // Blend Tree or 상태 분기용
        }

        // 애니메이션 이벤트로 호출
        public void EnableHitbox() => swordHitbox.Activate();
        public void DisableHitbox() => swordHitbox.Deactivate();

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
    }
}
