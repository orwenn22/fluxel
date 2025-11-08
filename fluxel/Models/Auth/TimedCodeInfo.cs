using System;
using System.Collections.Generic;
using MongoDB.Bson.Serialization.Attributes;

namespace fluxel.Models.Auth;

public class TimedCodeInfo
{
    [BsonId]
    public long UserID { get; init; }

    [BsonElement("key")]
    public string SharedKey { get; init; } = null!;

    [BsonElement("codes")]
    public List<TimedCodeBackup> BackupCodes { get; init; }

    public TimedCodeInfo(long id, string key, List<TimedCodeBackup> codes)
    {
        UserID = id;
        SharedKey = key;
        BackupCodes = codes;
    }

    [BsonConstructor]
    [Obsolete("This is for bson parsing only.")]
    public TimedCodeInfo(List<TimedCodeBackup> backupCodes)
    {
        BackupCodes = backupCodes;
    }
}
