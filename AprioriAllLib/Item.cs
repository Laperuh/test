using System;
using System.Collections.Generic;
using System.Text;

namespace AprioriAllLib
{
	/*!
	 * \addtogroup input
	 * @{
	 */

	/// <summary>
	/// Single item of a transaction.
	/// </summary>
	public class Item : IItem
	{
		/// <summary>
		/// Integer value (id) of the item
		/// </summary>
		//public int Value;
		private int value;

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="value">value of the item</param>
		public Item(int value)
		{
			this.value = value;
		}

		public int CompareTo(object obj)
		{
			return ((Item)obj).value == this.value ? 0 : (((Item)obj).value > this.value ? -1 : 1);
		}

		/// <summary>
		/// String representation of this class
		/// </summary>
		/// <returns>String representation</returns>
		public override string ToString()
		{
			return String.Format("{0}", value);
		}

		public override bool Equals(object obj)
		{
			if (typeof(Item).Equals(obj.GetType()) && ((Item)obj).value == value)
				return true;
			return false;
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public int GetId()
		{
			return value;
		}

		public void SetId(int id)
		{
			value = id;
		}

	}

	/// @}
}
