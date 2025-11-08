using System;
using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using fluXis.Online.API.Models.Maps;
using Midori.Networking;

namespace fluxel.API.Routes.Maps;

public class MapLookupRequest : IFluxelAPIRoute
{
    public string RoutePath => "/maps/lookup";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        var hash = interaction.GetStringQuery("hash");
        var mapper = interaction.GetIntQuery("mapper");
        var title = interaction.GetStringQuery("title");
        var artist = interaction.GetStringQuery("artist");

        var map = MapHelper.Get(m =>
            (string.IsNullOrEmpty(hash) || m.SHA256Hash == hash) &&
            (mapper == null || m.MapperID == mapper) &&
            (string.IsNullOrEmpty(title) || m.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase)) &&
            (string.IsNullOrEmpty(artist) || m.Artist.Equals(artist, StringComparison.CurrentCultureIgnoreCase)));

        if (map == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.ProvidedTypeNotFound("map", "filters"));
            return;
        }

        var set = MapSetHelper.Get(map.SetID);

        if (set == null) // bail out if the map set is not found
            throw new InvalidOperationException("Attempted to get a deleted MapSet of a Map.");

        var mapLookup = new APIMapLookup
        {
            ID = map.ID,
            SetID = map.SetID,
            CreatorID = set.CreatorID,
            Rating = map.Rating,
            Status = (int)set.Status,
            DateSubmitted = set.Submitted.ToUnixTimeSeconds(),
            DateRanked = set.DateRanked?.ToUnixTimeSeconds(),
            LastUpdated = set.LastUpdated.ToUnixTimeSeconds(),
            Hash = map.SHA256Hash
        };

        await interaction.Reply(HttpStatusCode.OK, mapLookup);
    }
}
