using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Attributes;

namespace RPG.Combat
{
    public class SwordHitbox : MonoBehaviour
    {
        [SerializeField] float damage = 10f;

        private GameObject owner;
        private HashSet<Health> alreadyHit = new HashSet<Health>();

        public void SetOwner(GameObject attacker)
        {
            owner = attacker;
        }

        public void Activate()
        {
            alreadyHit.Clear(); // �� ���� ���� �� �ʱ�ȭ
            GetComponent<Collider>().enabled = true;
        }

        public void Deactivate()
        {
            GetComponent<Collider>().enabled = false;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject == owner) return;

            Health targetHealth = other.GetComponent<Health>();
            if (targetHealth == null) return;
            if (targetHealth.IsDead()) return;

            // �ߺ� Ÿ�� ����
            if (alreadyHit.Contains(targetHealth)) return;

            targetHealth.TakeDamage(owner, damage);
            alreadyHit.Add(targetHealth);
        }
    }
}