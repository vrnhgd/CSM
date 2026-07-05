namespace CSM.GS.Http
{
    /// <summary>
    ///     A single entry in the public server list, as returned by the
    ///     GET /api/servers HTTP endpoint. Only contains information safe
    ///     to expose publicly (no token, no internal/external IP distinction).
    /// </summary>
    public class PublicServerListing
    {
        public string Name { get; set; }

        public int CurrentPlayers { get; set; }

        public int MaxPlayers { get; set; }

        public bool HasPassword { get; set; }

        /// <summary>
        ///     The address (ip:port) that a client should connect to in order to join.
        /// </summary>
        public string Address { get; set; }
    }

    /// <summary>
    ///     The response body of the GET /api/servers HTTP endpoint. The listings are
    ///     wrapped in an object (rather than returned as a bare JSON array) since Unity's
    ///     JsonUtility, used to parse this on the mod side, cannot deserialize root arrays.
    /// </summary>
    public class PublicServerListResponse
    {
        public PublicServerListing[] Servers { get; set; }
    }
}
