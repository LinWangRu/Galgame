using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CreditsScroller : MonoBehaviour
{
    public RectTransform creditsText;
    void Start()
    {
        LoadCreditsFromFile();

        creditsText.anchoredPosition = new Vector2(creditsText.anchoredPosition.x, -Screen.height);
    }

    void Update()
    {
        float speedMultiplier = Input.GetMouseButton(0) ? 3f : 1f;
        creditsText.anchoredPosition += Vector2.up * (Screen.height / 10) * speedMultiplier * Time.deltaTime;

        if (creditsText.anchoredPosition.y >= Constants.CREDITS_SCROLL_END_Y)
        {
            SceneManager.LoadScene(Constants.MENU_SCENE);
        }
    }

    void LoadCreditsFromFile()
    {
        string path = Path.Combine(Application.streamingAssetsPath,
                                    Constants.CREDITS_PATH,
                                    GameManager.Instance.currentLanguage + Constants.CREDITS_FILE_EXTENSION);
        if (File.Exists(path))
        {
            string content = File.ReadAllText(path);
            creditsText.GetComponent<TextMeshProUGUI>().text = content;
        }
        else
        {
            Debug.LogError(Constants.CREDITS_LOAD_FAILED + path);
        }
    }
}
