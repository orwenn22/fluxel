using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using fluxel.API.Components;
using fluxel.Constants;
using fluxel.Database.Helpers;
using Midori.Networking;

namespace fluxel.API.Routes.MapSets;

public class MapSetDownloadRoute : IFluxelAPIRoute
{
    public string RoutePath => "/mapset/:id/download";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetLongParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "long"));
            return;
        }

        var set = MapSetHelper.Get(id);

        if (set == null)
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.MapSetNotFound);
            return;
        }

        var path = Assets.GetPathForAsset(AssetType.Map, set.ID.ToString());

        if (!File.Exists(path))
        {
            await interaction.ReplyMessage(HttpStatusCode.NotFound, ResponseStrings.MapSetNotFound);
            return;
        }

        var data = Assets.GetAssetStream(AssetType.Map, set.ID.ToString());

        if (data is null)
        {
            await interaction.ReplyMessage(HttpStatusCode.InternalServerError, "Unable to find file on server.");
            return;
        }

        interaction.Response.ContentType = "application/zip";
        await interaction.ReplyData(data, HttpUtility.UrlEncode($"{set.ID} {set.Artist} - {set.Title}.fms"));
    }
}
