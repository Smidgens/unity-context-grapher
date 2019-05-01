#pragma warning disable 0168 // var declared, unused
#pragma warning disable 0219 // var assigned, unused
#pragma warning disable 0414 // private var assigned, unused
#pragma warning disable 0649 // never assigned, unused

namespace Smidgenomics.ContextGrapher.Editor
{
	using UnityEngine;
	using UnityEditor;
	
	[CustomEditor(typeof(ContextGraph))]
	internal class Editor_ContextGraph : Editor
	{
		public override void OnInspectorGUI()
		{
			if(GUILayout.Button("Edit..."))
			{
				EditorWindow_ContextGraph.Load((ContextGraph)target);
			}


			var current = ContextGrapher.CurrentGraph;
			bool t = GUI.enabled;

			EditorGUILayout.BeginHorizontal();
			
			GUI.enabled = current != _target;
			if(GUILayout.Button("Apply", EditorStyles.miniButtonLeft))
			{
				ContextGrapher.UseGraph((ContextGraph)target);
			}
			GUI.enabled = current == _target;
			if(GUILayout.Button("Clear", EditorStyles.miniButtonRight))
			{
				ContextGrapher.UseGraph(null);
			}

			EditorGUILayout.EndHorizontal();

			GUI.enabled = t;

			if(current == _target)
			{
				EditorGUILayout.HelpBox("This menu is currently in use.", MessageType.Info);
			}

			EditorGUILayout.Space();

			if(GUILayout.Button("Preview", EditorStyles.miniButton))
			{
				var m = ((ContextGraph)target).GetRecursive().GetMenu(true);
				m.ShowAsContext();
			}
		}

		private SerializedProperty _nodes = null;
		private SerializedProperty _edges = null;
		private SerializedProperty _rootNode = null;

		private ContextGraph _target = null;

		private void OnEnable()
		{
			_target = (ContextGraph)target;
			_nodes = serializedObject.FindProperty("_nodes");
			_edges = serializedObject.FindProperty("_edges");
			_rootNode = serializedObject.FindProperty("_rootNode");
		}
	
	}

}