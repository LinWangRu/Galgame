using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class VNManager : MonoBehaviour
{
    #region Variables
    public GameObject dialogueBox;
    public TextMeshProUGUI speakerName;
    public TypewriterEffect typewriterEffect;
    public ScreenShotter screenShotter;

    public Image avatarImage;
    public Image backgroundImage;
    public Image characterImage1;
    public Image characterImage2;

    public GameObject bottomButtons;
    public Button autoButton;
    public Button skipButton;
    public Button saveButton;
    public Button loadButton;
    public Button historyButton;
    public Button settingButton;
    public Button homeButton;
    public Button closeButton;

    private string saveFolderPath;
    private byte[] screenshotData; // �����ͼ����
    private string currentSpeakingContent; // ���浱ǰ�Ի�����

    private List<ExcelReader.ExcelData> storyData;
    private string currentStroyFileName;
    private int currentLine;
    private string currentStoryFileName;
    private float currentTypingSpeed = Constants.DEFAULT_TYPING_SPEED;

    private bool isAutoPlay = false;
    private bool isSkip = false;
    private int maxReachedLineIndex = 0;
    public static VNManager Instance { get; private set; }
    #endregion
    #region Lifecycle
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
    void Start()
    {
        var gm = GameManager.Instance;
        gm.hasStarted = true;
        gm.currentScene = Constants.GAME_SCENE;
        if (gm.pendingData != null)
        {
            var savedData = gm.pendingData;
            gm.pendingData = null;

            gm.currentStoryFile = savedData.savedStoryFileName;
            savedData.savedLine--;

            gm.currentLineIndex = savedData.savedLine;
            savedData.savedHistoryRecords.RemoveLast();

            gm.historyRecords = savedData.savedHistoryRecords;
            gm.currentBackgroundImg = savedData.savedBackgroundImg;
            gm.currentBackgroundMusic = savedData.savedBackgroundMusic;
            gm.currentCharacter1Img = savedData.savedCharacter1Img;
            gm.currentCharacter2Img = savedData.savedCharacter2Img;
            gm.currentCharacter1Position = savedData.savedCharacter1Position;
            gm.currentCharacter2Position = savedData.savedCharacter2Position;
            gm.isCharacter1Display = savedData.savedIsCharacter1Display;
            gm.isCharacter2Display = savedData.savedIsCharacter2Display;
        }
        currentLine = gm.currentLineIndex;
        bottomButtonsAddListener();
        InitializeImage();
        LoadStory(gm.currentStoryFile);
        DisplayNextLine();
    }
    void Update()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space))
        {
            if (!dialogueBox.activeSelf)
            {
                OpenUI();
            }
            else if (!IsHittingBottomButtons() && !ChoiceManager.Instance.choicePanel.activeSelf)
            {
                DisplayNextLine();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (dialogueBox.activeSelf)
            {
                CloseUI();
            }
            else
            {
                OpenUI();
            }
        }
        if (Input.GetKeyDown(KeyCode.LeftControl) || Input.GetKeyDown(KeyCode.RightControl))
        {
            CtrlSkip();
        }
    }
    #endregion
    #region Initialization
    void InitializeSaveFilePath()
    {
        saveFolderPath = Path.Combine(Application.persistentDataPath, Constants.SAVE_FILE_PATH);
        if (!Directory.Exists(saveFolderPath))
        {
            Directory.CreateDirectory(saveFolderPath);
        }
    }
    void bottomButtonsAddListener()
    {
        autoButton.onClick.AddListener(OnAutoButtonClick);
        skipButton.onClick.AddListener(OnSkipButtonClick);
        saveButton.onClick.AddListener(OnSaveButtonClick);
        loadButton.onClick.AddListener(OnLoadButtonClick);
        historyButton.onClick.AddListener(OnHistoryButtonClick);
        settingButton.onClick.AddListener(OnSettingButtonClick);
        homeButton.onClick.AddListener(OnHomeButtonClick);
        closeButton.onClick.AddListener(OnCloseButtonClick);
    }
    void LoadStory(string fileName)
    {
        LoadStoryFromFile(fileName);
        RecoverLastBackgroundAndCharacter();
    }
    void InitializeImage()
    {
        backgroundImage.gameObject.SetActive(false);
        avatarImage.gameObject.SetActive(false);
        characterImage1.gameObject.SetActive(false);
        characterImage2.gameObject.SetActive(false);
    }
    void LoadStoryFromFile(string fileName)
    {
        currentStoryFileName = fileName;
        string filePath = Path.Combine(Application.streamingAssetsPath,
                                        Constants.STORY_PATH,
                                        fileName + Constants.STORY_FILE_EXTENSION);
        storyData = ExcelReader.ReadExcel(filePath);
        if (storyData == null || storyData.Count == 0)
        {
            Debug.LogError(Constants.NO_DATA_FOUND);
        }
        GameManager.Instance.currentStoryFile = currentStoryFileName;

        if (GameManager.Instance.maxReachedLineIndices.ContainsKey(currentStoryFileName))
        {
            maxReachedLineIndex = GameManager.Instance.maxReachedLineIndices[currentStoryFileName];
        }
        else
        {
            maxReachedLineIndex = 0;
            GameManager.Instance.maxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
    }
    #endregion
    #region Display
    void DisplayNextLine()
    {
        // ��鵱ǰ���Ƿ񳬹��ѽ��������������
        if (currentLine > maxReachedLineIndex)
        {
            // �������������
            maxReachedLineIndex = currentLine;
            // ��GameManager�и��µ�ǰ�����ļ������������
            GameManager.Instance.maxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
        if (currentLine == 1)
        {
            DisplayThisLine();
            return;
        }
        // ����Ƿ񵽴�������ݵ�ĩβ
        if (currentLine >= storyData.Count - 1)
        {
            // ��������Զ�����ģʽ����ر��Զ�����
            if (isAutoPlay)
            {
                isAutoPlay = false;
                // �����Զ����Ű�ť��ͼ��Ϊ�ر�״̬
                UpdateButtonImage(Constants.AUTO_OFF, autoButton);
            }

            // ��鵱ǰ�е�˵�����Ƿ�Ϊ���½������
            if (storyData[currentLine].speakerName == Constants.END_OF_STORY)
            {
                // �����Ϸδ��ʼ״̬
                GameManager.Instance.hasStarted = false;
                // ���ؽ�������
                SceneManager.LoadScene(Constants.CREDITS_SCENE);
            }
            // ����Ƿ���ѡ���֧
            else if (storyData[currentLine].speakerName == Constants.CHOICE)
            {
                // ��ʾѡ�����
                ShowChoices();
            }
            // ����Ƿ�����ת����������
            else if (storyData[currentLine].speakerName == Constants.GOTO)
            {
                // ����ָ���Ĺ����ļ�
                LoadStory(storyData[currentLine].speakingContent);
                // ���õ�ǰ�е���ʼλ��
                currentLine = Constants.DEFAULT_START_LINE;
                // ��ʾ��һ�У����¹��µĵ�һ�У�
                DisplayNextLine();
            }
            // ����Ƿ��Ǽ���С��Ϸ
            else if (storyData[currentLine].speakerName == Constants.GAME)
            {
                // ����С��Ϸ
                LoadMiniGame();
            }
            return; // ������ǰ����ִ��
        }

        // �����һ�������Ƿ���ڱ����仯
        if (currentLine + 1 < storyData.Count)
        {
            var nextData = storyData[currentLine + 1];
            // �����һ���Ƿ��б���ͼƬ���뵱ǰ������ͬ
            if (NotNullNorEmpty(nextData.backgroundImageFileName) &&
                nextData.backgroundImageFileName != GameManager.Instance.currentBackgroundImg)
            {
                // ����������������ת��Э��
                StartCoroutine(FadeTransition(nextData));
                return; // ������ǰ����ִ�У��ȴ�ת�����
            }
        }
        
        // �����ֻ�Ч���Ƿ���������
        if (typewriterEffect.IsTyping())
        {
            // ������ɵ�ǰ�еĴ���Ч��
            typewriterEffect.CompleteLine();
        }
        else
        {
            // ������ʾ��ǰ������
            DisplayThisLine();
        }
    }

// ������������ת��Ч��Э��
IEnumerator FadeTransition(ExcelReader.ExcelData nextData)
    {
        // ����������������
        Sequence fadeOut = DOTween.Sequence();
        fadeOut.Join(dialogueBox.GetComponent<TextMeshProUGUI>().DOFade(0, 0.8f));
        fadeOut.Join(backgroundImage.DOFade(0, 0.8f));
        fadeOut.Join(characterImage1.DOFade(0, 0.8f));
        fadeOut.Join(characterImage2.DOFade(0, 0.8f));
        yield return fadeOut.WaitForCompletion();
        // ���������Ч���ϲ���һ�������У�

        // 1. �Ի������ֵ���
        //fadeOut.Join(dialogueBox.GetComponent<TextMeshProUGUI>().DOFade(0, 0.8f));
        // 2. ����ͼƬ����
        //fadeOut.Join(backgroundImage.DOFade(0, 0.8f));
        // 3. ��ɫ1ͼƬ����
        //fadeOut.Join(characterImage1.DOFade(0, 0.8f));
        // 4. ��ɫ2ͼƬ����
        //fadeOut.Join(characterImage2.DOFade(0, 0.8f));

        // �ȴ������������
        yield return fadeOut.WaitForCompletion();

        // ת��������ɺ�
        // ���±���ͼƬΪ�±���
        UpdateBackgroundImage(nextData.backgroundImageFileName);
        // ���ñ���͸����Ϊ��ȫ͸��
        backgroundImage.color = new Color(1, 1, 1, 0);
        // ���ضԻ���
        //dialogueBox.SetActive(false);

        // �������붯������
        Sequence fadeIn = DOTween.Sequence();

        // ���������Ч���ϲ���һ�������У�
        fadeIn.Join(characterImage2.DOFade(1, 0.8f)); // ��ɫ2
        fadeIn.Join(characterImage1.DOFade(1, 0.8f)); // ��ɫ1
        fadeIn.Join(backgroundImage.DOFade(1, 0.8f)); // ����
        // 1. �±���ͼƬ����
        //fadeIn.Join(backgroundImage.DOFade(1, 1.2f));
        // 2. ��ɫ1ͼƬ����
        //fadeIn.Join(characterImage1.DOFade(1, 1.2f));
        // 3. ��ɫ2ͼƬ����
        //fadeIn.Join(characterImage2.DOFade(1, 1.2f));

        // �ȴ����붯�����
        yield return fadeIn.WaitForCompletion();

        // ת����ɺ�
        // ������ʾ�Ի���
        //dialogueBox.SetActive(true);

        // ��ʾ��һ�����ݣ��Զ��������кŵ�����
        DisplayThisLine();
    }
    
    // ��ʾ��ǰ�е��������ݣ��Ի�����ɫ�������ȣ�
    void DisplayThisLine()
    {
        // ����GameManager�м�¼�ĵ�ǰ������
        GameManager.Instance.currentLineIndex = currentLine;

        // ��ȡ��ǰ�еĹ�������
        var data = storyData[currentLine];

        // ����˵�������֣�ͨ�����ػ���������ȡ��
        speakerName.text = LM.GetSpeakerName(data);

        // ��ȡ�����öԻ����ݣ�ͨ�����ػ���������
        currentSpeakingContent = LM.GetSpeakingContent(data);

        // �������ֻ�Ч����ʾ�Ի�����
        typewriterEffect.StartTyping(currentSpeakingContent, currentTypingSpeed);

        // ===== ��ʷ��¼ϵͳ =====
        // ����ǰ��������ӵ���ʷ��¼������
        GameManager.Instance.historyRecords.AddLast(data);

        // ===== ͷ��ͼƬ���� =====
        if (NotNullNorEmpty(data.avatarImageFileName))
        {
            // �����ͷ��ͼƬ��������ʾ
            UpdateAvatarImage(data.avatarImageFileName);
        }
        else
        {
            // û��ͷ��������ͷ��UI
            avatarImage.gameObject.SetActive(false);
        }

        // ===== �������� =====
        if (NotNullNorEmpty(data.vocalAudioFileName))
        {
            // ���Ž�ɫ����
            PlayVocalAudio(data.vocalAudioFileName);
        }

        // ===== ����ͼƬ���� =====
        if (NotNullNorEmpty(data.backgroundImageFileName))
        {
            // ���µ�ǰ����ͼƬ��¼
            GameManager.Instance.currentBackgroundImg = data.backgroundImageFileName;
            // ʵ�ʸ��±���ͼƬ��ʾ
            UpdateBackgroundImage(data.backgroundImageFileName);
        }

        // ===== �������ִ��� =====
        if (NotNullNorEmpty(data.backgroundMusicFileName))
        {
            // ���µ�ǰ�������ּ�¼
            GameManager.Instance.currentBackgroundMusic = data.backgroundMusicFileName;
            // �����µı�������
            PlayBackgroundMusic(data.backgroundMusicFileName);
        }

        // ===== ��ɫ1���� =====
        if (NotNullNorEmpty(data.character1Action))
        {
            if (data.character1Action == Constants.DISAPPEAR)
            {
                // ��ɫ1��ʧ���
                GameManager.Instance.isCharacter1Display = false;
            }
            else
            {
                // ��ɫ1��ʾ���
                GameManager.Instance.isCharacter1Display = true;
                // ���½�ɫ1��ͼƬ��λ�ü�¼
                GameManager.Instance.currentCharacter1Img = data.character1ImageFileName;
                GameManager.Instance.currentCharacter1Position = data.coordinateX1;
            }
            // ʵ�ʸ��½�ɫ1����ʾ״̬
            UpdateCharacterImage(data.character1Action, data.character1ImageFileName,
                                characterImage1, data.coordinateX1);
        }

        // ===== ��ɫ2���� =====
        if (NotNullNorEmpty(data.character2Action))
        {
            if (data.character2Action == Constants.DISAPPEAR)
            {
                // ��ɫ2��ʧ���
                GameManager.Instance.isCharacter2Display = false;
            }
            else
            {
                // ��ɫ2��ʾ���
                GameManager.Instance.isCharacter2Display = true;
                // ���½�ɫ2��ͼƬ��λ�ü�¼
                GameManager.Instance.currentCharacter2Img = data.character2ImageFileName;
                GameManager.Instance.currentCharacter2Position = data.coordinateX2;
            }
            // ʵ�ʸ��½�ɫ2����ʾ״̬
            UpdateCharacterImage(data.character2Action, data.character2ImageFileName,
                                characterImage2, data.coordinateX2);
        }

        // �ƶ�����һ�У�׼����ʾ��
        currentLine++;
    }
    void RecoverLastBackgroundAndCharacter()
    {
        if (NotNullNorEmpty(GameManager.Instance.currentBackgroundImg))
        {
            UpdateBackgroundImage(GameManager.Instance.currentBackgroundImg);
        }

        if (NotNullNorEmpty(GameManager.Instance.currentBackgroundMusic))
        {
            PlayBackgroundMusic(GameManager.Instance.currentBackgroundMusic);
        }
        if (GameManager.Instance.isCharacter1Display)
        {
            UpdateCharacterImage(Constants.APPEAR_AT_INSTANTLY, GameManager.Instance.currentCharacter1Img,
                                characterImage1, GameManager.Instance.currentCharacter1Position);
        }
        if (GameManager.Instance.isCharacter2Display)
        {
            UpdateCharacterImage(Constants.APPEAR_AT_INSTANTLY, GameManager.Instance.currentCharacter2Img,
                                characterImage2, GameManager.Instance.currentCharacter2Position);
        }
    }
    bool NotNullNorEmpty(string str)
    {
        return !string.IsNullOrEmpty(str);
    }
    #endregion
    #region Choices
    void ShowChoices()
    {
        var data = storyData[currentLine];
        var choices = LM.GetSpeakingContent(data)
                        .Split(Constants.ChoiceDelimiter)
                        .Select(s => s.Trim())
                        .ToList();
        var actions = data.avatarImageFileName
                        .Split(Constants.ChoiceDelimiter)
                        .Select(s => s.Trim())
                        .ToList();
        ChoiceManager.Instance.ShowChoices(choices, actions, HandleChoice);
    }
    void HandleChoice(string selectedChoice)
    {
        currentLine = Constants.DEFAULT_START_LINE;
        LoadStory(selectedChoice);
        DisplayNextLine();
    }
    #endregion
    #region MiniGame
    void LoadMiniGame()
    {
        /*var data = storyData[currentLine];
        GameState.WinStoryFileName = data.avatarImageFileName;
        GameState.LoseStoryFileName = data.vocalAudioFileName;
        SceneManager.LoadScene(data.speakingContent);*/
    }
    #endregion
    #region Audios
    void PlayVocalAudio(string audioFileName)
    {
        AudioManager.Instance.PlayVoice(audioFileName);
    }
    void PlayBackgroundMusic(string musicFileName)
    {
        AudioManager.Instance.PlayBackground(musicFileName);
    }
    #endregion
    #region Images
    void UpdateAvatarImage(string imageFileName)
    {
        var imagePath = Constants.AVATAR_PATH + imageFileName;
        UpdateImage(imagePath, avatarImage);
    }
    void UpdateBackgroundImage(string imageFileName)
    {
        string imagePath = Constants.BACKGROUND_PATH + imageFileName;
        UpdateImage(imagePath, backgroundImage);
        // ��¼�ñ�������ʾ
        if (!GameManager.Instance.unlockedBackgrounds.Contains(imageFileName))
        {
            GameManager.Instance.unlockedBackgrounds.Add(imageFileName);
        }
    }
    void UpdateCharacterImage(string action, string imageFileName, Image characterImage, string x)
    {
        // ����actionִ�ж�Ӧ�Ķ��������
        if (action.StartsWith(Constants.APPEAR_AT))
        {
            string imagePath = Constants.CHARACTER_PATH + imageFileName;
            if (NotNullNorEmpty(x))
            {
                UpdateImage(imagePath, characterImage);
                var newPosition = new Vector2(float.Parse(x), characterImage.rectTransform.anchoredPosition.y);
                characterImage.rectTransform.anchoredPosition = newPosition;

                var duration = Constants.DURATION_TIME;
                if (action == Constants.APPEAR_AT_INSTANTLY)
                {
                    duration = 0;
                }
                characterImage.DOFade(1, duration).From(0);
            }
            else
            {
                Debug.LogError(Constants.COORDINATE_MISSING);
            }
        }
        else if (action == Constants.DISAPPEAR)
        {
            characterImage.DOFade(0, Constants.DURATION_TIME).OnComplete(() => characterImage.gameObject.SetActive(false));
        }
        else if (action.StartsWith(Constants.MOVE_TO))
        {
            if (NotNullNorEmpty(x))
            {
                characterImage.rectTransform.DOAnchorPosX(float.Parse(x), Constants.DURATION_TIME);
            }
            else
            {
                Debug.LogError(Constants.COORDINATE_MISSING);
            }
        }
    }
    void UpdateButtonImage(string imageFileName, Button button)
    {
        string imagePath = Constants.BUTTON_PATH + imageFileName;
        UpdateImage(imagePath, button.image);
    }
    void UpdateImage(string imagePath, Image image)
    {
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite != null)
        {
            image.sprite = sprite;
            image.gameObject.SetActive(true);
        }
        else
        {
            Debug.LogError(Constants.IMAGE_LOAD_FAILED + imagePath);
        }
    }
    #endregion
    #region Buttons
    #region Bottom
    bool IsHittingBottomButtons()
    {
        return RectTransformUtility.RectangleContainsScreenPoint(
            bottomButtons.GetComponent<RectTransform>(),
            Input.mousePosition,
            Camera.main
        );
    }
    #endregion
    #region Auto
    void OnAutoButtonClick()
    {
        isAutoPlay = !isAutoPlay;
        UpdateButtonImage((isAutoPlay ? Constants.AUTO_ON : Constants.AUTO_OFF), autoButton);
        if (isAutoPlay)
        {
            StartCoroutine(StartAutoPlay());
        }
    }
    private IEnumerator StartAutoPlay()
    {
        while (isAutoPlay)
        {
            if (!typewriterEffect.IsTyping())
            {
                DisplayNextLine();
            }
            yield return new WaitForSeconds(Constants.DEFAULT_AUTO_WAITING_SECONDS);
        }
    }
    #endregion
    #region Skip
    void OnSkipButtonClick()
    {
        if (!isSkip && CanSkip())
        {
            StartSkip();
        }
        else if (isSkip)
        {
            StopCoroutine(SkipToMaxReachedLine());
            EndSkip();
        }
    }
    bool CanSkip()
    {
        return currentLine < maxReachedLineIndex;
    }
    void StartSkip()
    {
        isSkip = true;
        UpdateButtonImage(Constants.SKIP_ON, skipButton);
        currentTypingSpeed = Constants.SKIP_MODE_TYPING_SPEED; // ���ü���Ĵ��ֻ��ٶ�
        StartCoroutine(SkipToMaxReachedLine());
    }
    private IEnumerator SkipToMaxReachedLine()
    {
        while (isSkip)
        {
            if (CanSkip())
            {
                DisplayThisLine();
            }
            else
            {
                EndSkip();
            }
            yield return new WaitForSeconds(Constants.DEFAULT_SKIP_WAITING_SECONDS); // ���ƿ��������Ľ���
        }
    }
    void EndSkip()
    {
        isSkip = false;
        currentTypingSpeed = Constants.DEFAULT_TYPING_SPEED;
        UpdateButtonImage(Constants.SKIP_OFF, skipButton);
    }
    void CtrlSkip()
    {
        currentTypingSpeed = Constants.SKIP_MODE_TYPING_SPEED; // ���ü���Ĵ��ֻ��ٶ�
        StartCoroutine(SkipWhilePressingCtrl());
    }
    private IEnumerator SkipWhilePressingCtrl()
    {
        while (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            DisplayNextLine();
            yield return new WaitForSeconds(Constants.DEFAULT_SKIP_WAITING_SECONDS); // ���ƿ��������Ľ���
        }
    }
    #endregion
    #region Save
    void OnSaveButtonClick()
    {
        SaveData();
        GameManager.Instance.currentSaveLoadMode = GameManager.SaveLoadMode.Save;
        SceneManager.LoadScene(Constants.SAVE_LOAD_SCENE);
    }
    void SaveData()
    {
        CloseUI();
        Texture2D screenshot = screenShotter.CaptureScreenshot();
        OpenUI();
        var gm = GameManager.Instance;
        gm.pendingData = new GameManager.SaveData
        {
            savedStoryFileName = currentStoryFileName,
            savedLine = currentLine,
            savedScreenshotData=screenshot.EncodeToPNG(),
            savedHistoryRecords = gm.historyRecords,
            savedBackgroundImg = gm.currentBackgroundImg,
            savedBackgroundMusic = gm.currentBackgroundMusic,
            savedCharacter1Img = gm.currentCharacter1Img,
            savedCharacter2Img = gm.currentCharacter2Img,
            savedCharacter1Position = gm.currentCharacter1Position,
            savedCharacter2Position = gm.currentCharacter2Position,
            savedIsCharacter1Display = gm.isCharacter1Display,
            savedIsCharacter2Display = gm.isCharacter2Display
        };
    }
    #endregion
    #region Load
    void OnLoadButtonClick()
    {
        GameManager.Instance.currentSaveLoadMode = GameManager.SaveLoadMode.Load;
        SceneManager.LoadScene(Constants.SAVE_LOAD_SCENE);
    }
    #endregion
    #region History
    void OnHistoryButtonClick()
    {
        SceneManager.LoadScene(Constants.HISTORY_SCENE);
    }
    #endregion
    #region Setting
    void OnSettingButtonClick()
    {
        SceneManager.LoadScene(Constants.SETTING_SCENE);
    }
    #endregion
    #region Home
    void OnHomeButtonClick()
    {
        SceneManager.LoadScene(Constants.MENU_SCENE);
    }
    #endregion
    #region Close
    void OnCloseButtonClick()
    {
        CloseUI();
    }
    void OpenUI()
    {
        dialogueBox.SetActive(true);
        bottomButtons.SetActive(true);
    }
    void CloseUI()
    {
        dialogueBox.SetActive(false);
        bottomButtons.SetActive(false);
    }
    #endregion
    #endregion
}
