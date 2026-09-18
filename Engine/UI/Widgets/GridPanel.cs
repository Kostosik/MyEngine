using System.Numerics;

namespace MyEngine.UI.Widgets;

public enum GridAlignment { Start, Center, End, Stretch }

/// <summary>
/// Контейнер-сетка. Расставляет детей в Columns колонок.
/// Количество строк вычисляется автоматически по числу детей.
///
/// Режимы:
///   Fixed cell — все ячейки одного размера (CellSize.X × CellSize.Y).
///   Fit — ячейки растягиваются под доступное пространство.
///
/// Дети идут row-major: сначала заполняется первая строка, потом вторая.
/// </summary>
public sealed class GridPanel : UIElement
{
    /// <summary>Сколько колонок.</summary>
    public int Columns = 4;

    /// <summary>Отступ между ячейками.</summary>
    public float Spacing = 0;

    /// <summary>Внутренние отступы.</summary>
    public float Padding = 0;

    /// <summary>
    /// Если задано (X > 0, Y > 0) — все ячейки этого размера.
    /// Если (0, 0) — ячейки растягиваются под Bounds контейнера.
    /// </summary>
    public Vector2 CellSize = Vector2.Zero;

    /// <summary>Выравнивание содержимого внутри ячейки.</summary>
    public GridAlignment CellAlign = GridAlignment.Stretch;

    /// <summary>Сколько строк сейчас (по числу детей).</summary>
    public int Rows
    {
        get
        {
            int visible = 0;
            foreach (var c in Children) if (c.Visible) visible++;
            if (visible == 0) return 0;
            return (visible + Columns - 1) / Columns;
        }
    }

    public override void Layout(UIRect parentBounds)
    {
        if (!Visible) return;

        Bounds = ComputeBounds(parentBounds);
        LayoutCells();
    }

    private void LayoutCells()
    {
        if (Columns <= 0) return;

        float availW = Bounds.Width - Padding * 2;
        float availH = Bounds.Height - Padding * 2;
        if (availW <= 0 || availH <= 0) return;

        // Видимые дети — в порядке добавления
        var visible = new List<UIElement>();
        foreach (var c in Children) if (c.Visible) visible.Add(c);
        if (visible.Count == 0) return;

        int rows = (visible.Count + Columns - 1) / Columns;

        // Размер ячейки
        float cellW, cellH;
        if (CellSize.X > 0 && CellSize.Y > 0)
        {
            cellW = CellSize.X;
            cellH = CellSize.Y;
        }
        else
        {
            cellW = (availW - Spacing * (Columns - 1)) / Columns;
            cellH = (availH - Spacing * (rows - 1)) / rows;
        }

        // Позиции
        for (int i = 0; i < visible.Count; i++)
        {
            int col = i % Columns;
            int row = i / Columns;

            float cellX = Bounds.X + Padding + col * (cellW + Spacing);
            float cellY = Bounds.Y + Padding + row * (cellH + Spacing);

            var cellRect = new UIRect(cellX, cellY, cellW, cellH);

            // Для Stretch — ребёнок получает всю ячейку.
            // Для Start/Center/End — ребёнок получает свой ResolveSize и выравнивается.
            UIElement child = visible[i];

            if (CellAlign == GridAlignment.Stretch)
            {
                child.Layout(cellRect);
            }
            else
            {
                var cs = child.ResolveSize();
                float cw = MathF.Min(cs.X, cellW);
                float ch = MathF.Min(cs.Y, cellH);

                float cx = CellAlign switch
                {
                    GridAlignment.Center => cellX + (cellW - cw) * 0.5f,
                    GridAlignment.End => cellX + cellW - cw,
                    _ => cellX,
                };
                float cy = CellAlign switch
                {
                    GridAlignment.Center => cellY + (cellH - ch) * 0.5f,
                    GridAlignment.End => cellY + cellH - ch,
                    _ => cellY,
                };

                child.Layout(new UIRect(cx, cy, cw, ch));
            }
        }
    }
}