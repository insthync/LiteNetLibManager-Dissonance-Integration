using System;
using Dissonance.Networking;
using UnityEngine;
using LiteNetLibManager;

namespace Dissonance.Integrations.LiteNetLibManager
{
    [HelpURL("https://github.com/insthync/DissonanceForLiteNetLibManager/blob/master/README.md")]
    public class LnlMCommsNetwork
        : BaseCommsNetwork<LnlMServer, LnlMClient, long, Unit, Unit>
    {
        public ushort TypeCode = 18385;

        public LiteNetLibGameManager Manager { get; private set; }

        protected override LnlMServer CreateServer(Unit details)
        {
            return new LnlMServer(this);
        }

        protected override LnlMClient CreateClient(Unit details)
        {
            return new LnlMClient(this);
        }

        private void Awake()
        {
            Manager = FindObjectOfType<LiteNetLibGameManager>();
        }

        protected override void Update()
        {
            if (IsInitialized)
            {
                if (Manager.IsNetworkActive)
                {
                    // switch to the appropriate mode if we have not already
                    var server = Manager.IsServer;
                    var client = Manager.IsClient;

                    if (Mode.IsServerEnabled() != server || Mode.IsClientEnabled() != client)
                    {
                        if (server && client)
                            RunAsHost(Unit.None, Unit.None);
                        else if (server)
                            RunAsDedicatedServer(Unit.None);
                        else if (client)
                            RunAsClient(Unit.None);
                    }
                }
                else if (Mode != NetworkMode.None)
                {
                    // stop the network if networking system has shut down
                    Stop();
                }
            }

            base.Update();
        }
    }
}
