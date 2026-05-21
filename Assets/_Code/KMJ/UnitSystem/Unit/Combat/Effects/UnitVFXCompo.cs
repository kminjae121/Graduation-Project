using System.Collections.Generic;
using System.Linq;
using Code.UnitSystem;
using UnityEngine;

namespace Code.Effects
{
    public class UnitVFXCompo : MonoBehaviour, IUnitComponent
    {
        private Dictionary<string, IPlayableVFX> _vfxDict = new();

        public void Initialize(Unit owner)
        {
            _vfxDict = new Dictionary<string, IPlayableVFX>();
            GetComponentsInChildren<IPlayableVFX>().ToList()
                .ForEach(playable => _vfxDict.Add(playable.VFXName, playable));
        }
        
        public void PlayVFX(string vfxName, Vector3 position, Quaternion rotation)
        {
            IPlayableVFX vfx = _vfxDict.GetValueOrDefault(vfxName);
            Debug.Assert(vfx != null, $"{vfxName} is not exists in dictionary");
            
            vfx.PlayVFX(position, rotation);
        }

        public void PlayVFX(string vfxName)
        {
            IPlayableVFX vfx = _vfxDict.GetValueOrDefault(vfxName);
            Debug.Assert(vfx != null, $"{vfxName} is not exists in dictionary");
            
            vfx.PlayVFX(Vector3.zero , Quaternion.identity);
        }
        
        public void StopVFX(string vfxName)
        {
            IPlayableVFX vfx = _vfxDict.GetValueOrDefault(vfxName);
            Debug.Assert(vfx != null, $"{vfxName} is not exists in dictionary");
            
            vfx.StopVFX();
        }
    }
}