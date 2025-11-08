using System.Collections.Generic;
using Midori.API.Components.Json;
using Newtonsoft.Json;

namespace fluxel.API.Components;

public class FluxelAPIResponse : JsonResponse
{
    [JsonProperty("errors")]
    public Dictionary<string, string>? Errors { get; set; }
}
