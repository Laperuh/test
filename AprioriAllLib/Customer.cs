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
	/// Class which represents a customer from database having a list of transactions
	/// </summary>
	public class Customer : ICustomer
	{

		//public string Name;

		/// <summary>
		/// List of transactions
		/// </summary>
		private List<ITransaction> transactions;

		//public List<Transaction> Transactions;

		/// <summary>
		/// Constructor
		/// </summary>
		public Customer()
		{
			transactions = new List<ITransaction>();
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="transactions">Array of transactions of this client</param>
		public Customer(params Transaction[] transactions)
			: this()
		{
			foreach (Transaction t in transactions)
				this.transactions.Add(t);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="valuesArray">Two-dimensional array of integer values of items in transactions of this client</param>
		public Customer(params int[][] valuesArray)
			: this()
		{
			foreach (int[] values in valuesArray)
				transactions.Add(new Transaction(values));
		}

		/// <summary>
		/// Prints the content of client transactions
		/// </summary>
		/// <returns>String of values of transaction items</returns>
		public override string ToString()
		{
			string itemsStr = string.Join(",", transactions.Select(x => x.ToString()).ToArray());
			return String.Format("({0})", itemsStr);
		}

		public override bool Equals(object obj)
		{
			if (obj.GetType().Equals(typeof(Customer)) && transactions.Count == ((Customer)obj).transactions.Count
					&& Enumerable.SequenceEqual(transactions, ((Customer)obj).transactions))
				return true;
			return false;
			//return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public void AddTransaction(Transaction t)
		{
			transactions.Add(t);
		}


		public IEnumerable<ITransaction> GetTransactions()
		{
			return transactions;
		}

		public ITransaction GetTransaction(int index)
		{
			return transactions[index];
		}

		public int GetTransactionsCount()
		{
			return transactions.Count;
		}

		public void AddTransaction(ITransaction transaction)
		{
			transactions.Add(transaction);
		}

		public void AddTransactions(IEnumerable<ITransaction> transactions)
		{
			this.transactions.AddRange(transactions);
		}
	}

	/// @}
}
