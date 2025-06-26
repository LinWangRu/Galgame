using System.Collections.Generic;
using UnityEngine;
using System.IO;          // �ṩ File, Path
using Newtonsoft.Json;    // �ṩ JsonConvert, Formatting
public class GameManager : MonoBehaviour
{
    public string playerName;
    public string currentScene;
    public string currentStoryFile;
    public int currentLineIndex;
    public int currentLanguageIndex = Constants.DEFAULT_LANGUAGE_INDEX;
    public string currentLanguage = Constants.DEFAULT_LANGUAGE;
    public string currentBackgroundImg;
    public string currentBackgroundMusic;
    public bool isCharacter1Display;
    public bool isCharacter2Display;
    public string currentCharacter1Img;
    public string currentCharacter2Img;
    public string currentCharacter1Position;
    public string currentCharacter2Position;

    public bool hasStarted;
    public HashSet<string> unlockedBackgrounds = new HashSet<string>(); // �����ѽ����ı���
    public Dictionary<string, int> maxReachedLineIndices = new Dictionary<string, int>(); // ȫ�ִ洢ÿ���ļ�����Զ������
    public LinkedList<ExcelReader.ExcelData> historyRecords; // ������ʷ��¼
    public enum SaveLoadMode { None,Save, Load }
    public SaveLoadMode currentSaveLoadMode { get; set; }=SaveLoadMode.None;
    public SaveData pendingData;
    public void Save(int slotIndex)
    {
        string path = GenerateDataPath(slotIndex);
        string directory = Path.GetDirectoryName(path);
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        File.WriteAllText(path, JsonConvert.SerializeObject(pendingData, Formatting.Indented));
    }
    public void Load(int slotIndex)
    {
        string path = GenerateDataPath(slotIndex); 
        string jsonData = File.ReadAllText(path);
        pendingData = JsonConvert.DeserializeObject<SaveData>(jsonData);
        //pendingData = JsonConvert.DeserializeObject<SaveData>(currentStoryFile.ReadAllText(path));
    }
    public string GenerateDataPath(int index)
    {
        return Path.Combine(Application.persistentDataPath, Constants.SAVE_FILE_PATH, index + Constants.SAVE_FILE_EXTENSION);
    }
    public class SaveData
    {
        public string savedStoryFileName;
        public int savedLine;
        public bool savedIsCharacter1Display;
        public bool savedIsCharacter2Display;
        public byte[] savedScreenshotData;
        public LinkedList<ExcelReader.ExcelData> savedHistoryRecords;
        public string savedBackgroundImg;
        public string savedBackgroundMusic;
        public string savedCharacter1Img;
        public string savedCharacter2Img;
        public string savedCharacter1Position;
        public string savedCharacter2Position;
        public bool savedCharacter1Display;
        public bool savedCharacter2Display;
    }
    public static GameManager Instance { get; private set; }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // �糡������
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
