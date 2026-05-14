using System.Collections;
using System.Collections.Generic;
using _Code.KMJ.UnitSystem.Sound;
using UnityEngine;
using UnityEngine.Analytics;

namespace Code.Core
{
    public class SoundManager : MonoSingleton<SoundManager>
    {
        [SerializeField] private List<SoundClip> clips;

        [SerializeField] private AudioSource audioSourcePrefab;
        
        [SerializeField] private int sourceCnt;
        private Stack<AudioSource> audioSources = new Stack<AudioSource>();  
        
        private List<AudioSource> _usingSources = new List<AudioSource>(); 
        private Dictionary<string, AudioClip> _clipDictionary = new Dictionary<string, AudioClip>();
        private Dictionary<string, AudioClip> _loopingClipDictionary = new Dictionary<string, AudioClip>();

        protected override void Awake()
        {
            base.Awake();
            
            foreach (var audio in clips)
            {
                if (audio.IsLooping && !_loopingClipDictionary.ContainsKey(audio.AudioName))
                    _loopingClipDictionary.Add(audio.AudioName, audio.Clip);
                else if(!_clipDictionary.ContainsKey(audio.AudioName))
                    _clipDictionary.Add(audio.AudioName, audio.Clip); 
            }

            for (int i = 0; i < sourceCnt; ++i)
            {
                AudioSource src = Instantiate(audioSourcePrefab, transform);
                src.clip = null;
                src.loop = false;
                audioSources.Push(src);
                src.gameObject.SetActive(false);
            }
        }
        
        public IEnumerator ReturnSource(AudioSource source)
        {
            if (source == null || source.loop) yield break;

            while (source.isActiveAndEnabled && source.isPlaying)
                yield return null;
            
            if (!_usingSources.Contains(source))
                yield break;

            source.Stop();
            source.clip = null;
            source.loop = false;
            source.gameObject.SetActive(false);

            audioSources.Push(source);
            _usingSources.Remove(source);
        }

        public void PlayLooping(string name)
        {
            if (!_loopingClipDictionary.TryGetValue(name, out var clip)) return;
            if (audioSources.Count == 0) return;

            var pool = audioSources.Pop();
            pool.gameObject.SetActive(true);
            pool.clip = clip;
            pool.loop = true;
            pool.Play();
            
            _usingSources.Add(pool);
        }

        public void PlayClip(string name)
        {
            if (!_clipDictionary.TryGetValue(name, out var clip)) return;
            if (audioSources.Count == 0) return;

            var pool = audioSources.Pop();
            pool.gameObject.SetActive(true);
            pool.clip = clip;
            pool.loop = false;
            pool.Play();

            StartCoroutine(ReturnSource(pool));
            
            _usingSources.Add(pool);
        }

        public void StopLooping(string name)
        {
            if (!_loopingClipDictionary.TryGetValue(name, out var clip)) return;

            for (int i = _usingSources.Count - 1; i >= 0; --i)
            {
                var src = _usingSources[i];
                if (src == null)
                {
                    _usingSources.RemoveAt(i); continue;
                }

                if (!src.loop) continue;
                if (src.clip != clip) continue;

                src.Stop();
                src.clip = null;
                src.loop = false;
                src.gameObject.SetActive(false);

                audioSources.Push(src);
                _usingSources.RemoveAt(i);
            }
        }

        public void StopClip(string name)
        {
            if (!_clipDictionary.TryGetValue(name, out var clip)) return;

            for (int i = _usingSources.Count - 1; i >= 0; --i)
            {
                var src = _usingSources[i];
                if (src == null)
                {
                    _usingSources.RemoveAt(i); continue;
                }

                if (src.loop) continue;
                if (src.clip != clip) continue;

                src.Stop();
                src.clip = null;
                src.loop = false;
                src.gameObject.SetActive(false);

                audioSources.Push(src);
                _usingSources.RemoveAt(i);
            }
        }
    }
}