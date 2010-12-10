using System;

namespace Mobile_Framework
{
	/// <summary>
	/// This class represents the basic interface of the clock-driven algorithm used to 
	/// generate the virtual platoons and to schedule them at the intersections.
	/// </summary>
	abstract public class Algorithm
	{
		protected int[] arrivalVRoads;	// Arrival rate on NS roads (normalized to have integral values)
		protected int[] arrivalHRoads;	// Arrival rate on EW roads (normalized to have integral values)
		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="ip">Inputparameters instance</param>
		public Algorithm(InputParameters ip)
		{
			arrivalVRoads = arrivalHRoads = null;
		}

		/// <summary>
		/// This method is used to normalize the arrival rates such that minimum arrival rate is at least 1.
		/// </summary>
		/// <param name="input1">Arrival rate for NS roads</param>
		/// <param name="input2">Arrival rate for EW roads</param>
		protected void normalizeInput(double[] input1, double []input2)
		{
			double minVal;
			int cnt = 0;

			minVal = input1[0];

			for(int i = 1; i < input1.Length; i++)
				if(input1[i] < minVal)
				{
					minVal = input1[i];
				}

			for(int i = 0; i < input2.Length; i++)
				if(input2[i] < minVal)
					minVal = input2[i];

			while(minVal < 1.0 && minVal != 0)
			{
				minVal = minVal * 10;
				cnt++;
			}

			arrivalVRoads = new int[input1.Length];

			for(int i = 0; i < input1.Length; i++)
			{
				arrivalVRoads[i] = Convert.ToInt32(Math.Ceiling(input1[i] * Math.Pow(10, cnt)));
			}

			arrivalHRoads = new int[input2.Length];

			for(int i = 0; i < input2.Length; i++)
				arrivalHRoads[i] = Convert.ToInt32(Math.Ceiling(input2[i] * Math.Pow(10, cnt)));
		}

		/// <summary>
		/// This method defines the interface to obtain platoon characteristics. Platoon
		/// charcteristics include the number of vehicles in a platoon, period between their respective
		/// creation time on the specified road.
		/// </summary>
		/// <param name="roadNum">Road number for which platoon characteristics are desired</param>
		/// <param name="currTime">Current time</param>
		/// <param name="numVehicles">Number of vehicles in that platoon</param>
		/// <param name="period">Time interval between successive platoons on that road</param>
		/// <param name="travelSpeed">Initial speed of travel for the platoon</param>
		abstract public void obtainPlatoonCharacteristics(int roadNum, int roadOrientation, int currTime, out int numVehicles, out int period, out int travelSpeed);

		/// <summary>
		/// Method that determines if the platoon is allowed to cross intersection at specified times
		/// </summary>
		/// <param name="roadNum">Road number on which specified runs </param>
		/// <param name="roadOrientation"> Road's orientation</param>
		/// <param name="intxnNum">Intersection number to cross</param>
		/// <param name="platoonDirn">Platoon's direction</param>
		/// <param name="startTime">Time when platoon requests to cross intersection</param>
		/// <param name="endTime">Time when platoon requests to clear intersection</param>
		/// <returns>True if platoon can cross within the specified time, false otherwise</returns>
		abstract public bool allowedToCrossIntersection(int roadNum, int roadOrientation, int intxnNum, int platoonDirn, int startTime, int endTime);

		/// <summary>
		/// This method defines the interface to obtain the scheduling information of platoons at intersections.
		/// </summary>
		/// <param name="intxnNum">Intersection number</param>
		/// <param name="roadNum">Road number of which platoon is moving</param>
		/// <param name="platoonPosn">Position of the platoon on that road</param>
		/// <param name="dirn">Direction(s) of the platoon after crossing the intersection</param>
		/// <param name="currTime">Current time when platoon reaches intersection</param>
		/// <param name="startTimeToCross">Absolute time when platoon can start crossing</param>
		/// <param name="endTimeToCross">Absolute time when platoon must clear that intersection</param>
		/// <param name="travelSpeed">Speed with which platoon must travel from that intersection</param>
		abstract public void obtainPlatoonCrossingTimes(int intxnNum, int roadNum, int platoonPosn, int dirn, int currTime, out int startTimeToCross, out int endTimeToCross, out int travelSpeed);

		/// <summary>
		/// This method defines an interface to query the green times of a specified intersection for a specified direction
		/// </summary>
		/// <param name="intxnNum">Intersection number</param>
		/// <param name="roadNum">Road number</param>
		/// <param name="dirn">Direction of platoon movement for which green times are required</param>
		/// <param name="startCrossTime">Time offset from start of hyperperiod when intersection shows greem for specified direction</param>
		/// <param name="endCrossTime">Time offset from start of hyperperiod when intersection turns red for specified direction</param>
		/// <param name="travelSpeed">Speed with which platoon must travel -- unclear how to use right now</param>
		abstract public void obtainIntersectionCharacteristics(int intxnNum, int roadNum, int dirn, out int startGreenTime, out int endGreenTime, out int travelSpeed);

		/// <summary>
		/// This method returns true if the utilization of any intersection can be greater than one, else false.
		/// </summary>
		/// <returns>True if utilization of all intersections is not more than one, false otherwise</returns>
		abstract public bool scheduleHasGreaterThanOneUtilization();
	}
}
