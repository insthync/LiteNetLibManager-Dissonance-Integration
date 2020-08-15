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

        private SerializedProperty _typeCodeProperty;

        protected void OnEnable()
        {
            _typeCodeProperty = serializedObject.FindProperty("TypeCode");
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            using (new EditorGUI.DisabledScope(Application.isPlaying))
            {

                //Set the two QoS channels
                EditorGUILayout.HelpBox("Dissonance requires 2 HLAPI QoS channels.", MessageType.Info);

                _advanced = EditorGUILayout.Foldout(_advanced, "Advanced Configuration");
                if (_advanced)
                {
                    //Set type code
                    EditorGUILayout.HelpBox("Dissonance requires a type code. If you are not sending raw network packets you should use the default value.", MessageType.Info);
                    EditorGUILayout.PropertyField(_typeCodeProperty);

                    var tc = _typeCodeProperty.intValue;

                    if (tc >= ushort.MaxValue || tc < 1000)
                        EditorGUILayout.HelpBox("Event code must be between 1000 and 65535", MessageType.Error);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}