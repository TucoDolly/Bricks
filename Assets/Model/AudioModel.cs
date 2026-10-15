using UnityEngine;

/// <summary>
/// Dados de áudio do jogo. Clips deixados vazios são gerados proceduralmente
/// pelo AudioController (placeholders) até que os arquivos finais em
/// Assets/Audio/ sejam atribuídos aqui.
/// </summary>
public class AudioModel : MonoBehaviour
{
    [Header("Clips de Gameplay (opcional - vazio = placeholder gerado)")]
    [SerializeField] private AudioClip _music;
    [SerializeField] private AudioClip _ballHit;
    [SerializeField] private AudioClip _playerHit;
    [SerializeField] private AudioClip _brickHit;
    [SerializeField] private AudioClip _loseLife;
    [SerializeField] private AudioClip _gameOver;
    [SerializeField] private AudioClip _victory;

    [Header("Clips de UI (opcional - vazio = placeholder gerado)")]
    [SerializeField] private AudioClip _buttonHover;
    [SerializeField] private AudioClip _buttonClick;

    [Header("Volumes")]
    [Range(0f, 1f)] [SerializeField] private float _musicVolume = 0.3f;
    [Range(0f, 1f)] [SerializeField] private float _sfxVolume = 0.8f;

    public AudioClip Music       { get => _music;       set => _music = value; }
    public AudioClip BallHit     { get => _ballHit;     set => _ballHit = value; }
    public AudioClip PlayerHit   { get => _playerHit;   set => _playerHit = value; }
    public AudioClip BrickHit    { get => _brickHit;    set => _brickHit = value; }
    public AudioClip LoseLife    { get => _loseLife;    set => _loseLife = value; }
    public AudioClip GameOver    { get => _gameOver;    set => _gameOver = value; }
    public AudioClip Victory     { get => _victory;     set => _victory = value; }
    public AudioClip ButtonHover { get => _buttonHover; set => _buttonHover = value; }
    public AudioClip ButtonClick { get => _buttonClick; set => _buttonClick = value; }

    public float MusicVolume { get => _musicVolume; set => _musicVolume = value; }
    public float SfxVolume   { get => _sfxVolume;   set => _sfxVolume = value; }
}
