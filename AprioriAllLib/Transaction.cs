using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace AprioriAllLib
{
	/*!
	 * \addtogroup input
	 * @{
	 */

	/// <summary>
	/// Single client's transaction having a list of items
	/// </summary>
	public class Transaction : ITransaction
	{

		/// <summary>
		/// A list of items of this transaction.
		/// </summary>
		protected List<IItem> items;

		//public List<Item> Items
		//{
		//	get { return items; }
		//	//set { items = value; }
		//}

		///// <summary>
		///// The result of apriori appplied to Items.
		///// </summary>
		//[Obsolete]
		//public List<List<Item>> FrequentItems;

		/// <summary>
		/// Default constructor.
		/// </summary>
		public Transaction()
		{
			items = new List<IItem>();
			//FrequentItems = new List<List<Item>>();
		}

		/// <summary>
		/// Constructor from vararg list of ints.
		/// </summary>
		/// <param name="values">vararg list of values of items contained in this transaction</param>
		public Transaction(params int[] values)
			: this()
		{
			foreach (int value in values)
				items.Add(new Item(value));
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="listOfItems"></param>
		public Transaction(List<IItem> listOfItems)
			: this()
		{
			items.AddRange(listOfItems);
		}

		//public Transaction(List<IItem> list)
		//	: this()
		//{
		//	foreach (IItem i in list)
		//		items.Add(new Item(i.GetId()));
		//}

		/// <summary>
		/// Prints the content of transaction
		/// </summary>
		/// <returns>String of values of transaction items</returns>
		public override string ToString()
		{
			string itemsStr = string.Join(" ", items.Select(x => x.ToString()).ToArray());
			return String.Format("{{{0}}}", itemsStr);
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType().Equals(typeof(Transaction)) && items.Count == ((Transaction)obj).items.Count
					&& Enumerable.SequenceEqual(items, ((Transaction)obj).items))
				return true;
			return false;
			//return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public void AddItem(int value)
		{
			items.Add(new Item(value));
		}

		public bool Contains(int itemVal)
		{
			return items.Contains(new Item(itemVal));
		}


		public IEnumerable<IItem> GetItems()
		{
			return items;
		}

		public IItem GetItem(int index)
		{
			return items[index];
		}

		public int GetItemsCount()
		{
			return items.Count;
		}

		public void AddItem(IItem item)
		{
			items.Add(item);
		}

		public void AddItems(IEnumerable<IItem> items)
		{
			this.items.AddRange(items);
		}

	}

	/// @}
}
