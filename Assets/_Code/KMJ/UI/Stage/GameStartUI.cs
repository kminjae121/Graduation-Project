using System;
using System.Collections;
using Code.Core.Managers;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class GameStartUI : MonoBehaviour
    {
        [SerializeField] private Image img2;
        [SerializeField] private Image img;
        [SerializeField] private TextMeshProUGUI txt;
        [SerializeField] private Button btn;
        [SerializeField] private Transform img2Trm;
        [SerializeField] private Transform startTrm;
        [SerializeField] private Transform endTrm;

        private void Awake()
        {
            img2.transform.position = startTrm.position;
        }

        private void Start()
        {
            StartGame();
            StartCoroutine(WaitStart());
        }

        private void StartGame()
        {
            DOTween.Sequence()
                .SetDelay(0.35f)
                .Append(txt.DOFade(0,0.3f))
                .Append(img.DOFade(0, 2f))
                .SetEase(Ease.InQuint)
                .OnComplete(() =>
                {
                    img.gameObject.SetActive(false);
                    img2.gameObject.SetActive(false);
                });
        }

        private IEnumerator WaitStart()
        {
            yield return new WaitForSeconds(0.3f);
            btn.onClick?.Invoke();
        }
        
    }
}