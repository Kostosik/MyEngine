namespace MyEngine.Audio;

public sealed class AudioClip
{
    public byte[] Data { get; }
    public int SampleRate { get; }
    public bool IsStereo { get; }
    public bool Is16Bit { get; }

    public float DurationSeconds { get; }

    public AudioClip(byte[] pcmData, int sampleRate, bool isStereo, bool is16Bit)
    {
        Data = pcmData;
        SampleRate = sampleRate;
        IsStereo = isStereo;
        Is16Bit = is16Bit;

        int bytesPerSample = is16Bit ? 2 : 1;
        int channels = isStereo ? 2 : 1;
        int totalSamples = pcmData.Length / (bytesPerSample * channels);
        DurationSeconds = totalSamples / (float)sampleRate;
    }
}