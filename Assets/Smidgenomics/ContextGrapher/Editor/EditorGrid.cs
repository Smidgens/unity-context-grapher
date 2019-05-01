namespace Smidgenomics.ContextGrapher
{
	using System;
	using System.Linq;
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;

	public class EditorGrid
	{
		#region Public
		public Rect area { get { return _areaRect; } set { _areaRect = value; } }
		public Func<Vector2> offsetHandler = null;

		public EditorGrid(Color backgroundColor)
		{
			_backgroundColor = backgroundColor;
		}

		public void AddLayer(float spacing, float opacity, Color color)
		{
			_layers.Add(new GridLayer
			{
				opacity = opacity,
				cellspacing = spacing,
				color = color
			});
		}

		public void Draw()
		{
			Draw(_areaRect);
		}

		public void Draw(Rect area)
		{
			if(!_stylesInitialized) { InitializeStyles(); _stylesInitialized = true; }
			DrawBackground(area);
			_layers.ForEach(l => DrawLayer(l, area));
		}

		#endregion
		#region Private
		private void DrawLayer(GridLayer layer, Rect area)
		{
			DrawLines(area, GetOffset(), layer.cellspacing, layer.opacity, layer.color);
		}

		private void DrawBackground(Rect r)
		{
			var t1 = GUI.color;
			GUI.color = _backgroundColor;
			GUI.Box(r, GUIContent.none, _shadowStyle);
			GUI.color = t1;
		}

		private static void DrawLines(Rect rect, Vector2 offset, float gridSpacing, float gridOpacity, Color color)
		{
			Vector2 divs = new Vector2(Mathf.CeilToInt(rect.width / gridSpacing), Mathf.CeilToInt(rect.height / gridSpacing));
			Color tempColor = Handles.color;
			Handles.BeginGUI();
			Handles.color = new Color(color.r, color.g, color.b, gridOpacity);
			Vector3 newOffset = new Vector3(offset.x % gridSpacing, offset.y % gridSpacing, 0);
			Vertical((int)divs.x, gridSpacing, rect.height, newOffset);
			Horizontal((int)divs.y, gridSpacing, rect.width, newOffset);
			Handles.color = Color.white;
			Handles.EndGUI();
			Handles.color = tempColor;
		}
		private Rect _areaRect = new Rect();
		private List<GridLayer> _layers = new List<GridLayer>();
		private GUIStyle _shadowStyle = null;
		private bool _stylesInitialized = false;
		private Color _backgroundColor = Color.grey;

		private void InitializeStyles()
		{
			_shadowStyle = new GUIStyle(GUI.skin.GetStyle("InnerShadowBg"));
		}


		private Vector2 GetOffset()
		{
			if(offsetHandler != null) { return offsetHandler(); }
			return Vector2.zero;
		}

		private static void Vertical(int count, float spacing, float height, Vector3 offset)
		{
			for (int i = 0; i < count; i++)
			{
				Vector3 a = new Vector3(spacing * i, -height, 0);
				Vector3 b = new Vector3(spacing * i, height + height, 0);
				Handles.DrawLine(a + offset, b + offset);
			}
		}

		private static void Horizontal(int count, float spacing, float width, Vector3 offset)
		{
			for (int i = 0; i < count; i++)
			{
				Vector3 a = new Vector3(-width, spacing * i, 0);
				Vector3 b = new Vector3(width + width, spacing * i, 0f);
				Handles.DrawLine(a + offset, b + offset);
			}
		}

		private struct GridLayer
		{
			public Color color;
			public float cellspacing, opacity;
		}
		#endregion
	}
}