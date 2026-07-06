namespace CSM.Networking
{
    /// <summary>
    ///     A single entry in the public server list, as returned by the GS
    ///     GET /api/servers HTTP endpoint. Parsed by CSM.Util.MiniJson, since
    ///     both UnityEngine.JsonUtility (fails to populate arrays of custom
    ///     types, class or struct) and Newtonsoft.Json (missing
    ///     System.Runtime.Serialization in the game's Mono runtime) are
    ///     unusable for this shape in this environment.
    /// </summary>
    public struct PublicServerListing
    {
        public string Name;
        public int CurrentPlayers;
        public int MaxPlayers;
        public bool HasPassword;
        public string Address;
    }

    /// <summary>
    ///     The response body of the GET /api/servers HTTP endpoint.
    /// </summary>
    public class PublicServerListResponse
    {
        public PublicServerListing[] Servers;
    }
}
