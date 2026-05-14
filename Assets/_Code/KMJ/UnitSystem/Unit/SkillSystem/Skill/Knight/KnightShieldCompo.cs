using UnityEngine;

namespace Code.SkillSystem.Skill.Knight
{
    public class KnightShieldCompo : MonoBehaviour
    {
        [SerializeField] private MeshRenderer _meshRenderer;

        [SerializeField] private Material _baseMaterial;
        [SerializeField] private Material _changeMaterial;
        
        
        public void SetBaseMaterial()
        {
            _meshRenderer.material = _baseMaterial;
        }

        public void SetChangeMaterial()
        {
            _meshRenderer.material = _changeMaterial;
        }
    }
}