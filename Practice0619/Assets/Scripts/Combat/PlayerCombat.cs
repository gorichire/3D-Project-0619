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
                    Debug.Log("�޺� ����: comboIndex 1 Ʈ���� �ߵ�");
                    comboIndex = 1;
                    animator.SetInteger("comboIndex", comboIndex);
                    animator.SetTrigger("comboAttack");
                }
            }
        }
        void PlayComboAnimation(int index)
        {
            animator.SetTrigger("comboAttack");
            animator.SetInteger("comboIndex", index); // Blend Tree or ���� �б��
        }

        // �ִϸ��̼� �̺�Ʈ�� ȣ��
        public void EnableHitbox() => swordHitbox.Activate();
        public void DisableHitbox() => swordHitbox.Deactivate();

        // �ִϸ��̼� �̺�Ʈ: �޺� �Է��� ���� �� �ִ� ����
        public void AllowCombo()
        {
            canCombo = true;
        }

        // �ִϸ��̼� �̺�Ʈ: �޺� �Է� ����
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
