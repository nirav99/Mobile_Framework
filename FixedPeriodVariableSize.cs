#define DDEBUG

using System;

namespace Mobile_Framework
{
	/// <summary>
	/// This class implements the Fixed period and variable size algorithm. Each virtual platoon has
	/// period equal to the hyperperiod of the arrival rates on the roads and length of each virtual
	/// platoon is determined based on the arrival rate on that road.
	/// </summary>
	public class FixedPeriodVariableSize : Algorithm
	{
		private int H;					// Hyperperiod
		private int[] eVRoads;			// Execution time (intersection crossing interval) along NS roads)
		private int[] eHRoads;			// Execution time (intersection crossing interval) along EW roads)
		private int[] pLenVRoads;		// Length of platoons along NS roads
		private int[] pLenHRoads;		// Length of platoons along EW roads
		private int[] platoonSizeVRoads;// Number of vehicles in platoons along NS roads
		private int[] platoonSizeHRoads;// Number of vehicles in platoons along EW roads
		private int[] crossingTimeNS;	// Time instants when NS and SN platoons can cross (OFFSET WITHIN HYPERPERIOD)
		private int[] crossingTimeEW;	// Time instants when EW and WE platoons can cross (OFFSET WITHIN HYPERPERIOD)
		private InputParameters ip;		// Reference to input parameter object

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="ip">InputParameters instance</param>
		public FixedPeriodVariableSize(InputParameters _ip) : base(_ip)
		{
			double []temp1, temp2;
			int []temp3;
			int tempVar;
			int idx;

			this.ip = _ip;

			#region Code to normalize input arrival rates
			ip.getArrivalRateVRoads(out temp1);
			ip.getArrivalRateHRoads(out temp2);

			this.normalizeInput(temp1, temp2);
			#endregion

			#region Code to compute hyperperiod
			temp3 = new Int32[arrivalHRoads.Length + arrivalVRoads.Length];

			idx = 0;

			for(int i = 0; i < arrivalHRoads.Length; i++)
				temp3[idx++] = arrivalHRoads[i];
			for(int i = 0; i < arrivalVRoads.Length; i++)
				temp3[idx++] = arrivalVRoads[i];

			H = ComputeLCM.LCM(temp3);
			#endregion

			Console.WriteLine("Hyperperiod = " + H);

			/**
			 * We compute execution time along each intersection as the fraction of the traffic
			 * flowing through that intersection in the direction under consideration. Then, for
			 * each intersection along the road, we find the minimum value of execution time along
			 * that road in the specified direction. That value is the execution time (platoon length)
			 * along that road.
			 */

			#region Code to compute execution time on NS roads

			eVRoads = new int[arrivalVRoads.Length];

			for(int i = 0; i < arrivalVRoads.Length; i++)
			{
				tempVar = Convert.ToInt32(Math.Floor((1.0 * arrivalVRoads[i] / (arrivalVRoads[i] + arrivalHRoads[0]) * H)));
				eVRoads[i] = tempVar;
				
				for(int j = 1; j < arrivalHRoads.Length; j++)
				{
					tempVar = Convert.ToInt32(Math.Floor((1.0 * arrivalVRoads[i] / (arrivalVRoads[i] + arrivalHRoads[j]) * H)));

					if(tempVar < eVRoads[i])
						eVRoads[i] = tempVar;
				}
			}
			#endregion
//#if DDEBUG
			Console.WriteLine("Printing list of E along NS roads");

			for(int i = 0; i < eVRoads.Length; i++)
			{
				Console.WriteLine(eVRoads[i]);
			}
//#endif
			#region Code to compute execution time on EW roads

			eHRoads = new int[arrivalHRoads.Length];

			for(int i = 0; i < arrivalHRoads.Length; i++)
			{
				eHRoads[i] = Convert.ToInt32(Math.Floor((1.0 * arrivalHRoads[i] / (arrivalHRoads[i] + arrivalVRoads[0]) * H)));

				for(int j = 1; j < arrivalVRoads.Length; j++)
				{
					tempVar = Convert.ToInt32(Math.Floor((1.0 * arrivalHRoads[i] / (arrivalHRoads[i] + arrivalVRoads[j]) * H)));

						if(tempVar < eHRoads[i])
							eHRoads[i] = tempVar;
				}
			}
			#endregion
//#if DDEBUG
			Console.WriteLine("Printing list of E along EW roads");

			for(int i = 0; i < eHRoads.Length; i++)
			{
				Console.WriteLine(eHRoads[i]);
			}
//#endif

			/**
			 * From the execution time at each intersection, we can determine the platoon lengths and hence the 
			 * number of vehicles in each platoon.
			 */
			pLenVRoads = new int[eVRoads.Length];
			pLenHRoads = new int[eHRoads.Length];
			platoonSizeVRoads = new int[eVRoads.Length];
			platoonSizeHRoads = new int[eHRoads.Length];

			for(int i = 0; i < eVRoads.Length; i++)
			{
				/**
				 * Compute platoon length as
				 * SpeedLimit * E - W
				 */
				pLenVRoads[i] = ip.speedLimit * eVRoads[i] - ip.lengthW;

				/**
				 * Compute how many vehicles can fit in this platoon
				 */
				tempVar = pLenVRoads[i] / (ip.intraPlatoonDist + ip.vehicleLen);
				platoonSizeVRoads[i] = tempVar;

				/**
				 * Check if one more vehicle can be accomodated in this platoon. We do not need
				 * to consider intra-platoon spacing as it is already considered in the above
				 * expression.
				 */
				if((tempVar = pLenVRoads[i] % (ip.intraPlatoonDist + ip.vehicleLen)) >= ip.vehicleLen)
					platoonSizeVRoads[i] += 1;
//#if DDEBUG
				Console.WriteLine("Platoon Length on NS{0} Road = {1} Number of Vehicles in Platoon = {2}", i, pLenVRoads[i], platoonSizeVRoads[i]);
//#endif
			}
			for(int i = 0; i < eHRoads.Length; i++)
			{
				pLenHRoads[i] = ip.speedLimit * eHRoads[i] - ip.lengthW;

				/**
				 * Compute how many vehicles can fit in this platoon
				 */
				tempVar = pLenHRoads[i] / (ip.intraPlatoonDist + ip.vehicleLen);
				platoonSizeHRoads[i] = tempVar;

				if(pLenHRoads[i] % (ip.intraPlatoonDist + ip.vehicleLen) >= ip.vehicleLen)
					platoonSizeHRoads[i] += 1;
#if DDEBUG
                Console.WriteLine("Platoon Length on EW{0} Road = {1} Number of Vehicles in Platoon = {2}", i, pLenHRoads[i], platoonSizeHRoads[i]);
#endif
			}
			/**
			* Now we compute the time instants when NS (SN) and EW (WE) platoons cross the intersections.
			* Each time instant is the offset from the start of hyperperiod.
			*/

			crossingTimeNS = new int[ip.numHRoads * ip.numVRoads];
			crossingTimeEW = new int[ip.numHRoads * ip.numVRoads];

			Console.WriteLine("Schedule");
			for(int i = 0, j = 0; i < crossingTimeNS.Length; i++)
			{
				crossingTimeNS[i] = 0;
				crossingTimeEW[i] = eVRoads[j];
				Console.WriteLine("Intersection = {0} NS Crossing Time = {1} EW Crossing Time = {2}", i, crossingTimeNS[i], crossingTimeEW[i]);

				if(j < eVRoads.Length - 1)
					j++;
				else
					j = 0;
			}
		}
		
		public override void obtainPlatoonCharacteristics(int roadNum, int roadOrientation, int currTime, out int numVehicles, out int period, out int travelSpeed)
		{
			/**
			 * Each platoon on creation can potentially travel upto speed limit.
			 * The period of each platoon is equal to the hyperperiod.
			 * numVehicles variable contains the number of vehicles in platoon along that road.
			 */
			travelSpeed = ip.speedLimit;
			numVehicles = -1000000;
			period = H;

			if(roadOrientation == RoadOrientation.NS)
			{
				numVehicles = platoonSizeVRoads[roadNum];
			}
			if(roadOrientation == RoadOrientation.EW)
			{
				numVehicles = platoonSizeHRoads[roadNum];
			}
		}

		public override bool allowedToCrossIntersection(int roadNum, int roadOrientation, int intxnNum, int platoonDirn, int startTime, int endTime)
		{
			int numH1;		// Hyperperiod in which startTime lies
			int numH2;		// Hyperperiod in which endTime lies
			int sTime;		// startTime normalized to an offset within hyperperiod
			int eTime;		// endTime normalized to an offset within hyperperiod

			/**
			 * Calculate the hyperperiod in which start time and end times lie.
			 */
			numH1 = (int) Math.Floor(startTime / H * 1.0);
			numH2 = (int) Math.Floor(endTime / H * 1.0);

			/**
			 * If both these hyperperiods are different, return false as platoons cannot cross duringt
			 * that time.
			 */
//			if(numH1 != numH2)
//			{
//				Console.WriteLine("numH1 = " + numH1 + " numH2 = " + numH2);
//				return false;
//			}
			sTime = startTime - numH1 * H;
			eTime = endTime - numH2 * H;

			if(platoonDirn == Direction.NS || platoonDirn == Direction.SN)
			{
				if(sTime < crossingTimeNS[roadNum] || eTime > crossingTimeNS[roadNum] + eVRoads[roadNum])
					return false;
				else
					return true;
			}
			else
			if(platoonDirn == Direction.EW || platoonDirn == Direction.WE)
			{
			if(sTime < crossingTimeEW[roadNum] || eTime > crossingTimeEW[roadNum] + eHRoads[roadNum])
				return false;
			else
				return true;
			}
			return true;
		}

		public override void obtainPlatoonCrossingTimes(int intxnNum, int roadNum, int platoonPosn, int dirn, int currTime, out int startTimeToCross, out int endTimeToCross, out int travelSpeed)
		{
			int cntH;	// Count of hyperperiod
			int offset = -1000000; // Offset within hyperperiod
			
			travelSpeed = ip.speedLimit;

			/**
			 * Find the offset from the start of hyperperiod when platoon can cross.
			 */
			if(dirn == Direction.NS || dirn == Direction.SN)
			{
				offset = crossingTimeNS[intxnNum];
			}
			else
			if(dirn == Direction.EW || dirn == Direction.WE)
			{
				offset = crossingTimeEW[intxnNum];
			}
			
			/**
			 * Find the correct hyperperiod in which platoon crosses.
			 */
			cntH = Convert.ToInt32(Math.Floor(1.0 * currTime / H));
			if((currTime % H) > offset)
				cntH++;

			/**
			 * Compute the actual crossing time.
			 */
			startTimeToCross = cntH * H + offset;

			if(dirn == Direction.NS || dirn == Direction.SN)
			{
				endTimeToCross = startTimeToCross + crossingTimeEW[intxnNum];
			}
			else
			{
				endTimeToCross = startTimeToCross + eHRoads[intxnNum / ip.numVRoads];
			}
		}

		public override void obtainIntersectionCharacteristics(int intxnNum, int roadNum, int dirn, out int startGreenTime, out int endGreenTime, out int travelSpeed)
        {
			travelSpeed = ip.speedLimit;
			startGreenTime = endGreenTime = -1;

			/**
			 * Find the offset from the start of hyperperiod when platoon can cross.
			 */
			if(dirn == Direction.NS || dirn == Direction.SN)
			{
				startGreenTime = crossingTimeNS[intxnNum];
				endGreenTime = crossingTimeEW[intxnNum];
			}
			else
			if(dirn == Direction.EW || dirn == Direction.WE)
			{
				startGreenTime = crossingTimeEW[intxnNum];
				endGreenTime = startGreenTime + eHRoads[intxnNum / ip.numVRoads];
			}
		}

		public override bool scheduleHasGreaterThanOneUtilization()
		{
			return false;
		}
	}
}
