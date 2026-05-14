using Code.Managers;
using UnityEngine;
using UnityEngine.UI;

public class BattleStartCompo : MonoBehaviour
{
    [SerializeField] private TurnManager turnManager;
    [SerializeField] private Button thisBtn;
    private bool isPlaying = false;


    private void Awake()
    {
        thisBtn.onClick.AddListener(PlayGame);
    }

    private void OnDestroy()
    {
        thisBtn.onClick.RemoveListener(PlayGame);
    }

    public void PlayGame()
    {
        if (!isPlaying)
        {
            turnManager.StartBattle();
            isPlaying = true;
        }
    }
}
