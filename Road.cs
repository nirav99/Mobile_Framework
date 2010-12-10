using System;
using System.IO;
using System.Collections;

namespace Mobile_Framework
{
	/// <summary>
	/// Class representing a single road
	/// </summary>
	public class Road
	{
		#region Road structural parameters
		protected int roadNum;			// Road Number
		protected int startPos;			// Start position of road
		protected int endPos;			// End position of road
		protected int orient;			// Road's orientation
		protected InputParameters ip;	// Reference of InputParameters
		protected int R;				// Length of road segment in metres
		protected int W;				// Width of intersection in metres
		protected int numIntersection;	// Number of intersecting roads with this road
		protected int []posnIntersection;// Array representing coordinates of start of intersections (for NS or EW orientations)
		// For SN of WE orientations, position of next intersection will be value of corresponding orientation + width of intersection
		protected double arrivalRate;	// Arrival rate of the vehicles
		#endregion

		#region Scheduling algorithm, Random number reference and poisson generator for road
		protected Algorithm schedule;	// Reference of algorithm
		protected Random rN;			// Reference of Random class object
		protected PoissonGenerator pG;	// Reference of poisson generator class
		protected int period;			// Time period between successive platoons
		protected int numVehiclesInPlatoon;	// Max number of vehicles in platoon
		protected int travelSpeed;		// Initial speed of travel of platoons to first intersection
		#endregion
		
		#region Statistical Paramters to measure
//		ulong numPlatoonArrivals;	// Total number of platoons created on this road
//		ulong numVehDepartures;		// Total number of departing vehicles on this road
//		ulong numPlatoonDepartures;	// Total number of departing platoons on this road
//		ulong totalDelay;			// Total delay experienced on this road
//		ulong totalStopCount;		// Total stop count on this road
		RoadPerfCounter roadCntr;	// Performance counter for the road
		#endregion
	
		//	protected int toggleInterval;	// Time at which signal changes its phase
		#region Lanes of the roads and the holding queues
		protected ArrayList upLane;		// Lane of platoons moving in WE / SN directions
		protected ArrayList downLane;	// Lane of platoons moving in NS / EW directions
		protected Queue downQueue;		// Queue to hold vehicles in NS/EW directions
		protected Queue upQueue;		// Queue to hold vehicles in WE/SN directions
		#endregion

		protected int lastArrivalDownLane;	// Last arrival time in down lane
		protected int lastArrivalUpLane;	// Last arrival time in up lane

		private Network ns;

		/// <summary>
		/// Class constructor.
		/// </summary>
		/// <param name="_roadNum">A number for the road</param>
		/// <param name="_orientation">Road's orientation</param>
		/// <param name="_ip">Reference of InputParameters object</param>
		/// <param name="_rN">Reference of Random class object</param>
		/// <param name="_algo">Reference of Algorithm class object</param>
		/// <param name="ns">Reference of network simulator</param>
		public Road(int _roadNum, int _orientation, InputParameters _ip, Random _rN, Algorithm _algo, Network ns)
		{
			/**
			 * Set the values of the parameters
			 */
			roadNum = _roadNum;
			orient = _orientation;
			this.ip = _ip;
			this.rN = _rN;
			this.schedule = _algo;

			this.ns = ns;
//			totalDelay = totalStopCount = 0;
//			numPlatoonArrivals = numVehDepartures = numPlatoonDepartures = 0;
			
			R = ip.lengthR;
			W = ip.lengthW;

			lastArrivalDownLane = lastArrivalUpLane = 0;

			/**
			 * Instantiate poisson generator object.
			 */
			pG = new PoissonGenerator();

			/**
			 * Instantiate Road Perf Counter object.
			 */
			roadCntr = new RoadPerfCounter();

			/**
			 * Calculate the total number of intersections
			 */
			if(orient == RoadOrientation.NS)
				numIntersection = ip.numHRoads;
			else
				numIntersection = ip.numVRoads;

			/**
			 * Compute road's start and end coordinates based on the number of intersecting roads. The road is one
			 * segment longer than the last intersection.
			 */
			startPos = 0;
			endPos = numIntersection * (R + W) + R;
		
			/** 
			 * Compute the start position of each intersection
			 */
			posnIntersection = new int[numIntersection];

			posnIntersection[0] = R;	// Position of first intersection
			// position of all subsequent intersections
			for(int i = 1; i < numIntersection; i++)
				posnIntersection[i] = (i + 1)* (R + W) - W;

			upLane = new  ArrayList();
			downLane = new ArrayList();
			downQueue = new Queue();
			upQueue = new Queue();

			/**
			 * Obtain platoon generation characteristics from the algorithm object
			 */
			obtainPlatoonCreationParameters(0);
		}

		/// <summary>
		/// This method obtains the initial generation characteristics of platoons (period, num vehicles and initial travel speed).
		/// </summary>
		/// <param name="currTime">Current time - not used</param>
		protected void obtainPlatoonCreationParameters(int currTime)
		{
			schedule.obtainPlatoonCharacteristics(roadNum, orient, currTime, out numVehiclesInPlatoon, out period, out travelSpeed);
		}

		/// <summary>
		/// This is the main arrival method that generates vehicles in holding queues as per poisson process and
		/// platoons on the lanes as per the specified algorithm.
		/// </summary>
		/// <param name="currTime">Current time</param>
		protected void simulateArrival(int currTime)
		{
			/**
			 * Simulating an arrival is a 2 step process. The first step is creating the vehicles as per poisson
			 * process and then adding them in the corresponding queues.
			 * The second step is to dequeue them from the holding queues and create a platoon and add that platoon
			 * to the corresponding lane of the road.
			 */

            if (ip.useInputArrivalLog() == false)
            {
                addVehiclesToQueue(currTime, downQueue);
                addVehiclesToQueue(currTime, upQueue);
            }
            else
            {
                addVehiclesUsingInputLog(currTime);
            }

			createPlatoons(currTime);
		}

		/// <summary>
		/// The method that create platoons and puts on corresponding lanes of the roads
		/// </summary>
		/// <param name="currTime">Current Time</param>
		protected void createPlatoons(int currTime)
		{
			/**
			 * Create platoon on downlane (0) and then on upLane (1)
			 */
			createPlatoonHelper(currTime, 0);
			createPlatoonHelper(currTime, 1);
		}

		/// <summary>
		/// Method to depart the platoons from the lanes and gather the statistics
		/// </summary>
		/// <param name="currTime">Current time</param>
		protected void simulateDeparture(int currTime)
		{
			/**
			 * Call helper method to remove platoons on downLane and then on upLane.
			 */
			simulateDepartureHelper(currTime, 0);
			simulateDepartureHelper(currTime, 1);
		}

		/// <summary>
		/// Method to run one round of simulation
		/// </summary>
		/// <param name="currTime">Current time</param>
		public void runSimulation(int currTime)
		{
			 // Depart platoons from the roads
			simulateDeparture(currTime);

			movePlatoon(currTime, 0);
			movePlatoon(currTime, 1);
			
			// Generate platoons on the roads
				
			simulateArrival(currTime);
		}

		/// <summary>
		/// This method returns the starting point of the next intersection in the path of the platoons by knowing
		/// it's current position and it's direction (orientation)
		/// </summary>
		/// <param name="currPos">Integer representing the current platoons position</param>
		/// <param name="dirn">Integer representing platoons's direction</param>
		/// <returns>An integer representing next intersection coordinate, if no intersections present
		/// object with  -100000 is returned</returns>
		public int getNextIntersection(int currPos, int dirn)
		{
			int nextPos = -100000;

			if(dirn == Direction.NS || dirn == Direction.EW)
			{
				for(int i = 0; i < numIntersection; i++)
					if(posnIntersection[i] >= currPos)
					{
						nextPos = posnIntersection[i];
						break;
					}
			}
			else
				if(dirn == Direction.SN || dirn == Direction.WE)
			{
				for(int i = numIntersection - 1; i >= 0; i--)
					if(posnIntersection[i] + W <= currPos)
					{
						nextPos = posnIntersection[i] + W;
						break;
					}
			}
			return nextPos;
		}
		
		/// <summary>
		/// Method to return road speed limit
		/// </summary>
		/// <returns>Road Speed Limit</returns>
		public int getSpeedLimit()
		{
			return ip.speedLimit;
		}

		/// <summary>
		/// Method to get intersection's width
		/// </summary>
		/// <returns>An integer representing intersection's width in meters</returns>
		public int getIntersectionWidth()
		{
			return W;
		}

		/// <summary>
		/// Method to return starting position of the road 
		/// </summary>
		/// <returns>An integer representing starting position of the road</returns>
		public int getStartPosition()
		{
			return startPos;
		}

		/// <summary>
		/// Method to return ending position of the road
		/// </summary>
		/// <returns>An integer representing ending position of the road</returns>
		public int getEndPosition()
		{
			return endPos;
		}

		/// <summary>
		/// Returns the road number
		/// </summary>
		/// <returns>An integer representing road number</returns>
		public int getRoadNum()
		{
			return roadNum;
		}

		/// <summary>
		/// Returns road's orientation
		/// </summary>
		/// <returns>An integer representing road's orientation</returns>
		public int getRoadOrientation()
		{
			return orient;
		}

		/// <summary>
		/// Return an object of InputParameters class
		/// </summary>
		/// <returns></returns>
		public InputParameters getInputParameters()
		{
			return ip;
		}

		/// <summary>
		/// Print all the counters for the road
		/// </summary>
		public RoadStatistics getRoadStatistics(int currTime)
		{
		//	this.getRoadPlatoonStatistics(currTime);
			return roadCntr.getStatistics();
		}

		/// <summary>
		/// Returns true if platoons on this road do not require predecessor check, false otherwise.
		/// </summary>
		/// <returns>True or false</returns>
		public bool predecessorCheckReqd()
		{
			return schedule.scheduleHasGreaterThanOneUtilization();
		}

		/// <summary>
		/// This method returns true if the signal stays green for all time intervals between startTime and endTime, both inclusive.
		/// </summary>
		/// <param name="startTime">Start time when platoon might cross intersection</param>
		/// <param name="endTime">End time when platoon clears intersection</param>
		/// <param name="platoonPosn">Current position of the front of the position</param>
		/// <param name="intxnNum">Intersection number that a platoon is approaching</param>
		/// <param name="dirn">Direction of travel</param>
		/// <returns>True if the signal stays green between startTime and endTime, false otherwise</returns>
		public bool isSignalGreen(int startTime, int endTime, int platoonPosn, int intxnNum, int dirn)
		{
			return schedule.allowedToCrossIntersection(this.roadNum, this.orient, intxnNum, dirn, startTime, endTime);		
		}

        /// <summary>
        /// Method to add vehicles to input queue based on reading arrivals from an input file
        /// </summary>
        /// <param name="currTime"></param>
        private void addVehiclesUsingInputLog(int currTime)
        {
            int numArrivals;
            int dirn;
            Vehicle v;

            // Since we have data only for NS and EW direction, we only add to downQueue

            if (this.orient == RoadOrientation.NS)
                dirn = Direction.NS;
            else
                dirn = Direction.EW;

            numArrivals = ip.getNumCars(this.roadNum, dirn, currTime);

            for (int i = 0; i < numArrivals; i++)
            {
                v = new Vehicle(currTime, ip.vehicleLen, null);
                downQueue.Enqueue(v);

                roadCntr.vehicleCreated();
            }
        }


		/// <summary>
		/// Helper method to add vehicles to downQueue or upQueue
		/// </summary>
		/// <param name="currTime">Current time</param>
		/// <param name="q">Queue to add vehicles to</param>
		private void addVehiclesToQueue(int currTime, Queue q)
		{
			Vehicle v;
			int numArrivals;
            
            /**
             * When arrivals are to be generated using poisson process
             */
            numArrivals = pG.getNumArrivals(ip.getArrivalRate(this.orient, this.roadNum), this.rN);

			/**
			 * Log vehicle arrival only for NS and EW directions, not for SN and WE directions
			 */
			if(q == downQueue)
				ns.logArrivals(currTime, numArrivals, this.roadNum, this.orient);

			for(int i = 0; i < numArrivals; i++)
			{
				v = new Vehicle(currTime, ip.vehicleLen, null);
				q.Enqueue(v);

				/**
				 * Update vehicle arrival counter here
				 */
				roadCntr.vehicleCreated();
			}
		}

		/// <summary>
		/// Helper method used by createPlatoon to create platoons.
		/// </summary>
		/// <param name="currTime">Current time</param>
		/// <param name="laneID">Lane ID for which platoon must be created 0 - downLane, 1 - upLane</param>
		private void createPlatoonHelper(int currTime, int laneID)
		{
			Platoon p;		// Temp object Platoon
			int posn;		// Rear position of last platoon on that lane
			int numVeh;		// Number of vehicles to add in platoon
			int startPt;	// Starting position for platoon
			Vehicle []v;	// Array of vehicles to add to platoon
			int platoonDirn;// Platoon's direction
			Queue waitQ;	// Waiting queue
			ArrayList lane;	// Lane

			/**
			 * Select values corresponding to either downlane or uplane
			 */
			if(laneID == 0)
			{	
				lane = downLane;
				waitQ = downQueue;
				startPt = this.startPos;

				// For downlane, direction is either NS or EW
				if(orient == RoadOrientation.NS)
					platoonDirn = Direction.NS;
				else
					platoonDirn = Direction.EW;
			}
			else
			{
				lane = upLane;
				waitQ = upQueue;
				startPt = this.endPos;

				// For upLane, direction is either SN or WE
				if(orient == RoadOrientation.NS)
					platoonDirn = Direction.SN;
				else
					platoonDirn = Direction.WE;
			}

			/**
			 * If this is the time to create a platoon and some vehicles are waiting to join a platoon
			 * then only create a platoon.
			 */
			if((currTime % period == 0) && (waitQ.Count > 0))
			{

				/**
				 * Get the position of the last platoon on that lane.
				 */
				if(lane.Count > 0)
				{
					p = (Platoon) lane[lane.Count - 1];
					posn = p.rearPosition();

					/**
					 * If there isn't place to create a new platoon such that minimum inter platoon distance
					 * is maintained with last platoon on that lane, don't attempt to create a platoon and return.
					 */
					if(laneID == 0)
					{
						if(posn < this.startPos + ip.interPlatoonDist)
						{
							/**
							 * Increment platoon not created counter.
							 */
							roadCntr.incrementPlatoonNotCreatedCount();
							return;
						}
					}
					else
					{
						if(this.endPos - posn < ip.interPlatoonDist)
						{
							/**
							 * Increment platoon not created counter.
							 */
							roadCntr.incrementPlatoonNotCreatedCount();
							return;
						}
					}
				}
				// Get how many vehicles to add to this platoon
				numVeh = Math.Min(numVehiclesInPlatoon, waitQ.Count);

				v = new Vehicle[numVeh];

				// Remove those many vehicles from the waiting queue
				for(int i = 0; i < numVeh; i++)
				{
					v[i] = (Vehicle) waitQ.Dequeue();

					// Set vehicles start journey time - this determines vehicle's wait time
					v[i].setStartJourneyTime(currTime);
				}

				p = new Platoon(v, numVehiclesInPlatoon, platoonDirn, travelSpeed, startPt, this);

				if(laneID == 1)
				{
			//		Console.WriteLine("Platoon created at posn {0} Dirn {1} on Road {2} Orient {3}", startPt, platoonDirn, this.roadNum, this.orient);
				}
				lane.Add(p);

				/**
				 * Update platoon arrival counter in road perf counter
				 */
				roadCntr.platoonCreated();
				
			}
		}

		/// <summary>
		/// Method to move the platoons
		/// </summary>
		/// <param name="currTime">Current time</param>
		/// <param name="laneID">ID of lane 0 - downLane, 1 - upLane</param>
		private void movePlatoon(int currTime, int laneID)
		{
			Platoon prev;		// Previous platoon on the lane
			Platoon p;			// platoon on the lane 
			int nextPlatoon;	// Index of next platoon to move
			int prevPlatoon;	// Index of platoon previous to the next platoon to move
			ArrayList lane;		// downLane or upLane

			if(laneID == 0)
				lane = downLane;
			else
				lane = upLane;

			prevPlatoon = -1;
			nextPlatoon = 0;

			while(nextPlatoon < lane.Count)
			{
				p = (Platoon) lane[nextPlatoon];

				if(prevPlatoon == -1)
				{
					/**
					 * For the first platoon on the lane, set it's predecessor to null
					 */
					p.move(null, currTime);

					// Increment indexes
					prevPlatoon = nextPlatoon++;
				}
				else
				{
					/**
					 * Obtain current platoon (p) and its predecessor platoon (prev)
					 */
					prev = (Platoon) lane[prevPlatoon];
					p = (Platoon) lane[nextPlatoon];
		
					// Invoke move on it so that it moves
					p.move(prev, currTime);

					/**
					 * If while moving, p merged with its predecessor (prev) then remove it from the lane.
					 * Also update nextPlatoon index to point to the next element after prevPlatoon index.
					 */
					if(p.isPlatoonEmpty())
					{
						lane.Remove(p);
						nextPlatoon = prevPlatoon + 1;
					}
					else
					{
						/**
						 * If no platoon was deleted, then increment both prevPlatoon and nextPlatoon
						 */
						prevPlatoon = nextPlatoon++;
					}
				}
			}
		}

		/// <summary>
		/// Helper method to simulate departure. Removes a platoon and gathers its counters
		/// </summary>
		/// <param name="currTime">Current Time</param>
		/// <param name="laneID">Lane ID</param>
		private void simulateDepartureHelper(int currTime, int laneID)
		{
			Platoon p;					// Platoon object
			ArrayList lane;				// Road's lane
			PlatoonPerfCounter pCtr;	// Platoon's perf counter

			pCtr = null;

			if(laneID == 0)
			{
				lane = downLane;
			}
			else
			{
				lane = upLane;
			}

			while(lane.Count > 0)
			{
				/**
				 * Get the first platoon on the lane
				 */
				p = (Platoon) lane[0];

				/**
				 * If the first platoon has not reached end of road, return
				 */
				if(laneID == 0 && p.frontPosition() < this.endPos)
					break;
				if(laneID != 0 && p.frontPosition() > this.startPos)
					break;

				/**
				 * The platoon has reached the end of the road so remove it from the road
				 * and gather it's performance counters.
				 */
				lane.RemoveAt(0);

				pCtr = p.getCounters(currTime);

				roadCntr.platoonDeparted();
				roadCntr.addPlatoonCounter(pCtr);
			}
		}

		/// <summary>
		/// Computes the statistics for all the platoons in the system
		/// </summary>
		/// <param name="currTime">Current Time</param>
		public void getRoadPlatoonStatistics(int currTime)
		{
			Platoon p;

			/**
			 * Add the counters for all the platoons in transit.
			 */
			for(int i = 0; i < downLane.Count; i++)
			{
				p = (Platoon) downLane[i];
				roadCntr.addPlatoonCounter(p.getCounters(currTime));
			}
			for(int i = 0; i < upLane.Count; i++)
			{
				p = (Platoon) upLane[i];
				roadCntr.addPlatoonCounter(p.getCounters(currTime));
			}

			/**
			 * Create a new platoon containing all the vehicles in the holding queue and then 
			 * add that to the roadCntr so that transit time of these vehicles get counted also.
			 */
			Vehicle []v = new Vehicle[downQueue.Count];

			int cnt = downQueue.Count;

			for(int i = 0; i < cnt; i++)
			{
				v[i] = (Vehicle) downQueue.Dequeue();
			}
		
			p = new Platoon(v, v.Length, Direction.NS, ip.speedLimit, 0, this);
			roadCntr.addPlatoonCounter(p.getCounters(currTime));

			/**
			 * Do the same for up queue
			 */
			v = new Vehicle[upQueue.Count];

			cnt = upQueue.Count;
			for(int i = 0; i < cnt; i++)
			{
				v[i] = (Vehicle) upQueue.Dequeue();
			}

			p = new Platoon(v, v.Length, Direction.SN, ip.speedLimit, 0, this);
			roadCntr.addPlatoonCounter(p.getCounters(currTime));
		}
	}
}
