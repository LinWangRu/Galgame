using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public Image backgroundImage;

    public Button startButton;
    public Button continueButton;
    public Button loadButton;
    public Button galleryButton;
    public Button settingButton;
    public Button quitButton;
    public Button languageButton;

    private int currentLanguageIndex;
    public static MenuManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        GameManager.Instance.currentScene = Constants.MENU_SCENE;
        MenuButtonsAddListener();

        currentLanguageIndex = GameManager.Instance.currentLanguageIndex;
        LocalizationManager.Instance.LoadLanguage(Constants.LANGUAGES[currentLanguageIndex]);
        UpdateLanguageButtonText();
        
        string lastPlayedVideo = IntroManager.GetLastPlayedVideo();
        if (!string.IsNullOrEmpty(lastPlayedVideo))
        {
            string videoFileName = Path.GetFileNameWithoutExtension(lastPlayedVideo);
            string imagePath = Constants.BACKGROUND_PATH + videoFileName;
            Sprite sprite = Resources.Load<Sprite>(imagePath);
            if (sprite != null)
            {
                backgroundImage.sprite = sprite;
            }
            else
            {
                Debug.LogError(Constants.IMAGE_LOAD_FAILED + imagePath);
            }
        }
    }
    void MenuButtonsAddListener()
    {
        startButton.onClick.AddListener(StartGame);
        continueButton.onClick.AddListener(ContinueGame);
        loadButton.onClick.AddListener(LoadGame);
        galleryButton.onClick.AddListener(() => SceneManager.LoadScene(Constants.GALLERY_SCENE));
        settingButton.onClick.AddListener(() => SceneManager.LoadScene(Constants.SETTING_SCENE));
        quitButton.onClick.AddListener(QuitGame);
        languageButton.onClick.AddListener(UpdateLanguage);
    }
    void StartGame()
    {
        GameManager.Instance.hasStarted = true;
        GameManager.Instance.currentStoryFile = Constants.DEFAULT_STORY_FILE;
        GameManager.Instance.currentLineIndex = Constants.DEFAULT_START_LINE;
        GameManager.Instance.currentBackgroundImg = string.Empty;
        GameManager.Instance.currentBackgroundMusic = string.Empty;
        GameManager.Instance.isCharacter1Display = false;
        GameManager.Instance.isCharacter2Display = false;
        GameManager.Instance.historyRecords = new LinkedList<ExcelReader.ExcelData>();
        SceneManager.LoadScene(Constants.GAME_SCENE);
    }
    void ContinueGame()
    {
        if (GameManager.Instance.hasStarted)
        {
            GameManager.Instance.historyRecords.RemoveLast();
            SceneManager.LoadScene(Constants.GAME_SCENE);
        }
    }
    void LoadGame()
    {
        GameManager.Instance.currentSaveLoadMode = GameManager.SaveLoadMode.Load;
        SceneManager.LoadScene(Constants.SAVE_LOAD_SCENE);
    }
    void QuitGame()
    {
        Application.Quit();
    }
    void UpdateLanguage()
    {
        currentLanguageIndex = (currentLanguageIndex + 1) % Constants.LANGUAGES.Length;

        var language = Constants.LANGUAGES[currentLanguageIndex];
        LocalizationManager.Instance.LoadLanguage(language);

        GameManager.Instance.currentLanguageIndex = currentLanguageIndex;
        GameManager.Instance.currentLanguage = language;

        UpdateLanguageButtonText();
    }
    void UpdateLanguageButtonText()
    {
        var languageButtonTMP = languageButton.GetComponentInChildren<TextMeshProUGUI>();
        languageButtonTMP.text = LM.GLV(Constants.LANGUAGES[currentLanguageIndex]);
    }
}
