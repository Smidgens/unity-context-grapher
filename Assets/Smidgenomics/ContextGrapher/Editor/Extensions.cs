namespace Smidgenomics.ContextGrapher
{
	using UnityEngine;
	using UnityEditor;
	using System.Collections.Generic;
	using System.Linq;

	internal static partial class Extensions
	{
		public static Rect Resize(this Rect r, Vector2 offset, bool recenter = true)
		{
			Vector2 center = r.center;
			r.width += offset.x;
			r.height += offset.y;
			if(recenter) { r.center = center; }
			return r;
		}

		public static Rect SetSize(this Rect r, Vector2 size)
		{
			r.size = size;
			return r;
		}

		public static Rect[] SliceVerticalMixed(this Rect r, float padding, params float[] heights)
		{
			float absoluteHeight = padding * (heights.Length - 1);
			for(int i = 0; i < heights.Length; i++)
			{
				if(heights[i] <= 1f){ continue; }
				absoluteHeight += heights[i];
			}
			float remainder = r.height - absoluteHeight;
			if(remainder < 0) { remainder = 0f; }

			Rect[] rects = new Rect[heights.Length];
			float offset = 0f;
			for(int i = 0; i < heights.Length; i++)
			{
				rects[i] = new Rect(r.x, r.y + offset, r.width, heights[i] <= 1f ? heights[i] * remainder : heights[i]);
				offset += rects[i].height + padding;
			}
			return rects;
		}

		public static Vector2 Round(this Vector2 value, float factor = 1f)
		{
			value.x = Mathf.Round(value.x / factor) * factor;
			value.y = Mathf.Round(value.y / factor) * factor;
			return value;
		}

		public static Rect GetRect(this Vector2 position, float width, float height, bool centered = true)
		{
			Rect r = new Rect(0, 0, width, height);
			if(centered) { r.center = position; }
			else { r.position = position; }
			return r;
		}

		public static RecursiveNode GetRecursive(this ContextGraph graph)
		{
			if(graph.RootNode == null) { return null; }
			Dictionary<string, RecursiveNode> nodes = new Dictionary<string, RecursiveNode>();

			foreach(var n in graph.Nodes)
			{
				var rn = new RecursiveNode
				{
					id = n.id,
					label = n.label,
					command = n.command,
					type = n.Type,
					yPos = n.position.y
				};
				nodes.Add(rn.id, rn);
			}

			// check if there's a root node set
			RecursiveNode rootNode;
			if(!nodes.TryGetValue(graph.RootNode, out rootNode)) { return null; }

			

			foreach(var e in graph.Edges)
			{
				RecursiveNode from, to;
				if(!nodes.TryGetValue(e.from, out from)) { continue; }
				if(!nodes.TryGetValue(e.to, out to)) { continue; }
				from.children.Add(to);
			}
			SortChildren(rootNode);

			return rootNode;
		}

		public static GenericMenu GetMenu(this RecursiveNode root, bool isPreview = false)
		{
			GenericMenu m = new GenericMenu();

			foreach(var c in root.children)
			{
				InitMenu("", c, m, isPreview);
			}
			
			return m;
		}

		private static void SortChildren(RecursiveNode n)
		{
			if(n.children.Count > 0)
			{
				n.children = n.children.OrderBy(x => x.yPos).ToList();
				foreach(var c in n.children) { SortChildren(c); }
			}

			
		}

		private static void InitMenu(string path, RecursiveNode node, GenericMenu menu, bool isPreview)
		{
			string fullPath = path + node.label;

			// if(path.Length > 0) {  }


			if(node.type == NodeType.Separator)
			{
				menu.AddSeparator(path);
			}
			else if(node.type == NodeType.Command)
			{
				var label = new GUIContent(fullPath);
				if(isPreview)
				{
					menu.AddDisabledItem(label);
				}
				else
				{
					menu.AddItem(label, false, () =>
					{
						Utils.ExecuteCommandString(node.command);
					});
				}
			}
			else
			{
				foreach(var n in node.children)
				{
					InitMenu(fullPath + "/", n, menu, isPreview);
				}
			}
		} 

	}

}