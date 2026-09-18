namespace MyEngine.Assets;

/// <summary>
/// Счётчик ссылок на ресурс. Один ресурс — много пользователей.
/// </summary>
internal sealed class AssetRefCount<T> where T : class, IDisposable
{
    public T Asset;
    public int RefCount;

    public AssetRefCount(T asset)
    {
        Asset = asset;
        RefCount = 1;
    }
}