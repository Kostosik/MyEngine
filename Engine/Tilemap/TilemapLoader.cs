using MyEngine.Rendering;
using Silk.NET.OpenGL;
using System.Numerics;
using System.Text.Json;

namespace MyEngine.Tilemap;

public static class TilemapLoader
{
    /// <summary>
    /// Загрузить тайлмап из JSON. Текстура — рядом с JSON.
    /// </summary>
    public static TilemapComponent Load(GL gl, string jsonPath)
    {
        if (!File.Exists(jsonPath))
            throw new FileNotFoundException($"Tilemap JSON not found: {jsonPath}");

        var json = File.ReadAllText(jsonPath);
        var data = JsonSerializer.Deserialize<TilemapData>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        }) ?? throw new InvalidDataException("Failed to parse tilemap JSON");

        var dir = Path.GetDirectoryName(jsonPath) ?? "";
        var texPath = Path.Combine(dir, data.Tileset);

        var tex = new Texture2D(gl, texPath);
        var tileset = new Tileset(tex, data.TileSize);

        var map = new TilemapComponent { Tileset = tileset };
        map.Initialize(data.Width, data.Height, chunkSize: 32);

        // Сначала регистрируем слои
        foreach (var layerData in data.Layers)
        {
            map.AddLayer(new TilemapLayer(layerData.Name)
            {
                Solid = layerData.Solid,
                DrawOrder = layerData.DrawOrder
            });
        }
        map.SortLayers();

        // Теперь заполняем тайлы
        for (int li = 0; li < data.Layers.Count; li++)
        {
            var layerData = data.Layers[li];
            var layer = map.GetLayer(layerData.Name);
            if (layer == null) continue;

            for (int i = 0; i < layerData.Tiles.Count; i++)
            {
                int tile = layerData.Tiles[i];
                if (tile < 0) continue;

                int tx = i % data.Width;
                int ty = i / data.Width;
                map.SetTile(layer.Index, tx, ty, tile);
            }
        }

        return map;
    }
}