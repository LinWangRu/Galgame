using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GalleryManager : MonoBehaviour
{
    public TextMeshProUGUI panelTitle;
    public Button[] galleryButtons;
    public Button prevPageButton;
    public Button nextPageButton;
    public Button backButton;

    public GameObject bigImagePanel; // ȫ����ͼ���
    public Image bigImage;

    private int currentPage = Constants.DEFAULT_START_INDEX;
    private readonly int slotsPerPage = Constants.GALLERY_SLOTS_PER_PAGE;
    private readonly int totalSlots = Constants.ALL_BACKGROUNDS.Length;
    public static GalleryManager Instance { get; private set; }
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
        panelTitle.text = LM.GLV(Constants.GALLERY);
        prevPageButton.GetComponentInChildren<TextMeshProUGUI>().text = LM.GLV(Constants.PREV_PAGE);
        nextPageButton.GetComponentInChildren<TextMeshProUGUI>().text = LM.GLV(Constants.NEXT_PAGE);
        backButton.GetComponentInChildren<TextMeshProUGUI>().text = LM.GLV(Constants.BACK);

        prevPageButton.onClick.AddListener(PrevPage);
        nextPageButton.onClick.AddListener(NextPage);
        backButton.onClick.AddListener(GoBack);

        bigImagePanel.SetActive(false);
        Button bigImageButton = bigImagePanel.GetComponent<Button>();
        if (bigImageButton != null)
        {
            bigImageButton.onClick.AddListener(HideBigImage);
        }
        UpdateUI();
    }
    void UpdateUI()
    {
        for (int i = 0; i < slotsPerPage; ++i)
        {
            int slotIndex = currentPage * slotsPerPage + i;
            if (slotIndex < totalSlots)
            {
                UpdateGalleryButtons(galleryButtons[i], slotIndex);
            }
            else
            {
                galleryButtons[i].gameObject.SetActive(false);
            }
        }
    }
    private void UpdateGalleryButtons(Button button, int index)
    {
        button.gameObject.SetActive(true);
        string bgName = Constants.ALL_BACKGROUNDS[index];
        bool isUnlocked = GameManager.Instance.unlockedBackgrounds.Contains(bgName);

        string imagePath = Constants.THUMBNAIL_PATH + (isUnlocked ? bgName : Constants.GALLERY_PLACEHOLDER);
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite != null)
        {
            button.GetComponentInChildren<Image>().sprite = sprite;
        }
        else
        {
            Debug.LogError(Constants.IMAGE_LOAD_FAILED + imagePath);
        }
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => OnButtonClick(button, index));
    }
    private void OnButtonClick(Button button, int index)
    {
        string bgName = Constants.ALL_BACKGROUNDS[index];
        bool isUnlocked = GameManager.Instance.unlockedBackgrounds.Contains(bgName);

        if (!isUnlocked)
        {
            return;
        }

        string imagePath = Constants.BACKGROUND_PATH + bgName;
        Sprite sprite = Resources.Load<Sprite>(imagePath);
        if (sprite != null)
        {
            bigImage.sprite = sprite;
            bigImagePanel.SetActive(true);
        }
        else
        {
            Debug.LogError(Constants.BIG_IMAGE_LOAD_FAILED + imagePath);
        }
    }
    private void HideBigImage()
    {
        bigImagePanel.SetActive(false);
    }
    private void PrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            UpdateUI();
        }
    }

    private void NextPage()
    {
        if ((currentPage + 1) * slotsPerPage < totalSlots)
        {
            currentPage++;
            UpdateUI();
        }
    }
    private void GoBack()
    {
        SceneManager.LoadScene(Constants.MENU_SCENE);
    }
}