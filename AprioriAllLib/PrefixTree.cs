using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/*!
 * \defgroup prefixtree Prefix tree
 * \brief It enables the algorithm to work really fast.
 * 
 * Although the implementation would have worked without the use of prefix tree,
 * it would have been terribly slow. And it was, until the PrefixTree class was introduced.
 * It is used in many places in the serialized implementation.
 */

namespace AprioriAllLib
{
	/*!
	 * \addtogroup prefixtree
	 * @{
	 */

	/// <summary>
	/// Simple prefix tree implementation.
	/// </summary>
	public class PrefixTree
	{
		/// <summary>
		/// Maximal width of the tree. Large values greatly increase memory use in case of large trees.
		/// </summary>
		private int width;

		//private int depth;

		/// <summary>
		/// Root of the tree, this node is a starting point in all insertions/searches.
		/// </summary>
		private PrefixTreeNode root;

		/// <summary>
		/// Constructs a new prefix tree with a specified maximal width.
		/// </summary>
		/// <param name="width"></param>
		public PrefixTree(int width/*, int depth*/)
		{
			this.width = width;
			//this.depth = depth;
			root = null;
		}

		public bool TryAdd(List<int> list)
		{
			return TryAdd(list, -1, false);
		}

		public bool TryAdd(List<int> list, int omittedIndex)
		{
			return TryAdd(list, omittedIndex, false);
		}

		/// <summary>
		/// Tries to add a new list to the tree.
		/// </summary>
		/// <param name="list">list of numbers that will be added to prefix tree</param>
		/// <param name="omittedIndex">element of the list at this index is treated as non-existent</param>
		/// <param name="storeOmittedAtLeaf">omitted element is stored in the leaf node of the tree</param>
		/// <returns>true if a given element was not already in the tree</returns>
		public bool TryAdd(List<int> list, int omittedIndex, bool storeOmittedAtLeaf)
		{
			if (root == null)
				root = new PrefixTreeNode(width);
			PrefixTreeNode currentNode = root;
			var listEnum = list.GetEnumerator();
			for (int n = 0; n < list.Count; ++n)
			{
				listEnum.MoveNext();
				if (n == omittedIndex)
					continue;
				int i = listEnum.Current;
				if (currentNode.Get(i) == null)
					currentNode.Set(i, new PrefixTreeNode(width));
				currentNode = currentNode.Get(i);
			}

			if(storeOmittedAtLeaf)
				currentNode.StoreValue(list[omittedIndex]);

			return currentNode.SetEnd();
		}

		public bool Check(List<int> list)
		{
			return Check(list, -1);
		}

		/// <summary>
		/// Checks if a given element can be added to the tree.
		/// </summary>
		/// <param name="list">list of numbers that will be compared with the contents of prefix tree</param>
		/// <param name="omittedIndex">element of the list at this index is treated as non-existent</param>
		/// <returns>true if a given element is not in the tree</returns>
		public bool Check(List<int> list, int omittedIndex)
		{
			if (root == null)
				return true;
			PrefixTreeNode currentNode = root;
			var listEnum = list.GetEnumerator();
			for (int n = 0; n < list.Count; ++n)
			{
				listEnum.MoveNext();
				if (n == omittedIndex)
					continue;
				int i = listEnum.Current;
				if (currentNode.Get(i) == null)
					return true;
				currentNode = currentNode.Get(i);
			}
			return currentNode.CheckEnd();
		}

		public PrefixTreeNode GetNode(List<int> list, int omittedIndex)
		{
			if (root == null)
				return null;
			PrefixTreeNode currentNode = root;
			var listEnum = list.GetEnumerator();
			for (int n = 0; n < list.Count; ++n)
			{
				listEnum.MoveNext();
				if (n == omittedIndex)
					continue;
				int i = listEnum.Current;
				if (currentNode.Get(i) == null)
					return null;
				currentNode = currentNode.Get(i);
			}
			return currentNode;
		}

		//public bool CheckSubPath(List<int> path)
		//{
		//	return CheckSubPath(path, 0);
		//}

		///// <summary>
		///// Checks if a given path exists (at least partially) in the tree.
		///// </summary>
		///// <param name="path"></param>
		///// <param name="maxSkippedNodes">maximum number of nodes that can be skipped over;
		///// each skipped node increases time complexity by width of the tree, 
		///// i.e. for 1 skipped node and path length 5, the checking time is $5(width^1)$
		///// and for 2 skipped nodes, path 7, the time is $7(width^2)$
		///// </param>
		///// <returns></returns>
		//public bool CheckSubPath(List<int> path, int maxSkippedNodes)
		//{
		//	return false;
		//}
	}

	/// @}
}
