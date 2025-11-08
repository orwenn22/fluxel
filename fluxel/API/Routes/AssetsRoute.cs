using System.Net.Http;
using System.Threading.Tasks;
using fluxel.API.Components;
using fluxel.Constants;
using Midori.Networking;

namespace fluxel.API.Routes;

public class AssetsRoute : IFluxelAPIRoute
{
    public string RoutePath => "/assets/:type/:id";
    public HttpMethod Method => HttpMethod.Get;

    public async Task Handle(FluxelAPIInteraction interaction)
    {
        if (!interaction.TryGetStringParameter("type", out var type))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("type", "string"));
            return;
        }

        if (!interaction.TryGetStringParameter("id", out var id))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, ResponseStrings.InvalidParameter("id", "string"));
            return;
        }

        if (!Assets.TryGetType(type, out var assetType))
        {
            await interaction.ReplyMessage(HttpStatusCode.BadRequest, $"'{type}' is not a valid asset type.");
            return;
        }

        if (id.Contains('.'))
            id = id.Split('.')[0];

        var asset = Assets.GetAsset(assetType.Value, id);
        interaction.Response.ContentType = "text/plain";
        await interaction.ReplyData(asset);
    }
}
