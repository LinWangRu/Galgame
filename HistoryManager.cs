using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class HistoryManager : MonoBehaviour
{
    public Transform historyContent; // ScrollView��Content����
    public GameObject historyItemPrefab; // ������ʾ��ʷ��¼���ı�Ԥ�Ƽ�
    public GameObject historyScrollView; // ScrollView����
    public Button closeButton; // �رհ�ť

    private LinkedList<ExcelReader.ExcelData> historyRecords; // ������ʷ��¼

    public static HistoryManager Instance { get; private set; }
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
    void Update()
    {
        // �������Ҽ����
        if (Input.GetMouseButtonDown(1))
        {
            CloseHistory();
        }
    }
    void Start()
    {
        closeButton.GetComponentInChildren<TextMeshProUGUI>().text = LM.GLV(Constants.CLOSE);
        closeButton.onClick.AddListener(CloseHistory); // �󶨹رհ�ť�¼�
        ShowHistory(GameManager.Instance.historyRecords); // ��ʾ��ʷ��¼
    }

    // ��ʾ��ʷ��¼
    public void ShowHistory(LinkedList<ExcelReader.ExcelData> records)
    {
        // ������е���ʷ��¼
        foreach (Transform child in historyContent)
        {
            Destroy(child.gameObject);
        }
        historyRecords = records;
        LinkedListNode<ExcelReader.ExcelData> currentNode = historyRecords.Last;
        while (currentNode != null)
        {
            var name = LM.GetSpeakerName(currentNode.Value); // ��ȡ˵��������
            var content = LM.GetSpeakingContent(currentNode.Value); // ��ȡ˵������
            AddHistoryItem(name + LM.GLV(Constants.COLON) + content); // �����ʷ��¼��
            currentNode = currentNode.Previous; // �ƶ���ǰһ���ڵ�
        }

        historyContent.GetComponent<RectTransform>().localPosition = Vector3.zero; // ��������ͼ��λ������Ϊ����
        historyScrollView.SetActive(true); // ��ʾ��ʷ��¼����
    }

    // �ر���ʷ��¼
    public void CloseHistory()
    {
        GameManager.Instance.historyRecords.RemoveLast(); // �Ƴ����һ����ʷ��¼
        SceneManager.LoadScene(Constants.GAME_SCENE); // ���ص���ǰ����
    }
    
    // �����ʷ��¼��
    private void AddHistoryItem(string text)
    {
        GameObject historyItem = Instantiate(historyItemPrefab, historyContent);
        historyItem.GetComponentInChildren<TextMeshProUGUI>().text = text;
        historyItem.transform.SetAsFirstSibling(); // ����ʷ��¼����ڶ���
    }
}
