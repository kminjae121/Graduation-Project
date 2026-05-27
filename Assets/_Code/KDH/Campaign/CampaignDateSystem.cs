using Code.Core.Events.Bus;
using UnityEngine;

namespace Code.Campaign
{
    public enum CampaignDateAdvanceReason
    {
        Manual,
        ExpeditionReturned,
        ExpeditionFailed
    }

    public readonly struct CampaignDateSnapshot
    {
        public readonly int Week;
        public readonly int ExpeditionCount;
        public readonly int ReturnedExpeditionCount;
        public readonly int FailedExpeditionCount;

        public CampaignDateSnapshot(
            int week,
            int expeditionCount,
            int returnedExpeditionCount,
            int failedExpeditionCount)
        {
            Week = week;
            ExpeditionCount = expeditionCount;
            ReturnedExpeditionCount = returnedExpeditionCount;
            FailedExpeditionCount = failedExpeditionCount;
        }

        public string WeekLabel => $"Week {Week}";
        public string KoreanWeekLabel => $"{Week}주차";
    }

    public readonly struct CampaignDateChangedEvent : IEvent
    {
        public readonly CampaignDateSnapshot Snapshot;
        public readonly CampaignDateAdvanceReason Reason;
        public readonly bool Advanced;

        public CampaignDateChangedEvent(
            CampaignDateSnapshot snapshot,
            CampaignDateAdvanceReason reason,
            bool advanced)
        {
            Snapshot = snapshot;
            Reason = reason;
            Advanced = advanced;
        }
    }

    public static class CampaignDateSystem
    {
        private const string WeekKey = "CampaignDate.Week";
        private const string ExpeditionCountKey = "CampaignDate.ExpeditionCount";
        private const string ReturnedExpeditionCountKey = "CampaignDate.ReturnedExpeditionCount";
        private const string FailedExpeditionCountKey = "CampaignDate.FailedExpeditionCount";

        private static bool _isLoaded;
        private static int _week = 1;
        private static int _expeditionCount;
        private static int _returnedExpeditionCount;
        private static int _failedExpeditionCount;

        public static CampaignDateSnapshot Current
        {
            get
            {
                EnsureLoaded();
                return CreateSnapshot();
            }
        }

        public static int CurrentWeek => Current.Week;

        public static void AdvanceAfterExpedition(bool failed)
        {
            AdvanceWeek(failed
                ? CampaignDateAdvanceReason.ExpeditionFailed
                : CampaignDateAdvanceReason.ExpeditionReturned);
        }

        public static void AdvanceWeek(CampaignDateAdvanceReason reason = CampaignDateAdvanceReason.Manual)
        {
            EnsureLoaded();

            _week = Mathf.Max(1, _week + 1);
            _expeditionCount = Mathf.Max(0, _expeditionCount + 1);

            if (reason == CampaignDateAdvanceReason.ExpeditionFailed)
                _failedExpeditionCount = Mathf.Max(0, _failedExpeditionCount + 1);
            else if (reason == CampaignDateAdvanceReason.ExpeditionReturned)
                _returnedExpeditionCount = Mathf.Max(0, _returnedExpeditionCount + 1);

            Save();
            RaiseChanged(reason, true);
        }

        public static void ResetDate()
        {
            _isLoaded = true;
            _week = 1;
            _expeditionCount = 0;
            _returnedExpeditionCount = 0;
            _failedExpeditionCount = 0;

            Save();
            RaiseChanged(CampaignDateAdvanceReason.Manual, false);
        }

        public static void ReloadFromStorage()
        {
            _isLoaded = false;
            EnsureLoaded();
            RaiseChanged(CampaignDateAdvanceReason.Manual, false);
        }

        private static void EnsureLoaded()
        {
            if (_isLoaded)
                return;

            _week = Mathf.Max(1, PlayerPrefs.GetInt(WeekKey, 1));
            _expeditionCount = Mathf.Max(0, PlayerPrefs.GetInt(ExpeditionCountKey, 0));
            _returnedExpeditionCount = Mathf.Max(0, PlayerPrefs.GetInt(ReturnedExpeditionCountKey, 0));
            _failedExpeditionCount = Mathf.Max(0, PlayerPrefs.GetInt(FailedExpeditionCountKey, 0));
            _isLoaded = true;
        }

        private static void Save()
        {
            PlayerPrefs.SetInt(WeekKey, _week);
            PlayerPrefs.SetInt(ExpeditionCountKey, _expeditionCount);
            PlayerPrefs.SetInt(ReturnedExpeditionCountKey, _returnedExpeditionCount);
            PlayerPrefs.SetInt(FailedExpeditionCountKey, _failedExpeditionCount);
            PlayerPrefs.Save();
        }

        private static CampaignDateSnapshot CreateSnapshot()
        {
            return new CampaignDateSnapshot(
                _week,
                _expeditionCount,
                _returnedExpeditionCount,
                _failedExpeditionCount);
        }

        private static void RaiseChanged(CampaignDateAdvanceReason reason, bool advanced)
        {
            Bus<CampaignDateChangedEvent>.Raise(new CampaignDateChangedEvent(CreateSnapshot(), reason, advanced));
        }
    }
}
