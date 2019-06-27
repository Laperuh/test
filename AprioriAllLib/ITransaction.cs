using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib
{

	public interface ITransaction
	{

		IEnumerable<IItem> GetItems();

		IItem GetItem(int index);

		int GetItemsCount();

		void AddItem(IItem item);

		void AddItems(IEnumerable<IItem> items);

		//void AddItems(params int[] itemIds);

	}

}
