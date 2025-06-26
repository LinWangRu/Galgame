public class Constants
{
    public static string VIDEO_PATH = "video";
    public static string VIDEO_FILE_EXTENSION = ".mp4";

    public static string MENU_SCENE = "MenuScene";
    public static string GAME_SCENE = "GameScene";
    public static string INPUT_SCENE = "InputScene";
    public static string SETTING_SCENE = "SettingScene";
    public static string SAVE_LOAD_SCENE = "SaveLoadScene";
    public static string GALLERY_SCENE = "GalleryScene";
    public static string HISTORY_SCENE = "HistoryScene";
    public static string CREDITS_SCENE = "CreditsScene";

    public static string STORY_PATH = "story";
    public static string DEFAULT_STORY_FILE = "1";
    public static string STORY_FILE_EXTENSION = ".xlsx";
    public static int DEFAULT_START_LINE = 1;

    public static string AVATAR_PATH = "image/avatar/";
    public static string BACKGROUND_PATH = "image/background/";
    public static string BUTTON_PATH = "image/button/";
    public static string CHARACTER_PATH = "image/character/";
    public static string THUMBNAIL_PATH = "image/thumbnail/";
    public static string IMAGE_LOAD_FAILED = "Failed to load image: ";
    public static string BIG_IMAGE_LOAD_FAILED = "Failed to load big image: ";

    public static float DEFAULT_TYPING_SPEED = 0.05f;
    public static float SKIP_MODE_TYPING_SPEED = 0.01f;

    public static string AUTO_ON = "autoplayon";
    public static string AUTO_OFF = "autoplayoff";
    public static float DEFAULT_AUTO_WAITING_SECONDS = 2.5f;
    
    public static string SKIP_ON = "skipon";
    public static string SKIP_OFF = "skipoff";
    public static float DEFAULT_SKIP_WAITING_SECONDS = 0.3f;

    public static string VOCAL_PATH = "audio/vocal/";
    public static string MUSIC_PATH = "audio/music/";
    public static string AUDIO_LOAD_FAILED = "Failed to load audio: ";
    public static string MENU_MUSIC_FILE_NAME = "1";
    public static string CREDITS_MUSIC_FILE_NAME = "4";

    public static string NO_DATA_FOUND = "No data found";
    public static string END_OF_STORY = "End of story";
    public static string CHOICE = "choice";
    public static string GAME = "game";
    public static string MAIN_GAME_SCENE = "MainGame";

    public static string APPEAR_AT = "appearAt";
    public static string APPEAR_AT_INSTANTLY = "appearAtInstantly";
    public static string DISAPPEAR = "disappear";
    public static string MOVE_TO = "moveTo";
    public static int DURATION_TIME = 1;
    public static string COORDINATE_MISSING = "Coordinate missing";

    public static int DEFAULT_START_INDEX = 0;
    public static int SLOTS_PER_PAGE = 8;
    public static int TOTAL_SLOTS = 40;
    public static string COLON = "colon";
    public static string SAVE_GAME = "save_game";
    public static string LOAD_GAME = "load_game";
    public static string EMPLY_SLOT = "empty_slot";

    public static string CAMERA_NOT_FOUND = "Main camera not found!";
    public static string SAVE_FILE_PATH = "saves";
    public static string SAVE_FILE_EXTENSION = ".json";

    public static string X = "x";

    public static int MAX_LENGTH = 50;
    public static int GALLERY_SLOTS_PER_PAGE = 9;
    public static string GALLERY = "gallery";
    public static string GALLERY_PLACEHOLDER = "gallery_placeholder";
    public static string[] ALL_BACKGROUNDS = { "1", "2", "3", "4", "c_���", "c_���2", "c_���3", "c_��01", "c_��03", "c_��04", "c_��05" /* �����������ļ����� */ };
    public static string UNLOCKED = "unlocked";

    public static string CONFIRM = "confirm";
    public static string PROMPT_TEXT = "enter_name";
    public static string NAME_PLACEHOLDER = "[Name]";

    public static string GOTO = "goto";
    public static string NEW_STORY_FILE_NAME = "11";

    public static string DEFAULT_LANGUAGE = "zh";
    public static string LANGUAGE_PATH = "languages";
    public static string JSON_FILE_EXTENSION = ".json";
    public static string LOCALIZATION_LOAD_FAILED = "Failed to load localization file: ";
    public static int DEFAULT_LANGUAGE_INDEX = 0;

    public static string[] LANGUAGES = { "zh", "en", "ja" };

    public static string PREV_PAGE = "previous_page";
    public static string NEXT_PAGE = "next_page";
    public static string BACK = "back";
    public static string CLOSE = "close";

    public static string FULLSCREEN = "fullscreen";
    public static string WINDOWED = "windowed";
    public static string RESET = "reset";

    public static string MASTER_VOLUME = "MasterVolume";
    public static string MUSIC_VOLUME = "MusicVolume";
    public static string VOICE_VOLUME = "VoiceVolume";

    public static float DEFAULT_VOLUME = 0.8f;

    public static string CREDITS_PATH = "credits";
    public static string CREDITS_FILE_EXTENSION = ".txt";
    public static string CREDITS_LOAD_FAILED = "Failed to load credits file: ";
    public static float CREDITS_SCROLL_SPEED = 100f;
    public static float CREDITS_SCROLL_END_Y = 2000f;

    public static char ChoiceDelimiter = '\n';

    public static string CONFIRM_DELETE_SAVE_FILE = "confirm_delete_save_file";
    public static string CONFIRM_COVER_SAVE_FILE = "confirm_cover_save_file";
}