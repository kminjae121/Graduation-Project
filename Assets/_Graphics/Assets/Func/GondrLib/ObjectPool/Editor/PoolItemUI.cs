using System;
using GondrLib.ObjectPool.Runtime;
using UnityEngine.UIElements;

namespace GondrLib.ObjectPool.Editor
{
    public class PoolItemUI
    {
        private Label _nameLabel;
        private Button _deleteBtn;
        private VisualElement _rootElement;

        public event Action<PoolItemUI> OnDeleteEvent;
        public event Action<PoolItemUI> OnSelectEvent;

        public string Name 
        {
            get => _nameLabel.text;
            set => _nameLabel.text = value;
        }

        public PoolingItemSO poolItem;

        public bool IsActive
        {
            get => _rootElement.ClassListContains("active");
            set => _rootElement.EnableInClassList("active", value);
        }

        public PoolItemUI(VisualElement root, PoolingItemSO item)
        {
            poolItem = item;
            _rootElement = root.Q<VisualElement>("PoolItem");
            _nameLabel = root.Q<Label>("ItemName");
            _deleteBtn = root.Q<Button>("DeleteBtn");
            
            _deleteBtn.RegisterCallback<ClickEvent>(evt =>
            {
                OnDeleteEvent?.Invoke(this);
                evt.StopPropagation(); //이벤트 전파를 멈춘다.
            });
            
            _rootElement.RegisterCallback<ClickEvent>(evt =>
            {
                OnSelectEvent?.Invoke(this);
                evt.StopPropagation();
            });
        }
    }
}