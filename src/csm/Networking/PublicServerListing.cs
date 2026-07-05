using System;

namespace CSM.Networking
{
    /// <summary>
    ///     A single entry in the public server list, as returned by the GS
    ///     GET /api/servers HTTP endpoint. Field names match the JSON response
    ///     exactly, since UnityEngine.JsonUtility matches by name.
    ///     Deliberately a struct, not a class: this Unity engine version's
    ///     JsonUtility fails to populate arrays of class-typed elements, but
    ///     arrays of structs deserialize correctly.
    /// </summary>
    [Serializable]
    public struct PublicServerListing
    {
        public string Name;
        public int CurrentPlayers;
        public int MaxPlayers;
        public bool HasPassword;
        public string Address;
    }

    /// <summary>
    ///     The response body of the GET /api/servers HTTP endpoint. The listings
    ///     are wrapped in an object since JsonUtility cannot deserialize root arrays.
    /// </summary>
    [Serializable]
    public class PublicServerListResponse
    {
        public PublicServerListing[] Servers;
    }
}
