namespace SPUpdateFramework;

public interface IRibbonFunction
{
    public string Name { get; }
    public string Image { get; }
    void OnClick(object? sender, EventArgs e);
}