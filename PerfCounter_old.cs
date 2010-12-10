using System;
using System.Collections;

namespace Mobile_Framework
{
	/// <summary>
	/// Counters collected from a platoon
	/// </summary>
	public class PlatoonPerfCounter
	{
		private ulong totalTransitTime;	// Total transit time all vehicles in the platoon
		private ulong totalWaitTime;	// Total waiting time for all vehicles in platoon
		private ulong totalJourneyTime;	// Total journey time for all vehicles in platoon
		private ulong numVehicles;		// Num vehicles in platoon
		private ulong totalDistTraveled;// Dist travelled by platoon
		private ulong totalStopCount;	// Total stop count of all vehicles in platoon
	
		/// <summary>
		/// Default Class constructor
		/// </summary>
		public PlatoonPerfCounter()
		{
			totalTransitTime = totalWaitTime = 0;
			totalDistTraveled = numVehicles = totalStopCount = 0;
			totalJourneyTime = 0;
		}

		/// <summary>
		/// Parameterized class constructor
		/// </summary>
		/// <param name="totalTransitTime"></param>
		/// <param name="totalWaitTime"></param>
		/// <param name="totalJourneyTime"></param>
		/// <param name="numVehicles"></param>
		/// <param name="totalDistTraveled"></param>
		/// <param name="totalStopCount"></param>
		public PlatoonPerfCounter(ulong totalTransitTime, ulong totalWaitTime, ulong totalJourneyTime, ulong numVehicles, ulong totalDistTraveled, ulong totalStopCount)
		{
			this.totalTransitTime = totalTransitTime;
			this.totalWaitTime = totalWaitTime;
			this.totalJourneyTime = totalJourneyTime;
			this.numVehicles = numVehicles;
			this.totalDistTraveled = totalDistTraveled;
			this.totalStopCount = totalStopCount;
		}

		/// <summary>
		/// Print the state of the object
		/// </summary>
		public void printCounters()
		{
			Console.WriteLine("Total Transit Time = " + totalTransitTime);
			Console.WriteLine("Total Wait Time = " + totalWaitTime);
			Console.WriteLine("Total Journey Time = " + totalJourneyTime);
			Console.WriteLine("Total Vehicles = " + numVehicles);
			Console.WriteLine("Total Dist Traveled = " + totalDistTraveled);
			Console.WriteLine("Total Stop Count = " + totalStopCount);
		}

		/// <summary>
		/// Returns the state of the object
		/// </summary>
		/// <param name="_totalTransitTime"></param>
		/// <param name="_totalWaitTime"></param>
		/// <param name="_totalJourneyTime"></param>
		/// <param name="_numVehicles"></param>
		/// <param name="_totalDistTraveled"></param>
		/// <param name="_totalStopCount"></param>
		public void getState(out ulong _totalTransitTime, out ulong _totalWaitTime, out ulong _totalJourneyTime, out ulong _numVehicles, out ulong _totalDistTraveled, out ulong _totalStopCount)
		{
			_totalTransitTime = this.totalTransitTime;
			_totalWaitTime = this.totalWaitTime;
			_totalJourneyTime = this.totalJourneyTime;
			_numVehicles = this.numVehicles;
			_totalDistTraveled = this.totalDistTraveled;
			_totalStopCount = this.totalStopCount;
		}
	}

	/// <summary>
	/// Counters collected on a road
	/// </summary>
	public class RoadPerfCounter
	{
		private ArrayList platoonCntObj;		// Vector of platoon counter objects
		private ulong totalPlatoonArrivals;		// Total platoons created on this road
		private ulong totalPlatoonDepartures;	// Total platoons departed from this road
		private ulong totalVehiclesArrived;		// Total number of vehicles arrived on this road
		private ulong totalVehiclesDeparted;	// Total number of departed vehicles on this road
		private ulong totalTransitTime;			// Total Transit Time on the road
		private ulong totalWaitTime;			// Total Wait Time on the road
		private ulong totalJourneyTime;			// Total Journey Time (Transit - Wait)
		private ulong totalDistanceTraveled;	// Total Distance platoon traveled
		private ulong totalStopCount;			// Total Number of Stops
		private ulong totalNumVehicles;			// Total Number of Vehicles

		private double avgTransitTime;			// Avg. transit time 
		private double avgWaitTime;				// Avg. wait time
		private double avgJourneyTime;			// Avg. journey time
		private double avgDistTraveled;			// Avg. distance traveled
		private double avgPlatoonSize;			// Avg. number of vehicles in platoon
		/// <summary>
		/// Class constructor
		/// </summary>
		public RoadPerfCounter()
		{
			platoonCntObj = new ArrayList();
			totalPlatoonArrivals = totalPlatoonDepartures = 0;
			totalVehiclesArrived = totalVehiclesDeparted = 0;

			totalTransitTime = 0;
			totalWaitTime = 0;
			totalJourneyTime = 0;
			totalDistanceTraveled = 0;
			totalStopCount = 0;
			totalNumVehicles = 0;

			avgTransitTime = avgWaitTime = avgJourneyTime = avgDistTraveled = 0;
			avgPlatoonSize = 0;
		}

		/// <summary>
		/// Method to store a counter object for departed platoon
		/// </summary>
		/// <param name="pPC">PlatoonPerfCounter object</param>
		public void addPlatoonCounter(PlatoonPerfCounter pPC)
		{
			platoonCntObj.Add(pPC);
		}

		public void vehicleCreated()
		{
			totalVehiclesArrived++;
		}

		/// <summary>
		/// Method to update count of number of platoons created on this road
		/// </summary>
		public void platoonCreated()
		{
			totalPlatoonArrivals++;
		}

		/// <summary>
		/// Method to update count of number of platoons departed on this road
		/// </summary>
		public void platoonDeparted()
		{
			totalPlatoonDepartures++;
		}

		/// <summary>
		/// Returns the total vehicles arrived on this road
		/// </summary>
		/// <returns>Total vehicles arrived on this road</returns>
		public ulong getTotalVehicleArrivals()
		{
			return totalVehiclesArrived;
		}

		
		/// <summary>
		/// Computes the statistics for the road
		/// </summary>
		public void computeStatistics()
		{
			ulong tTTime;
			ulong tWTime;
			ulong tJTime;
			ulong tDTrav;
			ulong tSCnt;
			ulong tNVeh;

			PlatoonPerfCounter p;

			for(int i = 0; i < platoonCntObj.Count; i++)
			{
				p = (PlatoonPerfCounter) platoonCntObj[i];

				p.getState(out tTTime, out tWTime, out tJTime, out tNVeh, out tDTrav, out tSCnt);

				totalTransitTime += tTTime;
				totalWaitTime += tWTime;
				totalJourneyTime += tJTime;
				totalNumVehicles += tNVeh;
				totalDistanceTraveled += tDTrav;
				totalStopCount += tSCnt;
			}

			avgTransitTime = totalTransitTime * 1.0 / totalNumVehicles;
			avgWaitTime = totalWaitTime * 1.0 / totalNumVehicles;
			avgJourneyTime = totalJourneyTime * 1.0 / totalNumVehicles;
			avgDistTraveled = totalDistanceTraveled * 1.0 / totalPlatoonDepartures;
			avgPlatoonSize = totalNumVehicles * 1.0 / totalPlatoonDepartures;
		}

		/// <summary>
		/// Gets statistics of the road
		/// </summary>
		public void getStatistics()
		{
			computeStatistics();
			Console.WriteLine("Total Vehicles Arrived = " + totalVehiclesArrived);
			Console.WriteLine("Total Vehicles Departed (from Platoon obj) = " + totalNumVehicles);
			Console.WriteLine("Total Platoons Arrived = " + totalPlatoonArrivals);
			Console.WriteLine("Total Platoons Departed = " + totalPlatoonDepartures);

			Console.WriteLine("Avg Platoon Size = {0} Vehicles", avgPlatoonSize);

			Console.WriteLine("Avg. Transit Time = " + avgTransitTime);
			Console.WriteLine("Avg. Wait Time = " + avgWaitTime);
			Console.WriteLine("Avg. Journey Time = " + avgJourneyTime);
			Console.WriteLine("Avg. Dist Traveled = " + avgDistTraveled);
		}
	}

	public class RoadStatistics
	{
		private ulong totalVehiclesArrived;
		private ulong totalNumVehicles;
		private ulong totalPlatoonArrivals;
		private ulong totalPlatoonDepartures;
		private double avgPlatoonSize;
		private double avgTransitTime;
		private double avgWaitTime;
		private double avgJourneyTime;
		private double avgDistTraveled;

		public RoadStatistics()
		{
			totalVehiclesArrived = totalNumVehicles = totalPlatoonArrivals = totalPlatoonDepartures = 0;
			avgPlatoonSize = avgTransitTime = avgWaitTime = avgJourneyTime = avgDistTraveled = 0;
		}
	}

}
