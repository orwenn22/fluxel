using fluxel.API.Components;

namespace fluxel.Models;

public interface IHasCache
{
    RequestCache Cache { get; set; }
}
