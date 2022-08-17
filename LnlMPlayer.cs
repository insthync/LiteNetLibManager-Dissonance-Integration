using UnityEngine;
using LiteNetLibManager;

namespace Dissonance.Integrations.LiteNetLibManager
{
    public class LnlMPlayer : LiteNetLibBehaviour, IDissonancePlayer, ILnlMPlayer
    {
        private LnlMPlayerFunc playerFunc;

        public string PlayerId
        {
            get
            {
                if (playerFunc == null)
                    return string.Empty;
                return playerFunc.PlayerId;
            }
        }

        public Vector3 Position
        {
            get { return transform.position; }
        }

        public Quaternion Rotation
        {
            get { return transform.rotation; }
        }

        public NetworkPlayerType Type
        {
            get
            {
                if (playerFunc == null)
                    return NetworkPlayerType.Unknown;
                return playerFunc.Type;
            }
        }

        public bool IsTracking
        {
            get
            {
                if (playerFunc == null)
                    return false;
                return playerFunc.IsTracking;
            }
        }

        public override void OnStartClient()
        {
            playerFunc = new LnlMPlayerFunc(FindObjectOfType<DissonanceComms>(), FindObjectOfType<LnlMCommsNetwork>(), this);
        }

        private void OnEnable()
        {
            if (playerFunc != null)
                playerFunc.OnEnable();
        }

        private void OnDisable()
        {
            if (playerFunc != null)
                playerFunc.OnDisable();
        }

        private void OnDestroy()
        {
            if (playerFunc != null)
                playerFunc.OnDestroy();
        }
    }
}
