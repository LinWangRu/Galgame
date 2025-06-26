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
    private byte[] screenshotData; // 保存截图数据
    private string currentSpeakingContent; // 保存当前对话内容

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
        // 检查当前行是否超过已解锁的最大行索引
        if (currentLine > maxReachedLineIndex)
        {
            // 更新最大行索引
            maxReachedLineIndex = currentLine;
            // 在GameManager中更新当前故事文件的最大行索引
            GameManager.Instance.maxReachedLineIndices[currentStoryFileName] = maxReachedLineIndex;
        }
        if (currentLine == 1)
        {
            DisplayThisLine();
            return;
        }
        // 检查是否到达故事数据的末尾
        if (currentLine >= storyData.Count - 1)
        {
            // 如果处于自动播放模式，则关闭自动播放
            if (isAutoPlay)
            {
                isAutoPlay = false;
                // 更新自动播放按钮的图标为关闭状态
                UpdateButtonImage(Constants.AUTO_OFF, autoButton);
            }

            // 检查当前行的说话者是否为故事结束标记
            if (storyData[currentLine].speakerName == Constants.END_OF_STORY)
            {
                // 标记游戏未开始状态
                GameManager.Instance.hasStarted = false;
                // 加载结束场景
                SceneManager.LoadScene(Constants.CREDITS_SCENE);
            }
            // 检查是否是选择分支
            else if (storyData[currentLine].speakerName == Constants.CHOICE)
            {
                // 显示选择界面
                ShowChoices();
            }
            // 检查是否是跳转到其他故事
            else if (storyData[currentLine].speakerName == Constants.GOTO)
            {
                // 加载指定的故事文件
                LoadStory(storyData[currentLine].speakingContent);
                // 重置当前行到起始位置
                currentLine = Constants.DEFAULT_START_LINE;
                // 显示下一行（即新故事的第一行）
                DisplayNextLine();
            }
            // 检查是否是加载小游戏
            else if (storyData[currentLine].speakerName == Constants.GAME)
            {
                // 加载小游戏
                LoadMiniGame();
            }
            return; // 结束当前函数执行
        }

        // 检查下一行数据是否存在背景变化
        if (currentLine + 1 < storyData.Count)
        {
            var nextData = storyData[currentLine + 1];
            // 检查下一行是否有背景图片且与当前背景不同
            if (NotNullNorEmpty(nextData.backgroundImageFileName) &&
                nextData.backgroundImageFileName != GameManager.Instance.currentBackgroundImg)
            {
                // 启动背景淡出淡入转场协程
                StartCoroutine(FadeTransition(nextData));
                return; // 结束当前函数执行，等待转场完成
            }
        }
        
        // 检查打字机效果是否正在运行
        if (typewriterEffect.IsTyping())
        {
            // 立即完成当前行的打字效果
            typewriterEffect.CompleteLine();
        }
        else
        {
            // 正常显示当前行内容
            DisplayThisLine();
        }
    }

// 背景淡出淡入转场效果协程
IEnumerator FadeTransition(ExcelReader.ExcelData nextData)
    {
        // 创建淡出动画序列
        Sequence fadeOut = DOTween.Sequence();
        fadeOut.Join(dialogueBox.GetComponent<TextMeshProUGUI>().DOFade(0, 0.8f));
        fadeOut.Join(backgroundImage.DOFade(0, 0.8f));
        fadeOut.Join(characterImage1.DOFade(0, 0.8f));
        fadeOut.Join(characterImage2.DOFade(0, 0.8f));
        yield return fadeOut.WaitForCompletion();
        // 将多个淡出效果合并到一个序列中：

        // 1. 对话框文字淡出
        //fadeOut.Join(dialogueBox.GetComponent<TextMeshProUGUI>().DOFade(0, 0.8f));
        // 2. 背景图片淡出
        //fadeOut.Join(backgroundImage.DOFade(0, 0.8f));
        // 3. 角色1图片淡出
        //fadeOut.Join(characterImage1.DOFade(0, 0.8f));
        // 4. 角色2图片淡出
        //fadeOut.Join(characterImage2.DOFade(0, 0.8f));

        // 等待淡出动画完成
        yield return fadeOut.WaitForCompletion();

        // 转场动画完成后：
        // 更新背景图片为新背景
        UpdateBackgroundImage(nextData.backgroundImageFileName);
        // 重置背景透明度为完全透明
        backgroundImage.color = new Color(1, 1, 1, 0);
        // 隐藏对话框
        //dialogueBox.SetActive(false);

        // 创建淡入动画序列
        Sequence fadeIn = DOTween.Sequence();

        // 将多个淡入效果合并到一个序列中：
        fadeIn.Join(characterImage2.DOFade(1, 0.8f)); // 角色2
        fadeIn.Join(characterImage1.DOFade(1, 0.8f)); // 角色1
        fadeIn.Join(backgroundImage.DOFade(1, 0.8f)); // 背景
        // 1. 新背景图片淡入
        //fadeIn.Join(backgroundImage.DOFade(1, 1.2f));
        // 2. 角色1图片淡入
        //fadeIn.Join(characterImage1.DOFade(1, 1.2f));
        // 3. 角色2图片淡入
        //fadeIn.Join(characterImage2.DOFade(1, 1.2f));

        // 等待淡入动画完成
        yield return fadeIn.WaitForCompletion();

        // 转场完成后：
        // 重新显示对话框
        //dialogueBox.SetActive(true);

        // 显示下一行内容（自动处理了行号递增）
        DisplayThisLine();
    }
    
    // 显示当前行的所有内容（对话、角色、背景等）
    void DisplayThisLine()
    {
        // 更新GameManager中记录的当前行索引
        GameManager.Instance.currentLineIndex = currentLine;

        // 获取当前行的故事数据
        var data = storyData[currentLine];

        // 设置说话者名字（通过本地化管理器获取）
        speakerName.text = LM.GetSpeakerName(data);

        // 获取并设置对话内容（通过本地化管理器）
        currentSpeakingContent = LM.GetSpeakingContent(data);

        // 启动打字机效果显示对话内容
        typewriterEffect.StartTyping(currentSpeakingContent, currentTypingSpeed);

        // ===== 历史记录系统 =====
        // 将当前行数据添加到历史记录链表中
        GameManager.Instance.historyRecords.AddLast(data);

        // ===== 头像图片处理 =====
        if (NotNullNorEmpty(data.avatarImageFileName))
        {
            // 如果有头像图片，更新显示
            UpdateAvatarImage(data.avatarImageFileName);
        }
        else
        {
            // 没有头像则隐藏头像UI
            avatarImage.gameObject.SetActive(false);
        }

        // ===== 语音处理 =====
        if (NotNullNorEmpty(data.vocalAudioFileName))
        {
            // 播放角色语音
            PlayVocalAudio(data.vocalAudioFileName);
        }

        // ===== 背景图片处理 =====
        if (NotNullNorEmpty(data.backgroundImageFileName))
        {
            // 更新当前背景图片记录
            GameManager.Instance.currentBackgroundImg = data.backgroundImageFileName;
            // 实际更新背景图片显示
            UpdateBackgroundImage(data.backgroundImageFileName);
        }

        // ===== 背景音乐处理 =====
        if (NotNullNorEmpty(data.backgroundMusicFileName))
        {
            // 更新当前背景音乐记录
            GameManager.Instance.currentBackgroundMusic = data.backgroundMusicFileName;
            // 播放新的背景音乐
            PlayBackgroundMusic(data.backgroundMusicFileName);
        }

        // ===== 角色1处理 =====
        if (NotNullNorEmpty(data.character1Action))
        {
            if (data.character1Action == Constants.DISAPPEAR)
            {
                // 角色1消失标记
                GameManager.Instance.isCharacter1Display = false;
            }
            else
            {
                // 角色1显示标记
                GameManager.Instance.isCharacter1Display = true;
                // 更新角色1的图片和位置记录
                GameManager.Instance.currentCharacter1Img = data.character1ImageFileName;
                GameManager.Instance.currentCharacter1Position = data.coordinateX1;
            }
            // 实际更新角色1的显示状态
            UpdateCharacterImage(data.character1Action, data.character1ImageFileName,
                                characterImage1, data.coordinateX1);
        }

        // ===== 角色2处理 =====
        if (NotNullNorEmpty(data.character2Action))
        {
            if (data.character2Action == Constants.DISAPPEAR)
            {
                // 角色2消失标记
                GameManager.Instance.isCharacter2Display = false;
            }
            else
            {
                // 角色2显示标记
                GameManager.Instance.isCharacter2Display = true;
                // 更新角色2的图片和位置记录
                GameManager.Instance.currentCharacter2Img = data.character2ImageFileName;
                GameManager.Instance.currentCharacter2Position = data.coordinateX2;
            }
            // 实际更新角色2的显示状态
            UpdateCharacterImage(data.character2Action, data.character2ImageFileName,
                                characterImage2, data.coordinateX2);
        }

        // 移动到下一行（准备显示）
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
        // 记录该背景已显示
        if (!GameManager.Instance.unlockedBackgrounds.Contains(imageFileName))
        {
            GameManager.Instance.unlockedBackgrounds.Add(imageFileName);
        }
    }
    void UpdateCharacterImage(string action, string imageFileName, Image characterImage, string x)
    {
        // 根据action执行对应的动画或操作
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
        currentTypingSpeed = Constants.SKIP_MODE_TYPING_SPEED; // 设置极快的打字机速度
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
            yield return new WaitForSeconds(Constants.DEFAULT_SKIP_WAITING_SECONDS); // 控制快速跳过的节奏
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
        currentTypingSpeed = Constants.SKIP_MODE_TYPING_SPEED; // 设置极快的打字机速度
        StartCoroutine(SkipWhilePressingCtrl());
    }
    private IEnumerator SkipWhilePressingCtrl()
    {
        while (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            DisplayNextLine();
            yield return new WaitForSeconds(Constants.DEFAULT_SKIP_WAITING_SECONDS); // 控制快速跳过的节奏
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
