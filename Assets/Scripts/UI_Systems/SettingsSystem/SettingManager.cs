using UnityEngine;
using WekenDev.Settings.Sound;
using WekenDev.Settings.General;
using WekenDev.Settings.Graphic;
using System;
using WekenDev.InputSystem;

namespace WekenDev.Settings
{
    public class SettingManager : MonoBehaviour
    {
        [Header("Ссылки на настройки")]
        //General
        [SerializeField] private LanguageManager _language;
        [SerializeField] private LimitFPS _limitFps;
        [SerializeField] private SensitivityMouse _sensitivityMouse;

        //Graphic
        [SerializeField] private ScreenResolutionSetting _screenSetting;
        [SerializeField] private WindowModeSetting _windowSettign;

        //Sound
        [SerializeField] private SoundVolumeSetting _soundVolume;
        private SettingUI _settingUI;
        public event Action OnCloseSetting;

        private bool isOpen = false;

        public void Initialize()
        {
            _settingUI = GetComponent<SettingUI>();

            InputManager.Instance.Actions.UI.Cancel.performed += ctx => Hide();
            _settingUI.OnSettingsDisableUI += Hide;

            if (_settingUI != null)
            {
                _settingUI.Init();
                _settingUI.HideSetting();
                isOpen = false;
            }

            //Основное
            if (_language != null) _language.Init();
            if (_limitFps != null) _limitFps.Init();
            if (_sensitivityMouse != null) _sensitivityMouse.Init();

            //Графика
            if (_screenSetting != null) _screenSetting.Init();
            if (_windowSettign != null) _windowSettign.Init();

            //Звук
            if (_soundVolume != null) _soundVolume.Init();
        }

        private void Hide()
        {
            if (!isOpen) return;
            isOpen = false;
            _settingUI.HideSetting();
            OnCloseSetting?.Invoke();
        }

        public void Show()
        {
            if (isOpen) return;
            isOpen = true;
            _settingUI.ShowSetting();
        }

        private void OnDestroy()
        {
            InputManager.Instance.Actions.UI.Cancel.performed -= ctx => Hide();
            _settingUI.OnSettingsDisableUI -= Hide;
        }
    }
}
