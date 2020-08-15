using System;
using System.Collections.Generic;
using Dissonance.Networking;
using Dissonance.Networking.Server;
using LiteNetLib.Utils;
using LiteNetLibManager;

namespace Dissonance.Integrations.LiteNetLibManager
{
    public class LnlMServer
        : BaseServer<LnlMServer, LnlMClient, long>
    {
        #region Fields and properties
        private readonly LnlMCommsNetwork _network;
        private readonly List<long> _addedConnections = new List<long>();

        #endregion

        #region Constructors
        public LnlMServer(LnlMCommsNetwork network)
        {
            if (network == null) throw new ArgumentNullException("network");

            _network = network;
        }
        #endregion

        #region Server update
        public override ServerState Update()
        {
            // The only way to get an event regarding disconnections from HLAPI is to be a NetworkManager. We aren't a
            // NetworkManager and don't want to be because it would make setting up the HLAPI integration significantly
            // more complex. Instead we'll have to poll for disconnections.
            for (var i = _addedConnections.Count - 1; i >= 0; i--)
            {
                var conn = _addedConnections[i];
                if (!_network.Manager.ContainsPlayer(conn))
                {
                    ClientDisconnected(_addedConnections[i]);
                    _addedConnections.RemoveAt(i);
                }
            }

            return base.Update();
        }
        #endregion

        protected override void AddClient(ClientInfo<long> client)
        {
            base.AddClient(client);

            //Add this player to the list of known connections (do not add the local player)
            if (client.PlayerName != _network.PlayerName)
                _addedConnections.Add(client.Connection);
        }

        #region Connect/Disconnect
        public override void Connect()
        {
            _network.Manager.RegisterServerMessage(_network.TypeCode, OnMessageReceivedHandler);

            base.Connect();
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _network.Manager.UnregisterServerMessage(_network.TypeCode);
        }
        #endregion

        #region Send/Receive
        private void OnMessageReceivedHandler(LiteNetLibMessageHandler netmsg)
        {
            NetworkReceivedPacket(netmsg.connectionId, new ArraySegment<byte>(netmsg.reader.GetArray<byte>()));
        }

        protected override void ReadMessages()
        {
            //Messages are received in an event handler, so we don't need to do any work to read events
        }

        protected override void SendReliable(long connectionId, ArraySegment<byte> packet)
        {
            _network.Manager.ServerSendPacket(connectionId, LiteNetLib.DeliveryMethod.ReliableOrdered, _network.TypeCode, (writer) =>
            {
                writer.PutArray(packet.Array);
            });
        }

        protected override void SendUnreliable(long connectionId, ArraySegment<byte> packet)
        {
            _network.Manager.ServerSendPacket(connectionId, LiteNetLib.DeliveryMethod.Sequenced, _network.TypeCode, (writer) =>
            {
                writer.PutArray(packet.Array);
            });
        }
        #endregion
    }
}
