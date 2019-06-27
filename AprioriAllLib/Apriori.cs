using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Resources;
using System.Threading;
using OpenCL.Abstract;

namespace AprioriAllLib
{


    /// <summary>
    /// Используется для получения списка больших наборов предметов из набора транзакций клиентов.
    /// </summary>
    public class Apriori : IDisposable
	{
        /// <summary>
        /// Входные данные для алгоритма.
        /// </summary>
        protected IEnumerable<ICustomer> customerList;

		protected int customersCount;

		#region OpenCL

		/// <summary>
		/// Indicator whether OpenCL platorm and device information is alread gathered
		/// by this instance of Apriori, and whether context was created for it.
		/// </summary>
		private bool clInitialized;

		private Platform platform;

		private Device device;

		private Context context;

        #endregion

        /// <summary>
        /// Определяет, был ли исходный код ядер OpenCL уже прочитан этим экземпляром.
        /// </summary>
        private bool clProgramsInitialized;

		#region Kernels.(...)

		//private Semaphore[] sem;

		Kernels.AssignZero kernelZero;
		Kernels.AssignConst kernelConst;
		Kernels.Increment kernelIncrement;
		Kernels.MultiplyByTwo kernelMultiplyByTwo;

		Kernels.ChangeValueToConst kernelSubstitute;
		Kernels.ChangeAllButValueToConst kernelSubstituteIfNotEqual;

		//Kernels.Identity kernelCopy;
		//Kernels.Identity kernelCopySingle;
		Kernels.CopyOnlyOneValue kernelCopyIfEqual;

		Kernels.ReductionMin kernelMin;
		Kernels.ReductionMax kernelMax;
		Kernels.ReductionSum kernelSum;

		//Kernels.PairwiseAnd kernelPairwiseAnd;

		Kernels.ReductionOr kernelOr;
		//Kernels.And kernelAnd;
		Kernels.SegmentedReductionOr kernelSegmentedOr;

		#endregion

		//private Program programBasicFunctions;

		///// <summary>
		///// Program created from file 'distinct.cl'.
		///// </summary>
		//private Program programDistinct;

		///// <summary>
		///// Program created from file 'separation.cl'.
		///// </summary>
		//private Program programSeparation;

		/// <summary>
		/// Program created from file 'apriori.cl'.
		/// </summary>
		private Program programApriori;

		private MultiItemSupportKernel kernelMulitItemSupport;

        /// <summary>
        /// Конструктор.
        /// </summary>
        /// <param name="customerList">Объект CustomerList, содержащий список клиентов из базы данных</param>
        [Obsolete]
		public Apriori(CustomerList customerList)
		{
			this.customerList = customerList.Customers;
			this.customersCount = customerList.Customers.Count;

			clInitialized = false;
			clProgramsInitialized = false;
		}

		public Apriori(IEnumerable<ICustomer> customers)
		{
			this.customerList = customers;
			this.customersCount = customers.Count();
		}

		protected void InitOpenCL(bool progressOutput)
		{
			if (clInitialized)
				return; // уже инициализирован

            //Stopwatch initWatch = new Stopwatch();
            //initWatch.Start();

            platform = new Platform();
			device = new Device(platform, DeviceType.GPU);
			context = new Context(device);
			Kernels.Device = device;
			Kernels.Context = context;

			//initWatch.Stop();
			//if (progressOutput)
			//	Log.WriteLine("Initialized OpenCL in {0}ms.", initWatch.ElapsedMilliseconds);

			clInitialized = true;
		}

		protected void InitOpenCLPrograms(bool parallelBuild, bool progressOutput)
		{
			if (clProgramsInitialized)
				return;

			//Stopwatch buildWatch = new Stopwatch();
			//buildWatch.Start();

			ResourceManager manager = AprioriAllLib.Properties.Resources.ResourceManager;

			programApriori = new OpenCL.Abstract.Program(context, new SourceCode(manager, "cl_apriori"));
			programApriori.Build(device);

			kernelMulitItemSupport = new MultiItemSupportKernel(programApriori, "multiItemSupport");

			{

				kernelCopyIfEqual = new Kernels.CopyOnlyOneValue();

				kernelMin = new Kernels.ReductionMin();
				kernelMax = new Kernels.ReductionMax();
				kernelSum = new Kernels.ReductionSum();

				kernelZero = new Kernels.AssignZero();
				kernelConst = new Kernels.AssignConst();
				kernelSubstitute = new Kernels.ChangeValueToConst();
				kernelSubstituteIfNotEqual = new Kernels.ChangeAllButValueToConst();
				kernelMultiplyByTwo = new Kernels.MultiplyByTwo();
				kernelIncrement = new Kernels.Increment();

				kernelOr = new Kernels.ReductionOr();
				kernelSegmentedOr = new Kernels.SegmentedReductionOr();
			}

			//buildWatch.Stop();
			//if (progressOutput)
			//	Log.WriteLine("Built OpenCL programs in {0}ms.", buildWatch.ElapsedMilliseconds);

			clProgramsInitialized = true;
		}

		public void Dispose()
		{
			if (clProgramsInitialized)
			{
                // ядра от уровня абстракции
                //kernelCopy.Dispose();
                //kernelCopySingle.Dispose();
                kernelCopyIfEqual.Dispose();

				kernelMin.Dispose();
				kernelMax.Dispose();
				kernelSum.Dispose();

				kernelZero.Dispose();
				kernelConst.Dispose();
				kernelSubstitute.Dispose();
				kernelSubstituteIfNotEqual.Dispose();
				kernelMultiplyByTwo.Dispose();
				kernelIncrement.Dispose();

				//kernelPairwiseAnd.Dispose();

				kernelOr.Dispose();
				kernelSegmentedOr.Dispose();
                //kernelAnd.Dispose();

                // собственные ядра
                kernelMulitItemSupport.Dispose();
				programApriori.Dispose();

				clProgramsInitialized = false;
			}
			if (clInitialized)
			{
				context.Dispose();
				clInitialized = false;
			}
		}

        /// <summary>
        /// Производит все подмножества данного списка элементов в транзакции.
        /// </summary>
        /// <param name="items">список предметов, содержащихся в одной транзакции</param>
        /// <returns>список кандидатов на l-предметы (большие наборы предметов)</returns>
        protected List<ILitemset> GenerateCandidateLitemsets(List<IItem> items)
		{
			int count = items.Count;
			int i = 0;
			List<List<IItem>> candLitemsets = new List<List<IItem>>();

            // добавить частую последовательность, содержащую все элементы этой транзакции
            candLitemsets.Add(new List<IItem>(items));
			count--;
			while (count != 0)
			{
				List<IItem> temp;
				foreach (Item item in candLitemsets[i])
				{
					temp = new List<IItem>(candLitemsets[i]);
					temp.Remove(item);
                    // проверяем, есть ли такая подобласть в списке, добавьте
                    // если он не существует
                    if (!candLitemsets.Exists(list => list.Count == count &&
						 temp.All(tempItem => list.Exists(listItem => listItem.CompareTo(tempItem) == 0))))
						candLitemsets.Add(temp);
				}
				if (candLitemsets[i + 1].Count == candLitemsets[i].Count - 1)
					count--;
				i++;
			}
			var l = new List<ILitemset>();
			foreach (List<IItem> j in candLitemsets)
				l.Add(new Litemset(j));
			return l;
		}

		protected void RemoveNonMaximal(List<ILitemset> litemsets, bool progressOutput)
		{

		}

		protected void RemoveNonMaximal(List<ILitemset> litemsets, bool useOpenCL, bool progressOutput)
		{

		}

        /// <summary>
        /// Находит все l-элементы, которые имеют минимальную поддержку.
        /// </summary>
        /// <param name="minimalSupport">минимальная поддержка</param>
        /// <returns>Список L-элементов с поддержкой> = minimumSupport</returns>
        public List<ILitemset> RunApriori(double minimalSupport)
		{
			return RunApriori(minimalSupport, false);
		}

        /// <summary>
        /// \ingroup aprioriandall
        /// 
        /// Находит все l-элементы, которые имеют минимальную поддержку.
        /// </summary>
        /// <param name="minimalSupport">минимальная поддержка</param>
        /// <param name="progressOutput">если это правда, информация о прогрессе отправляется на стандартный вывод</param>
        /// <returns>Список L-элементов с поддержкой> = minimumSupport</returns>
        public List<ILitemset> RunApriori(double minimalSupport, bool progressOutput)
		{
			if (minimalSupport > 1 || minimalSupport <= 0)
				return null;

			if (customerList == null)
				return null;

			if (customersCount == 0)
				return new List<ILitemset>();

            //общая часть - инициализация
            minimalSupport *= customersCount;
			var litemsets = new List<ILitemset>();

            // сериализованная версия алгоритма

            Stopwatch watch = null;
			if (progressOutput)
			{
				watch = new Stopwatch();
				watch.Start();
			}

			int cIndex = 0;
			var idsInLitemsets = new Dictionary<ILitemset, List<int>>();
			foreach (ICustomer c in customerList)
			{
				foreach (ITransaction t in c.GetTransactions())
				{
                    //генерировать подмножества (кандидаты на l-предметы)
                    var candidateLitemsets = GenerateCandidateLitemsets(new List<IItem>(t.GetItems()));

                    //проверьте, существуют ли они в списке l-itemsets; если нет, добавьте l-itemset в l-itemsets
                    foreach (Litemset lset in candidateLitemsets)
					{
						IEnumerable<ILitemset> l = litemsets.Where(litemset => (litemset.GetItemsCount() == lset.GetItemsCount()) &&
							 litemset.GetItems().All(item => lset/*.items.Exists*/.GetItems().Any(lsetItem => lsetItem.CompareTo(item) == 0)));

						List<int> IDs_lset = null;
						if (!idsInLitemsets.TryGetValue(lset, out IDs_lset))
						{
							IDs_lset = new List<int>();
							idsInLitemsets.Add(lset, IDs_lset);
						}

						if (l.Count() == 0 && !IDs_lset.Contains(cIndex))
						{
							litemsets.Add(lset);
							lset.IncrementSupport();
							IDs_lset.Add(cIndex);
						}
						else
						{
							ILitemset litset = l.FirstOrDefault();

							List<int> IDs_litset = null;
							if (!idsInLitemsets.TryGetValue(litset, out IDs_litset))
							{
								IDs_litset = new List<int>();
								idsInLitemsets.Add(lset, IDs_litset);
							}

							if (!IDs_litset.Contains(cIndex))
							{
								litset.IncrementSupport();
								IDs_litset.Add(cIndex);
							}
						}
					}
				}
				cIndex++;
				//if (progressOutput)
				//{
				//	if (cIndex % 10 == 9)
				//	{
				//		watch.Stop();
				//		Log.Write("{0}ms ", watch.ElapsedMilliseconds);
				//		watch.Start();
				//	}
				//	++cIndex;
				//}

			}
			//if (progressOutput)
			//{
			//	Log.WriteLine();
			//}


			if (progressOutput)
				Log.WriteLine("Найдено {0} подмножества.", litemsets.Count);

            //переписать l-элементы с поддержкой> = минимум в новый список
            var properLitemsets = new List<ILitemset>();
			foreach (ILitemset litemset in litemsets)
				if (litemset.GetSupport() >= minimalSupport)
					properLitemsets.Add(litemset);

			if (progressOutput)
				Log.WriteLine("Очищенные неподдерживаемые, {0} остаются.", properLitemsets.Count);

			foreach (Litemset l in properLitemsets)
				l.SortItems();
			properLitemsets.Sort();

			if (progressOutput)
				Log.WriteLine("Sorted output.");

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("Сгенерировал все l-itemsets, нашел {0} в {1} мс.", properLitemsets.Count, watch.ElapsedMilliseconds);
			}

			return properLitemsets;
		}

		public List<ILitemset> RunAprioriWithPruning(double minimalSupport, bool progressOutput)
		{
			var results = RunApriori(minimalSupport, progressOutput);
			RemoveNonMaximal(results, progressOutput);
			return results;
		}

        /// <summary>
        /// Выполняет параллельную версию алгоритма Apriori.
        /// </summary>
        /// <param name="minimalSupport">минимальная поддержка</param>
        /// <returns>Список L-элементов с поддержкой> = minimumSupport</returns>
        public List<ILitemset> RunParallelApriori(double minimalSupport)
		{
			return RunParallelApriori(minimalSupport, false);
		}

		public List<ILitemset> RunParallelAprioriWithPruning(double minimalSupport, bool progressOutput)
		{
			var results = RunParallelApriori(minimalSupport, progressOutput);
			RemoveNonMaximal(results, true, progressOutput);
			return results;
		}

        /// <summary>
        /// Выполняет параллельную версию алгоритма Apriori.
        /// </summary>
        /// <param name="minimalSupport"></param>
        /// <param name="progressOutput">если это правда, информация о прогрессе отправляется на стандартный вывод</param>
        /// <returns>Список L-элементов с поддержкой> = minimumSupport</returns>
        public List<ILitemset> RunParallelApriori(double minimalSupport, bool progressOutput)
		{
			if (Platforms.InitializeAll().Length == 0)
				return RunApriori(minimalSupport, progressOutput);

			if (minimalSupport > 1 || minimalSupport <= 0)
				return null;

			if (customerList == null)
				return null;

			if (this.customersCount == 0)
				return new List<ILitemset>();

			int minSupport = (int)Math.Ceiling((double)this.customersCount * minimalSupport);

			Stopwatch watch = null;
			if (progressOutput)
			{
				watch = new Stopwatch();
				watch.Start();
			}

			#region conversion of input to int[]

			int itemsCountInt = 0;
			int transactionsCountInt = 0;
			int customersCountInt = 0;

			foreach (ICustomer c in customerList)
			{
				++customersCountInt;
				foreach (ITransaction t in c.GetTransactions())
				{
					++transactionsCountInt;
					foreach (IItem item in t.GetItems())
					{
						++itemsCountInt;
					}
				}
			}

			uint itemsCountUInt = (uint)itemsCountInt;
			uint transactionsCountUInt = (uint)transactionsCountInt;
			uint customersCountUInt = (uint)customersCountInt;

			int[] items = new int[itemsCountInt];
			int[] itemsTransactions = new int[itemsCountInt];
			int[] itemsCustomers = new int[itemsCountInt];
			int[] transactionsStarts = new int[transactionsCountInt];
			int[] transactionsLengths = new int[transactionsCountInt];
			int[] customersStarts = new int[customersCountInt]; // с точки зрения транзакций!
            int[] customersLengths = new int[customersCountInt]; // с точки зрения транзакций!
            {
				int currItem = 0;
				int currTransaction = 0;
				int currCustomer = 0;

				int currTransactionLenth = 0;
				int currCustomerLength = 0;
				foreach (ICustomer c in customerList)
				{
					currCustomerLength = 0;
					customersStarts[currCustomer] = currTransaction;
					foreach (ITransaction t in c.GetTransactions())
					{
						currTransactionLenth = 0;
						transactionsStarts[currTransaction] = currItem;
						foreach (IItem item in t.GetItems())
						{
							items[currItem] = item.GetId();
							itemsTransactions[currItem] = currTransaction;
							itemsCustomers[currItem] = currCustomer;
							++currItem;
							++currTransactionLenth;
						}
						++currCustomerLength;
						transactionsLengths[currTransaction] = currTransactionLenth;
						++currTransaction;
					}
					customersLengths[currCustomer] = currCustomerLength;
					++currCustomer;
				}
			}

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("преобразованный вход @ {0} мс", watch.ElapsedMilliseconds);
				watch.Start();
			}

			//Abstract.Diagnostics = true;

			uint localSize = 32;

			InitOpenCL(progressOutput);

			CommandQueue queue = new CommandQueue(device, context);

			#region input buffers allocation and initialization

			Buffer<int> itemsBuf = new Buffer<int>(context, queue, items);

			Buffer<int> transactionsStartsBuf = new Buffer<int>(context, queue, transactionsStarts);

			Buffer<int> customersStartsBuf = new Buffer<int>(context, queue, customersStarts);

			int[] itemsCount = new int[] { itemsCountInt };
			Buffer<int> itemsCountBuf = new Buffer<int>(context, queue, itemsCount);

			int[] transactionsCount = new int[] { transactionsCountInt };
			Buffer<int> transactionsCountBuf = new Buffer<int>(context, queue, transactionsCount);

			int[] customersCount = new int[] { customersCountInt };
			Buffer<int> customersCountBuf = new Buffer<int>(context, queue, customersCount);

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("буферы, написанные @ {0} мс", watch.ElapsedMilliseconds);
				watch.Start();
			}

			InitOpenCLPrograms(false, progressOutput);

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("программы построены @ {0} мс", watch.ElapsedMilliseconds);
				watch.Start();
			}

			#region empty buffers allocation and initialization

			Buffer<int> Zero = new Buffer<int>(context, queue, new int[] { 0 });
			Zero.Write(false);
			Buffer<int> One = new Buffer<int>(context, queue, new int[] { 1 });
			One.Write(false);

			Buffer<int> tempItemsBuf = new Buffer<int>(context, queue, itemsCountUInt);
			itemsBuf.Copy(0, itemsCountUInt, tempItemsBuf, 0);

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("Initialized OpenCL in {0}ms.", watch.ElapsedMilliseconds);
				watch.Restart();
			}

			// time: n*log^2(n)
			#region finding empty id

			int[] tempValue = new int[] { -1 };

			kernelMax.SetArgument(0, tempItemsBuf);
			kernelMax.SetArguments(null, itemsCountInt, 0, itemsCountInt);
			uint globalSize = itemsCountUInt;
			for (int scaling = 1; scaling < itemsCountInt; scaling *= (int)localSize)
			{
				kernelMax.SetArgument(4, scaling);
				globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
				kernelMax.Launch1D(queue, globalSize, localSize);
				globalSize /= localSize;
			}

			//tempItemsBuf.Read(tempValue, 0, 1); // debug

			int emptyId = -1;
			Buffer<int> emptyIdBuf = new Buffer<int>(context, queue, 1);
			kernelIncrement.SetArgument(0, tempItemsBuf);
			kernelIncrement.SetArguments(null, 1, 0, 1);
			kernelIncrement.Launch1D(queue, 1, 1);

			tempItemsBuf.Copy(0, 1, emptyIdBuf, 0);

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("empty id @ {0}ms", watch.ElapsedMilliseconds);
				watch.Start();
			}

			// time: 3n + n * ( 1.5n + log^2(n) ) == O(n^2)
			#region finding distinct item ids

			Buffer<int> itemsRemainingBuf = new Buffer<int>(context, queue, itemsCountUInt);
			itemsBuf.Copy(itemsRemainingBuf);
			itemsBuf.Copy(tempItemsBuf);

			int uniqueItemsCount = 0;

			int[] uniqueItems = new int[itemsCountInt];
			Buffer<int> uniqueItemsBuf = new Buffer<int>(context, queue, uniqueItems);

			kernelZero.SetArgument(0, uniqueItemsBuf);
			kernelZero.SetArguments(null, itemsCountInt, 0, itemsCountInt);
			kernelZero.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);

			kernelMin.SetArgument(0, tempItemsBuf);
			kernelMin.SetArguments(null, itemsCountInt, 0, itemsCountInt);
			kernelSubstitute.SetArgument(0, itemsRemainingBuf);
			kernelSubstitute.SetArguments(null, itemsCountInt, 0, itemsCountInt);
			kernelSubstitute.SetArgument(4, emptyIdBuf);
			kernelSubstitute.SetArgument(5, tempItemsBuf);

			while (true)
			{
				globalSize = itemsCountUInt;
				for (int scaling = 1; scaling < itemsCountInt; scaling *= (int)localSize)
				{
					kernelMin.SetArgument(4, scaling);
					globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
					kernelMin.Launch1D(queue, globalSize, localSize);
					globalSize /= localSize;
				}

				if (emptyId == -1)
				{
					emptyIdBuf.Read();
					emptyId = emptyIdBuf.Value;
				}
				tempItemsBuf.Read(tempValue, 0, 1);
				if (tempValue[0] == emptyId)
					break; // достигнут конец вычисления

                uniqueItems[uniqueItemsCount] = tempValue[0];
				uniqueItemsCount += 1;
				uniqueItemsBuf.Write(false, 0, (uint)uniqueItemsCount);

				kernelSubstitute.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);


				itemsRemainingBuf.Copy(0, itemsCountUInt, tempItemsBuf, 0);
			}

			uint uniqueItemsCountUInt = (uint)uniqueItemsCount;

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("distinct items @ {0}ms", watch.ElapsedMilliseconds);
				watch.Start();
			}

			// time: 2n + n^2 * log^2(n) == O(n^2 * log^2(n))
			#region finding supports for items

			int itemsSupportsCountInt = itemsCountInt * uniqueItemsCount;
			uint itemsSupportsCountUInt = (uint)itemsSupportsCountInt;

			int[] itemsSupportsCount = new int[] { itemsSupportsCountInt };
			Buffer<int> itemsSupportsCountBuf = new Buffer<int>(context, queue, itemsSupportsCount);

			int[] itemsSupports = new int[itemsSupportsCountInt];
			Buffer<int> itemsSupportsBuf = new Buffer<int>(context, queue, itemsSupports);

			kernelZero.SetArgument(0, itemsSupportsBuf);
			kernelZero.SetArguments(null, itemsSupportsCountInt, null, itemsSupportsCountInt);
			kernelZero.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsSupportsCountUInt), localSize);

			Buffer<int> uniqueItemBuf = new Buffer<int>(context, queue, 1);

			kernelCopyIfEqual.SetArgument(0, itemsBuf);
			kernelCopyIfEqual.SetArgument(4, itemsSupportsBuf);
			kernelCopyIfEqual.SetArguments(null, itemsCountInt, 0, itemsCountInt,
				null, itemsSupportsCountInt, null, itemsCountInt);
			kernelCopyIfEqual.SetArgument(8, uniqueItemBuf);

			int uniqueItemIndex = 0;
			for (int supportsOffset = 0; supportsOffset < itemsSupports.Length; supportsOffset += items.Length)
			{
				uniqueItemBuf.Value = uniqueItems[uniqueItemIndex];
				uniqueItemBuf.Write();
				kernelCopyIfEqual.SetArgument(6, supportsOffset);

				kernelCopyIfEqual.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsCountUInt), localSize);


				++uniqueItemIndex;
			}

			kernelSubstituteIfNotEqual.SetArgument(0, itemsSupportsBuf);
			kernelSubstituteIfNotEqual.SetArguments(null, itemsSupportsCountInt, 0, itemsSupportsCountInt);
			kernelSubstituteIfNotEqual.SetArgument(4, One);
			kernelSubstituteIfNotEqual.SetArgument(5, Zero);
			kernelSubstituteIfNotEqual.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, itemsSupportsCountUInt), localSize);


			#endregion

			// time: 2n + n^2 * log^2(n) == O(n^2 * log^2(n))
			#region finding supports for transactions

			int transactionsSupportsCountInt = transactionsCountInt * uniqueItemsCount;
			uint transactionsSupportsCountUInt = (uint)transactionsSupportsCountInt;

			int[] transactionsSupportsCount = new int[] { transactionsSupportsCountInt };
			Buffer<int> transactionsSupportsCountBuf = new Buffer<int>(context, queue, transactionsSupportsCount);

			int[] transactionsSupports = new int[transactionsSupportsCountInt];
			Buffer<int> transactionsSupportsBuf = new Buffer<int>(context, queue, transactionsSupports);

			kernelZero.SetArgument(0, transactionsSupportsBuf);
			kernelZero.SetArguments(null, transactionsSupportsCountInt, null, transactionsSupportsCountInt);
			kernelZero.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, transactionsSupportsCountUInt), localSize);

			Buffer<int> itemsSupportsBufCopy = new Buffer<int>(context, queue, itemsSupports);

			itemsSupportsBuf.Copy(0, itemsSupportsCountUInt, itemsSupportsBufCopy, 0);

			kernelSegmentedOr.SetArgument(0, itemsSupportsBufCopy);
			kernelSegmentedOr.SetArguments(null, itemsSupportsCountInt, null, null,
				null, itemsCountInt);


			for (int tn = 0; tn < transactionsCountInt; ++tn)
			{
				kernelSegmentedOr.SetArgument(2, transactionsStarts[tn]);
				kernelSegmentedOr.SetArgument(3, transactionsLengths[tn]);

				globalSize = (uint)transactionsLengths[tn];
				for (int scaling = 1; scaling < transactionsLengths[tn]; scaling *= (int)localSize)
				{
					kernelSegmentedOr.SetArgument(4, scaling);
					globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
					kernelSegmentedOr.Launch2D(queue, globalSize, localSize, uniqueItemsCountUInt, 1);
					globalSize /= localSize;
				}

				int offset = transactionsStarts[tn];
				int outputOffset = tn;
				for (int i = 0; i < uniqueItemsCount; ++i)
				{
					itemsSupportsBufCopy.Copy((uint)offset, 1, transactionsSupportsBuf, (uint)outputOffset);


					if (i == uniqueItemsCount - 1)
						break;

					offset += itemsCountInt;
					outputOffset += transactionsCountInt;
				}
			}


			#endregion

			// time: 2n + n^2 * log^2(n) == O(n^2 * log^2(n))
			#region finding supports for customers

			int customersSupportsCountInt = customersCountInt * uniqueItemsCount;
			uint customersSupportsCountUInt = (uint)customersSupportsCountInt;

			int[] customersSupportsCount = new int[] { customersSupportsCountInt };
			Buffer<int> customersSupportsCountBuf = new Buffer<int>(context, queue, customersSupportsCount);

			int[] customersSupports = new int[customersSupportsCountInt];
			Buffer<int> customersSupportsBuf = new Buffer<int>(context, queue, customersSupports);

			kernelZero.SetArgument(0, customersSupportsBuf);
			kernelZero.SetArguments(null, customersSupportsCountInt, null, customersSupportsCountInt);
			kernelZero.Launch1D(queue, Kernels.GetOptimalGlobalSize(localSize, customersSupportsCountUInt), localSize);

			Buffer<int> transactionsSupportsBufCopy = new Buffer<int>(context, queue, transactionsSupports);

			transactionsSupportsBuf.Copy(0, transactionsSupportsCountUInt, transactionsSupportsBufCopy, 0);

			kernelSegmentedOr.SetArgument(0, transactionsSupportsBufCopy);
			kernelSegmentedOr.SetArguments(null, transactionsSupportsCountInt, null, null,
				null, transactionsCountInt);


			for (int cn = 0; cn < customersCountInt; ++cn)
			{
				kernelSegmentedOr.SetArgument(2, customersStarts[cn]);
				kernelSegmentedOr.SetArgument(3, customersLengths[cn]);

				globalSize = (uint)customersLengths[cn];
				for (int scaling = 1; scaling < customersLengths[cn]; scaling *= (int)localSize)
				{
					kernelSegmentedOr.SetArgument(4, scaling);
					globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
					kernelSegmentedOr.Launch2D(queue, globalSize, localSize, uniqueItemsCountUInt, 1);
					globalSize /= localSize;
				}


				int offset = customersStarts[cn];
				int outputOffset = cn;
				for (int i = 0; i < uniqueItemsCount; ++i)
				{
					transactionsSupportsBufCopy.Copy((uint)offset, 1, customersSupportsBuf, (uint)outputOffset);


					if (i == uniqueItemsCount - 1)
						break;

					offset += transactionsCountInt;
					outputOffset += customersCountInt;
				}
			}

			#endregion

			// time: n + n * ( n*log^2(n) + n ) == O(n^2 * log^2(n))
			#region litemsets of size 1

			Buffer<int> customersSupportsBufCopy = new Buffer<int>(context, queue, customersSupports);

			customersSupportsBuf.Copy(0, customersSupportsCountUInt, customersSupportsBufCopy, 0);


			kernelSum.SetArgument(0, customersSupportsBufCopy);
			kernelSum.SetArguments(null, customersSupportsCountInt, null, customersCountInt);

			var litemsets = new List<ILitemset>();
			var supportedLocations = new List<int>();

			for (int un = 0; un < uniqueItemsCount; ++un)
			{
				int offset = un * customersCountInt;
				kernelSum.SetArgument(2, offset);

				globalSize = customersCountUInt;
				for (int scaling = 1; scaling < customersCountInt; scaling *= (int)localSize)
				{
					kernelSum.SetArgument(4, scaling);
					globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
					kernelSum.Launch1D(queue, globalSize, localSize);
					globalSize /= localSize;
				}


				customersSupportsBufCopy.Read(tempValue, (uint)offset, 1);

				int support = tempValue[0];

				if (support < 0 || support > customersCountInt)
				{
					Zero.Read();
					One.Read();
					emptyIdBuf.Read();
					itemsSupportsBuf.Read();
					transactionsSupportsBufCopy.Read();
					customersSupportsBufCopy.Read();
					itemsSupportsBuf.Read();
					transactionsSupportsBuf.Read();
					customersSupportsBuf.Read();
					throw new Exception(String.Format("support = {0} не имеет теоретических границ [0, {1}]!",
						support, customersCountInt));
				}

				if (support < minSupport)
					continue;

				litemsets.Add(new Litemset(support, uniqueItems[un]));
				supportedLocations.Add(un);
			}

			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("отдельные элементы @ {0} мс", watch.ElapsedMilliseconds);
				watch.Start();
			}

			// time: n*log^2(n) + n + n * ( 0.5n + n^2 + 0.5n * n^2 * log^2(n) + n*log^2(n) ) == O(n^4 * log^2(n))
			#region litemsets of size 2 and above

			bool moreCanBeFound = true;


			kernelMax.SetArgument(0, customersSupportsBufCopy);
			kernelMax.SetArguments(null, customersSupportsCountInt, null, customersSupportsCountInt);
			globalSize = customersSupportsCountUInt;
			for (int scaling = 1; scaling < customersSupportsCountInt; scaling *= (int)localSize)
			{
				kernelMax.SetArgument(4, scaling);
				globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
				kernelMax.Launch1D(queue, globalSize, localSize);
				globalSize /= localSize;

			}

			bool[] newSupportedLocations = new bool[uniqueItemsCount];
			for (int i = 0; i < uniqueItemsCount; ++i)
				newSupportedLocations[i] = false;

			customersSupportsBufCopy.Read(tempValue, 0, 1);
			if (tempValue[0] < minSupport)
				moreCanBeFound = false;
			else if (supportedLocations.Count == 1)
				moreCanBeFound = false;

			if (moreCanBeFound)
			{
				int[] indices = new int[uniqueItemsCount];

				int[] locations = new int[uniqueItemsCount];
				Buffer<int> locationsBuf = new Buffer<int>(context, queue, locations);

				int currLength = 1;
				Buffer<int> currLengthBuf = new Buffer<int>(context, queue, 1);

				kernelMulitItemSupport.SetArgument(0, transactionsSupportsBufCopy);
				kernelMulitItemSupport.SetArguments(null, transactionsSupportsCountInt,
					transactionsCountInt);
				kernelMulitItemSupport.SetArgument(3, locationsBuf);
				kernelMulitItemSupport.SetArgument(4, currLengthBuf);

				kernelOr.SetArgument(0, transactionsSupportsBufCopy);
				kernelOr.SetArguments(null, transactionsSupportsCountInt, null, null);

				while (moreCanBeFound)
				{
					++currLength;
					currLengthBuf.Value = currLength;
					currLengthBuf.Write(false);

					uint litemsetOffset = (uint)litemsets.Count;

					for (int i = 0; i < currLength; ++i)
						indices[i] = i;
					int currIndex = currLength - 1;


					while (true)
					{


						#region initialization of int[] locations
						for (int i = 0; i < currLength; ++i)
							locations[i] = supportedLocations[indices[i]];
						locationsBuf.Write(false, 0, (uint)currLength);
						#endregion

						#region copying of relevant parts of int[] transactionsSupports
						for (int i = 0; i < currLength; ++i)
						{
							#region debugging
							//if (currLength == 2
							//	&& uniqueItems[supportedLocations[indices[0]]] == 2
							//	&& uniqueItems[supportedLocations[indices[1]]] == 3)
							//{
							//	// debug only
							//	queue.Finish();
							//	transactionsSupportsBufCopy.Read();
							//	//transactionsSupportsBuf.Read();
							//}
							#endregion

							int offset = supportedLocations[indices[i]] * transactionsCountInt;
							transactionsSupportsBuf.Copy((uint)offset, transactionsCountUInt,
								transactionsSupportsBufCopy, (uint)offset);
						}
						#endregion



						int stepNo = 1;
						while (stepNo < currLength)
						{


							kernelMulitItemSupport.SetArgument(5, stepNo);
							kernelMulitItemSupport.Launch2D(queue, transactionsCountUInt, 1, (uint)currLength, 1);
							stepNo *= 2;
							queue.Finish();
						}


						for (int cn = 0; cn < customersCountInt; ++cn)
						{
							kernelOr.SetArgument(2, supportedLocations[indices[0]] * transactionsCountInt + customersStarts[cn]);
							kernelOr.SetArgument(3, customersLengths[cn]);

							globalSize = (uint)customersLengths[cn];
							for (int scaling = 1; scaling < customersLengths[cn]; scaling *= (int)localSize)
							{
								kernelOr.SetArgument(4, scaling);
								globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
								kernelOr.Launch1D(queue, globalSize, localSize);
								globalSize /= localSize;
							}
						}

						kernelSum.SetArgument(0, transactionsSupportsBufCopy);
						kernelSum.SetArguments(null, transactionsSupportsCountInt, null, transactionsCountInt);

						int offset2 = supportedLocations[indices[0]] * transactionsCountInt;
						kernelSum.SetArgument(2, offset2);

						globalSize = transactionsCountUInt;
						for (int scaling = 1; scaling < transactionsCountInt; scaling *= (int)localSize)
						{
							kernelSum.SetArgument(4, scaling);
							globalSize = Kernels.GetOptimalGlobalSize(localSize, globalSize);
							kernelSum.Launch1D(queue, globalSize, localSize);
							globalSize /= localSize;
						}

						transactionsSupportsBufCopy.Read(tempValue, (uint)offset2, 1);

						if (tempValue[0] > transactionsSupportsCountInt)
						{
                            // это означает, что данные были повреждены из-за:
                            // a) некоторая ошибка памяти gpu,
                            // б) плохая синхронизация,
                            // c) или какая-то другая неизвестная вещь
                            // эта ситуация встречается редко, только с очень большими данными
                            // Неизвестная причина неизвестна
                            itemsSupportsBufCopy.Read();
							transactionsSupportsBufCopy.Read();
							customersSupportsBufCopy.Read();
							itemsSupportsBuf.Read();
							transactionsSupportsBuf.Read();
							customersSupportsBuf.Read();
                            // эта ошибка является серьезной: она предотвращает продолжение алгоритма
                            throw new Exception(String.Format("{0} больше {1}, невозможно!\n {2}",
								tempValue[0], transactionsSupportsCountInt, String.Join(",", transactionsSupports)));
						}


						if (tempValue[0] >= minSupport)
						{
							ILitemset l = new Litemset(new List<IItem>());
							l.SetSupport(tempValue[0]);
							for (int i = 0; i < currLength; ++i)
							{
								int index = indices[i];
								int spprtd = supportedLocations[index];
								l.AddItem(new Item(uniqueItems[spprtd]));
								newSupportedLocations[spprtd] = true;
							}



							litemsets.Add(l);
						}

						#region selecting new indices (from supportedLocations) to analyze
						if ((currIndex == 0 && indices[currIndex] == supportedLocations.Count - currLength)
											|| currLength == supportedLocations.Count)
							break;
						if (indices[currIndex] == supportedLocations.Count - currLength + currIndex)
						{
							++indices[currIndex - 1];
							if (indices[currIndex - 1] + 1 < indices[currIndex])
							{
								while (currIndex < currLength - 1)
								{
									indices[currIndex] = indices[currIndex - 1] + 1;
									++currIndex;
								}
								indices[currIndex] = indices[currIndex - 1] + 1;
							}
							else
							{
								--currIndex;
							}
							continue;
						}
						indices[currIndex] += 1;
						#endregion
					}

					if (progressOutput)
						Log.WriteLine("наконец, {0}, до настоящего времени найдено {1} l-itemsets", currLength, litemsets.Count);


					if (litemsets.Count <= litemsetOffset || currLength >= uniqueItemsCount)
					{
						moreCanBeFound = false;
						break;
					}

					supportedLocations.Clear();
					for (int i = 0; i < uniqueItemsCount; ++i)
					{
						if (newSupportedLocations[i])
						{
							supportedLocations.Add(i);
							newSupportedLocations[i] = false;
						}
					}

					if (currLength >= supportedLocations.Count)
					{
                        // слишком мало поддерживаемых отдельных элементов, чтобы сделать кандидата требуемой длины
                        moreCanBeFound = false;
						break;
					}
				}

				queue.Finish();

				currLengthBuf.Dispose();
				locationsBuf.Dispose();
			}
			#endregion

			if (progressOutput)
			{
				watch.Stop();
				Log.WriteLine("Сгенерировал все l-itemsets, нашел {0} в {1} мс.", litemsets.Count, watch.ElapsedMilliseconds);
			}

			queue.Finish();

            #region disposing of buffers

            // входные буферы
            itemsBuf.Dispose();
			transactionsStartsBuf.Dispose();
			customersStartsBuf.Dispose();
			itemsCountBuf.Dispose();
			transactionsCountBuf.Dispose();
			customersCountBuf.Dispose();

            // создание элементовПоддержка
            emptyIdBuf.Dispose();

            // хранение уникальных предметов
            uniqueItemBuf.Dispose();
			uniqueItemsBuf.Dispose();

            // поддерживает хранение
            itemsSupportsBuf.Dispose();
			itemsSupportsCountBuf.Dispose();
			itemsSupportsBufCopy.Dispose();
			transactionsSupportsBuf.Dispose();
			transactionsSupportsCountBuf.Dispose();
			transactionsSupportsBufCopy.Dispose();
			customersSupportsBuf.Dispose();
			customersSupportsCountBuf.Dispose();
			customersSupportsBufCopy.Dispose();

            // временная память
            tempItemsBuf.Dispose();
			itemsRemainingBuf.Dispose();

            // константы
            Zero.Dispose();
			One.Dispose();

			#endregion

			queue.Dispose();
			Kernels.Dispose();

			return litemsets;
		}

	}

	/// @}
}
