using System;
using System.Linq;
using System.Globalization;
using Windows.Globalization;
using System.Collections.Generic;
using IOCore.Types;

namespace IOCore.Libs
{
    public class LanguageManager
    {
        private static readonly string SCOPE = nameof(LanguageManager);

        public enum Culture
        {
            UsingSystemSetting,
            EnUs,
            ZhHant,
            JaJp,
            ViVn,
        }

        public static readonly Dictionary<Culture, SwitchRecord> LANGUAGES = new()
        {
            { Culture.UsingSystemSetting,   new(string.Empty,   "Settings_LanguageUsingSystemSetting", true) },
            { Culture.EnUs,                 new("en-US",        "Settings_LanguageEnUs",               true) },
            { Culture.ZhHant,               new("zh-Hant",      "Settings_LanguageZhHant",             false) },
            { Culture.JaJp,                 new("ja",           "Settings_LanguageJaJp",               false) },
            { Culture.ViVn,                 new("vi",           "Settings_LanguageViVn",               false) },
        };

        public static void Init()
        {
            foreach (var l in ApplicationLanguages.ManifestLanguages)
            {
                var language = LANGUAGES.FirstOrDefault(i => (i.Value.Value as string) == l);
                if (language.Value != null)
                    LANGUAGES[language.Key].IsOn = true;
            }

            var correctCulture = LoadCorrectCulture();

            if (!LANGUAGES[correctCulture].IsOn)
                correctCulture = Culture.EnUs;

            ApplicationLanguages.PrimaryLanguageOverride = LANGUAGES[correctCulture].Value as string;
        }

        public static Culture LoadCulture() => LocalStorage.GetValueOrDefault(SCOPE, Culture.UsingSystemSetting);

        public static Culture LoadCorrectCulture()
        {
            static string GetLanguageName(string culture)
            {
                return new CultureInfo(culture).TwoLetterISOLanguageName.ToLowerInvariant();
            }

            static Culture FindCorrectCulture(Culture? culture)
            {
                if (culture == null || culture == Culture.UsingSystemSetting)
                {
                    var language = LANGUAGES.FirstOrDefault(i => (i.Value.Value as string).Equals(CultureInfo.CurrentCulture.Name, StringComparison.InvariantCultureIgnoreCase));

                    if (language.Value == null)
                        language = LANGUAGES.FirstOrDefault(i => i.Key != Culture.UsingSystemSetting && GetLanguageName(i.Value.Value as string) == GetLanguageName(CultureInfo.CurrentCulture.Name));

                    if (language.Value == null)
                        culture = Culture.EnUs;
                    else
                        culture = language.Key;
                }

                return culture.Value;
            }

            var culture = LocalStorage.GetValueOrDefault(SCOPE, Culture.UsingSystemSetting);
            var correctCulture = FindCorrectCulture(culture);

            if (culture != Culture.UsingSystemSetting)
                LocalStorage.Set(SCOPE, correctCulture);

            return correctCulture;
        }

        public static Culture SaveCultureSettingAutoFallback(Culture? culture)
        {
            culture ??= Culture.UsingSystemSetting;
            LocalStorage.Set(SCOPE, culture);
            return culture.Value;
        }

        public static bool IsPanda()
        {
            return ApplicationLanguages.ManifestLanguages.Any(l => l.StartsWith("zh"));
        }
    }
}