using System;

namespace Mobile_Framework
{
	/// <summary>
	/// This class represents the intersection algorithm where all intersections have same phase
	/// timings. Moreover, there's no control over platoon creation and each platoon contains only
	/// one vehicle.
	/// </summary>
	public class FixedIntersectionTiming : Algorithm
	{
		private int platoonSize;		// Size of platoon - fixed for all roads
		private int H;					// Hyperperiod
		private int greenPhaseInterval;	// Execution time (intersection crossing interval) - FIXED for all intersections
		
		private InputParameters ip;		// Reference to input parameter object

		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="ip">InputParameters instance</param>
		public FixedIntersectionTiming(InputParameters _ip) : base(_ip)
		{
			/**
			 * This is the most basic algorithm where there's exactly one vehicle per platoon and fixed green time interval of 20
			 * per phase. Moreover, it's possible to create a new platoon each time instant - thus we set H = 1.
			 */
			ip = _ip;
			platoonSize = 1;
			greenPhaseInterval = 20;
			H = 1;
		}

		public override void obtainPlatoonCharacteristics(int roadNum, int roadOrientation, int currTime, out int numVehicles, out int period, out int travelSpeed)
		{
			/**
			 * Each platoon on creation can potentially travel upto speed limit.
			 * The period of each platoon is equal to the hyperperiod.
			 * numVehicles variable contains the number of vehicles in platoon along that road.
			 */
			travelSpeed = ip.speedLimit;
			numVehicles = platoonSize;
			period = H;
		}

		public override void obtainPlatoonCrossingTimes(int intxnNum, int roadNum, int platoonPosn, int dirn, int currTime, out int startTimeToCross, out int endTimeToCross, out int travelSpeed)
		{
			Console.WriteLine("FixedIntersectionTiming Algorithm: obtainPlatoonCrossingTimes NOT implemented");

			startTimeToCross = endTimeToCross = -1;
			travelSpeed = -1;
		}

		public override bool allowedToCrossIntersection(int roadNum, int roadOrientation, int intxnNum, int platoonDirn, int startTime, int endTime)
		{
			int temp1, temp2;

		//	temp1 = (int) Math.Floor( 1.0 * (startTime / (2.0 * greenPhaseInterval)));
		//	temp2 = (int) Math.Floor(1.0 * (endTime / (2.0 * greenPhaseInterval)));

			temp1 = startTime  % ( 2* greenPhaseInterval);
			temp2 = endTime  % (2 * greenPhaseInterval);
			
			if(platoonDirn == Direction.NS || platoonDirn == Direction.SN)
			{
				if(temp1 >= 0 && temp2 <= greenPhaseInterval)
					return true;
				else
					return false;
			}

			if(platoonDirn == Direction.EW || platoonDirn == Direction.WE)
			{
				if(temp1 >= greenPhaseInterval && temp2 <= 2 * greenPhaseInterval)
					return true;
				else
					return false;
			}
			return false;	// To satisfy compiler
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
				startGreenTime = 0;
				endGreenTime = greenPhaseInterval - 1;
			}
			else
				if(dirn == Direction.EW || dirn == Direction.WE)
			{
				startGreenTime = greenPhaseInterval;
				endGreenTime = greenPhaseInterval + greenPhaseInterval - 1;
			}
		}

		public override bool scheduleHasGreaterThanOneUtilization()
		{
			return true;
		}
	}
}
