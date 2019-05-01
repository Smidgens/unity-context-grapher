namespace Smidgenomics.ContextGrapher
{
	using UnityEngine;
	using System.Collections.Generic;
	
	public enum NodeType { Command, Separator, Group }


	[System.Serializable]
	public class GraphEdge
	{
		public string from = "node1", to = "node2";
		public bool muted = false;

	}
	

	[System.Serializable]
	public class GraphNode
	{
		// public List<GraphNode> children = new List<GraphNode>();

		public GraphNode(NodeType type)
		{
			_type = type;
		}

		
		public string id = "uniqueid", label = "label", command = "cmd";
		public Vector2 position = Vector2.zero;
		public NodeType Type { get { return _type; } } 

		[SerializeField] private NodeType _type = NodeType.Command; 
	}


}