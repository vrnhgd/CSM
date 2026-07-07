using ProtoBuf;

namespace CSM.GS.Commands.Data.ApiServer
{
    /// <summary>
    ///     A single entry in the public server list. Only contains information
    ///     safe to expose publicly (no token, no internal/external IP distinction).
    /// </summary>
    [ProtoContract]
    public class PublicServerEntry
    {
        [ProtoMember(1)]
        public string Name { get; set; }

        [ProtoMember(2)]
        public int CurrentPlayers { get; set; }

        [ProtoMember(3)]
        public int MaxPlayers { get; set; }

        [ProtoMember(4)]
        public bool HasPassword { get; set; }

        /// <summary>
        ///     The address (ip:port) that a client should connect to in order to join.
        /// </summary>
        [ProtoMember(5)]
        public string Address { get; set; }

        /// <summary>
        ///     The server's NAT-relay token, usable as a fallback join method (via
        ///     "token:" prefixed address in JoinGamePanel) when the direct address
        ///     isn't reachable (e.g. the host is behind NAT without port forwarding).
        /// </summary>
        [ProtoMember(6)]
        public string ServerToken { get; set; }
    }
}
