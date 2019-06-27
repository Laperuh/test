/// \addtogroup opencl
/// @{

/*!
	Counts distinct item values in a list, and creates an ID
	for each unique value.

	\param items array of all items of all transactions of input
	\param itemsCount length of items array
	\param itemsExcluded when 1, a corresponding element from items array 
		is already established as a distinct item
	\param step current distance between two compared items, a power of 2
 */
__kernel void findNewDistinctItem(
	__global const int* items,
	__global const int* itemsCount,
	__global const int* itemsExcluded,
	__global int* newItemsExcluded,
	//__global int* uniqueItems,
	//__global int* uniqueItemsCount,
	__global int* discovered,
	__global const int* step
	)
{
	int x = get_global_id(0);
	if(x < *itemsCount && discovered[x] == 0 && *step > 0 && x % (2 * *step) == 0)
	{
		if(itemsExcluded[x] == 0)
			discovered[x] = items[x];
		else if(itemsExcluded[x + *step] == 0)
		{
			discovered[x] = items[x + *step];
			newItemsExcluded[x] = 0;
		}
		else if(newItemsExcluded[x + *step] == 0)
		{
			discovered[x] = discovered[x + *step];
		}
	}
}

/*!
	Excludes latest discovered distinct value from items.

	\param items array of all items of all transactions of input
	\param itemsCount length of items array
	\param itemsExcluded when 1, a corresponding element from items array 
		is already present in uniqueItems array
 */
__kernel void excludeLatestDistinctItem(
	__global const int* items,
	__global const int* itemsCount,
	__global int* itemsExcluded,
	__global const int* uniqueItems,
	__global const int* uniqueItemsCount
	)
{
	int x = get_global_id(0);
	if(x < *itemsCount)
	{
		if(*uniqueItemsCount == 0)
			itemsExcluded[x] = 0;
		else if(items[x] == uniqueItems[*uniqueItemsCount - 1])
			itemsExcluded[x] = 1;
	}
}

/*!
	Puts 1 in supports array where the unique item exists.
	
	\param items array of all items of all transactions of input
	\param itemsCount length of items array
	\param supports output
 */
__kernel void separateUniqueItems(
	__global const int* items,
	__global const int* itemsCount,
	//__global const int* itemsTransactions,
	__global const int* uniqueItems,
	__global const int* uniqueItemsCount,
	__global int* supports
	)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	if(x < *itemsCount && y < *uniqueItemsCount)
	{
		int supportsIndex = y * *itemsCount + x;
		if(items[x] == uniqueItems[y])
			supports[supportsIndex] = 1;
		else
			supports[supportsIndex] = 0;
	}
}

/*!
	Initializes supports array.
	
	\param items array of all items of all transactions of input
	\param itemsCount length of items array
 */
__kernel void supportInitial(
	__global const int* items,
	__global const int* itemsCount,
	__global const int* uniqueItems,
	__global const int* uniqueItemsCount,
	__global int* supports
	)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	if(x < *itemsCount && y < *uniqueItemsCount)
	{
		int supportsIndex = y * *itemsCount + x;
		if(items[x] == uniqueItems[y])
			supports[supportsIndex] = 1;
		else
			supports[supportsIndex] = 0;
	}
}

/*!
	Calculates support of single items, i.e.
	counts how many of each is stored if _different_ transactions.

	\param items array of all items of all transactions of input
	\param itemsCount length of items array
	\param itemsTransactions array with numbers from zero onward that indicates, 
		to which transaction a corresponding element of items array belongs
	\param step current distance between two compared items, a power of 2
 */
__kernel void supportDuplicatesRemoval(
	__global const int* items,
	__global const int* itemsCount,
	__global const int* itemsTransactions,
	__global const int* uniqueItems,
	__global const int* uniqueItemsCount,
	__global const int* step,
	__global int* supports
	)
{
	int x = get_global_id(0);
	int y = get_global_id(1);
	if(x < *itemsCount && y < *uniqueItemsCount && *step > 0 && x % (2 * *step) == 0)
	{
		int supportsIndex = y * *itemsCount + x;
		if(itemsTransactions[x] == uniqueItems[y] && itemsTransactions[x] == itemsTransactions[x + *step]
			&& supports[supportsIndex + *step] == 1)
		{
			supports[supportsIndex] = 1;
			supports[supportsIndex + *step] = 0;
		}
		//if(itemsTransactions[x] != itemsTransactions[x + *step])
		//	supports[supportsIndex] = supports[supportsIndex];  + supports[supportsIndex + *step];
		//else if(itemsTransactions[x + *step] != itemsTransactions[x + (*step / 2)])
		//	supports[supportsIndex] = supports[supportsIndex] + supports[supportsIndex + *step];
		//else
		//	supports[supportsIndex] = fmax(supports[supportsIndex], supports[supportsIndex + *step]);
	}
}

/// @}
