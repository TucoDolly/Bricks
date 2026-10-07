using UnityEngine;

/// <summary>Gera AudioClips placeholder (beeps e melodia) por código.</summary>
public static class ProceduralAudioGenerator
{
    private const int SampleRate = 44100;

    /// <summary>Beep curto com decaimento exponencial.</summary>
    public static AudioClip CreateBeep(string name, float frequency, float duration)
    {
        int count = (int)(SampleRate * duration);
        float[] data = new float[count];
        for (int i = 0; i < count; i++)
        {
            float t = (float)i / SampleRate;
            float envelope = Mathf.Exp(-6f * t / duration);
            data[i] = Mathf.Sin(2f * Mathf.PI * frequency * t) * envelope * 0.6f;
        }
        return MakeClip(name, data);
    }

    /// <summary>Melodia simples em loop (arpejo em escala menor pentatônica).</summary>
    public static AudioClip CreateMelody(string name)
    {
        float[] notes = { 261.63f, 311.13f, 392.00f, 466.16f, 392.00f, 311.13f, 349.23f, 293.66f };
        float noteDuration = 0.35f;
        int perNote = (int)(SampleRate * noteDuration);
        float[] data = new float[perNote * notes.Length];

        for (int n = 0; n < notes.Length; n++)
        {
            for (int i = 0; i < perNote; i++)
            {
                float t = (float)i / SampleRate;
                float envelope = Mathf.Clamp01(1f - t / noteDuration) * Mathf.Clamp01(t * 60f);
                // Onda triangular suave
                float phase = Mathf.Repeat(t * notes[n], 1f);
                float tri = 4f * Mathf.Abs(phase - 0.5f) - 1f;
                data[n * perNote + i] = tri * envelope * 0.35f;
            }
        }
        return MakeClip(name, data);
    }

    private static AudioClip MakeClip(string name, float[] data)
    {
        AudioClip clip = AudioClip.Create(name, data.Length, 1, SampleRate, false);
        clip.SetData(data, 0);
        return clip;
    }
}
