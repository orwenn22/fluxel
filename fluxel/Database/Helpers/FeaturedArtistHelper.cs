using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using fluxel.Models.Featured;
using MongoDB.Driver;

namespace fluxel.Database.Helpers;

public static class FeaturedArtistHelper
{
    private static IMongoCollection<FeaturedArtist> artists => MongoDatabase.GetCollection<FeaturedArtist>("fa-artists");
    private static IMongoCollection<FeaturedArtistAlbum> albums => MongoDatabase.GetCollection<FeaturedArtistAlbum>("fa-albums");
    private static IMongoCollection<FeaturedArtistTrack> songs => MongoDatabase.GetCollection<FeaturedArtistTrack>("fa-tracks");

    public static List<FeaturedArtist> AllArtists => artists.Find(m => true).ToList();

    public static List<FeaturedArtistAlbum> FromArtist(string artist)
    {
        var list = albums.Find(m => m.InternalID.StartsWith($"{artist}/")).ToList();
        list.Sort((a, b) => a.ReleaseDate.CompareTo(b.ReleaseDate));
        list.Reverse();
        return list;
    }

    public static List<FeaturedArtistTrack> FromAlbum(string artist, string album)
    {
        var list = songs.Find(m => m.InternalID.StartsWith($"{artist}/{album}/")).ToList();
        list.Sort((a, b) => string.Compare(a.Name, b.Name, StringComparison.InvariantCultureIgnoreCase));
        return list;
    }

    public static void Add(FeaturedArtist artist) => artists.InsertOne(artist);
    public static void Add(FeaturedArtistAlbum album) => albums.InsertOne(album);
    public static void Add(FeaturedArtistTrack track) => songs.InsertOne(track);

    public static FeaturedArtist? GetArtist(string id) => artists.Find(m => m.ID == id).FirstOrDefault();
    public static FeaturedArtistAlbum? GetAlbum(string artist, string id) => albums.Find(m => m.InternalID == $"{artist}/{id}").FirstOrDefault();
    public static FeaturedArtistTrack? GetTrack(string artist, string album, string id) => songs.Find(m => m.InternalID == $"{artist}/{album}/{id}").FirstOrDefault();

    public static bool TryGetArtist(string id, [NotNullWhen(true)] out FeaturedArtist? artist)
    {
        artist = GetArtist(id);
        return artist != null;
    }

    public static bool TryGetAlbum(string artist, string id, [NotNullWhen(true)] out FeaturedArtistAlbum? album)
    {
        album = GetAlbum(artist, id);
        return album != null;
    }

    public static bool TryGetTrack(string artist, string album, string id, [NotNullWhen(true)] out FeaturedArtistTrack? song)
    {
        song = GetTrack(artist, album, id);
        return song != null;
    }

    public static void UpdateArtist(FeaturedArtist artist) => artists.ReplaceOne(m => m.ID == artist.ID, artist);
    public static void UpdateAlbum(FeaturedArtistAlbum album) => albums.ReplaceOne(m => m.InternalID == album.InternalID, album);
    public static void UpdateTrack(FeaturedArtistTrack track) => songs.ReplaceOne(m => m.InternalID == track.InternalID, track);
}
