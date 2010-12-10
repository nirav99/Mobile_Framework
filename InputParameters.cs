using System;
using System.IO;
using System.Collections;

namespace Mobile_Framework
{
	/// <summary>
	/// This class reads the input configuration file and builds a map.
	/// </summary>
	public class InputParameters
	{
		private int _numHRoads;				// Num of horizontal roads
		private int _numVRoads;				// Num of vertical roads
		private int _R;						// Length of road segment between consecutive intersections
		private int _W;						// Width of intersection
		private int _speedLimit;			// Speed limit on the system
		private int _vehicleLen;			// Length of each vehicle
		private double[] arrivalRateHRoads;	// Arrival rate on all H roads
		private double[] arrivalRateVRoads;	// Arrival rate on all V roads
		private int _intraPlatoonDist;		// Distance between adjacent vehicles in platoon
		private int _interPlatoonDist;		// Minimum safe distance between platoons

        #region Variables to read an arrival log and maintain the collection of times when vehicles arrive
        private string arrivalLogFileName = "InputArrivalLog.txt";
        private Hashtable ns0HT = null;     // Hashtable to store arrival time -> num cars for NS0
        private Hashtable ns1HT = null;     // Hashtable to store arrival time -> num cars for NS1
        private Hashtable ew0HT = null;     // Hashtable to store arrival time -> num cars for EW0
        private Hashtable ew1HT = null;     // Hashtable to store arrival time -> num cars for EW1

        private bool useInputLog = false;   // Whether use poisson to generate arrival or use input log
        #endregion

        /// <summary>
		/// Class constructor - Assign values to the parameters either reading input file or
		/// pre-fixing
		/// </summary>
		/// <param name="inputFile">Path of input configuration file</param>
		public InputParameters(string inputFile, string inputArrivalFileName)
		{
			StreamReader reader = new StreamReader(inputFile);

            if(inputArrivalFileName != null)
            {
                arrivalLogFileName = inputArrivalFileName;
                useInputLog = true;
            }

			_numHRoads = 2;
			_numVRoads = 2;
			_R = 1000;
            _W = 20;
            _speedLimit = 24;
            _vehicleLen = 4;
            _intraPlatoonDist = 1;
            _interPlatoonDist = 1;
           // _interPlatoonDist = 1;

            //_intraPlatoonDist = 25;
            //_interPlatoonDist = 0;
            //_speedLimit = 25;
            //_W = 25;

			arrivalRateVRoads = new double[_numVRoads];
			arrivalRateHRoads = new double[_numHRoads]; 

			for(int i = 0; i < _numVRoads; i++)
			{
				arrivalRateVRoads[i] = Convert.ToDouble(reader.ReadLine());
			}

			for(int j = 0; j < _numHRoads; j++)
			{
				arrivalRateHRoads[j] = Convert.ToDouble(reader.ReadLine());
			}
			reader.Close();

            #region Code to read input arrival log and build up arrival hashtables
            buildArrivalMap();
            #endregion
        }

        private void buildArrivalMap()
        {
            StreamReader reader = null;
            string line;
            string[] tokens;
            ArrivalLog alog;

            ns0HT = new Hashtable();
            ns1HT = new Hashtable();
            ew0HT = new Hashtable();
            ew1HT = new Hashtable();

            char[] separator = { ' ' };
            try
            {
                reader = new StreamReader(arrivalLogFileName);

                while ((line = reader.ReadLine()) != null)
                {
                    tokens = null;
                    tokens = line.Split(separator);

                    alog = null;

                    if (tokens[1].Equals("NS", StringComparison.CurrentCultureIgnoreCase) && tokens[0].Equals("0"))
                    {
                        alog = new ArrivalLog(Direction.NS, Convert.ToInt32(tokens[3]));
                        ns0HT.Add(Convert.ToInt32(tokens[2]), alog);
                    }
                    if (tokens[1].Equals("NS", StringComparison.CurrentCultureIgnoreCase) && tokens[0].Equals("1"))
                    {
                        alog = new ArrivalLog(Direction.NS, Convert.ToInt32(tokens[3]));
                        ns1HT.Add(Convert.ToInt32(tokens[2]), alog);
                    }
                    if (tokens[1].Equals("EW", StringComparison.CurrentCultureIgnoreCase) && tokens[0].Equals("0"))
                    {
                        alog = new ArrivalLog(Direction.EW, Convert.ToInt32(tokens[3]));
                        ew0HT.Add(Convert.ToInt32(tokens[2]), alog);
                    }
                    if (tokens[1].Equals("EW", StringComparison.CurrentCultureIgnoreCase) && tokens[0].Equals("1"))
                    {
                        alog = new ArrivalLog(Direction.EW, Convert.ToInt32(tokens[3]));
                        ew1HT.Add(Convert.ToInt32(tokens[2]), alog);
                    }
                }

                

                reader.Close();
            }
            catch(Exception e)
            {
                if (reader != null)
                    reader.Close();
            }
        }

		/// <summary>
		/// Returns the total number of vertical (NS) roads
		/// </summary>
		public int numVRoads
		{
			get
			{
				return _numVRoads;
			}
		}

		/// <summary>
		/// Returns the total number of horizontal (EW) roads
		/// </summary>
		public int numHRoads
		{
			get
			{
				return _numHRoads;
			}
		}

		/// <summary>
		/// Returns length of road segment
		/// </summary>
		public int lengthR
		{
			get
			{
				return _R;
			}
		}

		/// <summary>
		/// Returns length of width of intersection
		/// </summary>
		public int lengthW
		{
			get
			{
				return _W;
			}
		}

		/// <summary>
		/// Returns the vehicle length
		/// </summary>
		public int vehicleLen
		{
			get
			{
				return _vehicleLen;
			}
		}

		/// <summary>
		/// Returns the speed limit on the network
		/// </summary>
		public int speedLimit
		{
			get
			{
				return _speedLimit;
			}
		}

		/// <summary>
		/// Returns distance between consecutive vehicles in a platoon
		/// </summary>
		public int intraPlatoonDist
		{
			get
			{
				return _intraPlatoonDist;
			}
		}

		/// <summary>
		/// Returns minimum safe distance to maintain between consecutuve platoons
		/// </summary>
		public int interPlatoonDist
		{
			get
			{
				return _interPlatoonDist;
			}
		}

		/// <summary>
		/// Returns the arrival rate of the specified road
		/// </summary>
		/// <param name="roadOrientation">Road's orientation</param>
		/// <param name="roadNum">Road number - zero based index</param>
		/// <returns>Arrival rate on the specified road</returns>
		public double getArrivalRate(int roadOrientation, int roadNum)
		{
			double aRate = 0.0;
			if(roadOrientation == RoadOrientation.NS)
				aRate = arrivalRateVRoads[roadNum];
			if(roadOrientation == RoadOrientation.EW)
				aRate = arrivalRateHRoads[roadNum];
			return aRate;
		}

		/// <summary>
		/// Returns the list of arrival rate on the NS roads
		/// </summary>
		/// <param name="result">Arrival rate on NS roads</param>
		public void getArrivalRateVRoads(out double[] result)
		{
			result = new double[arrivalRateVRoads.Length];

			for(int i = 0; i < arrivalRateVRoads.Length; i++)
				result[i] = arrivalRateVRoads[i];
		}

		/// <summary>
		/// Returns the list of arrival rate on EW roads
		/// </summary>
		/// <param name="result">Arrival rate on EW roads</param>
		public void getArrivalRateHRoads(out double[] result)
		{
			result = new double[arrivalRateHRoads.Length];

			for(int i = 0; i < arrivalRateHRoads.Length; i++)
				result[i] = arrivalRateHRoads[i];
		}


        /// <summary>
        /// Given arrival log and a map built of arrival log, this method returns how many cars arrive at the specified time
        /// in specified direction on the specified road.
        /// </summary>
        /// <param name="roadNum"></param>
        /// <param name="dirn"></param>
        /// <param name="time"></param>
        /// <returns></returns>
        public int getNumCars(int roadNum, int dirn, int time)
        {
            Hashtable ht = null;
            int numCars = -100;
            ArrivalLog alog = null;

            // Currently simulation data not logged for SN or WE directions so return 0
            if (dirn == Direction.SN || dirn == Direction.WE)
                return 0;

            if (dirn == Direction.NS)
            {
                if (roadNum == 0)
                {
                    ht = ns0HT;
                }
                if (roadNum == 1)
                {
                    ht = ns1HT;
                }
            }
            if (dirn == Direction.EW)
            {
                if (roadNum == 0)
                {
                    ht = ew0HT;
                }
                if (roadNum == 1)
                {
                    ht = ew1HT;
                }
            }

            alog = (ArrivalLog)ht[time];

            if (alog == null)
                numCars = 0;
            else
                numCars = alog.numCars;

            return numCars;

        }

        public bool useInputArrivalLog()
        {
            return useInputLog;
        }
	}
}
