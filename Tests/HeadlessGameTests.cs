using MyEngine;

public class HeadlessGameTests
{
    [Fact]
    public void PhysicsTick_DoesNotThrow()
    {
        using var app = new HeadlessApplication();
        app.Initialize();

        // Тут твоя логика
        // Например, world с несколькими сущностями
        // app.RunFrames(60);

        Assert.True(true);
    }

    //[Fact]
    //public void RunHeadless_For10Seconds()
    //{
    //    using var runner = new MyEngine.Game.HeadlessGameRunner();
    //    runner.Initialize();
    //    runner.RunFrames(600);  // 10 секунд при 60 FPS
    //    Assert.True(true);
    //}
}