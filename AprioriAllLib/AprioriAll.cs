using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Diagnostics;
using OpenCL.Abstract;


namespace AprioriAllLib
{
	/*!
	 * \addtogroup aprioriandall
	 * @{
	 */

	/// <summary>
	///реализация AprioriAll. 
	/// </summary>
	public class AprioriAll : Apriori
	{
        public List<ILitemset> temp = new List<ILitemset>();
        List<double> supp1 = new List<double>();
        /// <summary>
        /// Создает новый экземпляр AprioriAll.
        /// </summary>
        /// <param name="customerList">список клиентов, у которых есть транзакции с товарами</param>
        [Obsolete]
		public AprioriAll(CustomerList customerList)
			: base(customerList)
		{
		}

		public AprioriAll(IEnumerable<ICustomer> customers)
			: base(customers)
		{
		}

        /// <summary>
        /// Создает словари кодирования и декодирования для заданного набора 1-элементной последовательности.
        /// </summary>
        /// <param name="oneLitemsets">набор 1-элементной последовательности</param>
        /// <param name="encoding">словарь для кодирования</param>
        /// <param name="decoding">расшифровка словаря</param>
        protected void GenerateEncoding(List<ILitemset> oneLitemsets, out Dictionary<ILitemset, int> encoding,
			out Dictionary<int, ILitemset> decoding)
		{
			encoding = new Dictionary<ILitemset, int>();
			decoding = new Dictionary<int, ILitemset>();

			int i = 1;
			foreach (ILitemset li in oneLitemsets)
			{
				encoding.Add(li, i);
				decoding.Add(i, li);
				++i;
			}

        }

        /// <summary>
        /// Создает словарь, который запоминает для каждого l-набора х
        /// 
        /// Например:
        /// если {milk} => 1, {water} => 2, {milk, water} => 3, {milk, water, juice} => 4
        /// тогда 1 содержится в {3, 4}, 2 содержится в {3, 4}, 3 содержится в {4}
        /// </summary>
        /// <param name="oneLitemsets">1-элементная последовательность</param>
        /// <param name="encoding">словарь для кодирования </param>
        /// <returns></returns>
        protected Dictionary<int, List<int>> GenerateContainmentRules(List<ILitemset> oneLitemsets,
			Dictionary<ILitemset, int> encoding)
		{
			Dictionary<int, List<int>> litemsetsContaining = new Dictionary<int, List<int>>();

			foreach (Litemset l in oneLitemsets)
			{
				foreach (Litemset other in oneLitemsets)
				{
					if (ReferenceEquals(other, l))
						continue;

					if (l.GetItems().All(item => other.GetItems().Contains(item)))
					{
						int encoded = encoding[l];
						if (!litemsetsContaining.ContainsKey(encoded))
							litemsetsContaining[encoded] = new List<int>();
						int otherEncoded = encoding[other];
						if (!litemsetsContaining[encoded].Contains(otherEncoded))
							litemsetsContaining[encoded].Add(otherEncoded);
					}
				}
			}

			return litemsetsContaining;
		}

        /// <summary>
        /// 3 шаг трансформация
        /// </summary>
        /// <param name="oneLitemsets">1-элементная последовательность</param>
        /// <param name="encoding">словарь для кодирования</param>
        /// <returns>encoded customer list</returns>
        protected List<List<List<int>>> EncodeCustomerList(List<ILitemset> oneLitemsets, Dictionary<ILitemset, int> encoding)
		{
			var encodedList = new List<List<List<int>>>();

			foreach (ICustomer c in customerList)
			{
				var encodedCustomer = new List<List<int>>();

				foreach (ITransaction t in c.GetTransactions())
				{
					var encodedTransaction = new List<int>();
					int id = -1; 

					// добавить наборы длины >= 2
					foreach (Litemset li in oneLitemsets)
					{
						if (li.GetItemsCount() == 1)
							continue;
						bool someMissing = false;
						foreach (IItem litem in li.GetItems())
						{
							if (!t.GetItems().Contains(litem))
							{
								someMissing = true;
								break;
							}
						}

						if (!someMissing)
						{
							if (encoding.TryGetValue(li, out id))
								encodedTransaction.Add(id);
						}

					}

					foreach (IItem i in t.GetItems())
					{

                        // добавить наборы длины== 1
                        foreach (Litemset li in oneLitemsets)
						{
							if (li.GetItemsCount() > 1)
								continue;
							IItem item = li.GetItem(0);

							if (item.Equals(i) && encoding.TryGetValue(li, out id))
								encodedTransaction.Insert(0, id);
						}

					}

					if (encodedTransaction.Count > 0)
					{
						encodedTransaction.Sort();
						encodedCustomer.Add(encodedTransaction);
					}
				}

				if (encodedCustomer.Count > 0)
					encodedList.Add(encodedCustomer);
			}

			return encodedList;
		}

        /// <summary>
        /// Создает всех кандидатов для k-последовательностей, основываясь на множестве (k-1) -последовательностей.
        /// </summary>
        /// <param name="prev">предыдущие k-последовательности, т.е. (k-1) -последовательности</param>
        /// <param name="litemsetCount">количество отдельных l-наборов</param>
        /// <param name="progressOutput">если true, информация о прогрессе отправляется на стандартный вывод</param>
        /// <returns>кандидаты для k-последовательности</returns>
        protected Dictionary<List<int>, int> GenerateCandidates(List<List<int>> prev, int litemsetCount, bool progressOutput)
		{
			const int progressOutputFactor = 1000000;
			const char progressOutputLetter = 'M';

			var candidates = new Dictionary<List<int>, int>();

			int prevCount = prev.Count;
			if (prevCount == 0)
				return candidates;
			int prevLen = prev[0].Count;

			Stopwatch generationWatch = new Stopwatch();

			generationWatch.Start();
			{
				PrefixTree withoutFirst = new PrefixTree(litemsetCount + 1);
				foreach (List<int> prevList in prev)
					withoutFirst.TryAdd(prevList, 0, true);

				foreach (List<int> prevList in prev)
				{
					PrefixTreeNode node = withoutFirst.GetNode(prevList, prevLen - 1);
					if (node != null)
					{
						foreach (int value in node.Values)
						{
							if (prevLen == 1 && value == prevList[0])
								continue;
							List<int> newCandidate = new List<int>(prevList);
							newCandidate.Insert(0, value);
							candidates.Add(newCandidate, 0);

							if (progressOutput)
								if (candidates.Count > 0 && candidates.Count % progressOutputFactor == 0)
									Log.Write(" {0}{1}...", candidates.Count / progressOutputFactor, progressOutputLetter);
						}
					}
				}
			}
			generationWatch.Stop();

			#region old stuff
			//generationWatch.Start();
			//var enum1 = prev.GetEnumerator();
			//for (int i1 = 0; i1 < prevCount; ++i1)
			//{
			//	enum1.MoveNext();
			//	List<int> l1 = enum1.Current; //prev[i1];

			//	var enum2 = prev.GetEnumerator();
			//	for (int i2 = 0; i2 < prevCount; ++i2)
			//	{
			//		enum2.MoveNext();
			//		if (i1 == i2)
			//			continue;
			//		List<int> l2 = enum2.Current; //prev[i2];

			//		// check if last n-1 elements of first list sequence
			//		//  are equal to the first n-1 elements of the 2nd list
			//		bool partEqual = true;
			//		//var l1enum = l1.GetEnumerator();
			//		//var l2enum = l2.GetEnumerator();
			//		//l1enum.MoveNext();
			//		for (int i = 0; i < prevLen - 1; ++i)
			//		{
			//			//l1enum.MoveNext();
			//			//l2enum.MoveNext();
			//			if (!l1[i + 1].Equals(l2[i]))
			//			//if (!l1enum.Current.Equals(l2enum.Current))
			//			{
			//				partEqual = false;
			//				break;
			//			}
			//		}
			//		if (!partEqual)
			//			continue;

			//		// join l1 and l2
			//		List<int> candidate = new List<int>(l1);
			//		candidate.Add(l2[prevLen - 1]);

			//		candidates.Add(candidate, 0);
			//		if (progressOutput)
			//			if (candidates.Count > 0 && candidates.Count % 50000 == 0)
			//				Log.WriteLine(String.Format("   {0} and counting...", candidates.Count));
			//	}
			//}
			//generationWatch.Stop(); 
			#endregion

			if (progressOutput)
			{
				if (candidates.Count > 100000)
					Log.Write(" найдено {0} кандидатов,", candidates.Count);
				else if (candidates.Count == 0 || prevLen == 1)
					Log.WriteLine(" найдено {0} кандидатов.", candidates.Count);
				else
					Log.WriteLine(" найдено {0} кандидатов,", candidates.Count);
			}

			if (candidates.Count == 0 || prevLen == 1)
				return candidates;

			PrefixTree radixTree = new PrefixTree(litemsetCount + 1); // id с 1 

			Stopwatch sw3 = new Stopwatch();
			sw3.Start();

			foreach (List<int> onePrev in prev)
				if (!radixTree.TryAdd(onePrev))
					throw new ArgumentException("найденные дубликаты в предыдущих k-последовательностях", "предыдущая");

			sw3.Stop();

			Stopwatch sw2 = new Stopwatch();
			sw2.Start();

            // создаем список кандидатов, которые не получили всех своих подпоследовательностей
            // в наборе предыдущих k-последовательностей
            Dictionary<List<int>, int>.KeyCollection keys = candidates.Keys;
			var keysEnum = keys.GetEnumerator();
			List<List<int>> keysToRemove = new List<List<int>>();
			for (int ic = keys.Count - 1; ic >= 0; --ic)
			{
				keysEnum.MoveNext();
				List<int> currentList = keysEnum.Current;
				bool invalidCandidate = false;
				for (int not = prevLen; not >= 0; --not)
				{
					if (!radixTree.Check(currentList, not))
						continue; //если он не может быть добавлен, то он есть в дереве

                    invalidCandidate = true;
					break;
				}
				if (invalidCandidate)
					keysToRemove.Add(currentList);
				if (progressOutput)
					if (ic > 0 && ic % progressOutputFactor == 0)
					{
						Log.Write(" {0}{1}...", ic / progressOutputFactor, progressOutputLetter);
					}
			}

			sw2.Stop();

            //удалить неудачных кандидатов
            foreach (List<int> key in keysToRemove)
				candidates.Remove(key);

			if (progressOutput)
			{
				Log.WriteLine(" {0} valid, previous sequences did not contain {1}.",
					candidates.Count, keysToRemove.Count);
				Log.WriteLine(" generation: {0}ms, prev-to-tree: {1}ms, containment check: {2}ms",
					generationWatch.ElapsedMilliseconds, sw3.ElapsedMilliseconds, sw2.ElapsedMilliseconds);
			}

			return candidates;
		}

        /// <summary>
        /// пытается сопоставить k-последовательность кандидата с закодированным клиентом и возвращает true, если
        /// у клиента есть кандидат. Это означает, что конфигурация транзакций клиента
        /// таков, что он поддерживает последовательность-кандидат.
        /// </summary>
        /// <param name="candidate">последовательность k-кандидатов</param>
        /// <param name="encodedCustomer">кодированный клиент, то есть последовательность наборов элементов</param>
        /// <param name="containmentRules">rules of inclusion between encoded litemsets</param>
        /// <returns>true if the customer supports the candidate</returns>
        protected bool MatchCandidateToEncodedCustomer(List<int> candidate, List<List<int>> encodedCustomer,
			Dictionary<int, List<int>> containmentRules)
		{
			//if (encodedCustomer.Count < candidate.Count)
			//   return false; // the candidate is too long to possibly match the customer

			List<int> temporaryContainingList = null;
			bool allNeededItemsArePresent = true;
			int prevFoundIndex = 0/*-1*/;
			List<int> usedLitemsetsFromCurrentCandidate = new List<int>();
			for (int j = 0; j < candidate.Count; ++j)
			{
				int jthCandidateElem = candidate[j];
				bool jthCandidateFound = false;
				for (int i = prevFoundIndex /*+ 1*/ /*omitted*/; i </*=*/ encodedCustomer.Count /*- (candidate.Count - j)*/; ++i)
				{
					List<int> ithTransaction = encodedCustomer[i];
					// try to match j-th element of candidate
					//  with i-th transaction of the current customer
					bool foundJthItemInIthTransaction = false;
					for (int k = 0; k < ithTransaction.Count; ++k)
					{
						int kthLitemsetOfIthTransaction = ithTransaction[k];

						if (usedLitemsetsFromCurrentCandidate.Contains(kthLitemsetOfIthTransaction))
							continue;

						if (kthLitemsetOfIthTransaction == jthCandidateElem)
						{
							foundJthItemInIthTransaction = true;
							prevFoundIndex = i;
							usedLitemsetsFromCurrentCandidate.Add(kthLitemsetOfIthTransaction);
							jthCandidateFound = true;
							if (containmentRules != null)
							{
								if (containmentRules.TryGetValue(jthCandidateElem, out temporaryContainingList))
								{
									foreach (int containingLitemset in temporaryContainingList)
									{
										//int index = ithTransaction.FindIndex(x => x == containingLitemset);
										//if (index > 0)
										usedLitemsetsFromCurrentCandidate.Add(containingLitemset);
									}
								}
							}
							break;
						}
					}
					if (foundJthItemInIthTransaction)
						break;
					//if (ithTransaction.Contains(jthCandidateElem))
					//{
					//	prevFoundIndex = i;
					//	jthCandidateFound = true;
					//	break;
					//}
					usedLitemsetsFromCurrentCandidate.Clear();
				}
				if (!jthCandidateFound)
				{
					allNeededItemsArePresent = false;
					break; //return false;
				}
			}
			if (allNeededItemsArePresent)
				return true;
			//}
			return false;
		}

		/// <summary>
		/// Corresponds to 4th step of AprioriAll algorithm, namely "Sequence Phase".
		/// </summary>
		/// <param name="oneLitemsets">1-sequences</param>
		/// <param name="encoding">encoding dictionary</param>
		/// <param name="encodedList">encoded list of customers</param>
		/// <param name="containmentRules">rules of inclusion between encoded litemsets</param>
		/// <param name="minSupport">minimum number of occurances</param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		/// <returns>list of k-sequences, partitioned by k. i.e. i-th element of resulting List 
		/// contains all i-sequences</returns>
		protected List<List<List<int>>> FindAllFrequentSequences(List<ILitemset> oneLitemsets,
			Dictionary<ILitemset, int> encoding, List<List<List<int>>> encodedList,
			Dictionary<int, List<int>> containmentRules, int minSupport, bool progressOutput)
		{
			var kSequences = new List<List<List<int>>>();

			kSequences.Add(new List<List<int>>()); // placeholder for 0-sequences (whatever it means)
			kSequences.Add(new List<List<int>>()); // 1-seq, already done, just copy:
			foreach (Litemset li in oneLitemsets)
			{
				var lst = new List<int>();
				lst.Add(encoding[li]);
				kSequences[1].Add(lst);
			}

			Stopwatch matchingWatch = new Stopwatch();

			for (int k = 2; kSequences.Count >= k && kSequences[k - 1].Count > 0; ++k)
			{
				if (progressOutput)
					Log.Write("Looking for {0}-sequences...", k);
				// list of kSequences, initially empty
				kSequences.Add(new List<List<int>>());
				var prev = kSequences[k - 1];

				// generate candidates
				Dictionary<List<int>, int> candidates = GenerateCandidates(prev, oneLitemsets.Count, progressOutput);

				// calculate support of each candidate by analyzing the whole encoded input

				matchingWatch.Restart();
				Dictionary<List<int>, int>.KeyCollection keysOrig = candidates.Keys;
				List<List<int>> keys = new List<List<int>>(keysOrig);
				for (int n = 0; n < keys.Count; ++n)
				{
					List<int> candidate = keys[n]; // n-th candidate

					// check every customer for compatibility with the current candidate
					foreach (List<List<int>> encodedCustomer in encodedList)
					{
						//if (candidate.Count == 2
						//	&& candidate.SequenceEqual(new int[] { 5, 1 })
						//	&& encodedCustomer.Count == 2
						//	)
						//	n = n;

						if (MatchCandidateToEncodedCustomer(candidate, encodedCustomer, containmentRules))
						{
							candidates[candidate] += 1;
						}

						//bool allNeededItemsArePresent = true;
						//foreach (int candidateItem in candidate) {
						//   // check all transactions, item must exist in any of them
						//   bool foundCandidateItem = false;
						//   foreach (List<int> encodedTransaction in encodedCustomer) {
						//      //if (encodedTransaction.Count < k)
						//      //	continue;
						//      foundCandidateItem = encodedTransaction.Contains(candidateItem);
						//      if (foundCandidateItem)
						//         break;
						//   }
						//   // if item does not exist in any of the transactions, this customer
						//   // is not compatible with 'candidate' sequence
						//   if (!foundCandidateItem) {
						//      allNeededItemsArePresent = false;
						//      break;
						//   }
						//}
						//if (allNeededItemsArePresent) {
						//   candidates[candidate] += 1;
						//}
					}

                    // confront results with min. support
                    if (candidates[candidate] >= minSupport)
                    {
                        kSequences[k].Add(candidate);
                    }

				}
				matchingWatch.Stop();
				if (progressOutput)
					Log.WriteLine(" Found {0} sequences that have sufficient support, in {1}ms.",
						kSequences[k].Count, matchingWatch.ElapsedMilliseconds);
			}

			return kSequences;
		}

		protected bool PurgeUsingInclusionRules(List<List<List<int>>> kSequences, Dictionary<int, List<int>> containmentRules,
			int litemsetCount, bool progressOutput)
		{
			if (kSequences == null || kSequences.Count == 0)
				return false;
			int initialK = kSequences.Count - 1
				- 1 // additional "-1" because all largest k-sequences are for sure maximal
				- 1; // additional "-1" because the last entry in the list is empty
			if (kSequences[kSequences.Count - 1].Count > 0)
				throw new ArgumentException("last entry of kSequences is supposed to be empty", "kSequences");

			Stopwatch watchForEachK = new Stopwatch();
			bool somethingChanged = false;
			int totalRemoved = 0;

			for (int k = 1; k <= initialK; ++k)
			{
				watchForEachK.Restart();
				List<List<int>> sequencesOfLengthK = kSequences[k];
				if (sequencesOfLengthK == null || sequencesOfLengthK.Count == 0)
					continue;

				// build the tree of longer sequences, i.e. (k+1)-sequences
				PrefixTree treeOfLongerSequences = new PrefixTree(litemsetCount + 1);
				int nextK = k + 1;
				foreach (List<int> longerSequence in kSequences[nextK])
					for (int omitted = 0; omitted < nextK; ++omitted)
						treeOfLongerSequences.TryAdd(longerSequence, omitted);

				// confront the tree with k-sequences
				for (int n = sequencesOfLengthK.Count - 1; n >= 0; --n)
				{
					List<int> sequence = sequencesOfLengthK[n];
					if (!treeOfLongerSequences.Check(sequence))
					{
						sequencesOfLengthK.RemoveAt(n);
						somethingChanged = true;
						++totalRemoved;
					}
				}
				watchForEachK.Stop();

				//older & slower code, without prefix tree:

				//watchForEachK.Restart();
				//List<List<int>> sequencesOfLengthK = kSequences[k];
				//if (sequencesOfLengthK == null || sequencesOfLengthK.Count == 0)
				//	continue;

				//for (int n = sequencesOfLengthK.Count - 1; n >= 0; --n)
				//{
				//	// we analyze n-th k-sequence:
				//	List<int> sequence = sequencesOfLengthK[n];
				//	// compare it to every k+1-sequence:
				//	foreach (List<int> longerSequence in kSequences[k + 1])
				//		if (IsSubSequence(sequence, longerSequence/*, containmentRules*/))
				//		{
				//			// 'sequence' is a sub-seqence of 'longerSequence'
				//			sequencesOfLengthK.RemoveAt(n);
				//			somethingChanged = true;
				//			++totalRemoved;
				//			break;
				//		}
				//}
				//watchForEachK.Stop();

				if (progressOutput)
					Log.Write(" k={0}: {1}ms,", k, watchForEachK.ElapsedMilliseconds);
			}
			if (progressOutput)
				Log.Write(" removed {0} non-maximal in total", totalRemoved);

			//for (int k = initialK; k >= 0; --k)
			//{
			//	List<List<int>> sequencesOfLengthK = kSequences[k];
			//	if (sequencesOfLengthK == null || sequencesOfLengthK.Count == 0)
			//		continue;
			//	// we need a flag not to fall behind allowed memebers
			//	bool removedAny = false;
			//	for (int n = sequencesOfLengthK.Count - 1; n >= 0; --n)
			//	{
			//		// we analyze n-th k-sequence:
			//		List<int> sequence = sequencesOfLengthK[n];
			//		for (int i = k + 1; i < kSequences.Count; ++i)
			//		{
			//			foreach (List<int> longerSequence in kSequences[i])
			//			{
			//				//if (n < 0)
			//				//	break;
			//				//if (i == 5
			//				//	&& sequence.Count == 1
			//				//	//&& sequence.Contains(4) && sequence.Contains(8) 
			//				//	&& sequence.Contains(5)
			//				//	//&& !sequence.Contains(17)
			//				//	&& longerSequence.Count == 2
			//				//	&& longerSequence.Contains(1) && longerSequence.Contains(5)
			//				//	//&& longerSequence.Contains(9)
			//				//	//&& longerSequence.Contains(7) && longerSequence.Contains(17)
			//				//	)
			//				//	i = i;
			//				if (IsSubSequence(sequence, longerSequence, containmentRules))
			//				{
			//					// if 'sequence' is a sub-seqence of 'longerSequence'
			//					PurgeAllSubSeqsOf(kSequences, k, n);
			//					sequencesOfLengthK.RemoveAt(n);
			//					--n;
			//					removedAny = true;
			//					break;
			//				}
			//			}
			//			if (removedAny)
			//				break;
			//		}
			//		if (removedAny)
			//		{
			//			somethingChanged = true;
			//			++n;
			//			removedAny = false;
			//		}
			//	}
			//}
			return somethingChanged;
		}

		protected bool PurgeUsingInclusionRulesWithinSameSize(List<List<List<int>>> kSequences,
			Dictionary<int, List<int>> containmentRules)
		{
			if (kSequences == null || kSequences.Count == 0)
				return false;
			int initialK = kSequences.Count - 1
				- 1 // additional "-1" because all largest k-sequences are for sure maximal
				- 1; // additional "-1" because the last entry in the list is empty
			if (kSequences[kSequences.Count - 1].Count > 0)
				throw new ArgumentException("last entry of kSequences is supposed to be empty", "kSequences");

			bool somethingChanged = false;

			for (int k = initialK; k >= 0; --k)
			{
				List<List<int>> sequencesOfLengthK = kSequences[k];
				if (sequencesOfLengthK == null || sequencesOfLengthK.Count == 0)
					continue;

				for (int n1 = sequencesOfLengthK.Count - 1; n1 >= 0; --n1)
				{
					var sequence = sequencesOfLengthK[n1];
					for (int n2 = n1 - 1; n2 >= 0; --n2)
					{
						var maybeSubsequence = sequencesOfLengthK[n2];
						if (IsSubSequence(maybeSubsequence, sequence, containmentRules))
						{
							sequencesOfLengthK.RemoveAt(n2);
							--n1;
							somethingChanged = true;
						}
					}
				}
			}

			return somethingChanged;
		}

		protected bool PurgeUsingSequenceInnerRedundancy(List<List<List<int>>> kSequences,
			Dictionary<int, List<int>> containmentRules)
		{
			bool somethingChanged = false;
			// remove elements contained in other elements of the same maximal sequence
			foreach (List<List<int>> seqs in kSequences)
			{
				foreach (List<int> seq in seqs)
				{
					for (int n = seq.Count - 1; n >= 0; --n)
					{
						var currentLitem = seq[n];
						if (!containmentRules.ContainsKey(currentLitem))
							continue;

						var litemsContainingCurrentLitem = containmentRules[currentLitem];
						if (n > 0 && litemsContainingCurrentLitem.Contains(seq[n - 1]))
						{
							// current element is contained in the earlier element
							seq.RemoveAt(n);
							if (n <= seq.Count - 1)
								++n;
							somethingChanged = true;
						}
						else if (n < seq.Count - 1 && litemsContainingCurrentLitem.Contains(seq[n + 1]))
						{
							// current element is contained in the later element
							seq.RemoveAt(n);
							++n;
							somethingChanged = true;
						}
					}
				}
			}

			if (!somethingChanged)
				return false;

			// rearrange sequences to reflect recent changes
			// at the same time removes the duplicate sequences created by removal process
			for (int k = kSequences.Count - 2; k >= 0; --k)
			{
				List<List<int>> sequencesOfLengthK = kSequences[k];
				for (int n = sequencesOfLengthK.Count - 1; n >= 0; --n)
				{
					List<int> sequence = sequencesOfLengthK[n];
					int sequenceCount = sequence.Count;
					if (sequenceCount < k)
					{
						bool alreadyExists = false;
						foreach (List<int> existingSequence in kSequences[sequenceCount])
							if (IsSubSequence(sequence, existingSequence))
							{
								alreadyExists = true;
								break;
							}
						if (!alreadyExists)
							kSequences[sequenceCount].Add(sequence);
						sequencesOfLengthK.RemoveAt(n);
					}
				}
			}

			return true;

		}

		/// <summary>
		/// Deletes all non-maximal seqences from list of k-sequences.
		/// 
		/// Corresponds to 5th step of AprioriAll algorithm, namely "Maximal Phase".
		/// </summary>
		/// <param name="kSequences">list of all k-sequences, partitioned by k</param>
		/// <param name="containmentRules">rules of inclusion between encoded litemsets</param>
		/// <param name="litemsetCount">number of distinct litemsets</param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		protected void PurgeAllNonMax(List<List<List<int>>> kSequences, Dictionary<int, List<int>> containmentRules,
			int litemsetCount, bool progressOutput)
		{
			if (kSequences == null || kSequences.Count == 0)
				return;

			Stopwatch purgingStopwatch = new Stopwatch();

			bool shouldKeepRunning = true;
			while (shouldKeepRunning)
			{
				if (progressOutput)
					Log.Write(" started new run,");
				shouldKeepRunning = false;

				purgingStopwatch.Restart();
				if (PurgeUsingInclusionRules(kSequences, containmentRules, litemsetCount, progressOutput))
					shouldKeepRunning = true;
				purgingStopwatch.Stop();

				if (progressOutput)
					Log.Write("\n inclusion of smaller: {0}ms,", purgingStopwatch.ElapsedMilliseconds);

				purgingStopwatch.Restart();
				if (PurgeUsingSequenceInnerRedundancy(kSequences, containmentRules))
					shouldKeepRunning = true;
				purgingStopwatch.Stop();

				if (progressOutput)
					Log.Write(" inner redundancy: {0}ms,", purgingStopwatch.ElapsedMilliseconds);

				purgingStopwatch.Restart();
				if (PurgeUsingInclusionRulesWithinSameSize(kSequences, containmentRules))
					shouldKeepRunning = true;
				purgingStopwatch.Stop();

				if (progressOutput)
					Log.WriteLine(" same size: {0}ms", purgingStopwatch.ElapsedMilliseconds);
			}
		}

		/// <summary>
		/// Checks if one sequence is a subsequence of the other.
		/// </summary>
		/// <typeparam name="T">type of element of both lists</typeparam>
		/// <param name="hyptheticalSubSequence">supposed sub-sequence</param>
		/// <param name="sequence">suppoed super-sequence</param>
		/// <returns>true if 1st parameter is a subsequence of the 2nd</returns>
		protected bool IsSubSequence<T>(List<T> hyptheticalSubSequence, List<T> sequence)
			where T : IComparable
		{
			return IsSubSequence<T>(hyptheticalSubSequence, sequence, null);
		}

		protected bool IsSubSequence<T>(List<T> hyptheticalSubSequence, List<T> sequence,
			Dictionary<T, List<T>> containmentRules)
			where T : IComparable
		{
			if (hyptheticalSubSequence.Count == 0)
				return true;
			if (hyptheticalSubSequence.Count > sequence.Count)
				return false;

			// below code assumes that both lists are sorted, but it shouldn't!
			//int i1 = 0;
			//int i2 = 0;
			//while (true) {
			//   if (i1 >= hyptheticalSubSequence.Count)
			//      break;
			//   else if (i2 >= sequence.Count)
			//      return false;

			//   if (hyptheticalSubSequence[i1].Equals(sequence[i2])) {
			//      // in the next move we check next element
			//      ++i1;
			//   } else if (hyptheticalSubSequence[i1].CompareTo(sequence[i2]) < 0) {
			//      // since both lists are sorted, we cannot encounter elem i1 anywhere later, 
			//      //  because we know that all other further elements i2 are also larger
			//      return false;
			//   }

			//   ++i2;
			//}

			bool found;
			int found_index = -1;
			bool previousWasContainment = false;
			bool sequenceContainsElementOfSubsequence = false;
			List<T>.Enumerator sequenceEnumerator = sequence.GetEnumerator();
			int subsequenceIndex = -1;
			foreach (T elementOfHypotheticalSubsequence in hyptheticalSubSequence)
			{
				++subsequenceIndex;
				found = false;
				//int i = found_index + 1;
				int nextDiff = previousWasContainment ? 0 : 1;
				for (int i = found_index + nextDiff;
					((sequenceContainsElementOfSubsequence && previousWasContainment) ? true : sequenceEnumerator.MoveNext())
					/*&& i < sequence.Count*/; ++i)
				{
					T elementOfSequence = sequenceEnumerator.Current;
					//sequenceEnumerator.MoveNext();
					//T elementOfSequence = sequence[i];
					sequenceContainsElementOfSubsequence = false;

					if (elementOfHypotheticalSubsequence.Equals(elementOfSequence))
					{
						sequenceContainsElementOfSubsequence = true;
						previousWasContainment = false;
					}

					if (containmentRules != null && containmentRules.ContainsKey(elementOfHypotheticalSubsequence)
						&& containmentRules[elementOfHypotheticalSubsequence].Contains(elementOfSequence))
					{
						sequenceContainsElementOfSubsequence = true;
						previousWasContainment = true;
					}

					if (sequenceContainsElementOfSubsequence)
					{
						found = true;
						found_index = i;
						break;
					}
				}
				if (!found)
				{
					if (previousWasContainment && subsequenceIndex < hyptheticalSubSequence.Count - 1)
						previousWasContainment = false;
					else
						return false;
				}
			}

			return true;

		}

		/// <summary>
		/// Deletes all subsequences of ii-th kk-sequence from the list of k-sequences.
		/// </summary>
		/// <param name="kSequences">list of k-sequences partitioned by k</param>
		/// <param name="kk">corresponds to k in k-sequence</param>
		/// <param name="ii">index of sequence in the list of kk-sequences</param>
		protected void PurgeAllSubSeqsOf(List<List<List<int>>> kSequences, int kk, int ii)
		{
			//if (ii < 0)
			//	throw new ArgumentException(String.Format("kSequences.Count={2}, kk={1} ii={0}",
			//		ii, kk, kSequences.Count), "ii");
			if (kk <= 1)
				return;
			List<int> sequence = kSequences[kk][ii];
			for (int k = kk - 1; k >= 0; --k)
			{

				List<List<int>> sequencesOfLengthK = kSequences[k];

				for (int i = 0; i < sequencesOfLengthK.Count; ++i)
				{
					if (IsSubSequence(sequencesOfLengthK[i], sequence))
					{
						PurgeAllSubSeqsOf(kSequences, k, i);
						sequencesOfLengthK.RemoveAt(i);
					}
				}

			}
		}

		/// <summary>
		/// Concatenates all lists of k-sequences into a single list. Does nothing else.
		/// </summary>
		/// <param name="kSequences">k-sequences, partitioned with respect to k</param>
		/// <returns>not partitioned k-sequences list</returns>
		protected List<List<int>> CompactSequencesList(List<List<List<int>>> kSequences)
		{
			var compacted = new List<List<int>>();

			//for (int k = kSequences.Count - 1; k >= 0; --k) {
			for (int k = 0; k < kSequences.Count; ++k)
			{
				//for (int n = kSequences[k].Count - 1; n >= 0; --n) {
				for (int n = 0; n < kSequences[k].Count; ++n)
				{
					if (kSequences[k][n].Count > 0)
						//kSequences[n].RemoveAt(k);
						compacted.Add(kSequences[k][n]);
				}

				//if (kSequences[k].Count == 0)
				//   kSequences.RemoveAt(k);
			}

			return compacted;
		}

		/// <summary>
		/// Prepares cleaned-up and user-ready data that can be used as final output of AprioriAll algoritm.
		/// </summary>
		/// <param name="encodedList">encoded input</param>
		/// <param name="kSequences">all found maximal k-sequences, partitioned by k</param>
		/// <param name="decoding">decoding dictionary</param>
		/// <returns>cleaned-up data that can be used as final output of AprioriAll</returns>
		protected List<ICustomer> InferRealResults(List<List<List<int>>> encodedList, List<List<List<int>>> kSequences,
			Dictionary<int, ILitemset> decoding)
		{
			var decodedList = new List<ICustomer>();

			var compactSequences = CompactSequencesList(kSequences);

			foreach (List<int> sequence in compactSequences)
			{
				Customer c = new Customer();
				foreach (int encodedLitemset in sequence)
				{
					Transaction t = new Transaction();
					t.AddItems(decoding[encodedLitemset].GetItems());
					c.AddTransaction(t);
				}
				decodedList.Add(c);
			}

			// it seems that the below code is obsolete:

			//foreach (Customer customer in decodedList)
			//{
			//	for (int tn = customer.Transactions.Count - 1; tn >= 0; --tn)
			//	{
			//		// tn : transaction no.
			//		Transaction t = customer.Transactions[tn];
			//		foreach (Transaction comparedTransaction in customer.Transactions)
			//		{
			//			if (Object.ReferenceEquals(t, comparedTransaction))
			//				continue;
			//			if (t.Items.Count >= comparedTransaction.Items.Count)
			//				continue;
			//			if (IsSubSequence<Item>(t.Items, comparedTransaction.Items))
			//			{
			//				customer.Transactions.RemoveAt(tn);
			//				break;
			//			}
			//		}
			//	}
			//}

			//// compare each pair of customers for equality
			//List<int> duplicateCustomers = new List<int>();
			//for (int i1 = 0; i1 < decodedList.Count; i1++)
			//{
			//	Customer customer1 = decodedList[i1];
			//	for (int i2 = i1 + 1; i2 < decodedList.Count; i2++)
			//	{
			//		//if (i1 == i2)
			//		//	continue;
			//		Customer customer2 = decodedList[i2];
			//		if (customer1.Transactions.Count != customer2.Transactions.Count)
			//			continue;
			//		if (customer1.Equals(customer2))
			//		{
			//			duplicateCustomers.Add(i1);
			//			break;
			//		}
			//	}
			//}
			//for (int index = duplicateCustomers.Count - 1; index >= 0; --index)
			//	decodedList.RemoveAt(duplicateCustomers[index]);

			return decodedList;
		}

		/// <summary>
		/// Executes AprioriAll algorithm on a given input and minimum suport threshold.
		/// </summary>
		/// <param name="threshold">greater than 0, and less or equal 1</param>
		/// <returns>list of frequently occurring customers transaction's patters</returns>
		public List<ICustomer> RunAprioriAll(double threshold)
		{
			return RunAprioriAll(threshold, false,out supp1);
		}

		/// <summary>
		/// \ingroup aprioriandall
		/// 
		/// Executes AprioriAll algorithm on a given input and minimum suport threshold.
		/// </summary>
		/// <param name="threshold">greater than 0, and less or equal 1</param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		/// <returns>list of frequently occurring customers transaction's patters</returns>
		public List<ICustomer> RunAprioriAll(double threshold, bool progressOutput, out List<double> supp)
		{
			if (customerList == null)
				throw new ArgumentNullException("customers list is null", "customerList");
			if (threshold > 1 || threshold <= 0)
				throw new ArgumentException("threshold is out of range = (0,1]", "threshold");

			int minSupport = (int)Math.Ceiling((double)customersCount * threshold);
			if (progressOutput)
				Log.WriteLine("Threshold = {0}  =>  Minimum support = {1}", threshold, minSupport);

			if (minSupport <= 0)
				throw new ArgumentException("minimum support must be positive", "minSupport");

			Stopwatch totalTimeTaken = new Stopwatch();
			totalTimeTaken.Start();

			// 1. sort the input!
			if (progressOutput)
				Log.WriteLine("1) Sort Phase - list is already sorted as user sees fit");

			// corresponds to 1st step of AprioriAll algorithm, namely "Sort Phase".

			// already done because input is sorted by the user in an apropriate way

			// 2. find all frequent 1-sequences
			if (progressOutput)
				Log.WriteLine("2) Litemset Phase");
			if (progressOutput)
				Log.WriteLine("Launching Apriori...");
			// this corresponds to 2nd step of AprioriAll algorithm, namely "Litemset Phase".
			List<ILitemset> oneLitemsets = RunApriori(threshold, progressOutput);

			// 3. transform input into list of IDs
			if (progressOutput)
				Log.WriteLine("3) Transformation Phase");

			// 3.a) give an ID to each 1-seq
			Dictionary<ILitemset, int> encoding;
			Dictionary<int, ILitemset> decoding;

			GenerateEncoding(oneLitemsets, out encoding, out decoding);

			Dictionary<int, List<int>> litemsetsContaining = GenerateContainmentRules(oneLitemsets, encoding);

			if (progressOutput)
			{
				Log.WriteLine("Encoding dictionary for litemsets:");
				foreach (KeyValuePair<int, ILitemset> kv in decoding)
				{
					StringBuilder s = new StringBuilder();
                    temp.Add(kv.Value);
                    s.AppendFormat(" {0} <= {1}", kv.Key, kv.Value);
					if (litemsetsContaining.ContainsKey(kv.Key))
					{
						List<int> superLitemsets = litemsetsContaining[kv.Key];
						s.AppendFormat("; {0} is in {1}", kv.Key, String.Join(", ", superLitemsets.ToArray()));
					}
					Log.WriteLine(s.ToString());
				}
			}

            if (progressOutput)
            {
                Console.Out.WriteLine("Containment rules for litemsets:");
                foreach (KeyValuePair<int, List<int>> kv in litemsetsContaining)
                    Console.Out.WriteLine(" {0} is in {1}", kv.Key, String.Join(", ", kv.Value.ToArray()));
            }

            // 3.b) using created IDs, transform the input

            // list of lists of list of IDs:
            // - each list of IDs is a frequent itemset
            // - list of those means a list of frequent itemsets, 
            //   meaning a list of transaction represented as a list of frequent 
            //   itemsets performed by one customer
            // - outer list means list of customers

            if (progressOutput)
				Log.WriteLine("Encoding input data...");
			var encodedList = EncodeCustomerList(oneLitemsets, encoding);

			if (progressOutput && encodedList.Count <= 100)
			{
				var customersEnumerator = customerList.GetEnumerator();
				Log.WriteLine("How the input is encoded:");
				foreach (List<List<int>> c in encodedList)
				{
					StringBuilder s = new StringBuilder();
					customersEnumerator.MoveNext();
					//var transactionsEnumerator = customersEnumerator.Current.Transactions.GetEnumerator();
					s.AppendFormat(" - {0} => (", customersEnumerator.Current);
					foreach (List<int> t in c)
					{
						//transactionsEnumerator.MoveNext();
						//var itemsEnumerator = transactionsEnumerator.Current.Items.GetEnumerator();
						s.Append("{");
						bool first = true;
						foreach (int i in t)
						{
							//itemsEnumerator.MoveNext();
							if (!first)
								s.Append(" ");
							if (first)
								first = false;
							s.AppendFormat("{0}", i);
						}
						s.Append("}");
					}
					s.Append(")");
					Log.WriteLine(s.ToString());
				}
			}

			// 4. find all frequent sequences in the input
			if (progressOutput)
				Log.WriteLine("4) Sequence Phase");

			if (progressOutput)
				Log.WriteLine("Searching for all possible k-sequences");
			var kSequences = FindAllFrequentSequences(oneLitemsets, encoding, encodedList, litemsetsContaining,
				minSupport, progressOutput);
			if (progressOutput)
				Log.WriteLine("Maximal k is {0}.", kSequences.Count - 2);

			// 5. purge all non-maximal sequences
			if (progressOutput)
				Log.WriteLine("5) Maximal Phase");

			if (progressOutput)
				Log.WriteLine("Purging all non-maximal sequences...");
			PurgeAllNonMax(kSequences, litemsetsContaining, oneLitemsets.Count, progressOutput);

			// 6. decode results
			if (progressOutput)
				Log.WriteLine("Decoding results and purging again...");
			var decodedList = InferRealResults(encodedList, kSequences, decoding);

			if (progressOutput)
			{
				var decodedEnumerator = decodedList.GetEnumerator();
				Log.WriteLine("How maximal sequences are decoded:");
				foreach (List<List<int>> kSequencesPartition in kSequences)
					foreach (List<int> sequene in kSequencesPartition)
					{
						StringBuilder s = new StringBuilder();
						s.Append(" - <");
						bool first = true;
						foreach (int i in sequene)
						{
							if (!first)
								s.Append(" ");
							if (first)
								first = false;
							s.AppendFormat("{0}", i);
						}
						s.Append(">");
						if (decodedEnumerator.MoveNext())
							s.AppendFormat(" => {0}", decodedEnumerator.Current);
						Log.WriteLine(s.ToString());
					}
			}

			totalTimeTaken.Stop();

			if (progressOutput)
				Log.WriteLine(" total time taken to complete the algorithm: {0}ms", totalTimeTaken.ElapsedMilliseconds);

            #region trash
            foreach (ICustomer t in customerList)
            {
                int sup = 0;
                int iter = 0;
                bool b = true;
                foreach (ILitemset i in temp)
                {
                    char[] charSeparators = new char[] { '{', '}', '(', ')',' ',',' };
                    char[] charSeparators2 = new char[] { 'L','i','t','(','S','u','p','=',';',')'};
                    string st = t.ToString();
                   string[] st2 = st.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries);
                    string st1 = i.ToString();
                    string[] st4 = st1.Split(charSeparators2, StringSplitOptions.RemoveEmptyEntries);
                    foreach(string str in st2)
                    {
                        if (st4.Count()<=1) {
                            if (str == st4[1])
                            {
                                if (sup == 0)
                                    sup = Convert.ToInt32(st4[0]);
                                if (Convert.ToInt32(st4[0]) <= sup)
                                    sup = Convert.ToInt32(st4[0]);
                            }
                        }
                        else
                        {
                            foreach(string str1 in st4)
                            {
                                if (iter != 0)
                                {
                                    if (str == str1)
                                    {
                                        if (sup == 0)
                                            sup = Convert.ToInt32(st4[0]);
                                        if (Convert.ToInt32(st4[0]) <= sup)
                                            sup = Convert.ToInt32(st4[0]);
                                    }
                                }
                                else iter++;

                            }
                        }
                    }

                }
                Log.WriteLine("{0} --- {1} tik tak", t, sup);
            }
            #endregion trash

            foreach (ICustomer i in decodedList)
            {
                double sup = 0;

                int iter = 0;
                bool b = true;
                foreach (ICustomer t in customerList)
                {
                    char[] charSeparators = new char[] { '{', '}', '(', ')', ' ', ',' };
                    char[] charSeparators2 = new char[] { 'L', 'i', 't', '(', 'S', 'u', 'p', '=', ';', ')' };
                    string st = t.ToString();
                    string[] st2 = st.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries); // polniy
                    string st1 = i.ToString();
                    string[] st4 = st1.Split(charSeparators, StringSplitOptions.RemoveEmptyEntries); // itog
                    foreach(string str in st4)
                    {
                        if (st2.Contains(str))
                        {}
                        else b = false;
                    }
                    if (b == true) sup++;
                    b = true;
                }
                supp1.Add(sup/customersCount);
                sup = 0;
            }
            supp = supp1;
                // 7. return results
                return decodedList;
		}

		/// <summary>
		/// Executes AprioriAll algorithm on a given input and minimum suport threshold. It uses parallel version 
		/// of the algorithm, unless no OpenCL platforms are available.
		/// </summary>
		/// <param name="threshold">greater than 0, and less or equal 1</param>
		/// <returns>list of frequently occurring customers transaction's patters</returns>
		public List<ICustomer> RunParallelAprioriAll(double threshold)
		{
			return RunParallelAprioriAll(threshold, false);
		}

		/// <summary>
		/// Executes AprioriAll algorithm on a given input and minimum suport threshold. It uses parallel version 
		/// of the algorithm, unless no OpenCL platforms are available.
		/// </summary>
		/// <param name="threshold">greater than 0, and less or equal 1</param>
		/// <param name="progressOutput">if true, information about progress is sent to standard output</param>
		/// <returns>list of frequently occurring customers transaction's patters</returns>
		public List<ICustomer> RunParallelAprioriAll(double threshold, bool progressOutput)
		{
			if (Platforms.InitializeAll().Length == 0)
				return RunAprioriAll(threshold, progressOutput,out supp1);

			if (customerList == null)
				throw new ArgumentNullException("customerList", "customerList is null.");
			if (threshold > 1 || threshold <= 0)
				throw new ArgumentException("threshold", "threshold is out of range = (0,1]");

			int minSupport = (int)Math.Ceiling((double)customersCount * threshold);
			if (progressOutput)
				Log.WriteLine("Threshold = {0}  =>  Minimum support = {1}", threshold, minSupport);

			if (minSupport <= 0)
				throw new ArgumentException("minimum support must be positive", "minSupport");

			//var oneLitemsets = RunParallelApriori(threshold, progressOutput);

			return null;
		}

	}

	/// @}
}
