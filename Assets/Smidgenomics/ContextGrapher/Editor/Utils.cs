namespace Smidgenomics.ContextGrapher
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;

	internal static class Utils
	{
		// execute unity editor command
		public static void ExecuteCommandString(string command)
		{
			EditorApplication.ExecuteMenuItem(command);
		}

		public static bool IsUsingProSkin()
		{
			return UnityEditorInternal.InternalEditorUtility.HasPro();
		}

		public static string GetGUID()
		{
			return System.Guid.NewGuid().ToString("N");
		}

		public static bool ClickedRect(int button, Rect r)
		{
			return Event.current.type == EventType.MouseDown && Event.current.button == button && r.Contains(Event.current.mousePosition);
		}
	}
}