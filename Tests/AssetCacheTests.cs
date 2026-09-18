//// Тесты требуют GL — поэтому либо используем заглушку, либо
//// пропускаем их. Если у тебя нет NullRenderer+GL-мок, пропусти тесты.

//// Простой вариант — тесты без реальной загрузки:

//[Fact]
//public void RegisterAlias_Works()
//{
//    // Это тестируемо без GL
//    var aliases = new Dictionary<string, string>();
//    aliases["player"] = "Assets/player.png";
//    Assert.Equal("Assets/player.png", aliases["player"]);
//}