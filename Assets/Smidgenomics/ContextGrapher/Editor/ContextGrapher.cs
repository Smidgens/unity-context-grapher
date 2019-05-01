namespace Smidgenomics.ContextGrapher
{
	using UnityEngine;
	using UnityEditor;
	
	[InitializeOnLoad]
	internal static class ContextGrapher
	{
		public static void UseGraph(ContextGraph g)
		{
			EditorPrefs.SetInt(_prefsKey, g ? g.GetInstanceID() : -1);
			_cachedCurrent = g;
		}


		public static ContextGraph CurrentGraph { get { return GetCurrentGraph(); } }

		private static ContextGraph _cachedCurrent = null;
		private static string _prefsKey = typeof(ContextGrapher).Name + "_" + "Current";

		static ContextGrapher()
		{
			EditorApplication.projectWindowItemOnGUI += OnProjectWindowGUI;
		}

		[MenuItem("Plugins/Context Grapher/Use Default", false, 11)]
		private static void UseDefault()
		{
			UseGraph(null);
		}


		/// <summary>The callback called on drawing the GUI of project window.</summary>
		private static void OnProjectWindowGUI(string guid, Rect selectionRect)
		{
			if(Event.current.type != EventType.ContextClick) { return; }
			var g = GetCurrentGraph();
			if(!g) { return; }
			g.GetRecursive().GetMenu().ShowAsContext();
			Event.current.Use();
		}

		private static ContextGraph GetCurrentGraph()
		{
			if(!_cachedCurrent)
			{
				int guid = EditorPrefs.GetInt(_prefsKey, -1);
				if(guid > 0)
				{
					var ob = EditorUtility.InstanceIDToObject(guid);	
					// BUG: Should be looked at in the future to prevent data loss
					// Causes in some instances, Unity to retrieve the asset as a monoscript instance
					// rather than an actual instance of the asset's class type
					// For now, if this happens, the cached GUID reference to the asset is detached
					// to prevent casting errors
					if(ob.GetType() != typeof(MonoScript))
					{
						EditorPrefs.SetInt(_prefsKey, -1);
						_cachedCurrent = ob != null ? (ContextGraph)ob : null;
					}	
				}
			}
			return _cachedCurrent;
		}
	}
}