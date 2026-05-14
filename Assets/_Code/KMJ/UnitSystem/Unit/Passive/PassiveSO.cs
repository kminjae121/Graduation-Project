using UnityEngine;

namespace _Code.Passive
{
    [CreateAssetMenu(fileName = "Passive", menuName = "SO/UnitSO/Passive", order = 0)]
    public class PassiveSO : ScriptableObject
    {
        public string PassiveName;
        public string ClassName;
    }
}