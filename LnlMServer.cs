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
        private readonly Dictionary<long, HashSet<long>> _pendingIdRequests = new Dictionary<long, HashSet<long>>();
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
                    _pendingIdRequests.Remove(connectionId);
                }
            }
            return base.Update();
        }
        #endregion

        protected override void AddClient(ClientInfo<long> client)
        {
            base.AddClient(client);
            _addedClients[client.Connection] = client;
            if (_pendingIdRequests.ContainsKey(client.Connection))
            {
                foreach (long requesterConnectionId in _pendingIdRequests[client.Connection])
                {
                    _network.SendPlayerResponse(requesterConnectionId, client.Connection, client.PlayerName);
                }
                _pendingIdRequests.Remove(client.Connection);
            }
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
                // Send player info back to the client
                ClientInfo<long> client = _addedClients[connectionId];
                _network.SendPlayerResponse(netMsg.ConnectionId, connectionId, client.PlayerName);
            }
            else
            {
                // No added client yet, add the requester's connection Id to pending collection, then it will send data back to requesters later
                if (!_pendingIdRequests.ContainsKey(connectionId))
                    _pendingIdRequests[connectionId] = new HashSet<long>();
                _pendingIdRequests[connectionId].Add(netMsg.ConnectionId);
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
