using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AprioriAllLib
{
	/// <summary>
	/// Class that represents a litemset (1-itemset)
	/// </summary>
	public class Litemset : Transaction, ILitemset
	{

		/// <summary>
		/// Support of this litemset.
		/// </summary>
		public int Support;

		///// <summary>
		///// List of IDs of clients that support this litemset.
		///// </summary>
		//public List<int> IDs;

		///// <summary>
		///// List of items that this litemset contains.
		///// </summary>
		//public List<IItem> Items;

		///// <summary>
		///// Default constructor.
		///// </summary>
		//public Litemset()
		//{
		//}

		/// <summary>
		/// Constructs new large itemset from a provided list of items.
		/// </summary>
		/// <param name="items">list of items</param>
		public Litemset(List<IItem> items)
			: base(items)
		{
			//Items = items;
			//IDs = new List<int>();
		}

		/// <summary>
		/// Vararg constructor.
		/// </summary>
		/// <param name="support">support</param>
		/// <param name="values">values of items</param>
		public Litemset(int support, params int[] values)
			: base()
		{
			if (values == null)
				throw new ArgumentNullException("values", "values is null.");
			Support = support;
			//Items = new List<IItem>();
			foreach (int value in values)
				items.Add(new Item(value));
		}

		//-1 - greater, 1 - smaller, 0 - error(equal)
		public int CompareTo(object obj)
		{
			Litemset litemset = (Litemset)obj;
			if (items.Count > litemset.items.Count)
				return 1;
			else if (items.Count < litemset.items.Count)
				return -1;
			else
			{
				for (int i = 0; i < items.Count; i++)
				{
					if (items[i].CompareTo(litemset.items[i]) == 0) // equal
						continue;
					else
						return items[i].CompareTo(litemset.items[i]);
				}
				return 0;
			}
		}

		/// <summary>
		/// String representation of instance of this class.
		/// </summary>
		/// <returns>string representation</returns>
		public override string ToString()
		{
			string itemsStr = string.Join(",", items.Select(x => x.ToString()).ToArray());
			return String.Format("Lit(Supp={0};{1})", Support, itemsStr);
		}

		public override bool Equals(object obj)
		{
			if (items.Count != ((Litemset)obj).items.Count)
				return false;
			for (int i = 0; i < items.Count; i++)
			{
				if (items[i].GetId() != ((Litemset)obj).items[i].GetId())
					return false;
			}
			return true;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		//public IEnumerable<IItem> GetItems()
		//{
		//	return items;
		//}

		//public IItem GetItem(int index)
		//{
		//	return items[index];
		//}

		//public int GetItemsCount()
		//{
		//	return items.Count;
		//}

		//public void AddItem(IItem item)
		//{
		//	Items.Add(item);
		//}

		//public void AddItems(IEnumerable<IItem> items)
		//{
		//	this.Items.AddRange(items);
		//}

		[Obsolete]
		public void SortItems()
		{
			items.Sort();
		}

		public int GetSupport()
		{
			return Support;
		}

		public void SetSupport(int support)
		{
			this.Support = support;
		}

		public void IncrementSupport()
		{
			Support++;
		}

	}
}
