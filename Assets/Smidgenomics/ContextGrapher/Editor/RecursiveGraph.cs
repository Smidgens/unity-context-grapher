namespace Smidgenomics.ContextGrapher
{
	using UnityEngine;
	using System.Collections.Generic;
	

	internal class RecursiveNode
	{
		public string id = null, label = null, command = null;
		public NodeType type = NodeType.Command;
		public List<RecursiveNode> children = new List<RecursiveNode>();
		public int visits = 0;
		public float yPos;
	}

}