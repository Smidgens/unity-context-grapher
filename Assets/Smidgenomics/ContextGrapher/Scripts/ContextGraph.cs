#pragma warning disable 0168 // var declared, unused
#pragma warning disable 0219 // var assigned, unused
#pragma warning disable 0414 // private var assigned, unused
#pragma warning disable 0649 // never assigned, unused

namespace Smidgenomics.ContextGrapher
{
	using UnityEngine;

	[CreateAssetMenu(menuName="Smidgenomics/Context Grapher/Graph")]
	public class ContextGraph : ScriptableObject
	{
		public GraphNode[] Nodes { get { return _nodes; } }
		public GraphEdge[] Edges { get { return _edges; } }
		public string RootNode { get { return _rootNode; } }

		[SerializeField] private GraphNode[] _nodes = {};
		[SerializeField] private GraphEdge[] _edges = {};
		[SerializeField] private string _rootNode = null;
		
	}

}