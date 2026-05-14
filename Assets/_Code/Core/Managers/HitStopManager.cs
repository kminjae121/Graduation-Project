using Code.Core.Events.Bus;
using System.Collections;
using UnityEngine;

namespace _00.Core._02.Scripts._01.Manager
{
    public class HitStopManager : MonoBehaviour
    {
        private Coroutine _hitStopCoroutine;

        private void OnEnable()
        {
            Bus<HitStopEvent>.Subscribe(OnHitStopEvent);
        }

        private void OnDisable()
        {
            Bus<HitStopEvent>.Unsubscribe(OnHitStopEvent);
        }

        private void OnHitStopEvent(HitStopEvent evt)
        {
            if (_hitStopCoroutine != null)
            {
                StopCoroutine(_hitStopCoroutine);
            }
            
            _hitStopCoroutine = StartCoroutine(ProcessHitStop(evt));
        }

        private IEnumerator ProcessHitStop(HitStopEvent evt)
        {
            Time.timeScale = evt.TimeScale;

            yield return new WaitForSecondsRealtime(evt.Duration);
            
            Time.timeScale = 1f;
            _hitStopCoroutine = null;
        }
    }
}