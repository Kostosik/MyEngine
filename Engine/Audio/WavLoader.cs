using System.Text;

namespace MyEngine.Audio;

public static class WavLoader
{
    public static AudioClip Load(string path)
    {
        using var stream = File.OpenRead(path);
        using var reader = new BinaryReader(stream);

        // RIFF header
        string riff = Encoding.ASCII.GetString(reader.ReadBytes(4));
        if (riff != "RIFF") throw new InvalidDataException("Not a RIFF file");

        reader.ReadInt32(); // file size, не нужен

        string wave = Encoding.ASCII.GetString(reader.ReadBytes(4));
        if (wave != "WAVE") throw new InvalidDataException("Not a WAVE file");

        int sampleRate = 44100;
        short channels = 1;
        short bitsPerSample = 16;
        byte[]? data = null;

        while (stream.Position < stream.Length)
        {
            string chunkId = Encoding.ASCII.GetString(reader.ReadBytes(4));
            int chunkSize = reader.ReadInt32();

            if (chunkId == "fmt ")
            {
                reader.ReadInt16(); // audioFormat
                channels = reader.ReadInt16();
                sampleRate = reader.ReadInt32();
                reader.ReadInt32(); // byteRate
                reader.ReadInt16(); // blockAlign
                bitsPerSample = reader.ReadInt16();

                // Пропустить остаток chunk'а
                int consumed = 16;
                if (chunkSize > consumed) reader.ReadBytes(chunkSize - consumed);
            }
            else if (chunkId == "data")
            {
                data = reader.ReadBytes(chunkSize);
            }
            else
            {
                reader.ReadBytes(chunkSize);
            }
        }

        if (data == null) throw new InvalidDataException("No 'data' chunk in WAV");

        return new AudioClip(
            data,
            sampleRate,
            isStereo: channels == 2,
            is16Bit: bitsPerSample == 16);
    }
}