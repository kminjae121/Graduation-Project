using Code.Managers;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class BattleStartCompo : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private Button thisBtn;
    [SerializeField] private bool autoStartOnStart = true;
    private bool isPlaying = false;


    private void Awake()
    {
        if (thisBtn != null)
            thisBtn.onClick.AddListener(PlayGame);
    }

    private IEnumerator Start()
    {
        yield return null;

        if (autoStartOnStart)
            PlayGame();
    }

    private void OnDestroy()
    {
        if (thisBtn != null)
            thisBtn.onClick.RemoveListener(PlayGame);
    }

    public void PlayGame()
    {
        if (!isPlaying)
        {
            if (turnManager == null)
            {
                Debug.LogError("[BattleStartCompo] TurnManager is not assigned.");
                return;
            }

            turnManager.StartBattle();
            isPlaying = true;
        }
    }
}
