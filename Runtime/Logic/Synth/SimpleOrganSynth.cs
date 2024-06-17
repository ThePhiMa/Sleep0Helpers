using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SimpleOrganSynth : MonoBehaviour
{
    public float baseFrequency = 440f; // Base frequency of the organ (A4)
    public float[] drawbarLevels = new float[9]; // Amplitude levels for each drawbar (9 drawbars as example)
    public float gain = 0.1f; // Master gain to prevent clipping

    public float detune = 0.01f; // Slight detune for more richness
    public float keySensitivity = 1f; // Sensitivity for key press handling

    private float sampleRate;
    private float phase;

    private void Start()
    {
        sampleRate = AudioSettings.outputSampleRate;
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
        for (int i = 0; i < data.Length; i += channels)
        {
            float sample = 0f;

            // Generate additive harmonics based on drawbar levels
            for (int j = 0; j < drawbarLevels.Length; j++)
            {
                float harmonicFrequency = baseFrequency * (j + 1);
                sample += Mathf.Sin(2 * Mathf.PI * harmonicFrequency * phase / sampleRate) * drawbarLevels[j];
            }

            // Advance phase
            phase += 1;

            // Normalize and apply gain
            sample *= gain;
            sample = Mathf.Clamp(sample, -1f, 1f);

            // Write the sample to the output buffer
            for (int ch = 0; ch < channels; ch++)
            {
                data[i + ch] = sample;
            }
        }
    }

    private void Update()
    {
        // Update base frequency based on key press
        if (Input.GetKey(KeyCode.A)) baseFrequency = NoteToFrequency("C4");
        if (Input.GetKey(KeyCode.W)) baseFrequency = NoteToFrequency("C#4");
        if (Input.GetKey(KeyCode.S)) baseFrequency = NoteToFrequency("D4");
        if (Input.GetKey(KeyCode.E)) baseFrequency = NoteToFrequency("D#4");
        if (Input.GetKey(KeyCode.D)) baseFrequency = NoteToFrequency("E4");
        if (Input.GetKey(KeyCode.F)) baseFrequency = NoteToFrequency("F4");
        if (Input.GetKey(KeyCode.T)) baseFrequency = NoteToFrequency("F#4");
        if (Input.GetKey(KeyCode.G)) baseFrequency = NoteToFrequency("G4");
        if (Input.GetKey(KeyCode.Y)) baseFrequency = NoteToFrequency("G#4");
        if (Input.GetKey(KeyCode.H)) baseFrequency = NoteToFrequency("A4");
        if (Input.GetKey(KeyCode.U)) baseFrequency = NoteToFrequency("A#4");
        if (Input.GetKey(KeyCode.J)) baseFrequency = NoteToFrequency("B4");
        if (Input.GetKey(KeyCode.K)) baseFrequency = NoteToFrequency("C5");
        // Add more keys as needed for additional notes

        // Detune effect (optional)
        baseFrequency += Random.Range(-detune, detune) * keySensitivity;
    }

    // Helper function to convert note names to frequencies
    private float NoteToFrequency(string note)
    {
        switch (note)
        {
            case "C4": return 261.63f;
            case "C#4": return 277.18f;
            case "D4": return 293.66f;
            case "D#4": return 311.13f;
            case "E4": return 329.63f;
            case "F4": return 349.23f;
            case "F#4": return 369.99f;
            case "G4": return 392.00f;
            case "G#4": return 415.30f;
            case "A4": return 440.00f;
            case "A#4": return 466.16f;
            case "B4": return 493.88f;
            case "C5": return 523.25f;
            // Add more notes as needed
            default: return 440.00f; // Default to A4
        }
    }
}
