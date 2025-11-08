using System.Collections.Generic;
using fluXis.Online.Collections;

namespace fluxel.Modules.Messages;

public class UserCollectionMessage
{
    public long UserID { get; }
    public string CollectionID { get; }
    public List<CollectionItem> Added { get; }
    public List<CollectionItem> Changed { get; }
    public List<string> Removed { get; }

    public UserCollectionMessage(long user, string collection, List<CollectionItem> added, List<CollectionItem> changed, List<string> removed)
    {
        UserID = user;
        CollectionID = collection;
        Added = added;
        Changed = changed;
        Removed = removed;
    }
}
