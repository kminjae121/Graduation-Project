using UnityEngine;

namespace Code.Tower.UI
{
    public static class TowerRoomTypePresentation
    {
        public static string GetDisplayName(TowerRoomType roomType)
        {
            return roomType switch
            {
                TowerRoomType.Start => "시작방",
                TowerRoomType.Event => "이벤트방",
                TowerRoomType.Combat => "전투방",
                TowerRoomType.EliteCombat => "엘리트 전투방",
                TowerRoomType.Reward => "보상방",
                TowerRoomType.Portal => "포탈방",
                TowerRoomType.Boss => "보스방",
                _ => roomType.ToString()
            };
        }

        public static string GetShortName(TowerRoomType roomType)
        {
            return roomType switch
            {
                TowerRoomType.Start => "S",
                TowerRoomType.Event => "E",
                TowerRoomType.Combat => "C",
                TowerRoomType.EliteCombat => "EL",
                TowerRoomType.Reward => "R",
                TowerRoomType.Portal => "P",
                TowerRoomType.Boss => "B",
                _ => "?"
            };
        }

        public static Color GetColor(TowerRoomType roomType)
        {
            return roomType switch
            {
                TowerRoomType.Start => new Color(0.35f, 0.75f, 1f),
                TowerRoomType.Event => new Color(0.76f, 0.56f, 1f),
                TowerRoomType.Combat => new Color(1f, 0.45f, 0.38f),
                TowerRoomType.EliteCombat => new Color(1f, 0.24f, 0.18f),
                TowerRoomType.Reward => new Color(1f, 0.78f, 0.28f),
                TowerRoomType.Portal => new Color(0.38f, 0.95f, 0.78f),
                TowerRoomType.Boss => new Color(0.92f, 0.12f, 0.16f),
                _ => Color.white
            };
        }
    }
}
