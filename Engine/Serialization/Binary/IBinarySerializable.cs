namespace MyEngine.Serialization.Binary;

/// <summary>
/// Компонент, умеющий сохраняться в бинарный поток.
/// Каждый компонент пишет и читает свои поля вручную.
///
/// Порядок Write и Read ДОЛЖЕН совпадать.
/// </summary>
public interface IBinarySerializable
{
    void Write(BinaryWriter writer);
    void Read(BinaryReader reader);
}