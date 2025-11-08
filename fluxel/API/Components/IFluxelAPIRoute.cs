using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Midori.API.Components;

namespace fluxel.API.Components;

[UsedImplicitly(ImplicitUseTargetFlags.WithInheritors)]
public interface IFluxelAPIRoute : IAPIRoute<FluxelAPIInteraction>
{
    /// <summary>
    /// Validates whether the request is valid.
    /// <br />
    /// This is NOT for checking if a resource exists or not, just purely for the request body.
    /// <br />
    /// <br />
    /// To share data between validate and handle use <see cref="FluxelAPIInteraction.AddCache"/>
    /// and <see cref="FluxelAPIInteraction.TryGetCache{T}"/> from <see cref="FluxelAPIInteraction"/>.
    /// </summary>
    /// <param name="interaction">The interaction responsible for this request.</param>
    /// <returns>the list of errors (field, reason)</returns>
    IEnumerable<(string, string)> Validate(FluxelAPIInteraction interaction) => Array.Empty<(string, string)>();
}
