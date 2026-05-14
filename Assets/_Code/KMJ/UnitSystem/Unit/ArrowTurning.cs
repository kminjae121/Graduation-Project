    using System;
using UnityEngine;

namespace Code.UnitSystem
{
    public class ArrowTurning : MonoBehaviour
    {
        private float _xValue = 0;
        
        private void Update()
        {
            transform.rotation = Quaternion.Euler(0,_xValue += 1f,0);
        }
    }
}