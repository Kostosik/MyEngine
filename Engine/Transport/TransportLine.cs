using System.Numerics;

namespace MyEngine.Transport;

/// <summary>
/// Универсальная транспортная линия: N слотов, движется в направлении.
///
/// Работает с любым содержимым через ContentId + Amount.
/// Конвейеры, трубы, кабели — все используют одну систему.
///
/// ConnectionRange — на каком расстоянии искать соседа.
/// Не зависит от размера тайла — просто радиус в пикселях.
/// </summary>
public sealed class TransportLine
{
    /// <summary>Сколько слотов в линии.</summary>
    public int SlotCount = 4;

    /// <summary>Направление: 0=вправо, 1=вниз, 2=вверх, 3=влево.</summary>
    public int Direction;

    /// <summary>Слоты.</summary>
    public TransportSlot[] Slots;

    /// <summary>Секунд между сдвигами на один слот.</summary>
    public float SecondsPerMove = 0.4f;

    /// <summary>Таймер до следующего сдвига.</summary>
    public float MoveTimer;

    /// <summary>Максимальное расстояние до соседа (пиксели).</summary>
    public float ConnectionRange = 24f;

    /// <summary>Максимум содержимого на один слот.</summary>
    public int MaxAmountPerSlot = 1;

    public TransportLine(int slots = 4)
    {
        SlotCount = slots;
        Slots = new TransportSlot[slots];
        for (int i = 0; i < slots; i++)
            Slots[i] = TransportSlot.Empty;
    }

    /// <summary>Полный ли последний слот.</summary>
    public bool IsInputFull()
        => !Slots[SlotCount - 1].IsEmpty;

    /// <summary>Пустой ли первый слот.</summary>
    public bool IsOutputEmpty()
        => Slots[0].IsEmpty;

    /// <summary>Попытаться положить в конец. Возвращает остаток (0 = всё влезло).</summary>
    public int TryAdd(in TransportSlot item)
    {
        ref var last = ref Slots[SlotCount - 1];

        if (last.IsEmpty)
        {
            int toTake = System.Math.Min(item.Amount, MaxAmountPerSlot);
            last.ContentId = item.ContentId;
            last.Amount = toTake;
            return item.Amount - toTake;
        }

        if (last.ContentId == item.ContentId && last.Amount < MaxAmountPerSlot)
        {
            int space = MaxAmountPerSlot - last.Amount;
            int toTake = System.Math.Min(item.Amount, space);
            last.Amount += toTake;
            return item.Amount - toTake;
        }

        return item.Amount;   // не поместилось
    }
}