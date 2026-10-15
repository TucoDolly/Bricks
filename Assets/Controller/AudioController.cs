using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Controla música de fundo e efeitos sonoros. É único e persiste ao recarregar
/// a cena (DontDestroyOnLoad), para a música não reiniciar a cada fase.
/// Inicializado automaticamente no início do jogo para garantir áudio no Menu e nas Fases.
/// </summary>
[RequireComponent(typeof(AudioModel))]
public class AudioController : MonoBehaviour
{
    private static AudioController _instance;

    public static AudioController Instance
    {
        get
        {
            if (_instance == null)
            {
                EnsureInstance();
            }
            return _instance;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Bootstrap()
    {
        if (_instance != null) return;
        EnsureInstance();
    }

    private static void EnsureInstance()
    {
        if (_instance != null) return;
        _instance = FindAnyObjectByType<AudioController>();
        if (_instance == null)
        {
            GameObject obj = new GameObject("AudioManager");
            _instance = obj.AddComponent<AudioController>();
        }
    }

    private AudioModel _audioModel;
    private AudioSource _musicSource;
    private AudioSource _sfxSource;

    private float _lastHoverTime = -1f;
    private float _lastClickTime = -1f;

    void Awake()
    {
        // Singleton: se a cena recarregada trouxer outro AudioManager, descarta o duplicado
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        transform.SetParent(null);
        DontDestroyOnLoad(gameObject);

        _audioModel = GetComponent<AudioModel>();
        PrepareClips();

        _musicSource = gameObject.AddComponent<AudioSource>();
        _musicSource.loop = true;
        _musicSource.playOnAwake = false;

        _sfxSource = gameObject.AddComponent<AudioSource>();
        _sfxSource.loop = false;
        _sfxSource.playOnAwake = false;
        _sfxSource.ignoreListenerPause = true; // Permite tocar efeitos sonoros mesmo pausado

        EnsureAudioListener();
        PlayMusic();
    }

    private void OnEnable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void Start()
    {
        EnsureAudioListener();
        RegisterAllButtonsInScene();
    }

    private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        EnsureAudioListener();
        RegisterAllButtonsInScene();
    }

    /// <summary>
    /// Garante que haja um AudioListener ativo na cena atual (na câmera ou no próprio AudioManager).
    /// Sem AudioListener, a Unity não reproduz nenhum som para o jogador.
    /// </summary>
    public void EnsureAudioListener()
    {
        if (FindAnyObjectByType<AudioListener>() == null)
        {
            Camera cam = Camera.main;
            if (cam == null) cam = FindAnyObjectByType<Camera>();
            if (cam != null)
            {
                cam.gameObject.AddComponent<AudioListener>();
            }
            else
            {
                gameObject.AddComponent<AudioListener>();
            }
        }
    }

    /// <summary>
    /// Garante que qualquer botão existente na cena receba feedback sonoro ao passar o mouse e ao clicar.
    /// </summary>
    public void RegisterAllButtonsInScene()
    {
        Button[] buttons = FindObjectsByType<Button>(FindObjectsInactive.Include);
        foreach (Button btn in buttons)
        {
            if (btn == null) continue;
            // Se o botão não tem NeonButtonFx nem ButtonAudioTrigger, adiciona ButtonAudioTrigger
            if (!btn.TryGetComponent<NeonButtonFx>(out _) && !btn.TryGetComponent<ButtonAudioTrigger>(out _))
            {
                btn.gameObject.AddComponent<ButtonAudioTrigger>();
            }
        }
    }

    // Usa geradores procedurais caso nenhum clip tenha sido atribuído no AudioModel
    private void PrepareClips()
    {
        if (_audioModel.Music       == null) _audioModel.Music       = ProceduralAudioGenerator.CreateMelody("Music");
        if (_audioModel.BallHit     == null) _audioModel.BallHit     = ProceduralAudioGenerator.CreateBeep("BallHit", 440f, 0.1f);
        if (_audioModel.PlayerHit   == null) _audioModel.PlayerHit   = ProceduralAudioGenerator.CreatePaddleHit("PlayerHit");
        if (_audioModel.BrickHit    == null) _audioModel.BrickHit    = ProceduralAudioGenerator.CreateBeep("BrickHit", 660f, 0.12f);
        if (_audioModel.LoseLife    == null) _audioModel.LoseLife    = ProceduralAudioGenerator.CreateLoseLife("LoseLife");
        if (_audioModel.GameOver    == null) _audioModel.GameOver    = ProceduralAudioGenerator.CreateGameOver("GameOver");
        if (_audioModel.Victory     == null) _audioModel.Victory     = ProceduralAudioGenerator.CreateVictory("Victory");
        if (_audioModel.ButtonHover == null) _audioModel.ButtonHover = ProceduralAudioGenerator.CreateButtonHover("ButtonHover");
        if (_audioModel.ButtonClick == null) _audioModel.ButtonClick = ProceduralAudioGenerator.CreateButtonClick("ButtonClick");
    }

    public void PlayMusic()
    {
        _musicSource.clip = _audioModel.Music;
        _musicSource.volume = _audioModel.MusicVolume;
        if (!_musicSource.isPlaying) _musicSource.Play();
    }

    public void PlayBallHit()     => PlaySfx(_audioModel.BallHit);
    public void PlayPlayerHit()   => PlaySfx(_audioModel.PlayerHit, 1.05f); // Som aveludado e confortável
    public void PlayBrickHit()    => PlaySfx(_audioModel.BrickHit);
    public void PlayLoseLife()    => PlaySfx(_audioModel.LoseLife, 1.0f);
    public void PlayGameOver()    => PlaySfx(_audioModel.GameOver, 1.0f);
    public void PlayVictory()     => PlaySfx(_audioModel.Victory, 1.0f);

    public void PlayButtonHover()
    {
        if (Time.unscaledTime - _lastHoverTime < 0.035f) return;
        _lastHoverTime = Time.unscaledTime;
        PlaySfx(_audioModel.ButtonHover, 0.95f); // Volume aumentado conforme pedido
    }

    public void PlayButtonClick()
    {
        if (Time.unscaledTime - _lastClickTime < 0.05f) return;
        _lastClickTime = Time.unscaledTime;
        PlaySfx(_audioModel.ButtonClick, 1.25f); // Volume aumentado conforme pedido
    }

    private void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip != null && _sfxSource != null)
        {
            _sfxSource.PlayOneShot(clip, Mathf.Clamp01(_audioModel.SfxVolume * volumeScale));
        }
    }
}
