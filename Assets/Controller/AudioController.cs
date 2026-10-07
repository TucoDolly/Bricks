using UnityEngine;

/// <summary>
/// Controla música de fundo e efeitos sonoros. É único e persiste ao recarregar
/// a cena (DontDestroyOnLoad), para a música não reiniciar a cada fase.
/// Se não houver um AudioManager na cena, um é criado automaticamente.
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
                _instance = FindAnyObjectByType<AudioController>();
                if (_instance == null)
                {
                    new GameObject("AudioManager").AddComponent<AudioController>();
                }
            }
            return _instance;
        }
    }

    private AudioModel _audioModel;
    private AudioSource _musicSource;
    private AudioSource _sfxSource;

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

        PlayMusic();
    }

    // Usa placeholders gerados para qualquer clip não atribuído no AudioModel
    private void PrepareClips()
    {
        if (_audioModel.Music     == null) _audioModel.Music     = ProceduralAudioGenerator.CreateMelody("Music");
        if (_audioModel.BallHit   == null) _audioModel.BallHit   = ProceduralAudioGenerator.CreateBeep("BallHit", 440f, 0.1f);
        if (_audioModel.PlayerHit == null) _audioModel.PlayerHit = ProceduralAudioGenerator.CreateBeep("PlayerHit", 220f, 0.15f);
        if (_audioModel.BrickHit  == null) _audioModel.BrickHit  = ProceduralAudioGenerator.CreateBeep("BrickHit", 660f, 0.12f);
    }

    public void PlayMusic()
    {
        _musicSource.clip = _audioModel.Music;
        _musicSource.volume = _audioModel.MusicVolume;
        if (!_musicSource.isPlaying) _musicSource.Play();
    }

    public void PlayBallHit()   => PlaySfx(_audioModel.BallHit);
    public void PlayPlayerHit() => PlaySfx(_audioModel.PlayerHit);
    public void PlayBrickHit()  => PlaySfx(_audioModel.BrickHit);

    private void PlaySfx(AudioClip clip)
    {
        if (clip != null) _sfxSource.PlayOneShot(clip, _audioModel.SfxVolume);
    }
}
