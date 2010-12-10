#define DDEBUG

using System;

namespace Mobile_Framework
{
	/// <summary>
	/// This class represents the Fixed platoon size variable period algorithm. Each platoon's
	/// size limit is set to a fixed number of vehicles. The period of platoons is varied in 
	/// proportion to the incoming traffic.
	/// </summary>
	public class FixedSizeVariablePeriod : Algorithm
	{
		private int H;							// Hyperperiod
		private double maxArrivalRateVRoads;	// Max. arrival rate among all NS roads
		private double maxArrivalRateHRoads;	// Max. arrival rate among all EW roads
		private double k;						// Represents parameter "k" used in the paper
		private int platoonSize;				// Size (num vehicles) of platoons across all roads
		private int platoonLen;					// Length of platoon across all roads
		private int executionTime;				// Time allocated to each platoon on the intersection
		private int periodNS;					// Period of platoons along all NS roads
		private int periodEW;					// Period of platoons along all EW roads
		private int[] crossingTimeNS;			// Time instants when NS and SN platoons can start crossing (OFFSET WITHIN HYPERPERIOD)
		private int[] crossingTimeEW;			// Time instants when EW and WE platoons can start crossing (OFFSET WITHIN HYPERPERIOD)
		private InputParameters ip;				// Reference to input parameter object

		/// <summary>
		/// Class Constructor
		/// </summary>
		/// <param name="_ip">InputParameters instance</param>
		public FixedSizeVariablePeriod(InputParameters _ip) : base(_ip)
		{
			double []temp1;		// Temporary variable
			this.ip = _ip;

			/**
			 * NOTE: We are setting 10 vehicles per platoon in the algorithm
			 */
			platoonSize = 10;
			platoonLen = platoonSize * ip.vehicleLen + (platoonSize - 1) * ip.intraPlatoonDist;

			/**
			 * The first step is to compute the execution time for the platoons.
			 * Execution time is computed as the total distance that a platoon must travel at speed limit on the intersection
			 * such that it's trailing end just clears the intersection. We take ceiling of that value for safety considerations.
			 */
			executionTime = (int) Math.Ceiling((ip.lengthW + platoonLen) * 1.0 / ip.speedLimit);
						
			/**
			 * Obtain arrival rate on NS and EW roads.
			 */
			ip.getArrivalRateVRoads(out temp1);

			/**
			 * Obtain the maximum arrival rate among all NS roads.
			 */
            maxArrivalRateVRoads = temp1[0];

			for(int i = 1; i < temp1.Length; i++)
			{
				if(maxArrivalRateVRoads < temp1[i])
					maxArrivalRateVRoads = temp1[i];
			}

			/**
			 * Repeat the same to get maximum arrival rate among all EW roads.
			 */
			ip.getArrivalRateHRoads(out temp1);
			maxArrivalRateHRoads = temp1[0];

			for(int i = 1; i < temp1.Length; i++)
			{
				if(maxArrivalRateHRoads < temp1[i])
					maxArrivalRateHRoads = temp1[i];
			}

			/**
			 * Now compute K and hence obtain period for NS platoons and period for EW platoons
			 */
			if(maxArrivalRateVRoads >= maxArrivalRateHRoads)
			{
				k = maxArrivalRateVRoads * 1.0 / maxArrivalRateHRoads;
				periodNS = (int) Math.Ceiling(executionTime * 1.0 * (1 + k) / k);
				periodEW = (int) Math.Ceiling(executionTime * 1.0 * (1 + k));
			}
			else
			{
				k = maxArrivalRateHRoads * 1.0 / maxArrivalRateVRoads;
				periodNS = (int) Math.Ceiling(executionTime * 1.0 * (1 + k));
				periodEW = (int) Math.Ceiling(executionTime * 1.0 * (1 + k) / k);
			}
			/**
			 * Compute hyperperiod now.
			 */
			int []tempArray = new int[2];
			tempArray[0] = periodNS;
			tempArray[1] = periodEW;
			H = ComputeLCM.LCM(tempArray);

			Console.WriteLine("Period NS = {0} Period EW = {1} H = {2} e = {3}", periodNS, periodEW, H, executionTime);

			double util; // utilization of intersection
			util = executionTime * 1.0 / periodNS + executionTime * 1.0 / periodEW;

			if(util > 1.0)
			{
				Console.WriteLine("Utilization EXCEEDS one. Actual Value = {0}", util);
			}
			computeClockSchedule();
		}

		/// <summary>
		/// Method to compute a clock-driven schedule given period of jobs and execution times.
		/// </summary>
		private void computeClockSchedule()
		{
			int idx1, idx2;
			int nextTimeNSArrival = 0;		// Time of next arrival of NS job
			int nextTimeEWArrival = 0;		// Time of next arrival of EW job
			int currentTime = 0;

			idx1 = idx2 = 0;

			/**
			 * Allocate the arrays to hold the times when NS and EW platoons can start
			 * crossing the intersection.
			 */
			crossingTimeNS = new int[H / periodNS];
			crossingTimeEW = new int[H / periodEW];

			for(currentTime = 0; currentTime < H;)
			{
				if(currentTime >= nextTimeNSArrival)
				{
					// Schedule NS job
					crossingTimeNS[idx1++] = currentTime;
					nextTimeNSArrival += periodNS;
					currentTime += executionTime;
				}
				if(currentTime >= nextTimeEWArrival)
				{
					// Schedule EW job
					crossingTimeEW[idx2++] = currentTime;
					nextTimeEWArrival += periodEW;
					currentTime += executionTime;
				}
				if(currentTime < nextTimeNSArrival && currentTime < nextTimeEWArrival)
					currentTime++;
			}
#if DDEBUG
			Console.WriteLine("Schedule for NS/SN platoons");
			for(int i = 0; i < crossingTimeNS.Length; i++)
			{
				Console.WriteLine("Start = {0} End = {1}", crossingTimeNS[i], crossingTimeNS[i] + executionTime);
			}

			Console.WriteLine("Schedule for EW/WE platoons");
			for(int i = 0; i < crossingTimeEW.Length; i++)
			{
				Console.WriteLine("Start = {0} End = {1}", crossingTimeEW[i], crossingTimeEW[i] + executionTime);
			}
#endif
		}

		public override void obtainPlatoonCharacteristics(int roadNum, int roadOrientation, int currTime, out int numVehicles, out int period, out int travelSpeed)
		{
			/**
			 * Each platoon on creation can potentially travel upto speed limit.
			 * Period of NS and EW jobs is different. It is determined by periodNS and periodEW.
			 * The number of vehicles for both NS and EW platoons is fixed.
			 */
			period = -1;
			travelSpeed = ip.speedLimit;
			numVehicles = this.platoonSize;
			
			if(roadOrientation == RoadOrientation.NS)
			{
				period = this.periodNS;
			}
			if(roadOrientation == RoadOrientation.EW)
			{
				period = this.periodEW;
			}
		}

		public override void obtainPlatoonCrossingTimes(int intxnNum, int roadNum, int platoonPosn, int dirn, int currTime, out int startTimeToCross, out int endTimeToCross, out int travelSpeed)
		{
			startTimeToCross = endTimeToCross = travelSpeed = -1;
		}

		public override bool allowedToCrossIntersection(int roadNum, int roadOrientation, int intxnNum, int platoonDirn, int startTime, int endTime)
		{
			int numH1;		// Hyperperiod in which startTime lies
			int numH2;		// Hyperperiod in which endTime lies
			int sTime;		// startTime normalized to an offset within hyperperiod
			int eTime;		// endTime normalized to an offset within hyperperiod
			int []timeToCross = null;	// Temp variable to point to crossingTimeNS or crossingTimeEW

			/**
			 * Calculate the hyperperiod in which start time and end times lie.
			 */
			numH1 = (int) Math.Floor(startTime * 1.0/ H );
			numH2 = (int) Math.Floor(endTime * 1.0/ H );

			/**
			 * If both these hyperperiods are different, return false as platoons cannot cross duringt
			 * that time.
			 */
			//if(numH1 != numH2)
			//	return false;
			
			/**
			 * Can also be written as sTime = startTime % numH1
			 */
			sTime = startTime - numH1 * H;
			eTime = endTime - numH2 * H;

			if(platoonDirn == Direction.NS || platoonDirn == Direction.SN)
			{
				timeToCross = crossingTimeNS;
			}
			if(platoonDirn == Direction.EW || platoonDirn == Direction.WE)
			{
				timeToCross = crossingTimeEW;
			}
			
			for(int i = 0; i < timeToCross.Length; i++)
			{
				if(sTime >= timeToCross[i] && eTime <= timeToCross[i] + executionTime)
					return true;
			}
			return false;
		}

		public override void obtainIntersectionCharacteristics(int intxnNum, int roadNum, int dirn, out int startGreenTime, out int endGreenTime, out int travelSpeed)
		{
			startGreenTime = endGreenTime = travelSpeed = -1;
		}

		public override bool scheduleHasGreaterThanOneUtilization()
		{
			return false;
		}
	}
}
