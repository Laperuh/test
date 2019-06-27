using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib
{
	/*!
	 * \addtogroup prefixtree
	 * @{
	 */

	/// <summary>
	/// Represents a single node of PrefixTree.
	/// </summary>
	public class PrefixTreeNode
	{
		/// <summary>
		/// True if this node is a leaf.
		/// </summary>
		private bool end;

		/// <summary>
		/// Stores sub-nodes of this node.
		/// </summary>
		private PrefixTreeNode[] nodes;

		private List<int> values;

		/// <summary>
		/// Values stored at this node.
		/// </summary>
		public List<int> Values
		{
			get { return values; }
			// set { values = value; }
		}

		/// <summary>
		/// Constructs a new PrefixTree node.
		/// </summary>
		/// <param name="width"></param>
		public PrefixTreeNode(int width)
		{
			end = false;
			nodes = new PrefixTreeNode[width];
			values = null;
			//for (int i = 0; i < width; ++i)
			//	nodes[i] = null;
		}

		/// <summary>
		/// Sets a reference of sub-node at specified index to a new one.
		/// </summary>
		/// <param name="n"></param>
		/// <param name="node"></param>
		public void Set(int n, PrefixTreeNode node)
		{
			nodes[n] = node;
		}

		/// <summary>
		/// Gets a sub-node at a specified index.
		/// </summary>
		/// <param name="n"></param>
		/// <returns></returns>
		public PrefixTreeNode Get(int n)
		{
			return nodes[n];
		}

		public bool CheckEnd()
		{
			if (end)
				return false;
			return true;
		}

		public bool SetEnd()
		{
			if (end)
				return false;
			end = true;
			return true;
		}

		/// <summary>
		/// Stores a value in this node. This value has undefined purpose, and is totally optional.
		/// </summary>
		/// <param name="value">value to be stored</param>
		/// <returns>currently, always returns true</returns>
		public bool StoreValue(int value)
		{
			if (values == null)
				values = new List<int>();
			values.Add(value);
			return true;
		}

	}

	/// @}
}
