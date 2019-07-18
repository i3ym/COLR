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
        Translations.Add("mainmenu.Settings", new Translation((Language.Russian, "Настройки"), (Language.English, "Settings")));
        Translations.Add("mainmenu.Quit", new Translation((Language.Russian, "Выйти"), (Language.English, "Quit")));

        Translations.Add("settingsmenu.Graphics", new Translation((Language.Russian, "Графика"), (Language.English, "Graphics")));
        Translations.Add("settingsmenu.Audio", new Translation((Language.Russian, "Аудио"), (Language.English, "Audio")));
        Translations.Add("settingsmenu.Language", new Translation((Language.Russian, "Язык"), (Language.English, "Language")));

        Translations.Add("audiomenu.Music", new Translation((Language.Russian, "Музыка"), (Language.English, "Music")));
        Translations.Add("audiomenu.Sounds", new Translation((Language.Russian, "Звуки"), (Language.English, "Sounds")));

        Translations.Add("menu.Back", new Translation((Language.Russian, "Назад"), (Language.English, "Back")));
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
        if (string.IsNullOrWhiteSpace(TextID)) return;

        try
        {
            Text.text = Translations[TextID][lang];
        }
        catch
        {
            Debug.LogError("Could not found translation for GameObject '" + name + "'. TextID: '" + TextID + "', Language: '" + lang.ToString() + "'");
            Text.text = TextID;
        }
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
}

public enum Language
{
    Russian,
    English
}