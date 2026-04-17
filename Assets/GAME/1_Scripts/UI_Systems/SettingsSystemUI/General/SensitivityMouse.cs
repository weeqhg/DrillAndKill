using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WekenDev.Settings.General
{

    public class SensitivityMouse : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TextMeshProUGUI _text;

        public void Init()
        {
            float savedValue = PlayerPrefs.GetFloat(PlayerPrefsKeys.Sensitivity);
            _slider.minValue = 0f;
            _slider.maxValue = 100f;
            _slider.onValueChanged.AddListener(SetSensitivityMouse);
            _slider.value = savedValue;
        }

        private void SetSensitivityMouse(float value)
        {
            if (_text != null)
            {
                _text.text = $"{value:F2}";
            }

            ConsoleEvents.CommandSensitivityChanged(value);
        }

        private void OnDestroy()
        {
            if (_slider != null)
                _slider.onValueChanged.RemoveListener(SetSensitivityMouse);
        }
    }

}