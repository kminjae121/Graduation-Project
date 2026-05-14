using System;
using System.Collections.Generic;
using System.Linq;
using Code.UnitSystem.Combat;
using Code.UnitSystem.Enemies;
using UnityEngine;

namespace Code.UnitSystem
{
    public class UnitGetEnemyCompo : MonoBehaviour, IUnitComponent
    {
        public List<AbstractEnemyUnit> Enemies { get; set; }


        public void Initialize(Unit owner)
        {
            
        }
        private void Start()
        {
            Enemies = FindObjectsByType<AbstractEnemyUnit>(FindObjectsSortMode.None).ToList();
        }

        public void FindEnemies()
        {
            Enemies.Clear();
            Enemies = FindObjectsByType<AbstractEnemyUnit>(FindObjectsSortMode.None).ToList();
        }
    }
}