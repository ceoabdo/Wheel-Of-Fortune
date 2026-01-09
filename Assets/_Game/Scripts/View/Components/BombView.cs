using System;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using WheelOfFortune.Infrastructure.Interfaces;

namespace WheelOfFortune.View.Components
{
    public sealed class BombView : MonoBehaviour, IBombView
    {
        private const string REVIVE_PREFIX = "REVIVE (";
        private const string REVIVE_SUFFIX = "$)";

        [SerializeField] private Button _giveUpButton;
        [SerializeField] private Button _reviveButton;
        [SerializeField] private TMP_Text _reviveCostText;
        [SerializeField] private CanvasGroup _canvasGroup;

        private readonly StringBuilder _stringBuilder = new(32);

        public event Action GiveUpRequested;
        public event Action ReviveRequested;

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (_giveUpButton == null)
            {
                _giveUpButton = FindButtonByName("GiveUp");
            }

            if (_reviveButton == null)
            {
                _reviveButton = FindButtonByName("Revive");
            }

            if (_reviveCostText == null)
            {
                _reviveCostText = FindTextByName("Revive");
            }
        }

        private Button FindButtonByName(string buttonName)
        {
            Button[] buttons = GetComponentsInChildren<Button>(true);

            for (int index = 0; index < buttons.Length; index++)
            {
                Button button = buttons[index];
                if (button.gameObject.name.Contains(buttonName))
                {
                    return button;
                }
            }

            return null;
        }

        private TMP_Text FindTextByName(string textName)
        {
            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            
            for (int index = 0; index < texts.Length; index++)
            {
                TMP_Text text = texts[index];
                if (text.gameObject.name.Contains(textName))
                {
                    return text;
                }
            }

            return null;
        }
#endif

        private void OnEnable()
        {
            SubscribeToButtons();
        }

        private void SubscribeToButtons()
        {
            if (_giveUpButton)
            {
                _giveUpButton.onClick.AddListener(HandleGiveUpClicked);
            }

            if (_reviveButton)
            {
                _reviveButton.onClick.AddListener(HandleReviveClicked);
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);

            if (_canvasGroup)
            {
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }
        }

        public void Hide()
        {
            if (_canvasGroup)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            gameObject.SetActive(false);
        }

        public void UpdateReviveButtonState(bool canRevive)
        {
            if (_reviveButton)
            {
                _reviveButton.interactable = canRevive;
            }
        }

        public void UpdateReviveCostText(int cost)
        {
            if (!_reviveCostText)
            {
                return;
            }

            _stringBuilder.Clear();
            _stringBuilder.Append(REVIVE_PREFIX);
            _stringBuilder.Append(cost);
            _stringBuilder.Append(REVIVE_SUFFIX);
            _reviveCostText.text = _stringBuilder.ToString();
        }

        private void HandleGiveUpClicked()
        {
            GiveUpRequested?.Invoke();
        }

        private void HandleReviveClicked()
        {
            ReviveRequested?.Invoke();
        }

        private void UnsubscribeFromButtons()
        {
            if (_giveUpButton)
            {
                _giveUpButton.onClick.RemoveListener(HandleGiveUpClicked);
            }

            if (_reviveButton)
            {
                _reviveButton.onClick.RemoveListener(HandleReviveClicked);
            }
        }
        
        private void OnDisable()
        {
            UnsubscribeFromButtons();
        }
    }
}
