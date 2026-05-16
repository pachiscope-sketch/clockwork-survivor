using UnityEngine;

namespace ClockworkSurvivor
{
    [CreateAssetMenu(menuName = "Clockwork Survivor/Weapon Config")]
    public class WeaponConfig : ScriptableObject
    {
        public string weaponName = "Clock Bolt";
        public GameObject projectilePrefab;
        public float damage = 8f;
        public float fireRate = 2.2f;
        public float range = 13f;
        public float projectileSpeed = 15f;
        public float projectileLifetime = 1.2f;
        public int projectileCount = 1;
        public float spreadAngle = 12f;
    }
}
