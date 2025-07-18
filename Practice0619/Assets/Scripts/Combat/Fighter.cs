using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RPG.Movement;
using RPG.Core;
using RPG.Saving;
using RPG.Attributes;
using RPG.Stats;
using System;
using RPG.Utils;
using Newtonsoft.Json.Linq;

namespace RPG.Combat
{
    public class Fighter : MonoBehaviour, IAction , ISaveable , IModifierProvider /*, IJsonSaveable*/
    {
        [SerializeField] float timeBetweenAttacks = 1f;
        [SerializeField] Transform rightHandTransform  = null;
        [SerializeField] Transform leftHandTransform = null;
        [SerializeField] WeaponConfig defaultWeapon = null;

        Health target;
        Weapon equippedWeapon;
        float timeSinceLastAttack = Mathf.Infinity;
        WeaponConfig currentWeaponConfig;
        LazyValue<Weapon> currentWeapon;
        PlayerCombat playerCombat;

        private void Awake()
        {
            PlayerCombat playerCombat = GetComponent<PlayerCombat>();
            currentWeaponConfig = defaultWeapon;
            currentWeapon = new LazyValue<Weapon>(SetupDefaultWeapon);
        }

        private Weapon SetupDefaultWeapon()
        {
            Weapon weaponInstance = AttachWeapon(defaultWeapon);
            equippedWeapon = weaponInstance;  
            return weaponInstance;
        }
        private void Start()
        {
            currentWeapon.ForceInit();
        }

        private void Update()
        {
            timeSinceLastAttack += Time.deltaTime;

            if (target == null) return;
            if (target.IsDead()) return;

            if (!GetIsInRange(target.transform))
            {
                GetComponent<Mover>().MoveTo(target.transform.position, 1f);
            }
            else
            {
                GetComponent<Mover>().Cancel();
                AttackBehaviour();
            }
        }

        public void EquipWeapon(WeaponConfig weapon)
        {
            //currentWeaponConfig = weapon;
            //currentWeapon.value = AttachWeapon(weapon);

            currentWeaponConfig = weapon;
            Weapon weaponInstance = AttachWeapon(weapon);

            if (weaponInstance == null)
            {
                if (weapon != defaultWeapon) EquipWeapon(defaultWeapon);
                return;
            }

            currentWeapon.value = weaponInstance;
            equippedWeapon = weaponInstance;

            Transform effectPoint = weaponInstance.transform.Find("EffectPoint");


            var playerCombat = GetComponent<PlayerCombat>();
            if (playerCombat != null)
            {
                playerCombat.SetSwordHitbox(weaponInstance.GetComponentInChildren<SwordHitbox>());
            }

            var swordHitbox = weaponInstance.GetComponentInChildren<SwordHitbox>();
            if (swordHitbox != null)
            {
                swordHitbox.SetOwner(gameObject);

                if (playerCombat != null)
                {
                    playerCombat.SetSwordHitbox(swordHitbox);
                }
            }
        }
        private Weapon AttachWeapon(WeaponConfig weapon)
        {
            Animator animator = GetComponent<Animator>();
            return weapon.Spawn(rightHandTransform, leftHandTransform, animator);
        }

        public Health GetTarget()
        {
            return target;
        }

        private void AttackBehaviour()
        {
            transform.LookAt(target.transform);
            if (timeSinceLastAttack > timeBetweenAttacks)
            {
                TriggerAttack();
                timeSinceLastAttack = 0;
            }
        }

        private void TriggerAttack()
        {
            GetComponent<Animator>().ResetTrigger("stopAttack"); 
            GetComponent<Animator>().SetTrigger("attack");
        }
        
        // 애니메이션 이벤트
        void Hit()
        {
            if (target == null) { return; }
            float damage = GetComponent<BaseStats>().GetStat(Stat.Damage);
            if (currentWeapon.value != null)
            {
                currentWeapon.value.OnHit();
            }
            if (currentWeaponConfig.HasProjectile())
            {
                currentWeaponConfig.LaunchProjectile(rightHandTransform, leftHandTransform, target, gameObject, damage);
            }
            else
            {
                target.TakeDamage(gameObject, damage);
            }
        }

        void Shoot()
        {
            Hit();
        }

        private bool GetIsInRange(Transform targetTransform)
        {
            return Vector3.Distance(transform.position, targetTransform.position) < currentWeaponConfig.GetRange();
        }

        public bool CanAttack(GameObject combatTarget)
        {
            if (combatTarget == null) { return false; }
            if (!GetComponent<Mover>().CanMoveTo(combatTarget.transform.position) &&
                !GetIsInRange(combatTarget.transform))
            {
                return false;
            }
            Health targetToTest = combatTarget.GetComponent<Health>();
            return targetToTest != null && !targetToTest.IsDead();
        }

        public void Attack(GameObject combatTarget)
        {
            GetComponent<ActionSchduler>().StartAction(this);
            target = combatTarget.GetComponent<Health>();
        }

        public void Cancel()
        {
            StopAttack();
            target = null;
            GetComponent<Mover>().Cancel();
        }

        private void StopAttack()
        {
            GetComponent<Animator>().ResetTrigger("attack");
            GetComponent<Animator>().SetTrigger("stopAttack");
        }
        public IEnumerable<float> GetAdditiveModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetDamage();
            }
        }
        public IEnumerable<float> GetPercentageModifiers(Stat stat)
        {
            if (stat == Stat.Damage)
            {
                yield return currentWeaponConfig.GetPercentageBonus();
            }
        }
        public Weapon GetCurrentWeapon()
        {
            //Debug.Log($"[Fighter] GetCurrentWeapon() → {equippedWeapon}");
            return equippedWeapon;
        }
        public float CalculateAttackDamage()
        {
            // 오른쪽 클릭 공격에 쓰는 공식 (예시)
            return currentWeaponConfig.GetDamage() + GetComponent<BaseStats>().GetStat(Stat.Damage);
        }

        public object CaptureState()
        {
            return currentWeaponConfig.name;
        }

        public void RestoreState(object state)
        {
            string weaponName = (string)state;

            if (string.IsNullOrEmpty(weaponName))
            {
                EquipWeapon(defaultWeapon);
                return;
            }

            WeaponConfig weapon = UnityEngine.Resources.Load<WeaponConfig>(weaponName);

            if (weapon == null)
            {
                EquipWeapon(defaultWeapon);
                return;
            }

            EquipWeapon(weapon);
        }
        //public JToken CaptureAsJToken()
        //{
        //    return JToken.FromObject(currentWeaponConfig.name);
        //}

        //public void RestoreFromJToken(JToken state)
        //{
        //    string weaponName = state.ToObject<string>();
        //    WeaponConfig weapon = UnityEngine.Resources.Load<WeaponConfig>(weaponName);
        //    EquipWeapon(weapon);
        //}


    }
}
