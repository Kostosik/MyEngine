using System.Numerics;

namespace MyEngine.UI;

/// <summary>
/// Базовый элемент UI. Иерархия: у каждого элемента может быть родитель
/// и список детей. Позиция и размер считаются относительно родителя
/// по правилам якорей и смещений.
/// </summary>
public abstract class UIElement
{
    // --- Иерархия ---
    public UIElement? Parent { get; internal set; }
    public List<UIElement> Children { get; } = new();

    // --- Позиция и размер ---
    /// <summary>
    /// Якорь элемента относительно родителя.
    /// По умолчанию TopLeft — обычная позиция в пикселях.
    /// </summary>
    public Anchor Anchor = Anchor.TopLeft;

    /// <summary>
    /// Смещение относительно точки, определённой якорем.
    /// При Anchor.TopLeft — это позиция левого верхнего угла.
    /// При Anchor.MiddleCenter — смещение от центра родителя.
    /// </summary>
    public Vector2 Offset = Vector2.Zero;

    /// <summary>Размер в пикселях. Игнорируется при растяжении якорем.</summary>
    public Vector2 Size = new(100, 30);

    /// <summary>
    /// Margins — внутренние отступы от границ родителя при растяжении.
    /// Используется при Anchor.StretchHorizontal / StretchVertical / StretchAll.
    /// </summary>
    public float MarginLeft = 0;
    public float MarginRight = 0;
    public float MarginTop = 0;
    public float MarginBottom = 0;

    // --- Состояние ---
    public bool Visible = true;
    public bool Enabled = true;

    /// <summary>Финальный прямоугольник в screen-space. Обновляется в Layout.</summary>
    public UIRect Bounds { get; protected set; }

    // --- События ---
    public event Action<UIElement>? OnClick;
    public event Action<UIElement>? OnHoverEnter;
    public event Action<UIElement>? OnHoverExit;

    internal bool IsHovered;
    internal bool WasHoveredLastFrame;
    internal bool WasClicked;

    /// <summary>
    /// Признак: вычислять размер автоматически, а не брать из Size.
    /// Если true — Size.X / Size.Y используются только как минимальные.
    /// Если false — Size приоритетен, PreferredSize игнорируется.
    /// </summary>
    public bool AutoSizeX = false;
    public bool AutoSizeY = false;

    /// <summary>
    /// Желаемый размер элемента. Переопределяется в виджетах.
    /// Для базового UIElement — это Size (то есть ничего не меняется).
    /// </summary>
    public virtual Vector2 PreferredSize => Size;

    /// <summary>
    /// Итоговый размер по каждой оси:
    ///   AutoSize = true  → берём PreferredSize, но не меньше Size (Size = минимальный)
    ///   AutoSize = false → берём Size
    /// </summary>
    public Vector2 ResolveSize()  
    {
        var pref = PreferredSize;
        return new Vector2(
            AutoSizeX ? MathF.Max(pref.X, Size.X) : Size.X,
            AutoSizeY ? MathF.Max(pref.Y, Size.Y) : Size.Y
        );
    }

    // ============================================================
    // Иерархия
    // ============================================================

    public T Add<T>(T child) where T : UIElement
    {
        child.Parent = this;
        Children.Add(child);
        return child;
    }

    public void Remove(UIElement child)
    {
        if (Children.Remove(child))
            child.Parent = null;
    }

    public void RemoveFromParent() => Parent?.Remove(this);

    // ============================================================
    // Layout
    // ============================================================

    /// <summary>
    /// Пересчитать позицию и размер относительно родителя.
    /// Вызывается UIRoot каждый кадр.
    /// </summary>
    public virtual void Layout(UIRect parentBounds)
    {
        if (!Visible) return;

        Bounds = ComputeBounds(parentBounds);

        // Рекурсивно разложить детей
        foreach (var child in Children)
            child.Layout(Bounds);
    }

    protected virtual UIRect ComputeBounds(UIRect parent)
    {
        var size = ResolveSize();   // ← вот тут

        float x, y, w, h;

        // Горизонталь — как было, но size.X вместо Size.X
        bool stretchX = (Anchor & Anchor.StretchHorizontal) == Anchor.StretchHorizontal;
        if (stretchX)
        {
            x = parent.X + MarginLeft;
            w = parent.Width - MarginLeft - MarginRight;
        }
        else if ((Anchor & Anchor.CenterX) != 0)
        {
            x = parent.X + parent.Width * 0.5f - size.X * 0.5f + Offset.X;
            w = size.X;
        }
        else if ((Anchor & Anchor.Right) != 0)
        {
            x = parent.X + parent.Width - size.X + Offset.X;
            w = size.X;
        }
        else
        {
            x = parent.X + Offset.X;
            w = size.X;
        }

        // Вертикаль — аналогично
        bool stretchY = (Anchor & Anchor.StretchVertical) == Anchor.StretchVertical;
        if (stretchY)
        {
            y = parent.Y + MarginTop;
            h = parent.Height - MarginTop - MarginBottom;
        }
        else if ((Anchor & Anchor.CenterY) != 0)
        {
            y = parent.Y + parent.Height * 0.5f - size.Y * 0.5f + Offset.Y;
            h = size.Y;
        }
        else if ((Anchor & Anchor.Bottom) != 0)
        {
            y = parent.Y + parent.Height - size.Y + Offset.Y;
            h = size.Y;
        }
        else
        {
            y = parent.Y + Offset.Y;
            h = size.Y;
        }

        return new UIRect(x, y, w, h);
    }

    // ============================================================
    // Update и Draw
    // ============================================================

    /// <summary>Обработать ввод и обновить состояние (hover, click).</summary>
    public virtual void Update(Vector2 mousePos, bool mouseDown, bool mouseJustPressed)
    {
        if (!Visible || !Enabled) return;

        WasHoveredLastFrame = IsHovered;
        IsHovered = Bounds.Contains(mousePos);
        WasClicked = false;

        if (IsHovered && !WasHoveredLastFrame)
            OnHoverEnter?.Invoke(this);
        if (!IsHovered && WasHoveredLastFrame)
            OnHoverExit?.Invoke(this);

        // === Сначала дети ===
        bool childConsumed = false;
        for (int i = Children.Count - 1; i >= 0; i--)
        {
            Children[i].Update(mousePos, mouseDown, mouseJustPressed);
            if (Children[i].WasClicked) childConsumed = true;
        }

        // === Потом сам элемент — если дети не съели клик ===
        if (IsHovered && mouseJustPressed && !childConsumed)
        {
            WasClicked = true;
            OnClick?.Invoke(this);
        }
    }

    /// <summary>Отрисовать элемент и его детей.</summary>
    public virtual void Draw(UIRenderContext ctx)
    {
        if (!Visible) return;
        foreach (var child in Children)
            child.Draw(ctx);
    }
}