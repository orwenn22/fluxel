namespace fluxel.Tasks;

public interface ICronTask : IBasicTask
{
    int Hour { get; }
    int Minute { get; }
    bool Valid { get; set; }
}
