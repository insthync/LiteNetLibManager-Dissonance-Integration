using Dissonance.Networking;
using System.Collections.Generic;
using UnityEngine;
using LnlM = LiteNetLibManager.LiteNetLibManager;

namespace Dissonance.Integrations.LiteNetLibManager
{
    [HelpURL("https://github.com/insthync/DissonanceForLiteNetLibManager/blob/master/README.md")]
    public class LnlMCommsNetwork
        : BaseCommsNetwork<LnlMServer, LnlMClient, long, Unit, Unit>
    {
        public ushort voiceOpCode = 18385;
        public ushort reqIdOpCode = 18386;
        public ushort resIdOpCode = 18387;
        public byte clientDataChannel = 3;
        public byte serverDataChannel = 3;
        public LnlM manager;

        private Dictionary<long, LnlMPlayerFunc> registeredPlayers = new Dictionary<long, LnlMPlayerFunc>();

        protected override LnlMServer CreateServer(Unit details)
        {
            return new LnlMServer(this);
        }

        protected override LnlMClient CreateClient(Unit details)
        {
            return new LnlMClient(this);
        }

        private void Start()
        {
            if (manager == null)
                manager = FindObjectOfType<LnlM>();
        }

        protected override void Update()
        {
            if (!IsInitialized)
                return;

            if (manager.IsNetworkActive)
            {
                // Switch to the appropriate mode if we have not already
                bool isServer = manager.IsServer;
                bool isClient = manager.IsClient;

                if (Mode.IsServerEnabled() != isServer || Mode.IsClientEnabled() != isClient)
                {
                    if (isServer && isClient)
                        RunAsHost(Unit.None, Unit.None);
                    else if (isServer)
                        RunAsDedicatedServer(Unit.None);
                    else if (isClient)
                        RunAsClient(Unit.None);
                }
            }
            else if (Mode != NetworkMode.None)
            {
                // Stop the network if networking system has shut down
                Stop();
            }

            base.Update();
        }

        public void RegisterPlayer(LnlMPlayerFunc player)
        {
            if (registeredPlayers.ContainsKey(player.ConnectionId))
                return;
            registeredPlayers[player.ConnectionId] = player;
        }

        public void UnregisterPlayer(long connectionId)
        {
            registeredPlayers.Remove(connectionId);
        }

        public void SetupPlayer(long connectionId, bool isOwnerClient, string playerId)
        {
            if (!registeredPlayers.ContainsKey(connectionId))
                return;
            registeredPlayers[connectionId].Setup(isOwnerClient, playerId);
        }
    }
}
