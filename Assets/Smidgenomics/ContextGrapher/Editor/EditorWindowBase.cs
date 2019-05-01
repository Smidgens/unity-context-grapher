namespace Smidgenomics.ContextGrapher.Editor
{
	using UnityEngine;
	using UnityEditor;
	using UnityEditorInternal;

	
	internal abstract class EditorWindowBase : EditorWindow
	{
		protected virtual void DoWindowGUI(Rect area){}


		protected static T InitWindow<T>(string title) where T : EditorWindow
		{
			T window = GetWindow<T>(title, typeof(SceneView));
			window.Show();
			return window;
		}

		protected void OnGUI()
		{
			DoWindowGUI(GetWindowRect());
		}

		private static float[] _windowPadding = { -2, 10, 0, 25 };

		private static Rect GetWindowRect()
		{
			Rect r = EditorGUILayout.GetControlRect(GUILayout.Width(Screen.width), GUILayout.Height(Screen.height));
			r.x += _windowPadding[0];
			r.width -= _windowPadding[0] + _windowPadding[1];
			r.y += _windowPadding[2];
			r.height -= _windowPadding[2] + _windowPadding[3];
			return r;
		}
	}
}