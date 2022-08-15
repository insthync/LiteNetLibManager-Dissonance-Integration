using Dissonance.Editor;
using UnityEditor;
using UnityEngine;

namespace Dissonance.Integrations.LiteNetLibManager.Editor
{
    [CustomEditor(typeof(LnlMCommsNetwork))]
    public class LnlMCommsNetworkEditor
        : BaseDissonnanceCommsNetworkEditor<LnlMCommsNetwork, LnlMServer, LnlMClient, long, Unit, Unit>
    {
        private bool _advanced;

        private SerializedProperty _voiceOpCodeProperty;
        private SerializedProperty _reqIdOpCodeProperty;
        private SerializedProperty _resIdOpCodeProperty;
        private SerializedProperty _clientDataChannelProperty;
        private SerializedProperty _serverDataChannelProperty;

        protected void OnEnable()
        {
            _voiceOpCodeProperty = serializedObject.FindProperty("voiceOpCode");
            _reqIdOpCodeProperty = serializedObject.FindProperty("reqIdOpCode");
            _resIdOpCodeProperty = serializedObject.FindProperty("resIdOpCode");
            _clientDataChannelProperty = serializedObject.FindProperty("clientDataChannel");
            _serverDataChannelProperty = serializedObject.FindProperty("serverDataChannel");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {
                _advanced = EditorGUILayout.Foldout(_advanced, "Advanced Configuration");
                if (_advanced)
                {
                    EditorGUILayout.PropertyField(_voiceOpCodeProperty);
                    EditorGUILayout.PropertyField(_reqIdOpCodeProperty);
                    EditorGUILayout.PropertyField(_resIdOpCodeProperty);
                    EditorGUILayout.PropertyField(_clientDataChannelProperty);
                    EditorGUILayout.PropertyField(_serverDataChannelProperty);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}
