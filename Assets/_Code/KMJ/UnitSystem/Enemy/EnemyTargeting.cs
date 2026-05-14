using UnityEngine;

namespace EnemySystem
{
    public class EnemyTargeting : MonoBehaviour
    {
        [SerializeField] private GameObject targetingSpere;

        public void Targeting()
        {
            targetingSpere.SetActive(true);
        }

        public void OffTargeting()
        {
            targetingSpere.SetActive(false);
        }
    }
}