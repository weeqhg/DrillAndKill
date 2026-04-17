using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Settings;
using TMPro;

namespace WekenDev.Settings.General
{
    public class LanguageManager : MonoBehaviour
    {
        [System.Serializable]
        public class LanguageOption
        {
            public string displayName;              // "Русский", "English", "Deutsch"
            public string localeCode;               // "ru", "en", "de"
        }

        [Header("Language Dropdown")]
        [SerializeField] private TMP_Dropdown languageDropdown;
        [SerializeField] private List<LanguageOption> languageOptions = new List<LanguageOption>();

        public void Init()
        {
            if (languageDropdown != null)
            {
                // Заполняем Dropdown displayName
                var options = new List<TMP_Dropdown.OptionData>();
                foreach (var lang in languageOptions)
                {
                    options.Add(new TMP_Dropdown.OptionData(lang.displayName));
                }
                languageDropdown.ClearOptions();
                languageDropdown.AddOptions(options);
                
                LoadSavedLanguage();
                languageDropdown.onValueChanged.AddListener(OnDropdownValueChanged);
            }
        }

        private void OnDropdownValueChanged(int selectedIndex)
        {
            if (selectedIndex < 0 || selectedIndex >= languageOptions.Count) return;

            string localeCode = languageOptions[selectedIndex].localeCode;
            var locale = LocalizationSettings.AvailableLocales.GetLocale(localeCode);

            if (locale != null)
            {
                LocalizationSettings.SelectedLocale = locale;
                PlayerPrefs.SetString(PlayerPrefsKeys.SelectedLanguage, localeCode);
                PlayerPrefs.Save();
            }
        }

        private void LoadSavedLanguage()
        {
            string savedCode = PlayerPrefs.GetString(PlayerPrefsKeys.SelectedLanguage, GetDefaultLanguage());
            
            int index = languageOptions.FindIndex(opt => opt.localeCode == savedCode);
            if (index == -1) index = 0;

            languageDropdown.value = index;
            OnDropdownValueChanged(index);
        }
        
        private string GetDefaultLanguage()
        {
            string systemLang = Application.systemLanguage.ToString().ToLower();
            int index = languageOptions.FindIndex(opt => opt.localeCode == systemLang);
            
            if (index != -1) return systemLang;
            
            return "en";
        }
        
        private void OnDestroy()
        {
            if (languageDropdown != null)
            {
                languageDropdown.onValueChanged.RemoveListener(OnDropdownValueChanged);
            }
        }
    }
}