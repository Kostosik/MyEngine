using MyEngine.Components;
using MyEngine.Diagnostics;
using MyEngine.Ecs;

namespace MyEngine.Serialization.Binary;

/// <summary>
/// Сохраняет/загружает World в бинарный поток.
///
/// Формат:
///   [magic: "MYE1"]        — 4 байта
///   [version: int]         — 1
///   [entityCount: int]
///   для каждой сущности:
///     [entityId: int]
///     [componentCount: int]
///     для каждого компонента:
///       [typeId: int]
///       [dataSize: int]
///       [bytes]            — данные, записанные самим компонентом
/// </summary>
public sealed class BinaryWorldSerializer
{
    private const string Magic = "MYE1";
    private const int Version = 1;

    private readonly BinaryComponentRegistry _registry;

    public BinaryWorldSerializer(BinaryComponentRegistry registry)
        => _registry = registry;

    // ============================================================
    // Save
    // ============================================================

    public void Save(World world, string path)
    {
        using var fs = File.Create(path);
        using var bw = new BinaryWriter(fs);
        Save(world, bw);
    }

    public byte[] SaveToBytes(World world)
    {
        using var ms = new MemoryStream();
        using var bw = new BinaryWriter(ms);
        Save(world, bw);
        return ms.ToArray();
    }

    public void Save(World world, BinaryWriter bw)
    {
        // Заголовок
        bw.Write(System.Text.Encoding.ASCII.GetBytes(Magic));
        bw.Write(Version);

        // Считаем живые сущности
        int aliveCount = 0;
        foreach (var e in world.Entities)
            if (e.IsAlive) aliveCount++;
        bw.Write(aliveCount);

        // Сущности
        foreach (var e in world.Entities)
        {
            if (!e.IsAlive) continue;
            SaveEntity(e, bw);
        }
    }

    private void SaveEntity(Entity e, BinaryWriter bw)
    {
        bw.Write(e.Id);

        // Собираем компоненты (только сериализуемые)
        var components = new List<IBinarySerializable>();
        foreach (var type in e.ComponentTypes)
        {
            var comp = e.GetBoxed(type);
            if (comp is IBinarySerializable serializable
                && _registry.GetId(comp) >= 0)
            {
                components.Add(serializable);
            }
        }

        bw.Write(components.Count);

        foreach (var comp in components)
        {
            int typeId = _registry.GetId(comp);
            bw.Write(typeId);

            // Пишем данные во временный буфер, чтобы знать размер
            using var ms = new MemoryStream();
            using (var temp = new BinaryWriter(ms, System.Text.Encoding.UTF8, leaveOpen: true))
            {
                comp.Write(temp);
            }
            var bytes = ms.ToArray();
            bw.Write(bytes.Length);
            bw.Write(bytes);
        }
    }

    // ============================================================
    // Load
    // ============================================================

    public void Load(World world, string path)
    {
        using var fs = File.OpenRead(path);
        using var br = new BinaryReader(fs);
        Load(world, br);
    }

    public void LoadFromBytes(World world, byte[] data)
    {
        using var ms = new MemoryStream(data);
        using var br = new BinaryReader(ms);
        Load(world, br);
    }

    public void Load(World world, BinaryReader br)
    {
        // Заголовок
        var magic = System.Text.Encoding.ASCII.GetString(br.ReadBytes(4));
        if (magic != Magic)
            throw new InvalidDataException($"Not a MyEngine binary save. Got: {magic}");

        int version = br.ReadInt32();
        if (version != Version)
            Log.Warn("BinarySerializer", $"Save version {version} != current {Version}. May not load correctly.");

        int entityCount = br.ReadInt32();

        for (int i = 0; i < entityCount; i++)
            LoadEntity(world, br);
    }

    private void LoadEntity(World world, BinaryReader br)
    {
        int entityId = br.ReadInt32();
        var e = world.CreateWithId(entityId);

        int componentCount = br.ReadInt32();

        for (int i = 0; i < componentCount; i++)
        {
            int typeId = br.ReadInt32();
            int size = br.ReadInt32();
            var bytes = br.ReadBytes(size);

            var factory = _registry.GetFactory(typeId);
            if (factory == null)
            {
                Log.Warn("BinarySerializer", $"Unknown component typeId={typeId}. Skipped.");
                continue;
            }

            var component = factory();
            using var ms = new MemoryStream(bytes);
            using var tempReader = new BinaryReader(ms);
            component.Read(tempReader);

            e.AddBoxed(component);
        }
    }
}