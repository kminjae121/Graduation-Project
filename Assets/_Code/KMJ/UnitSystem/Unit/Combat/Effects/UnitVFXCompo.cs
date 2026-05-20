using System.Collections.Generic;
using System.Linq;
using Code.UnitSystem;
using UnityEngine;

namespace Code.Effects
{
    public class UnitVFXCompo : MonoBehaviour, IUnitComponent
    {
        private UnitAnimation animationCompo;
        private Dictionary<string, IPlayableVFX> _vfxDict = new();
        private Unit _owner;

        public void Initialize(Unit owner)
        {
            _owner = owner;
            _vfxDict = new Dictionary<string, IPlayableVFX>();
            GetComponentsInChildren<IPlayableVFX>().ToList()
                .ForEach(playable => _vfxDict.Add(playable.VFXName, playable));

            animationCompo = owner.GetUnitCompo<UnitAnimation>();
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