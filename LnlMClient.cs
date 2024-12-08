using System;
using Dissonance.Networking;
using LiteNetLibManager;

namespace Dissonance.Integrations.LiteNetLibManager
{
    public class LnlMClient
        : BaseClient<LnlMServer, LnlMClient, long>
    {
        #region Fields and properties
        private readonly LnlMCommsNetwork _network;
        #endregion

        #region Constructors
        public LnlMClient(LnlMCommsNetwork network)
            : base(network)
        {
            if (network == null)
                throw new ArgumentNullException("network");
            _network = network;

        }
        #endregion

        #region Connect/Disconnect
        public override void Connect()
        {
            _network.manager.RegisterClientMessage(_network.voiceOpCode, OnVoiceReceivedHandler);
            _network.manager.RegisterClientMessage(_network.resIdOpCode, OnResIdReceivedHandler);
            Connected();
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _network.manager.UnregisterClientMessage(_network.voiceOpCode);
            _network.manager.UnregisterClientMessage(_network.resIdOpCode);
        }
        #endregion

        #region Send/Receive
        private void OnVoiceReceivedHandler(MessageHandlerData netMsg)
        {
            NetworkReceivedPacket(new ArraySegment<byte>(netMsg.Reader.GetBytesWithLength()));
        }

        private void OnResIdReceivedHandler(MessageHandlerData netMsg)
        {
            long connectionId = netMsg.Reader.GetLong();
            bool isOwnerClient = netMsg.Reader.GetBool();
            string id = netMsg.Reader.GetString();
            _network.SetupPlayer(connectionId, isOwnerClient, id);
        }

        protected override void ReadMessages()
        {
            // Messages are received and read by event handlers, so it doesn't have to read messages here
        }

        protected override void SendReliable(ArraySegment<byte> packet)
        {
            _network.manager.ClientSendPacket(_network.clientDataChannel, LiteNetLib.DeliveryMethod.ReliableOrdered, _network.voiceOpCode, (writer) =>
            {
                writer.PutBytesWithLength(packet.Array, packet.Offset, (ushort)packet.Count);
            });
        }

        protected override void SendUnreliable(ArraySegment<byte> packet)
        {
            _network.manager.ClientSendPacket(_network.clientDataChannel, LiteNetLib.DeliveryMethod.Unreliable, _network.voiceOpCode, (writer) =>
            {
                writer.PutBytesWithLength(packet.Array, packet.Offset, (ushort)packet.Count);
            });
        }
        #endregion
    }
}
