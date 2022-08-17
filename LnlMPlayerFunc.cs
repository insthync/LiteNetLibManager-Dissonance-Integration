using UnityEngine;

namespace Dissonance.Integrations.LiteNetLibManager
{
    public class LnlMPlayerFunc : IDissonancePlayer
    {
        private static readonly Log Log = Logs.Create(LogCategory.Network, "LiteNetLibManager Player Component");

        public DissonanceComms Comms { get; private set; }
        public LnlMCommsNetwork CommsNetwork { get; private set; }
        public ILnlMPlayer Player { get; private set; }
        public bool IsOwnerClient { get; private set; }
        public string PlayerId { get; private set; }
        public bool IsTracking { get; private set; }

        public long ConnectionId
        {
            get { return Player.ConnectionId; }
        }

        public Vector3 Position
        {
            get { return Player.Position; }
        }

        public Quaternion Rotation
        {
            get { return Player.Rotation; }
        }

        public NetworkPlayerType Type
        {
            get
            {
                if (Comms == null || string.IsNullOrWhiteSpace(PlayerId))
                    return NetworkPlayerType.Unknown;
                return Comms.LocalPlayerName.Equals(PlayerId) ? NetworkPlayerType.Local : NetworkPlayerType.Remote;
            }
        }

        public System.Action<string> onSetPlayerId;

        public LnlMPlayerFunc(DissonanceComms comms, LnlMCommsNetwork commsNetwork, ILnlMPlayer player)
        {
            Comms = comms;
            CommsNetwork = commsNetwork;
            Player = player;
            CommsNetwork.RegisterPlayer(this);
        }

        public void Setup(bool isOwnerClient, string playerId)
        {
            IsOwnerClient = isOwnerClient;
            SetPlayerId(playerId);
        }

        public void OnEnable()
        {
            if (!IsTracking && !string.IsNullOrWhiteSpace(PlayerId))
                StartTracking();
        }

        public void OnDisable()
        {
            if (IsTracking)
                StopTracking();
        }

        public void OnDestroy()
        {
            if (IsTracking)
                StopTracking();
            CommsNetwork.UnregisterPlayer(ConnectionId);
        }

        private void SetPlayerId(string playerId)
        {
            // We need to stop and restart tracking to handle the name change
            if (IsTracking)
                StopTracking();

            // Perform the actual work
            PlayerId = playerId;
            StartTracking();

            if (onSetPlayerId != null)
                onSetPlayerId.Invoke(PlayerId);
        }

        private void StartTracking()
        {
            if (IsTracking)
                throw Log.CreatePossibleBugException("Attempting to start player tracking, but tracking is already started", "B7D1F25E-72AF-4E93-8CFF-90CEBEAC68CF");

            if (Comms != null)
            {
                Comms.TrackPlayerPosition(this);
                IsTracking = true;
            }
        }

        private void StopTracking()
        {
            if (!IsTracking)
                throw Log.CreatePossibleBugException("Attempting to stop player tracking, but tracking is not started", "EC5C395D-B544-49DC-B33C-7D7533349134");

            if (Comms != null)
            {
                Comms.StopTracking(this);
                IsTracking = false;
            }
        }
    }
}
