using MyEngine.Math;
using MyEngine.Spatial;
using System.Numerics;

namespace Tests;

public class SpatialHashTests
{
    [Fact]
    public void InsertAndQuery_FindsItem()
    {
        var hash = new SpatialHash<string>(cellSize: 100f);
        var box = Aabb.FromCenterSize(new Vector2(50, 50), new Vector2(10, 10));
        hash.Insert("a", box);

        var results = new List<string>();
        var query = Aabb.FromCenterSize(new Vector2(50, 50), new Vector2(10, 10));
        hash.Query(query, results);

        Assert.Contains("a", results);
        Assert.Single(results);
    }

    [Fact]
    public void Query_DoesNotFindFarItem()
    {
        var hash = new SpatialHash<string>(cellSize: 100f);
        hash.Insert("a", Aabb.FromCenterSize(new Vector2(50, 50), new Vector2(10, 10)));

        var results = new List<string>();
        hash.Query(Aabb.FromCenterSize(new Vector2(500, 500), new Vector2(10, 10)), results);

        Assert.Empty(results);
    }

    [Fact]
    public void ItemAcrossCells_NotDuplicatedInQuery()
    {
        var hash = new SpatialHash<string>(cellSize: 50f);
        // Большой объект 200x200 пересекает несколько клеток
        hash.Insert("big", Aabb.FromCenterSize(new Vector2(100, 100), new Vector2(200, 200)));

        var results = new List<string>();
        hash.Query(Aabb.FromCenterSize(new Vector2(100, 100), new Vector2(10, 10)), results);

        // Должен быть один, несмотря на регистрацию в 16+ клетках
        Assert.Single(results);
        Assert.Equal("big", results[0]);
    }

    [Fact]
    public void Clear_EmptiesAll()
    {
        var hash = new SpatialHash<string>(cellSize: 100f);
        hash.Insert("a", Aabb.FromCenterSize(new Vector2(50, 50), new Vector2(10, 10)));
        hash.Clear();

        var results = new List<string>();
        hash.Query(Aabb.FromCenterSize(new Vector2(50, 50), new Vector2(10, 10)), results);
        Assert.Empty(results);
    }

    [Fact]
    public void MultipleItems_AllFound()
    {
        var hash = new SpatialHash<string>(cellSize: 100f);
        hash.Insert("a", Aabb.FromCenterSize(new Vector2(50, 50), new Vector2(10, 10)));
        hash.Insert("b", Aabb.FromCenterSize(new Vector2(60, 60), new Vector2(10, 10)));
        hash.Insert("c", Aabb.FromCenterSize(new Vector2(70, 70), new Vector2(10, 10)));

        var results = new List<string>();
        hash.Query(Aabb.FromCenterSize(new Vector2(60, 60), new Vector2(50, 50)), results);

        Assert.Equal(3, results.Count);
    }
}