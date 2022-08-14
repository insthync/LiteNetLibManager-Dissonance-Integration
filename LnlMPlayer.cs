using UnityEngine;
using LiteNetLibManager;

namespace Dissonance.Integrations.LiteNetLibManager
{
    public class LnlMPlayer : LiteNetLibBehaviour, IDissonancePlayer
    {
        private LnlMPlayerFunc player;

        public string PlayerId => player.PlayerId;

        public Vector3 Position => player.Position;

        public Quaternion Rotation => player.Rotation;

        public NetworkPlayerType Type => player.Type;

        public bool IsTracking => player.IsTracking;

        public override void OnStartClient()
        {
            player = new LnlMPlayerFunc(FindObjectOfType<DissonanceComms>(), FindObjectOfType<LnlMCommsNetwork>(), transform, ConnectionId);
        }

        private void OnDestroy()
        {
            if (player != null)
                player.OnDestroy();
        }
    }
}