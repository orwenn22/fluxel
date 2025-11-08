namespace fluxel.Tasks;

public interface IBasicTask
{
    string Name { get; }
    void Run();
}
