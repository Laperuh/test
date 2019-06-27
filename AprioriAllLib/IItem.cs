using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AprioriAllLib
{

	public interface IItem : IComparable
	{

		int GetId();

		void SetId(int id);

	}

}
