using UnityEngine;

namespace Code.KMJ.UnitSystem.Sound
{
    [CreateAssetMenu(menuName = "Sound/SoundClip", fileName = "Sound/SoundClip")]
    public class SoundClip : ScriptableObject
    {
        public AudioClip Clip;

        public string AudioName;

        public bool IsLooping;

        [Range(0, 101)] public float Volume;
    }
}