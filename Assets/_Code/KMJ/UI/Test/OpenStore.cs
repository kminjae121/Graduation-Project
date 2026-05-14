using System;
using UnityEngine;

namespace Code.UI.Test
{
    public class OpenStore : MonoBehaviour
    {
        [SerializeField] private GameObject storeObj;
        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.Tab))
            {
                storeObj.SetActive(true);
            }
        }
    }
}