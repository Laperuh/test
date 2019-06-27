using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib
{

	public interface ILitemset : ITransaction, IComparable
	{

		int GetSupport();

		void SetSupport(int support);

		void IncrementSupport();

	}

}
