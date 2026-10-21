using System.Collections.Generic;

namespace FarseerPhysics.Common.Decomposition
{
	internal abstract class Node
	{
		protected Node LeftChild;

		public List<Node> ParentList;

		protected Node RightChild;

		protected Node(Node left, Node right)
		{
			ParentList = new List<Node>();
			LeftChild = left;
			RightChild = right;
			if (left != null)
			{
				left.ParentList.Add(this);
			}
			if (right != null)
			{
				right.ParentList.Add(this);
			}
		}

		public abstract Sink Locate(Edge s);

		public void Replace(Node node)
		{
			foreach (Node parent in node.ParentList)
			{
				if (parent.LeftChild == node)
				{
					parent.LeftChild = this;
				}
				else
				{
					parent.RightChild = this;
				}
			}
			ParentList.AddRange(node.ParentList);
		}
	}
}
