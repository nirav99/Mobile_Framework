using System;
using System.Collections;

namespace Mobile_Framework
{
	/// <summary>
	/// This class represents a single vehicle. It may or may not be contained inside a platoon.
	/// </summary>
	public class Vehicle
	{
		private int createTime;			// Time when vehicle is created
		internal int vehicleLen;		// Length of a vehicle
		private static uint totalVehicles = 0;	// Total number of vehicles created in the system
		private uint vehNum;			// Vehicle Number
		private int []pathToVisit;		// List of intersections to visit
		private int idxNextIntxnToVisit;// Index of last intersection visited

		private int startJourneyTime;	// Time when vehicle is scheduled in a platoon
		private int stopCount;			// Number of times vehicles stopped - unused for now

		/// <summary>
		/// Vehicle class constructor. 
		/// </summary>
		/// <param name="_createTime">An integer representing time when vehicle is created</param>
		public Vehicle(int _createTime, int _vehicleLen, int []_pathToVisit)
		{
			vehicleLen = _vehicleLen;
			createTime = _createTime;
			vehNum = ++totalVehicles;

			startJourneyTime = -1;

			stopCount = 0;
			/**
			 * Copy the list of intersections to pathToVisit array and then initialize
			 * pathVisited to empty.
			 */
			pathToVisit = _pathToVisit;
		
			/**
			 * Set the index of the next intersection to visit as zero. (starting index of
			 * pathToVisit array.
			 */
			if(pathToVisit != null)
				idxNextIntxnToVisit = 0;
			else
				idxNextIntxnToVisit = -1;
		}

		/// <summary>
		/// Returns the total number of vehicles created in the system from system startup
		/// </summary>
		/// <returns>An int representing total number of vehicles</returns>
		public static uint getTotalVehicles()
		{
			return totalVehicles;
		}

		/// <summary>
		/// Returns the intersection number of the next intersection to visit. If the path is
		/// not set, return -100000. Returns -1 when all the intersections are visited.
		/// </summary>
		/// <returns>The next intersection to visit or -100000 if the path is not set and returns
		/// -1 if all the intersections are visited</returns>
		public int nextIntersectionToVisit()
		{
			if(pathToVisit == null)
				return -100000;
			else
			{
				/**
				 * If intersections are to be visited, return the intersection number 
				 * else return -1.
				 */
				if(idxNextIntxnToVisit < pathToVisit.Length)
					return pathToVisit[idxNextIntxnToVisit];
				else
					return -1;
			}
		}

		/// <summary>
		/// To be invoked on visiting an intersection. It updates the index pointing to the next
		/// intersection.
		/// </summary>
		public void visitedIntersection()
		{
			if(pathToVisit != null)
			{
				idxNextIntxnToVisit++;
			}
		}

		/// <summary>
		/// Set vehicle's starting journey time - when vehicle is scheduled in platoon
		/// </summary>
		/// <param name="currTime">Current time</param>
		public void setStartJourneyTime(int currTime)
		{
			startJourneyTime = currTime;
		}

		/// <summary>
		/// Get waiting time for the vehicle
		/// </summary>
		/// <returns>Time interval for which vehicle had to wait</returns>
		public int getWaitingTime(int currTime)
		{
			if(startJourneyTime >= 0)
				return (startJourneyTime - createTime);
			else
				return (currTime - createTime);

		}

		public int getWaitingTime()
		{
			return startJourneyTime - createTime;
		}

		/// <summary>
		/// Get journey time for the vehicle
		/// </summary>
		/// <param name="currTime"></param>
		/// <returns>Journey time of vehicle</returns>
		public int journeyTime(int currTime)
		{
			return currTime - startJourneyTime;
		}

		/// <summary>
		/// Returns the total transit time of vehicle
		/// </summary>
		/// <param name="currTime">An integer representing the current time</param>
		/// <returns>An integer representing the total delay vehicle faced in the system</returns>
		public int getTransitTime(int currTime)
		{
			return currTime - createTime;
		}

		/// <summary>
		/// Returns stop count for vehicle
		/// </summary>
		/// <returns>Stop count</returns>
		public int getStopCount()
		{
			return stopCount;
		}
	}
}
