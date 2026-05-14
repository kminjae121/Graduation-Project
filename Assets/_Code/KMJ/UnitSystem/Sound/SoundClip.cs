using UnityEngine;

namespace _Code.KMJ.UnitSystem.Sound
{
    public class SoundClip : MonoBehaviour
    {
        [field: SerializeField] public AudioClip Clip { get; private set; }
        
        [field: SerializeField] public string AudioName { get; private set; }
        
        [field: SerializeField] public bool IsLooping { get; private set; }
        
        [field: SerializeField] public float Volume { get; private set; }
    }
}