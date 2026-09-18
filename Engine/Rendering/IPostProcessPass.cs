using MyEngine.Rendering;

public interface IPostProcessPass
{
    Texture2D Process(Texture2D input, PostProcessContext context);

}