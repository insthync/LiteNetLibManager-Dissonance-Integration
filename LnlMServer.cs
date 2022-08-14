using System;
using System.Collections.Generic;
using Dissonance.Networking;
using Dissonance.Networking.Server;
using LiteNetLibManager;

namespace Dissonance.Integrations.LiteNetLibManager
{
    public class LnlMServer
        : BaseServer<LnlMServer, LnlMClient, long>
    {
        #region Fields and properties
        private readonly LnlMCommsNetwork _network;
        private readonly Dictionary<long, ClientInfo<long>> _addedClients = new Dictionary<long, ClientInfo<long>>();
        #endregion

        #region Constructors
        public LnlMServer(LnlMCommsNetwork network)
        {
            if (network == null)
                throw new ArgumentNullException("network");
            _network = network;
        }
        #endregion

        #region Server update
        public override ServerState Update()
        {
            // The only way to get an event regarding disconnections from HLAPI is to be a NetworkManager. We aren't a
            // NetworkManager and don't want to be because it would make setting up the HLAPI integration significantly
            // more complex. Instead we'll have to poll for disconnections.
            List<long> keys = new List<long>(_addedClients.Keys);
            for (var i = keys.Count - 1; i >= 0; --i)
            {
                var connectionId = keys[i];
                if (!_network.manager.ContainsConnectionId(connectionId))
                {
                    ClientDisconnected(connectionId);
                    _addedClients.Remove(connectionId);
                }
            }

            return base.Update();
        }
        #endregion

        protected override void AddClient(ClientInfo<long> client)
        {
            base.AddClient(client);

            // Add this player to the list of known connections (do not add the local player)
            if (client.PlayerName != _network.PlayerName)
                _addedClients[client.Connection] = client;
        }

        #region Connect/Disconnect
        public override void Connect()
        {
            _network.manager.RegisterServerMessage(_network.voiceOpCode, OnVoiceReceivedHandler);
            _network.manager.RegisterServerMessage(_network.reqIdOpCode, OnReqIdReceivedHandler);
            base.Connect();
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _network.manager.UnregisterServerMessage(_network.voiceOpCode);
            _network.manager.UnregisterServerMessage(_network.reqIdOpCode);
        }
        #endregion

        #region Send/Receive
        private void OnVoiceReceivedHandler(MessageHandlerData netMsg)
        {
            NetworkReceivedPacket(netMsg.ConnectionId, new ArraySegment<byte>(netMsg.Reader.GetBytesWithLength()));
        }

        private void OnReqIdReceivedHandler(MessageHandlerData netMsg)
        {
            long connectionId = netMsg.Reader.GetLong();
            if (_addedClients.ContainsKey(connectionId))
            {
                ClientInfo<long> clientInfo = _addedClients[connectionId];
                _network.manager.ServerSendPacket(connectionId, _network.serverDataChannel, LiteNetLib.DeliveryMethod.ReliableOrdered, _network.resIdOpCode, (writer) =>
                {
                    writer.Put(connectionId);
                    writer.Put(connectionId == netMsg.ConnectionId);
                    writer.Put(clientInfo.PlayerName);
                });
            }
        }

        protected override void ReadMessages()
        {
            // Messages are received and read by event handlers, so it doesn't have to read messages here
        }

        protected override void SendReliable(long connectionId, ArraySegment<byte> packet)
        {
            _network.manager.ServerSendPacket(connectionId, _network.serverDataChannel, LiteNetLib.DeliveryMethod.ReliableOrdered, _network.voiceOpCode, (writer) =>
            {
                writer.PutBytesWithLength(packet.Array);
            });
        }

        protected override void SendUnreliable(long connectionId, ArraySegment<byte> packet)
        {
            _network.manager.ServerSendPacket(connectionId, _network.serverDataChannel, LiteNetLib.DeliveryMethod.Sequenced, _network.voiceOpCode, (writer) =>
            {
                writer.PutBytesWithLength(packet.Array);
            });
        }
        #endregion
    }
}
