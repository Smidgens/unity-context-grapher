#pragma warning disable 0168 // var declared, unused
#pragma warning disable 0219 // var assigned, unused
#pragma warning disable 0414 // private var assigned, unused
#pragma warning disable 0649 // never assigned, unused

namespace Smidgenomics.ContextGrapher.Editor
{
	using UnityEngine;
	using UnityEditor;
	using UnityEditorInternal;
	using System.Collections.Generic;

	internal class EditorWindow_ContextGraph : EditorWindowBase
	{
		public static void Load(ContextGraph g)
		{
			var w = EditorWindowBase.InitWindow<EditorWindow_ContextGraph>("✪ Context Graph");
			// var w = EditorWindowBase.InitWindow<EditorWindow_ContextGraph>("◉ Context Graph");
			w._currentGraph = g;
		}


		protected override void DoWindowGUI(Rect area)
		{

			var panelArea = area;
			panelArea.width = 200f;

			area.x += panelArea.width;
			area.width -= panelArea.width;

			


			if(_nodeStyle == null)
			{
				_nodeStyle = "flow node 0";
				_selectedNodeStyle = "flow node 0 on";
				_rootNodeStyle = "flow node 5";
				_selectedRootNodeStyle = "flow node 5 on";

				_centeredLabel = new GUIStyle(EditorStyles.miniBoldLabel);
				_centeredLabel.alignment = TextAnchor.MiddleCenter;
			}


			if(_serializedGraph == null || _serializedGraph.targetObject != _currentGraph)
			{
				_serializedGraph = null;
				if(_currentGraph)
				{
					_serializedGraph = new SerializedObject(_currentGraph);
					_serializedGraph.Update();
					_nodes = _serializedGraph.FindProperty("_nodes");
					_rootNode = _serializedGraph.FindProperty("_rootNode");
					_edges = _serializedGraph.FindProperty("_edges");

				}
			}
			

			// if(_grid == null) { DoInit(); }
			DrawBackground(area);

			



			DrawNodes();
			DrawEdges();

			DrawCurrentConnection();

			DrawSidePanel(panelArea);

			

			if(IsDragging(area))
			{
				Event.current.Use();
				_dOffset += Event.current.delta;
				Repaint();
			}

			if(HasRightClicked())
			{
				GetAddContext().ShowAsContext();
				
				_clickPos = Event.current.mousePosition;
				Event.current.Use();
			}
			
		}

		private Vector2 _dOffset = Vector2.zero;
		private EditorGrid _grid = null;


		private Vector2 _clickPos = Vector2.zero;

		private bool _refreshSelected = false;

		private bool _dragging = false;

		private ContextGraph _currentGraph = null;
		private SerializedObject _serializedGraph = null;
		private SerializedProperty _nodes = null;
		private SerializedProperty _edges = null;
		private SerializedProperty _rootNode = null;

		private GUIStyle _centeredLabel = null;

		private string _selectedID = null;
		private SerializedProperty _selectedProp = null;


		
		private static Color GridLineColor { get { return Utils.IsUsingProSkin() ? Color.black : Color.white; } }
		private static Color GridBackgroundColor { get { return Utils.IsUsingProSkin() ?  Color.white : Color.grey; } }
		

		private GUIStyle _nodeStyle = null, _selectedNodeStyle = null, _rootNodeStyle = null, _selectedRootNodeStyle = null;

		private Dictionary<string, Rect> _cachedAreas = new Dictionary<string, Rect>();

		private class Connection { public string to = null, from = null; }

		private Connection _currentConnection = null;


		private void DrawSidePanel(Rect area)
		{
			EditorGUI.DrawRect(area, Color.grey);

			var innerArea = area.Resize(new Vector2(-10f, -10f));

			var fields = innerArea.SliceVerticalMixed(0f, EditorGUIUtility.singleLineHeight, 1f);

			try
			{
				int ti = EditorGUI.indentLevel;
				EditorGUI.indentLevel = 0;
				if(_selectedProp != null)
				{
					var labelProp = _selectedProp.FindPropertyRelative("label");
					EditorGUI.PropertyField(fields[0], labelProp, GUIContent.none);
					_serializedGraph.ApplyModifiedProperties();
				}

				EditorGUI.indentLevel = ti;
			}
			catch{}

			

			

		}

		private void DrawBackground(Rect area)
		{
			_grid.Draw(area);
		}

	

		private void DrawNodes()
		{
			if(_serializedGraph == null) { return; }
			for(int i = 0; i < _nodes.arraySize; i++)
			{
				DrawNode(i);
			}
		}

		private void DrawEdges()
		{
			if(_serializedGraph == null) { return; }
			for(int i = 0; i < _edges.arraySize; i++)
			{
				DrawEdge(i);
			}
		}

		// "flow node {0} on"
		// "flow node {0}"

		private void OnUndo()
		{
			if(_serializedGraph != null)
			{
				_serializedGraph.Update();
				Repaint();
			}
		}

		private void OnEnable()
		{
			_grid = new EditorGrid(GridBackgroundColor);
			var lColor = GridLineColor;
			_grid.AddLayer(10, 0.2f, lColor);
			_grid.AddLayer(100, 0.4f, lColor);
			_grid.offsetHandler = () => _dOffset;
			Undo.undoRedoPerformed += OnUndo;
		}

		private void OnDisable()
		{
			Undo.undoRedoPerformed -= OnUndo;
		}


		private Vector2 _clickOffset = Vector2.zero;

		private void DrawNode(int index)
		{
			var rootNode = _rootNode.stringValue;
			var p = _nodes.GetArrayElementAtIndex(index);
			var type = p.FindPropertyRelative("_type");
			var positionX = p.FindPropertyRelative("position.x");
			var positionY = p.FindPropertyRelative("position.y");
			var nodeID = p.FindPropertyRelative("id");
			var cmd = p.FindPropertyRelative("command");
			var label = p.FindPropertyRelative("label");

			var position = new Vector2(positionX.floatValue, positionY.floatValue);
			var area = new Rect(position + _dOffset, new Vector2(75f, 75f));
			var center = area.center;

			bool isRoot = rootNode == nodeID.stringValue;

			var nodeStyle = isRoot ? _rootNodeStyle : _nodeStyle;
			var selectedNodeStyle = isRoot ? _selectedRootNodeStyle : _selectedNodeStyle;
			
			
			var clickArea = new Rect();

			if((NodeType)type.enumValueIndex == NodeType.Command)
			{
				area = area.SetSize(new Vector2(150f, 50f));

				GUI.Box(area, GUIContent.none, nodeID.stringValue == _selectedID ? selectedNodeStyle : nodeStyle);
				clickArea = area;

				Rect[] fieldRects = area.Resize(new Vector2(-20f, -20f)).SliceVerticalMixed(0f, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight, 1f);
				GUI.Label(fieldRects[0], label.stringValue, _centeredLabel);

				string[] split = cmd.stringValue.Split('/');
				if(GUI.Button(fieldRects[1], new GUIContent("/" + split[split.Length - 1], cmd.stringValue), EditorStyles.popup))
				{
					GetCMDMenu(cmd).ShowAsContext();
				}
			}
			else if((NodeType)type.enumValueIndex == NodeType.Group)
			{
				area = area.SetSize(new Vector2(150f, 50f));
				GUI.Box(area, GUIContent.none, nodeID.stringValue == _selectedID ? selectedNodeStyle : nodeStyle);

				clickArea = area;
				Rect[] fieldRects = area.Resize(new Vector2(-20f, -20f)).SliceVerticalMixed(0f, 1f);
				GUI.Label(fieldRects[0], isRoot ? "<ROOT>" : label.stringValue, _centeredLabel);

			}
			else if((NodeType)type.enumValueIndex == NodeType.Separator)
			{
				area = area.SetSize(new Vector2(150f, 50f));
				GUI.Box(area, GUIContent.none, nodeID.stringValue == _selectedID ? selectedNodeStyle : nodeStyle);

				clickArea = area;

				var innerArea = area.Resize(new Vector2(-20f, 0f));
				var sepRect = innerArea;
				sepRect.height = 3f;
				sepRect.center = innerArea.center;
				EditorGUI.DrawRect(sepRect.Resize(new Vector2(1f, 1f)), Color.black);
				EditorGUI.DrawRect(sepRect, Color.white);
			}

			_cachedAreas[nodeID.stringValue] = clickArea;


			
			var leftPortPos = new Vector2(clickArea.center.x - clickArea.width * 0.5f, clickArea.center.y);
			var rightPortPos = new Vector2(clickArea.center.x + clickArea.width * 0.5f, clickArea.center.y);

			var leftPortArea = leftPortPos.GetRect(7f, 7f);
			var rightPortArea = rightPortPos.GetRect(7f, 7f);

			if(!isRoot)
			{
				EditorGUI.DrawRect(leftPortArea.Resize(new Vector2(1f, 1f)), Color.black);
				EditorGUI.DrawRect(leftPortArea, Color.white);
			}
			

			if((NodeType)type.enumValueIndex == NodeType.Group)
			{
				EditorGUI.DrawRect(rightPortArea.Resize(new Vector2(1f, 1f)), Color.black);
				EditorGUI.DrawRect(rightPortArea, Color.white);
			}

			if(Utils.ClickedRect(0, leftPortArea))
			{

				if(_currentConnection == null)
				{
					_currentConnection = new Connection();
					_currentConnection.from = nodeID.stringValue;
				}
				else
				{
					if(_currentConnection.from == null && _currentConnection.to != null)
					{
						AddEdge(_currentConnection.to, nodeID.stringValue);
						_currentConnection = null;
					}
				}
				Event.current.Use();
			}

			if(Utils.ClickedRect(0, rightPortArea))
			{
				if(_currentConnection == null)
				{
					_currentConnection = new Connection();
					_currentConnection.to = nodeID.stringValue;
				}
				else
				{
					if(_currentConnection.to == null && _currentConnection.from != null)
					{
						AddEdge(nodeID.stringValue, _currentConnection.from);
						_currentConnection = null;
					}
				}
				Event.current.Use();
			}

			


			if(Event.current.type == EventType.MouseUp && Event.current.button == 0)
			{
				_dragging = false;
			}

			if(_currentConnection == null && Event.current.type == EventType.MouseDown && Event.current.button == 1 && clickArea.Contains(Event.current.mousePosition) )
			{
				var m = GetNodeContext(index, p);
				m.ShowAsContext();
				Event.current.Use();
			}

			if(_currentConnection == null && Event.current.type == EventType.MouseDown && Event.current.button == 0 && clickArea.Contains(Event.current.mousePosition))
			{
				_refreshSelected = true;
				_dragging = true;
				_selectedID = nodeID.stringValue;
				_selectedProp = p;

				_clickOffset = Event.current.mousePosition - clickArea.position;
				Event.current.Use();
				Repaint();
			}

			

			if(Event.current.type == EventType.MouseDrag && Event.current.button == 0 && _selectedID == nodeID.stringValue && _dragging)
			{
				Vector2 pos = (Event.current.mousePosition) - _dOffset;
				pos -= _clickOffset;
				pos = pos.Round(10f);
				positionX.floatValue = pos.x;
				positionY.floatValue = pos.y;
				p.serializedObject.ApplyModifiedProperties();
				Event.current.Use();
				Repaint();
			}


		}

		private void AddEdge(string from, string to)
		{
			// add new edge
			_edges.arraySize++;
			var ep = _edges.GetArrayElementAtIndex(_edges.arraySize - 1);
			ep.FindPropertyRelative("from").stringValue = from;
			ep.FindPropertyRelative("to").stringValue = to;
			ep.FindPropertyRelative("muted").boolValue = false;

			_serializedGraph.ApplyModifiedProperties();
			_serializedGraph.Update();
			Repaint();
		}

		private void DrawCurrentConnection()
		{
			if(_currentConnection == null) { return; }

			var a = Event.current.mousePosition;
			
			var r =  _cachedAreas[_currentConnection.from != null ? _currentConnection.from : _currentConnection.to];

			var b = r.center + new Vector2(r.width * 0.5f * ( _currentConnection.from == null ? 1f : -1f), 0f);
			
			Handles.DrawLine(a, b);
			Repaint();

		}


		private void DrawEdge(int index)
		{
			var ep = _edges.GetArrayElementAtIndex(index);

			var from = ep.FindPropertyRelative("from");
			var to = ep.FindPropertyRelative("to");
			var muted = ep.FindPropertyRelative("muted");

			var leftArea = _cachedAreas[from.stringValue];
			var rightArea = _cachedAreas[to.stringValue];

			var a = leftArea.center + new Vector2(leftArea.width * 0.5f, 0f);
			var b = rightArea.center - new Vector2(leftArea.width * 0.5f, 0f);

			Color t = Handles.color;
			Handles.color = muted.boolValue ? Color.grey : Color.white;
			Handles.DrawLine(a, b);
			Handles.color = t;

			var center = (a + b) * 0.5f;

			var area = center.GetRect(10, 10f);
			EditorGUI.DrawRect(area.Resize(new Vector2(2f, 2f)), Color.black);
			EditorGUI.DrawRect(area, Color.white);


			if(Utils.ClickedRect(1, area))
			{
				GenericMenu m = new GenericMenu();
				m.AddItem(new GUIContent("Muted"), muted.boolValue, () => {
					muted.boolValue = !muted.boolValue;
					_serializedGraph.ApplyModifiedProperties();
					_serializedGraph.Update();
					Repaint();
					
				});
				m.AddSeparator("");

				m.AddItem(new GUIContent("Delete"), false, () => DeleteEdge(index));
				m.ShowAsContext();
				Event.current.Use();
			}

		}
		


		private GenericMenu GetCMDMenu(SerializedProperty cmdProp)
		{
			string[] options = Unsupported.GetSubmenus("Assets");
			var m = new GenericMenu();

			foreach(var o in options)
			{
				m.AddItem(new GUIContent(o), false, () => {
					cmdProp.stringValue = o;
					cmdProp.serializedObject.ApplyModifiedProperties();
				});
			}
			return m;
		}

		private GenericMenu GetCMDMenu(GraphNode node)
		{
			string[] options = Unsupported.GetSubmenus("Assets");
			var m = new GenericMenu();

			foreach(var o in options)
			{
				m.AddItem(new GUIContent(o), false, () => {
					node.command = o;
				});
			}
			return m;
		}

		private void AddOption()
		{
			AddNode(NodeType.Command);
		}

		private void AddSeparator()
		{
			AddNode(NodeType.Separator);
		}

		private void AddGroup()
		{
			AddNode(NodeType.Group);
		}

		private void AddNode(NodeType type)
		{
			var position = _clickPos - _dOffset;
			var nodes = _serializedGraph.FindProperty("_nodes");
			nodes.arraySize++;
			var node = nodes.GetArrayElementAtIndex(nodes.arraySize - 1);
			var nType = node.FindPropertyRelative("_type");
			var posX = node.FindPropertyRelative("position.x");
			var posY = node.FindPropertyRelative("position.y");
			var label = node.FindPropertyRelative("label");
			var cmd = node.FindPropertyRelative("command");

			label.stringValue = "cmd";
			cmd.stringValue = null;

			posX.floatValue = position.x;
			posY.floatValue = position.y;

			var id = node.FindPropertyRelative("id");
			id.stringValue = Utils.GetGUID();
			nType.enumValueIndex = (int)type;
			_serializedGraph.ApplyModifiedProperties();
		}

		private GenericMenu GetNodeContext(int index, SerializedProperty nodeProp)
		{
			var m = new GenericMenu();
			var rootNode = _serializedGraph.FindProperty("_rootNode");
			var id = nodeProp.FindPropertyRelative("id");
			var type = nodeProp.FindPropertyRelative("_type");
			if(type.enumValueIndex == (int)NodeType.Group)
			{
				bool isRoot = rootNode.stringValue == id.stringValue;
				m.AddItem(new GUIContent("Root Node"), isRoot, () =>
				{
					if(!isRoot)
					{
						rootNode.stringValue = id.stringValue;
						_serializedGraph.ApplyModifiedProperties();
						_serializedGraph.Update();
						Repaint();
					}
					
				});
				m.AddSeparator("");
			}
			
			m.AddItem(new GUIContent("Delete"), false, () => DeleteNode(index, nodeProp));
			return m;
		}

		private void DeleteEdge(int index)
		{
			_edges.DeleteArrayElementAtIndex(index);
			_serializedGraph.ApplyModifiedProperties();
			_serializedGraph.Update();
			Repaint();
		}

		private void DeleteNode(int index, SerializedProperty nodeProp)
		{
			_selectedProp = null;
			var id = nodeProp.FindPropertyRelative("id").stringValue;
			_nodes.DeleteArrayElementAtIndex(index);

			List<GraphEdge> keepEdges = new List<GraphEdge>(); 

			for(int i = 0; i < _edges.arraySize; i++)
			{
				var ep = _edges.GetArrayElementAtIndex(i);

				var from = ep.FindPropertyRelative("from");
				var to = ep.FindPropertyRelative("to");
				var muted = ep.FindPropertyRelative("muted");
				
				if(id != from.stringValue && id != to.stringValue)
				{
					keepEdges.Add(new GraphEdge
					{
						from = from.stringValue,
						to = to.stringValue,
						muted = muted.boolValue
					});
				}

			}
			_edges.arraySize = keepEdges.Count;
			for(int i = 0; i < keepEdges.Count; i++)
			{

				var ep = _edges.GetArrayElementAtIndex(i);

				var from = ep.FindPropertyRelative("from");
				var to = ep.FindPropertyRelative("to");
				var muted = ep.FindPropertyRelative("muted");

				from.stringValue = keepEdges[i].from;
				to.stringValue = keepEdges[i].to;
				muted.boolValue = keepEdges[i].muted;
				
			}
			_serializedGraph.ApplyModifiedProperties();
			_serializedGraph.Update();
		}


		private GenericMenu GetAddContext()
		{
			var m = new GenericMenu();
			m.AddItem(new GUIContent("Add/Command"), false, AddOption);
			m.AddItem(new GUIContent("Add/Separator"), false, AddSeparator);
			m.AddItem(new GUIContent("Add/Group"), false, AddGroup);
			return m;
		}

		


		[MenuItem("Plugins/Context Grapher/Open", false, 0)]
		private static void Init()
		{
			EditorWindowBase.InitWindow<EditorWindow_ContextGraph>("Context Graph");
		}

		private static bool IsDragging(Rect area)
		{
			return Event.current.type == EventType.MouseDrag && Event.current.button == 2 && area.Contains(Event.current.mousePosition);
		}

		private static bool HasRightClicked()
		{
			return Event.current.type == EventType.MouseDown && Event.current.button == 1;
		}
	}
}

	// private void DrawNodes()
		// {
		// 	foreach(var n in _nodes)
		// 	{
		// 		DrawNode(n);
		// 	}

		// 	if(_refreshSelected && _selectedNode != null)
		// 	{
		// 		_nodes.Remove(_selectedNode);
		// 		_nodes.Add(_selectedNode);
		// 		_refreshSelected = false;
		// 	}
		// }

		// private void DrawNode(GraphNode node)
		// {
		// 	var area = new Rect(node.position + _dOffset, new Vector2(75f, 75f));
		// 	var center = area.center;
			
			
		// 	var clickArea = new Rect();

		// 	if(node.Type == NodeType.Command)
		// 	{
		// 		var styleName = "flow node 0";
		// 		if(node == _selectedNode) { styleName += " on"; }
		// 		GUIStyle s = styleName;

		// 		area.height = 50f;
		// 		area.width = 150f;
		// 		GUI.Box(area, GUIContent.none, s);
		// 		clickArea = area;

		// 		Rect[] fieldRects = area.Resize(new Vector2(-20f, -20f)).SliceVerticalMixed(0f, EditorGUIUtility.singleLineHeight, EditorGUIUtility.singleLineHeight, 1f);
		// 		GUI.Label(fieldRects[0], node.label, EditorStyles.miniBoldLabel);

		// 		string[] split = node.command.Split('/');
		// 		if(GUI.Button(fieldRects[1], new GUIContent("/" + split[split.Length - 1], node.command), EditorStyles.popup))
		// 		{
		// 			GetCMDMenu(node).ShowAsContext();
		// 		}

		// 	}
		// 	else if(node.Type == NodeType.Group)
		// 	{
		// 		var styleName = "flow node 0";
		// 		if(node == _selectedNode) { styleName += " on"; }
		// 		GUIStyle s = styleName;

		// 		area.height = 40f;
		// 		area.width = 100f;
		// 		GUI.Box(area, GUIContent.none, s);
		// 		clickArea = area;

		// 		Rect[] fieldRects = area.Resize(new Vector2(-20f, -20f)).SliceVerticalMixed(0f, EditorGUIUtility.singleLineHeight, 1f);
		// 		GUI.Label(fieldRects[0], node.label, EditorStyles.miniBoldLabel);

		// 	}
		// 	else if(node.Type == NodeType.Separator)
		// 	{
		// 		var styleName = "flow node 0";
		// 		if(node == _selectedNode) { styleName += " on"; }
		// 		GUIStyle s = styleName;
		// 		area.height = 30f;
		// 		area.width = 50f;
		// 		// area.center = center;
		// 		GUI.Box(area, GUIContent.none, s);
		// 		clickArea = area;

		// 		var innerArea = area.Resize(new Vector2(-20f, 0f));
		// 		var sepRect = innerArea;
		// 		sepRect.height = 3f;
		// 		sepRect.center = innerArea.center;
		// 		EditorGUI.DrawRect(sepRect.Resize(new Vector2(1f, 1f)), Color.black);
		// 		EditorGUI.DrawRect(sepRect, Color.white);
		// 	}

		// 	var leftPortPos = new Vector2(clickArea.center.x - clickArea.width * 0.5f, clickArea.center.y);
		// 	var rightPortPos = new Vector2(clickArea.center.x + clickArea.width * 0.5f, clickArea.center.y);

		// 	var leftPortArea = new Rect(0, 0, 7f, 7f);
		// 	leftPortArea.center = leftPortPos;

		// 	var rightPortArea = new Rect(0, 0, 7f, 7f);
		// 	rightPortArea.center = rightPortPos;

		// 	// EditorGUI.DrawRect(leftPortArea, Color.white);

		// 	EditorGUI.DrawRect(leftPortArea.Resize(new Vector2(1f, 1f)), Color.black);
		// 	EditorGUI.DrawRect(leftPortArea, Color.white);

			

		// 	if(node.Type == NodeType.Group)
		// 	{
		// 		// EditorGUI.DrawRect(rightPortArea, Color.white);
		// 		EditorGUI.DrawRect(rightPortArea.Resize(new Vector2(1f, 1f)), Color.black);
		// 		EditorGUI.DrawRect(rightPortArea, Color.white);
		// 	}


		// 	if(Event.current.type == EventType.MouseUp && Event.current.button == 0)
		// 	{
		// 		_dragging = false;
		// 	}

		// 	if(Event.current.type == EventType.MouseDown && Event.current.button == 0 && clickArea.Contains(Event.current.mousePosition))
		// 	{
		// 		_refreshSelected = true;
		// 		_dragging = true;
		// 		_selectedNode = node;

		// 		_clickOffset = Event.current.mousePosition - clickArea.position;
		// 		Event.current.Use();
		// 		Repaint();
		// 	}

			

		// 	if(Event.current.type == EventType.MouseDrag && Event.current.button == 0 && node == _selectedNode && _dragging)
		// 	{
		// 		Vector2 pos = (Event.current.mousePosition) - _dOffset;
		// 		pos -= _clickOffset;
		// 		node.position = pos.Round(10f);
		// 		Event.current.Use();
		// 		Repaint();
		// 	}
		// }