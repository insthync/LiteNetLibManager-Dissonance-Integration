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
            _network.Manager.RegisterClientMessage(_network.typeCode, OnMessageReceivedHandler);
            Connected();
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _network.Manager.UnregisterClientMessage(_network.typeCode);
        }
        #endregion

        #region Send/Receive
        private void OnMessageReceivedHandler(MessageHandlerData netmsg)
        {
            NetworkReceivedPacket(new ArraySegment<byte>(netmsg.Reader.GetBytesWithLength()));
        }

        protected override void ReadMessages()
        {
            // Messages are received and read by event handlers, so it doesn't have to read messages here
        }

        protected override void SendReliable(ArraySegment<byte> packet)
        {
            _network.Manager.ClientSendPacket(_network.clientChannelId, LiteNetLib.DeliveryMethod.ReliableOrdered, _network.typeCode, (writer) =>
            {
                writer.PutBytesWithLength(packet.Array);
            });
        }

        protected override void SendUnreliable(ArraySegment<byte> packet)
        {
            _network.Manager.ClientSendPacket(_network.clientChannelId, LiteNetLib.DeliveryMethod.Sequenced, _network.typeCode, (writer) =>
            {
                writer.PutBytesWithLength(packet.Array);
            });
        }
        #endregion
    }
}
