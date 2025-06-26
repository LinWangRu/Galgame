using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public static class LM
{
    public static string GLV(string key)
    {
        return LocalizationManager.Instance.GetLocalizedValue(key);
    }
    public static string GetSpeakerName(ExcelReader.ExcelData data)
    {
        string currentSpeakerName = string.Empty;
        switch (GameManager.Instance.currentLanguageIndex)
        {
            case 0:
                currentSpeakerName = data.speakerName;
                break;
            case 1:
                currentSpeakerName = data.englishName;
                break;
            case 2:
                currentSpeakerName = data.japaneseName;
                break;
        }
        return currentSpeakerName;
    }
    public static string GetSpeakingContent(ExcelReader.ExcelData data)
    {
        string currentSpeakingContent = string.Empty;
        switch (GameManager.Instance.currentLanguageIndex)
        {
            case 0:
                currentSpeakingContent = data.speakingContent;
                break;
            case 1:
                currentSpeakingContent = data.englishContent;
                break;
            case 2:
                currentSpeakingContent = data.japaneseContent;
                break;
        }
        return currentSpeakingContent;
    }
   
}
public class LocalizationManager : MonoBehaviour
{
    public Dictionary<string, string> localizedText;
    public delegate void OnLanguageChanged();
    public event OnLanguageChanged LanguageChanged;
    public static LocalizationManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        LoadLanguage(Constants.DEFAULT_LANGUAGE);
    }

    public void LoadLanguage(string language)
    {
        string filePath = Path.Combine(Application.streamingAssetsPath,
                                        Constants.LANGUAGE_PATH,
                                        language + Constants.JSON_FILE_EXTENSION);

        if (File.Exists(filePath))
        {
            string dataAsJson = File.ReadAllText(filePath);
            localizedText = JsonConvert.DeserializeObject<Dictionary<string, string>>(dataAsJson);

            LanguageChanged?.Invoke();
        }
        else
        {
            Debug.LogError(Constants.LOCALIZATION_LOAD_FAILED + filePath);
        }
    }

    public string GetLocalizedValue(string key)
    {
        if (localizedText != null && localizedText.ContainsKey(key))
        {
            return localizedText[key];
        }
        return key;
    }
}
