using System.Collections.Generic;
using System.Linq;
using Code.UI;
using GameEventChannel;
using GondrLib.Dependencies;
using GondrLib.ObjectPool.Runtime;
using UnityEngine;

namespace Core
{
    public class TextManager : MonoBehaviour
    {
        [SerializeField] private GameEventChannelSO textChannel;
        [SerializeField] private PoolingItemSO popupItem;
        
        [Inject] private PoolManagerMono _poolManager;
        [SerializeField] private TextInfo[] textInfos;
        private Dictionary<int, TextInfo> _textInfoDict;

        private void Awake()
        {
            textChannel.AddListener<PopupTextEvent>(HandlePopupTextEvent);
            _textInfoDict = textInfos.ToDictionary(X => X.nameHash);
        }

        private void OnDestroy()
        {
            textChannel.RemoveListener<PopupTextEvent>(HandlePopupTextEvent);
        }

        private void HandlePopupTextEvent(PopupTextEvent obj)
        {
            PopupText text = _poolManager.Pop<PopupText>(popupItem);
            TextInfo textInfo = _textInfoDict.GetValueOrDefault(obj.textTypeHash);
            Debug.Assert(textInfo != default, "안돼요");
            
            text.ShowPopupText(obj.text, textInfo, obj.position, obj.showDuraction);
        }
    }
}