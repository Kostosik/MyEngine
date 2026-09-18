using MyEngine.Ecs;
using MyEngine.Rendering;
using MyEngine.WorldEngine;
using System.Numerics;

namespace MyEngine.Tilemap;

public static class TilemapRenderer
{
    public static void Draw(SpriteBatch batch, World world, Camera2D camera,
        int screenWidth, int screenHeight)
    {
        float halfW = screenWidth / (2f * camera.Zoom) + 32;
        float halfH = screenHeight / (2f * camera.Zoom) + 32;
        var viewMin = camera.Position - new Vector2(halfW, halfH);
        var viewMax = camera.Position + new Vector2(halfW, halfH);

        foreach (var e in world.With<TilemapComponent>())
        {
            var map = e.Get<TilemapComponent>()!;
            var t = e.Get<MyEngine.Components.Transform>();
            var origin = t?.Position ?? map.Origin;
            map.Origin = origin;

            // Диапазон чанков в поле зрения
            int cs = map.ChunkSize;
            int tileSize = map.Tileset.TileSize;

            // Мировые координаты → тайловые
            var (vMinTX, vMinTY) = map.WorldToTile(viewMin);
            var (vMaxTX, vMaxTY) = map.WorldToTile(viewMax);

            // → в координаты чанков
            int minCX = System.Math.Max(0, vMinTX / cs);
            int minCY = System.Math.Max(0, vMinTY / cs);
            int maxCX = System.Math.Min((map.Width - 1) / cs, vMaxTX / cs);
            int maxCY = System.Math.Min((map.Height - 1) / cs, vMaxTY / cs);

            for (int cy = minCY; cy <= maxCY; cy++)
            {
                for (int cx = minCX; cx <= maxCX; cx++)
                {
                    var coord = new ChunkCoord(cx, cy);
                    var chunk = map.GetChunk(coord);
                    if (chunk == null) continue;

                    // Рисуем слои в порядке DrawOrder
                    foreach (var layer in map.Layers)
                    {
                        DrawChunkLayer(batch, map, chunk, layer, coord, origin);
                    }
                }
            }
        }
    }

    private static void DrawChunkLayer(
        SpriteBatch batch,
        TilemapComponent map,
        TilemapChunk chunk,
        TilemapLayer layer,
        ChunkCoord coord,
        Vector2 origin)
    {
        int ts = map.Tileset.TileSize;
        float tsF = ts;
        int cs = chunk.Size;

        var tex = map.Tileset.Texture;
        float invW = 1f / tex.Width;
        float invH = 1f / tex.Height;

        for (int ly = 0; ly < cs; ly++)
        {
            for (int lx = 0; lx < cs; lx++)
            {
                int tile = chunk.Get(layer.Index, lx, ly);
                if (tile < 0) continue;

                var (px, py) = map.Tileset.GetTilePixel(tile);
                if (px < 0) continue;

                // Глобальные тайловые координаты
                int gx = coord.X * cs + lx;
                int gy = coord.Y * cs + ly;

                // Мировой центр тайла
                var center = origin + new Vector2(
                    gx * tsF + tsF * 0.5f,
                    gy * tsF + tsF * 0.5f);

                float u0 = px * invW;
                float v0 = py * invH;
                float u1 = (px + ts) * invW;
                float v1 = (py + ts) * invH;

                batch.Draw(
                    tex, center, new Vector2(tsF, tsF),
                    new Vector2(u0, v0), new Vector2(u1, v1),
                    0f, new Vector2(0.5f), Vector4.One);
            }
        }
    }
}