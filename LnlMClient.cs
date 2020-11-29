using System;
using Dissonance.Networking;
using LiteNetLib.Utils;
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
            _network.Manager.RegisterClientMessage(_network.TypeCode, OnMessageReceivedHandler);

            Connected();
        }

        public override void Disconnect()
        {
            base.Disconnect();
            _network.Manager.UnregisterClientMessage(_network.TypeCode);
        }
        #endregion

        #region Send/Receive
        private void OnMessageReceivedHandler(MessageHandlerData netmsg)
        {
            NetworkReceivedPacket(new ArraySegment<byte>(netmsg.Reader.GetArray<byte>()));
        }

        protected override void ReadMessages()
        {
            //Messages are received in an event handler, so we don't need to do any work to read events
        }

        protected override void SendReliable(ArraySegment<byte> packet)
        {
            _network.Manager.ClientSendPacket(LiteNetLib.DeliveryMethod.ReliableOrdered, _network.TypeCode, (writer) =>
            {
                writer.PutArray(packet.Array);
            });
        }

        protected override void SendUnreliable(ArraySegment<byte> packet)
        {
            _network.Manager.ClientSendPacket(LiteNetLib.DeliveryMethod.Sequenced, _network.TypeCode, (writer) =>
            {
                writer.PutArray(packet.Array);
            });
        }
        #endregion
    }
}
