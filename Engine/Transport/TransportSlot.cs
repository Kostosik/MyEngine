namespace MyEngine.Transport;

/// <summary>
/// Один слот в транспортной линии.
/// ContentId — id содержимого (-1 = пусто).
/// Amount — количество (1 для предметов, N для жидкостей).
/// </summary>
public struct TransportSlot
{
    public int ContentId;
    public int Amount;

    public static TransportSlot Empty => new() { ContentId = -1, Amount = 0 };

    public bool IsEmpty => ContentId < 0 || Amount <= 0;

    public bool CanMergeWith(in TransportSlot other)
    {
        if (IsEmpty) return !other.IsEmpty;
        if (other.IsEmpty) return false;
        return ContentId == other.ContentId;
    }

    public void Merge(in TransportSlot other)
    {
        if (IsEmpty)
        {
            ContentId = other.ContentId;
            Amount = other.Amount;
        }
        else
        {
            Amount += other.Amount;
        }
    }
}