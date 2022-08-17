using UnityEngine;

namespace Dissonance.Integrations.LiteNetLibManager
{
    public interface ILnlMPlayer
    {
        public long ConnectionId { get; }
        public bool IsOwnerClient { get; }
        public Vector3 Position { get; }
        public Quaternion Rotation { get; }
    }
}
