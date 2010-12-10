using System;
using System.IO;
using System.Collections;

namespace Mobile_Framework
{
	/// <summary>
	/// Class representing a platoon
	/// </summary>
	public class Platoon
	{
		private ulong platoonID;		// ID of platoon
		protected int platoonLen;		// Length of platoon
		protected int sizeLimit;		// Max allowed number of vehicles in platoon
		protected ArrayList vehicles;	// List of vehicles in the platoon

		private Road r;				// Reference to the road on which the platoon moves
		private int startPosn;		// Starting position of front edge of platoon
		private int currSpeed;		// current speed
		private int currPosn;		// Current position of leading (front) edge (i.e. leader vehicle)
		private int speedLimit;		// Speed limit on the road
		private int dirn;			// Direction of the platoon

		protected int interPlatoonDist;	// Min dist to maintain between successive platoons
		protected int intraPlatoonDist;	// Distance between vehicles in same platoon

		protected PlatoonPerfCounter platoonCtr;	// Platoon's perf counter
		protected static uint totalPlatoons = 0;	// Total platoons created in the system
	
		/// <summary>
		/// Class constructor
		/// </summary>
		/// <param name="_vehicles">Array of vehicles to be included in platoon</param>
		/// <param name="_sizeLimit">Max number of vehicles in platoon</param>
		/// <param name="_initDirn">Initial direction</param>
		/// <param name="_initSpeed">Initial speed</param>
		/// <param name="_initPosn">Initial position</param>
		/// <param name="_r">Reference of road on which it is moving</param>
		public Platoon(Vehicle[] _vehicles, int _sizeLimit, int _initDirn, int _initSpeed, int _initPosn, Road _r)
		{
			vehicles = new ArrayList(_vehicles);

			this.sizeLimit = _sizeLimit;
			this.dirn = _initDirn;
			this.currSpeed = _initSpeed;
			this.currPosn = _initPosn;

			this.startPosn = _initPosn;

			this.r = _r;
			this.interPlatoonDist = r.getInputParameters().interPlatoonDist;
			this.intraPlatoonDist = r.getInputParameters().intraPlatoonDist;

			this.speedLimit = r.getSpeedLimit();

			if(this.currPosn != this.r.getStartPosition() && (dirn == Direction.NS || dirn == Direction.EW))
			{
				Console.WriteLine("NS/EW Platoon  starting at " + _initPosn );
			}
			if(this.currPosn != this.r.getEndPosition() && (dirn == Direction.SN || dirn == Direction.WE))
			{
				Console.WriteLine("SN/WE Platoon starting at " + _initPosn);
			}
			/**
			 * Platoon length is the sum of the lengths of all vehicles and the sum of the length of intra-platoon distance for
			 * all but the first vehicle.
			 */
			platoonLen = vehicles.Count * r.getInputParameters().vehicleLen + (vehicles.Count - 1) * intraPlatoonDist;

			platoonID = totalPlatoons++;
#if DDEBUG
			Console.WriteLine("Platoon " + totalPlatoons + " created");
#endif
		}

		#region Methods involved with merge operation
		/**
		 * There are 3 methods involved in merge operation. Method "mergeCapacity" provides the
		 * additional vehicles that can be merged in predecessor. Method "acceptSuccessor" is invoked
		 * on the preceding platoon to accept the vehicles from the succeeding platoon. This method must
		 * be invoked from the method "mergeWithPredecessor". "mergeWithPredecessor" removes the vehicles
		 * from the platoon under consideration and invokes "acceptSuccessor" on the predecessor.
		 * 
		 * NOTE : ONLY COMPLETE MERGES ARE ALLOWED. PARTIAL MERGES ARE NOT ALLOWED. Thus, if succeeding platoon
		 * can merge all its vehicles into its predecessor, then merge operation is allowed, else it is not allowed.
		 */ 
		/// <summary>
		/// Returns the number of additional vehicles this platoon can merge
		/// </summary>
		/// <returns>Number of additional vehicles that this platoon can accomodate</returns>eturns>
		private int mergeCapacity()
		{
			return (sizeLimit - vehicles.Count);
		}

		/// <summary>
		/// This method is invoked by succeeding platoon to merge all of its vehicles in this platoon
		/// </summary>
		/// <param name="_vehicles">An arraylist representing the list of vehicles of successor platoon</param>
		/// <returns>True is merge was successful, false otherwise</returns>
		private bool acceptSuccessor(ArrayList _vehicles)
		{
			int numSuccVehicles;		// Number of vehicles to be merged from successor platoon
			Vehicle tempVehicle;		// A temporary vehicle variable

			numSuccVehicles = _vehicles.Count;

			/**
			 * If the number of vehicles to merge is more than what the platoon can accept, return false.
			 * Else perform the merge operation and increase length accordingly.
			 */
			if(vehicles.Count + numSuccVehicles > sizeLimit)
				return false;
			
			// Merge is successful
			tempVehicle = (Vehicle) _vehicles[0];
			platoonLen = platoonLen + numSuccVehicles * (intraPlatoonDist + tempVehicle.vehicleLen);

			// Add the vehicles to this platoon
			for(int i = 0; i < numSuccVehicles; i++)
				vehicles.Add(_vehicles[i]);
			
			return true;
		}

		/// <summary>
		/// This method is invoked by the platoon to merge with its predecessor platoon. The merging platoon
		/// first obtains the capacity of the succeeding platoon and then merges all possible vehicles to its 
		/// predecessor. If this platoon merges all its vehicles to predecessor, then it gets itself removed from the road
		/// ONLY COMPLETE MERGE IS ALLOWED. 
		/// A succeeding platoon can invoke this method when it is intra-platoon dist away from its predecessor. Distance check
		/// is not done in this method.
		/// </summary>
		/// <param name="pred">Instance of preceding platoon</param>
		/// <returns>A boolean value true if merge is possible, false otherwise</returns>
		public bool mergeWithPredecessor(Platoon pred)
		{
			int predCapacity;				// Additional capacity of predecessor platoon

			/**
			 * Query the preceding platoon for additional platoons that it can accept.
			 * If it's not possible to merge more platoons then return false.
			 */
			predCapacity = pred.mergeCapacity();

			/**
			 * ONLY COMPLETE MERGES ARE ALLOWED. Thus, if the number of vehicles in current platoon
			 * are greater than the capacity of preceding platoon, do not attempt to merge and return
			 * false.
			 */
			if(vehicles.Count > predCapacity)
				return false;

			// Add the removed vehicles into the preceding platoon
			pred.acceptSuccessor(vehicles);

			/**
			 * This loop removes all the vehicles from the vehicles array of this platoon
			 */
			vehicles.RemoveRange(0, vehicles.Count);
//			while(vehicles.Count > 0)
//			{
//				vehicles.RemoveAt(0);
//			}
		
			// Update self length
			platoonLen = 0;

			// Set currposn to some invalid value
			currPosn = -1000000;
			return true;
		}
		#endregion

		/// <summary>
		/// This method returns true if an intersection is present between the current position 
		/// and a possible new position
		/// </summary>
		/// <param name="newPosn">An integer representing a possible new position to move to</param>
		/// <param name="isecPosn">An output integer representing position of next intersection if present, -100000 otherwise</param>
		/// <returns>A bool value true if an intersection is present between currPosn and newPosn (including only currPosn), false otherwise</returns>
		private bool isIntersectionPresent(int newPosn, out int isecPosn)
		{
			//#if DEBUG
			//			Console.WriteLine("Checking if intersection present between " + currPosn + " and " + newPosn);
			//#endif
			isecPosn = r.getNextIntersection(currPosn, dirn);

			if(isecPosn == -100000) // Intersection not present along the direction of vehicle, return false
				return false;

			if(currPosn == isecPosn)
				return true;

			if((dirn == Direction.NS || dirn == Direction.EW) && ((currPosn < isecPosn) && (isecPosn < newPosn)))
				return true;

			if((dirn == Direction.SN || dirn == Direction.WE) && ((currPosn > isecPosn) && (isecPosn > newPosn)))
				return true;
			return false;
		}

		/// <summary>
		/// Algorithm used by the platoons to move
		/// </summary>
		/// <param name="prev">Previous platoon's reference</param>
		/// <param name="currTime">Current time</param>
		/// <returns></returns>
		public void move(Platoon prev, int currTime)
		{
			int newSpeed = -1;			// New speed
			int newPosn = -1;			// New position
			bool canMerge = true;		// Can merge with previous platoon
			int nextXPosn;				// Position of next intersection
			bool moveSucceeded;			// Move to new position succeeded

			int nextIntxnNum = -2000;	// Number of next intersection - NOT USED RIGHT NOW
		
			/**
			 * Compute new speed and new position for the platoon
			 */
			computeNewSpeedAndPosition(prev, out newPosn, out newSpeed);

			/**
			 * If newSpeed was zero, that means that the platoon cannot move due to predecessor platoon
			 * blocking it's path in this iteration, so return without moving.
			 */
			if(newSpeed == 0)
			{
				return;
			}

			moveSucceeded = checkIfMoveAllowed(currTime, newPosn, newSpeed, out nextXPosn);
			
			if(moveSucceeded == true)
			{
				currPosn = newPosn;
				currSpeed = newSpeed;
			}
			else
			{
				currPosn = nextXPosn;
				currSpeed = 0;
			}
		}

		/// <summary>
		/// Method to compute new speed and new position for the current platoon based on
		/// speed limit restrictions and maintaining minimum required distance with predecessor.
		/// </summary>
		/// <param name="prev">Reference to previous platoon</param>
		/// <param name="newPosn">New tentative position to move to</param>
		/// <param name="newSpeed">New speed to move to newPosn</param>
		private void computeNewSpeedAndPosition(Platoon prev, out int newPosn, out int newSpeed)
		{
			int distToTravel = 0;		// Distance to travel through in one time slot

			/**
			 * If there's no platoon ahead of this platoon then in one time unit, a platoon
			 * can move as much as the speedLimit.
			 */
			if(prev == null)
			{
				distToTravel = speedLimit;
			}
			else
			{
				/**
				 * If there is a platoon ahead of this platoon, compute distToTravel
				 * such that it maintains "interPlatoonDist" with it.
				 */
				if(this.dirn == Direction.NS || this.dirn == Direction.EW)
				{
					distToTravel = prev.rearPosition() - currPosn;
				}
				if(this.dirn == Direction.SN || this.dirn == Direction.WE)
				{
					distToTravel = currPosn - prev.rearPosition();
				}
				distToTravel = distToTravel - interPlatoonDist;

				/**
				 * However, speedLimit can't be exceeded. So if distToTravel > speedLimit, then
				 * reduce distToTravel to speedLimit.
				 */
				if(distToTravel > speedLimit)
					distToTravel = speedLimit;
			}

			/**
			 * Compute newSpeed and newPosn.
			 */
			newSpeed = distToTravel;

			if(this.dirn == Direction.NS || this.dirn == Direction.EW)
			{
				newPosn = currPosn + newSpeed;
			}
			else
			if(this.dirn == Direction.SN || this.dirn == Direction.WE)
			{
				newPosn = currPosn - newSpeed;
			}
			else
			{
				/**
				 * Written ONLY to satisfy the compiler
				 */
				newPosn = -100000;
				newSpeed = -100000;
			}
		}

		/// <summary>
		/// Helper method of move - it checks if platoon can move to newPosn or will have to stop
		/// at intersection.
		/// </summary>
		/// <param name="newPosn">New position</param>
		/// <param name="newSpeed">New speed</param>
		/// <param name="nextXPosn">Next intersection position</param>
		/// <returns>True - platoon can move to new position
		///			 False - platoon has to stop at intersection
		///	</returns>
		private bool checkIfMoveAllowed(int currTime, int newPosn, int newSpeed, out int posnNextX)
		{
			bool moveSuccessful;		// whether to move to new position or stop at intersection
			int nextXPosn;				// Position of next intersection

			int distReachIntersection;	// Distance to cover to reach start of next intersection
			int distClearIntersection;	// Distance to travel such that trailing end of platoon clears intersection
			int timeReachIntersection;	// Time to reach start of next intersection traveling at newSpeed
			int timeClearIntersection;	// Time to clear intersection
			int nextIntxnNum = -1;		// Number of next intersection - ignore all its usage positions

			int distToTravel;			// Distance platoon can travel through in this iteration
			/**
			 * Check if intersection is present.
			 */
			if(isIntersectionPresent(newPosn, out nextXPosn) == false)
			{
				/**
				 * No intersection - move will succeed. Set nextXPosn to junk value
				 */
				moveSuccessful = true;
				posnNextX = -11111;
			}
			else
			{
				posnNextX = nextXPosn;
				/**
				 * Intersection is present. Calculate timeReachIntersection and timeClearIntersection
				 * and find out if intersection stays green during that time.
				 */
				distReachIntersection = -100000;
				if(dirn == Direction.NS || dirn == Direction.EW)
					distReachIntersection = nextXPosn - currPosn;
				else
					if(dirn == Direction.SN || dirn == Direction.WE)
					distReachIntersection = currPosn - nextXPosn;
				
				timeReachIntersection = currTime + (int) Math.Floor(1.0 * distReachIntersection / newSpeed);
				distClearIntersection = distReachIntersection + r.getInputParameters().lengthW + platoonLen;
				timeClearIntersection = currTime + (int) Math.Ceiling(1.0 * distClearIntersection / newSpeed);

				if(true == r.isSignalGreen(timeReachIntersection, timeClearIntersection, currPosn, nextIntxnNum, this.dirn))
				{
					/**
					 * If predecessor clearance check is required or not
					 */
					if(true == r.predecessorCheckReqd())
					{
						distToTravel = newPosn - currPosn;

						if(distToTravel < 0 && (this.dirn == Direction.SN || this.dirn == Direction.WE))
							distToTravel = distToTravel * -1;

						if(distClearIntersection > distToTravel)
							moveSuccessful = false;
						else
							moveSuccessful = true;
					}
					else
					{
						/**
						 * Signal stays green and predecessor check is not required. So move is successful.
						 */
						moveSuccessful = true;
					}
				}
				else
				{
					/**
					 * Signal does not stay green. So stop at intersection.
					 */
					moveSuccessful = false;
//					if(this.r.getRoadNum() == 1 && this.r.getRoadOrientation() == 1)
//					{
//						Console.WriteLine("Platoon ID = " + platoonID + " with " + this.numVehicles + " vehicles has stopped on EW road 1 at position = " + currPosn + " TimeReachIntersection = " + timeReachIntersection + " TimeClearIntersection = " + timeClearIntersection + " CurrTime = " + currTime + " New Speed = " + newSpeed);
//					}
				}
			}
			return moveSuccessful;
		}

		/// <summary>
		/// Method returns if platoon is currently moving or not
		/// </summary>
		/// <returns>A boolean value true if vehicle is moving, false otherwise</returns>
		public bool isMoving()
		{
			if(currSpeed > 0) 
				return true;
			else
				return false;
		}

		/// <summary>
		/// This method returns the front position of the platoon
		/// </summary>
		/// <returns>An integer representing the position of the front of the platoon</returns>
		public int frontPosition()
		{
			return currPosn;
		}

		/// <summary>
		/// This method returns the position corresponding to the rear of the platoon
		/// depending on its current position.
		/// </summary>
		/// <returns>An integer representing rear position of the platoon</returns>
		public int rearPosition()
		{
			int p = currPosn;

			if(dirn == Direction.EW || dirn == Direction.NS)
				p = p - platoonLen;
			else
				p = p + platoonLen;
			return p;
		}

		/// <summary>
		/// Property that returns current speed
		/// </summary>
		public int currentSpeed
		{
			get
			{
				return currSpeed;
			}
		}

		/// <summary>
		/// Returns the current (front) position of the platoon
		/// </summary>
		public int currentPosition
		{
			get
			{
				return currPosn;
			}
		}

		/// <summary>
		/// Returns the number of vehicles in platoon
		/// </summary>
		public int numVehicles
		{
			get
			{
				return vehicles.Count;
			}
		}
		
		/// <summary>
		/// This method returns true if it's empty platoon, false otherwise
		/// </summary>
		/// <returns>An boolean value true if platoon is empty, false otherwise</returns>
		public bool isPlatoonEmpty()
		{
			return ((vehicles.Count == 0) ? true : false);
		}

		public PlatoonPerfCounter getCounters(int currTime)
		{
			ulong transitTime = 0;
			ulong waitingTime = 0;
			ulong journeyTime = 0;
			ulong numVehicles = 0;
			ulong distTraveled = 0;
			ulong stopCount = 0;

			Vehicle v;
			// Represents number of vehicles in platoon
			numVehicles = (ulong) vehicles.Count;

			// Represents distance traveled by platoon
			if(this.dirn == Direction.SN || this.dirn == Direction.WE)
				distTraveled = (ulong) (startPosn - currPosn);
			if(this.dirn == Direction.NS || this.dirn == Direction.EW)
				distTraveled = (ulong) (currPosn - startPosn);

			// Calculates transit time, wait time and journey time
			// Stop count not calculated now
			for(int i = 0; i < vehicles.Count; i++)
			{
				v = (Vehicle) vehicles[i];
				transitTime = transitTime + (ulong) v.getTransitTime(currTime);
				waitingTime = waitingTime + (ulong) v.getWaitingTime();
				journeyTime = journeyTime + (ulong) v.journeyTime(currTime);
             }

			/**
			 * Create a counter object
			 */
			platoonCtr = new PlatoonPerfCounter(transitTime, waitingTime, journeyTime, numVehicles, distTraveled, stopCount);
			return platoonCtr;
		}
	}
}
