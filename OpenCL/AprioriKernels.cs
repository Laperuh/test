using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OpenCL.Abstract;

namespace AprioriAllLib
{
	/// <summary>
	/// Linked to the kernel code responsible for calculating support of Litemsets of length 2 and larger.
	/// </summary>
	public class MultiItemSupportKernel : Kernel
	{
		//private Buffer<int> dataBuffer;

		//private Buffer<int> lengthBuffer;

		//private Buffer<int> zoneLengthBuffer;

		//private Buffer<int> zonesStartsBuffer;

		//private Buffer<int> zonesCountBuffer;

		//private Buffer<int> includedZonesBuffer;

		//private Buffer<int> includedZonesCountBuffer;

		//private Buffer<int> stepBuffer;

		public MultiItemSupportKernel(Program program, string kernelName)
			: base(program, kernelName)
		{
		}

		//public void SetArguments(Buffer<int> transactionsSupports, Buffer<int> transactionsSupportsCount,
		//	Buffer<int> itemsCount, Buffer<int> itemsTimesUniqueSequence, Buffer<int> uniqueItemsCount,
		//	Buffer<int> candidateSequence, Buffer<int> candidateSequenceLength, Buffer<int> reductionStep)
		//{
		//	this.dataBuffer = b;
		//	this.lengthBuffer = bLength;
		//	this.zoneLengthBuffer = itemsCount;
		//	this.zonesStartsBuffer = itemsTimesUniqueSequence;
		//	this.zonesCountBuffer = uniqueItemsCount;
		//	this.includedZonesBuffer = candidateSequence;
		//	this.includedZonesCountBuffer = candidateSequenceLength;
		//	this.stepBuffer = reductionStep;

		//	base.SetArguments(dataBuffer, lengthBuffer, zoneLengthBuffer, zonesStartsBuffer,
		//		zonesCountBuffer, includedZonesBuffer, includedZonesCountBuffer, stepBuffer);
		//}

		/// <summary>
		/// Launches the Kernel on a given command queue.
		/// </summary>
		/// <param name="queue">command queue</param>
		/// <param name="zoneLength"></param>
		/// <param name="zonesCount"></param>
		public void Launch(CommandQueue queue, uint zoneLength, uint zonesCount)
		{
			Launch2D(queue, zoneLength, 1, zonesCount, 1);
		}

	}
}
