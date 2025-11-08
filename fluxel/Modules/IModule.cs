using JetBrains.Annotations;

namespace fluxel.Modules;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IModule
{
    void OnLoad(ServerHost host);
    void OnMessage(object data) { }
}
