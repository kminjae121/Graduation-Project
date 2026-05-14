using System;
using EPOOutline;
using UnityEngine;

namespace Code.UnitSystem
{
    public class UnitOutLineCompo : MonoBehaviour, IUnitComponent
    {
        private Outlinable[] _outLines;

        private Color _originColor;

        public void Initialize(Unit owner)
        {
            _outLines = GetComponentsInChildren<Outlinable>();
        }
        private void Start()
        {
            _originColor = _outLines[0].OutlineParameters.Color;
            ResetOutLine();
        }

        public void SetOutLine()
        {
            foreach (var outline in _outLines)
            {
                outline.enabled = true;
            }
        }

        public void ResetOutLine()
        {
            foreach (var outline in _outLines)
            {
                outline.enabled = false;
                outline.OutlineParameters.Color = _originColor;
            }
        }

        public void SetOutSelectOutLine()
        {
            foreach (var outline in _outLines)
            {
                outline.enabled = true;
                outline.OutlineParameters.Color = Color.white;
            }
        }
    }
}