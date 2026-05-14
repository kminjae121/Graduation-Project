using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI
{
    public class MarkUI : MonoBehaviour
    {
        [SerializeField] private Image dieMarkUI;
        [SerializeField] private GameObject markUI;

        private GameObject cam;

        private void Awake()
        {
            cam = GameObject.FindWithTag("TopCam");   
        }
        
        public void SetMarkUI(int markCnt)
        {
            markUI.SetActive(true);

            dieMarkUI.gameObject.SetActive(true);

            dieMarkUI.DOFillAmount((float)markCnt / 3,0.8f);

            if (markCnt == 4)
            {
                markUI.SetActive(false);
                dieMarkUI.gameObject.SetActive(false);
            }   
        }

        private void Update()
        {
            dieMarkUI.transform.LookAt(cam.transform);
            markUI.transform.LookAt(cam.transform);   
        }
    }
}