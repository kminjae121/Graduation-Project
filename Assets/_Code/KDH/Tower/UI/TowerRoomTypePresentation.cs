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

        public static string GetDescription(TowerRoomType roomType)
        {
            return roomType switch
            {
                TowerRoomType.Start => "원정이 시작되는 안전 지점입니다.",
                TowerRoomType.Event => "알 수 없는 사건이나 선택지가 기다립니다.",
                TowerRoomType.Combat => "일반 전투가 발생합니다.",
                TowerRoomType.EliteCombat => "강한 적이 등장하지만 더 좋은 보상을 기대할 수 있습니다.",
                TowerRoomType.Reward => "전투 없이 보상을 획득할 수 있습니다.",
                TowerRoomType.Portal => "다음 층으로 이동하거나 원정을 마칠 수 있습니다.",
                TowerRoomType.Boss => "층의 보스가 기다립니다. 승리하면 포탈이 열립니다.",
                _ => string.Empty
            };
        }
    }
}
