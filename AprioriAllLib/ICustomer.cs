using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib
{

	public interface ICustomer
	{

		IEnumerable<ITransaction> GetTransactions();

		ITransaction GetTransaction(int index);

		int GetTransactionsCount();

		void AddTransaction(ITransaction transaction);

		void AddTransactions(IEnumerable<ITransaction> transactions);

		//void AddTransactions(params int[][] transactionsWithItemIds);

	}

}
