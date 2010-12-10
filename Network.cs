using System;
using System.Collections;
using System.IO;

namespace Mobile_Framework
{
	/// <summary>
	/// Network of roads
	/// </summary>
	public class Network
	{
		#region Structural parameters for the network
		protected InputParameters ip;	// Class containing the parameters of the algorithm
		private int numHRoads;		// Number of Horizontal roads
		private int numVRoads;		// number of vertical roads
		private int R;				// Length of road segment between intersections
		private int W;				// Length of an intersection
		private int speedLimit;		// Speed limit over the network
		private ArrayList horRoads;	// Array of horizontal roads
		private ArrayList vertRoads;// Array of vertical roads
		#endregion

		protected Random rN;		// Random number generator
		protected Algorithm algo;	// Scheduling algorithm for the intersections and the platoon arrival
		
		#region System wide statistics counters
		private NetworkStatistics ns;		// System-wide statistics counters
//		private ulong numVehArrivals;		// System wide total number of vehicle arrivals
//		private ulong numVehDepartures;		// System wide total number of vehicle departures
//		private ulong numPlatoonDepartures;	// System wide total number of platoon departures
//		private double avgVehiclesPerPlatoon; // System wide total number of vehicles per platoon
//
//		private double transitTimeAverage;	// System wide average transit time
//		private double numStopAverage;		// System wide average stop count
//		
//		private ulong totalTransitTime;		// System wide total transit time
//		private ulong totalStopCount;		// System wide total stop count

        private ulong totalCarsLogged = 0;  // Determines total cars for which log is written
		#endregion

		#region Individual stop counts - not used right now
//		private ulong intXStopCount;		// System wide stop count due to red light at intersection
//		private ulong prevStopCount;		// System wide stop count due to preceding platoon stopping
//		private ulong lackClearanceCount;	// System wide stop count due to lack of clearance during green light
		#endregion

		internal StreamWriter writer;				// To log arrival times to a file

		public Network(InputParameters ip, Random _rN, Algorithm _algorithm)
		{
			ns = new NetworkStatistics();

			this.rN = _rN;
			this.algo = _algorithm;
			numHRoads = ip.numHRoads;
			numVRoads = ip.numVRoads;

			Road r = null;					// Build roads as elements in array list
			vertRoads = new ArrayList();
			horRoads = new ArrayList();
			
			R = ip.lengthR;
			W = ip.lengthW;
			speedLimit = ip.speedLimit;
			
			for(int i = 0; i < numVRoads; i++)
			{
				// Create vertical roads
				r = new Road(i, RoadOrientation.NS, ip, rN, algo, this);
			//	r = new Road(i + 1, Orientation.VERTICAL, R, W, numHRoads, speedLimit, toggleInterval, num);
				vertRoads.Add(r);
			}
			for(int j = 0; j < numHRoads; j++)
			{
				// Create horizontal roads
				r = new Road(j, RoadOrientation.EW, ip, rN, algo, this);
			//	r = new Road(j + 1, Orientation.HORIZONTAL, R, W, numVRoads, speedLimit, toggleInterval, num);
				horRoads.Add(r);
			}

			string fileName;
			writer = new StreamWriter("arrivalLog.txt");
		}

//		/// <summary>
//		/// This method initializes the network and creates the roads belonging to the network
//		/// </summary>
//		/// <param name="_numHRoads">Number of Horizontal roads</param>
//		/// <param name="_numVRoads">Number of Vertical roads</param>
//		/// <param name="_R">Length of a road segment between intersections</param>
//		/// <param name="_W">Width of an intersection</param>
//		/// <param name="_speedLimit">Speed limit on the road</param>
//		/// <param name="_toggleInterval">Signal phase change time interval</param>
//		public Network(int _numHRoads, int _numVRoads, int _R, int _W, int _speedLimit, int _toggleInterval)
//		{
//			transitTimeAverage = numStopAverage = avgVehiclesPerPlatoon = 0;
//			numVehArrivals = numVehDepartures = numPlatoonDepartures = 0;
//			totalStopCount = totalTransitTime = 0;
//
//			intXStopCount = lackClearanceCount = prevStopCount = 0;
//
//			numHRoads = _numHRoads;
//			numVRoads = _numVRoads;
//			R = _R;
//			W = _W;
//			speedLimit = _speedLimit;
//			toggleInterval = _toggleInterval;
//
//			num = new RandomNum();		// Create a random number generator
//
//			Road r;						// Build roads as elements in array list
//			vertRoads = new ArrayList();
//			horRoads = new ArrayList();
//
//			pg = new PoissonGenerator(Road.lambda);
//
//			for(int i = 0; i < numVRoads; i++)
//			{
//				// Create vertical roads
//				r = new Road(i + 1, Orientation.VERTICAL, R, W, numHRoads, speedLimit, toggleInterval, num);
//				vertRoads.Add(r);
//			}b
//			for(int j = 0; j < numHRoads; j++)
//			{
//				// Create horizontal roads
//				r = new Road(j + 1, Orientation.HORIZONTAL, R, W, numVRoads, speedLimit, toggleInterval, num);
//				horRoads.Add(r);
//			}
//
//			r = (Road)horRoads[0];
//			
//		}

		internal void logArrivals(int currTime, int numArrivals, int roadNum, int roadOrient)
		{
			string line;
			if(numArrivals > 0)
			{

                totalCarsLogged += (ulong) numArrivals;

                
				if(roadOrient == RoadOrientation.NS)
					line = roadNum + " NS " + currTime + " " + numArrivals;
				else
					line = roadNum + " EW " + currTime + " " + numArrivals;

                // Log only first 41 cars
           //     if (totalCarsLogged <= 41)
                {
                    writer.WriteLine(line);
                    writer.Flush();
                }
			}
		}

		/// <summary>
		/// Method to run the simulation till the end time
		/// </summary>
		/// <param name="endTime">End time of the simulation</param>
		public void runSimulation(int endTime)
		{
			int currTime;
			int count;
			int count2;
            Road r;
			int i;
			
			#region Code to run the simulation on the roads till the end time is reached

			count = vertRoads.Count;
			count2 = horRoads.Count;

            for(currTime = 0; currTime <= endTime; currTime++)
			{
				if(currTime % 500 == 0)
					Console.WriteLine("Running for time = " + currTime);

				for(i = 0; i < count; i++)
				{
					r = (Road) vertRoads[i];
					r.runSimulation(currTime);
				}
				
				for(i = 0; i < count2; i++)
				{
					r = (Road) horRoads[i];
					r.runSimulation(currTime);
				}
			}
			#endregion

			#region Code segment to generate and show statistics
		
#if DDEBUG
			r = (Road)vertRoads[0];
			Console.WriteLine("North South Roads Starting Position {0} Ending Position {1}", r.getStartPosition(),r.getEndPosition());

			r = (Road)horRoads[0];
			Console.WriteLine("East West Roads Starting Position {0} Ending Position {1}", r.getStartPosition(),r.getEndPosition());
#endif
			/**
			 * Get the total count of the vehicles departed and arrived
			 */

			// Get statistics for all vertical roads

			Console.WriteLine("Statistics for V Roads");
			for(i = 0; i < count; i++)
			{
				r = (Road) vertRoads[i];

				Console.WriteLine("Road {0}", i);
				ns.addRoadStatistics(r.getRoadStatistics(endTime));
			}

				Console.WriteLine("Statistics for H Roads");
			// Get statistics for all horizontal roads
			for(i = 0; i < count2; i++)
			{
				r = (Road) horRoads[i];

				Console.WriteLine("Road {0}", i);
				ns.addRoadStatistics(r.getRoadStatistics(endTime));
			}
		
			ns.computeStatistics();
			ns.displayStatistics();
						
			#endregion
		}

        //~Network()
        //{
        //    writer.Close();
        //}
	}
}
