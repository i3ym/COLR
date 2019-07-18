using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LocalizedText : MonoBehaviour
{
    static Dictionary<string, Translation> Translations = new Dictionary<string, Translation>();

    [SerializeField]
    string TextID = null;

    TextMeshProUGUI Text = null;

    static LocalizedText()
    {
        Translations.Add("mainmenu.Start", new Translation((Language.Russian, "Начать"), (Language.English, "Start")));
        Translations.Add("mainmenu.Continue", new Translation((Language.Russian, "Продолжить"), (Language.English, "Continue")));
        Translations.Add("mainmenu.Settings", new Translation((Language.Russian, "Настройки"), (Language.English, "Settings")));
        Translations.Add("mainmenu.Quit", new Translation((Language.Russian, "Выйти"), (Language.English, "Quit")));

        Translations.Add("settingsmenu.Graphics", new Translation((Language.Russian, "Графика"), (Language.English, "Graphics")));
        Translations.Add("settingsmenu.Audio", new Translation((Language.Russian, "Аудио"), (Language.English, "Audio")));
        Translations.Add("settingsmenu.Language", new Translation((Language.Russian, "Язык"), (Language.English, "Language")));

        Translations.Add("audiomenu.Music", new Translation((Language.Russian, "Музыка"), (Language.English, "Music")));
        Translations.Add("audiomenu.Sounds", new Translation((Language.Russian, "Звуки"), (Language.English, "Sounds")));

        Translations.Add("menu.Back", new Translation((Language.Russian, "Назад"), (Language.English, "Back")));
        
        Translations.Add("text.Highscore", new Translation((Language.Russian, "Рекорд"), (Language.English, "Highscore")));
    }

    void Start()
    {
        Text = GetComponent<TextMeshProUGUI>();

        if (!Text)
        {
            Destroy(this);
            return;
        }

        if (!Game.LocalizedTexts.Contains(this))
            Game.LocalizedTexts.Add(this);
    }

    public void SetLanguage(Language lang)
    {
        if (!string.IsNullOrWhiteSpace(TextID)) Text.text = GetTranslation(TextID, lang);
    }

    public static string GetTranslation(string textID, Language lang)
    {
        if (Translations.ContainsKey(textID) && Translations[textID].ContainsKey(lang))
            return Translations[textID][lang];

        Debug.LogError("Could not found translation for TextID: '" + textID + "', Language: '" + lang.ToString() + "'");
        return textID;
    }
}

public struct Translation
{
    Dictionary<Language, string> Translations;

    public Translation(params(Language lang, string translation) [] translations)
    {
        Translations = new Dictionary<Language, string>();

        foreach (var tr in translations) Translations.Add(tr.lang, tr.translation);
    }

    public string this [Language lang] => Translations[lang];

    public bool ContainsKey(Language lang) => Translations.ContainsKey(lang);
}

public enum Language
{
    Russian,
    English
}