using UnityEngine;
using WekenDev.Settings.General;
using WekenDev.Settings.Graphic;

public class SettingManager : UIWindow
{
    [Header("Ссылки на настройки")]
    // General
    [SerializeField] private LanguageManager _language;
    [SerializeField] private LimitFPS _limitFps;
    [SerializeField] private SensitivityMouse _sensitivityMouse;

    // Graphic
    [SerializeField] private ScreenResolutionSetting _screenSetting;
    [SerializeField] private WindowModeSetting _windowSettign;

    // Sound
    [SerializeField] private SoundVolumeSetting _soundVolume;

    private SettingUI _settingUI;

    public void Initialize()
    {
        _settingUI = GetComponent<SettingUI>();

        if (_settingUI != null)
        {
            _settingUI.Init();
            _settingUI.HideSetting();
        }

        // Init настроек
        _language?.Init();
        _limitFps?.Init();
        _sensitivityMouse?.Init();

        _screenSetting?.Init();
        _windowSettign?.Init();

        _soundVolume?.Init();
    }

    public override void Show()
    {
        base.Show();
        _settingUI?.ShowSetting();
    }

    public override void Hide()
    {
        base.Hide();
        _settingUI?.HideSetting();
    }
}