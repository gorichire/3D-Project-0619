using UnityEngine;
using RPG.Combat; // Fighter, WeaponConfig 등 참조용

namespace RPG.Control
{
    public class PlayerWeaponHotkeys : MonoBehaviour
    {
        [SerializeField] WeaponConfig unarmedWeapon;
        [SerializeField] WeaponConfig swordWeapon;
        [SerializeField] WeaponConfig bowWeapon;
        [SerializeField] WeaponConfig iceWeapon;

        Fighter fighter;

        private void Start()
        {
            fighter = GetComponent<Fighter>();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                fighter.EquipWeapon(unarmedWeapon);
            }
            if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                fighter.EquipWeapon(swordWeapon);
            }
            if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                fighter.EquipWeapon(bowWeapon);
            }
            if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                fighter.EquipWeapon(iceWeapon);
            }
        }
    }
}