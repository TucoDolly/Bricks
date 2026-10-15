using UnityEngine;

/// <summary>Gera AudioClips placeholder (beeps, melodia e SFX neon) por código.</summary>
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

    /// <summary>
    /// Batida da bola na raquete: som aveludado, macio e encorpado (pop/thump suave),
    /// com ataque gradual de 6ms para eliminar completamente estalos ou ruídos agudos
    /// incômodos no fone de ouvido.
    /// </summary>
    public static AudioClip CreatePaddleHit(string name)
    {
        float duration = 0.11f;
        int count = (int)(SampleRate * duration);
        float[] data = new float[count];
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / SampleRate;

            // Frequência confortável e acolhedora: 310Hz deslizando suavemente para 190Hz
            float freq = Mathf.Lerp(310f, 190f, t / duration);
            phase += 2f * Mathf.PI * freq / SampleRate;

            // Fade-in de 6ms para suavizar a entrada no fone (zero estalo/clique de corte)
            float attack = Mathf.Clamp01(t / 0.006f);
            // Decaimento exponencial suave e orgânico
            float decay = Mathf.Exp(-8.5f * t / duration);
            float envelope = attack * decay;

            // Onda senoidal pura combinada com um sub-grave sutil para dar corpo e profundidade
            float sine = Mathf.Sin(phase);
            float sub = Mathf.Sin(phase * 0.5f) * 0.22f;
            float wave = (sine + sub) / 1.22f;

            data[i] = wave * envelope * 0.9f;
        }
        return MakeClip(name, data);
    }

    /// <summary>Som de perder vida: tom descendente estilo retro/synth.</summary>
    public static AudioClip CreateLoseLife(string name)
    {
        float duration = 0.45f;
        int count = (int)(SampleRate * duration);
        float[] data = new float[count];
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / SampleRate;
            // Desce de 550Hz para 160Hz com leve modulação de 16Hz
            float baseFreq = Mathf.Lerp(550f, 160f, t / duration);
            float warble = Mathf.Sin(2f * Mathf.PI * 16f * t) * 18f;
            float freq = baseFreq + warble;
            phase += 2f * Mathf.PI * freq / SampleRate;

            float attack = Mathf.Clamp01(t * 120f);
            float envelope = attack * Mathf.Pow(1f - t / duration, 1.4f);

            // Onda triangular suave com presença
            float tri = 4f * Mathf.Abs(Mathf.Repeat(phase / (2f * Mathf.PI), 1f) - 0.5f) - 1f;
            float sine = Mathf.Sin(phase);
            data[i] = (tri * 0.55f + sine * 0.45f) * envelope * 0.85f;
        }
        return MakeClip(name, data);
    }

    /// <summary>Som de Game Over: sequência descendente melancólica em 4 notas.</summary>
    public static AudioClip CreateGameOver(string name)
    {
        // Notas: Eb4 (311Hz), C4 (261Hz), Ab3 (207Hz), F3 (174Hz caindo)
        float[] noteFreqs = { 311.13f, 261.63f, 207.65f, 174.61f };
        float[] noteDurations = { 0.22f, 0.22f, 0.24f, 0.65f };

        float totalDuration = 0f;
        foreach (float d in noteDurations) totalDuration += d;

        int count = (int)(SampleRate * totalDuration);
        float[] data = new float[count];

        int sampleIndex = 0;
        for (int n = 0; n < noteFreqs.Length; n++)
        {
            int noteSamples = (int)(SampleRate * noteDurations[n]);
            float phase = 0f;

            for (int i = 0; i < noteSamples && sampleIndex < count; i++)
            {
                float t = (float)i / SampleRate;
                float freq = noteFreqs[n];
                if (n == noteFreqs.Length - 1)
                {
                    // Última nota desliza suavemente para baixo
                    freq = Mathf.Lerp(noteFreqs[n], 135f, t / noteDurations[n]);
                }
                phase += 2f * Mathf.PI * freq / SampleRate;

                float attack = Mathf.Clamp01(t * 80f);
                float decay = (n == noteFreqs.Length - 1) ? Mathf.Exp(-3.5f * t / noteDurations[n]) : Mathf.Clamp01(1f - t / noteDurations[n]);
                float envelope = attack * decay;

                float sine = Mathf.Sin(phase);
                float tri = 4f * Mathf.Abs(Mathf.Repeat(phase / (2f * Mathf.PI), 1f) - 0.5f) - 1f;
                data[sampleIndex++] = (tri * 0.5f + sine * 0.5f) * envelope * 0.85f;
            }
        }
        return MakeClip(name, data);
    }

    /// <summary>Som de Vitória na Fase: fanfarra triunfante ascendente com acorde final sustentado.</summary>
    public static AudioClip CreateVictory(string name)
    {
        // Arpejo: G4 (392Hz), C5 (523Hz), E5 (659Hz), G5 (784Hz) + Acorde Final C6 (1046Hz)
        float[] introFreqs = { 392.00f, 523.25f, 659.25f, 783.99f };
        float introNoteDur = 0.12f;
        float finalChordDur = 0.85f;
        float totalDuration = introFreqs.Length * introNoteDur + finalChordDur;

        int count = (int)(SampleRate * totalDuration);
        float[] data = new float[count];

        int sampleIndex = 0;

        // Notas da subida
        for (int n = 0; n < introFreqs.Length; n++)
        {
            int noteSamples = (int)(SampleRate * introNoteDur);
            float phase = 0f;
            for (int i = 0; i < noteSamples && sampleIndex < count; i++)
            {
                float t = (float)i / SampleRate;
                phase += 2f * Mathf.PI * introFreqs[n] / SampleRate;
                float attack = Mathf.Clamp01(t * 120f);
                float decay = Mathf.Clamp01(1f - t / introNoteDur);
                float sine = Mathf.Sin(phase);
                data[sampleIndex++] = sine * attack * decay * 0.8f;
            }
        }

        // Acorde final cintilante (C6 + E5 + G5)
        int chordSamples = (int)(SampleRate * finalChordDur);
        float pC = 0f, pE = 0f, pG = 0f;
        for (int i = 0; i < chordSamples && sampleIndex < count; i++)
        {
            float t = (float)i / SampleRate;
            pC += 2f * Mathf.PI * 1046.50f / SampleRate;
            pE += 2f * Mathf.PI * 659.25f / SampleRate;
            pG += 2f * Mathf.PI * 783.99f / SampleRate;

            float attack = Mathf.Clamp01(t * 150f);
            float decay = Mathf.Exp(-3.2f * t / finalChordDur);
            float tremolo = 0.9f + 0.1f * Mathf.Sin(2f * Mathf.PI * 8f * t);

            float mix = Mathf.Sin(pC) * 0.45f + Mathf.Sin(pE) * 0.3f + Mathf.Sin(pG) * 0.25f;
            data[sampleIndex++] = mix * attack * decay * tremolo * 0.85f;
        }

        return MakeClip(name, data);
    }

    /// <summary>Efeito sonoro ao passar o mouse por cima de um botão (hover): blip sutil, nítido e limpo.</summary>
    public static AudioClip CreateButtonHover(string name)
    {
        float duration = 0.04f;
        int count = (int)(SampleRate * duration);
        float[] data = new float[count];
        float phase = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / SampleRate;
            float freq = Mathf.Lerp(1100f, 1450f, t / duration);
            phase += 2f * Mathf.PI * freq / SampleRate;

            float attack = Mathf.Clamp01(t / 0.003f); // 3ms fade in suave para não estalar no fone
            float envelope = attack * Mathf.Exp(-12f * t / duration);
            data[i] = Mathf.Sin(phase) * envelope * 0.65f;
        }
        return MakeClip(name, data);
    }

    /// <summary>Efeito sonoro ao clicar em um botão: confirmação firme, satisfatória e presente.</summary>
    public static AudioClip CreateButtonClick(string name)
    {
        float duration = 0.08f;
        int count = (int)(SampleRate * duration);
        float[] data = new float[count];
        float phase1 = 0f;
        float phase2 = 0f;

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / SampleRate;
            float freq = Mathf.Lerp(600f, 1250f, Mathf.Pow(t / duration, 0.4f));
            phase1 += 2f * Mathf.PI * freq / SampleRate;
            phase2 += 2f * Mathf.PI * (freq * 1.5f) / SampleRate;

            float attack = Mathf.Clamp01(t / 0.003f); // 3ms fade in suave
            float decay = Mathf.Exp(-10f * t / duration);
            float envelope = attack * decay;

            data[i] = (Mathf.Sin(phase1) * 0.75f + Mathf.Sin(phase2) * 0.25f) * envelope * 0.95f;
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
