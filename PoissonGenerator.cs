using System;

namespace Mobile_Framework
{
	/// <summary>
	/// This class generates poisson arrivals for the roads
	/// </summary>
	public class PoissonGenerator
	{
        public PoissonGenerator()
		{
		}
		
		/// <summary>
		/// Returns the next arrival time according to poisson process
		/// </summary>
		/// <param name="lambda">Arrival rate</param>
		/// <param name="rN">Object of class random</param>
		/// <returns>A double representing time interval to the next arrival</returns>
		public double getNextArrivalTime(double lambda, Random rN)
		{
			double nextArrivalTime = rN.NextDouble();

			if(nextArrivalTime != 0)
				nextArrivalTime = -1 / lambda * Math.Log(nextArrivalTime);

			return nextArrivalTime;
		}

		/// <summary>
		/// Returns the number of arrivals in one time unit according to poisson process
		/// </summary>
		/// <param name="lambda">Arrival Rate</param>
		/// <param name="rN">Object of class random</param>
		/// <returns>An int representing number of arrivals in one time unit</returns>
		public int getNumArrivals(double lambda, Random rN)
		{
			int numArrivals = 0;
			
			double timeInterval = 0;
			double nextArrivalTime = 0;

			while(true)
			{
				nextArrivalTime = getNextArrivalTime(lambda, rN);
				
				timeInterval += nextArrivalTime;

				if(nextArrivalTime < 1.0 && timeInterval < 1.0)
					numArrivals++;
				else
					break;
			}
			return numArrivals;
		}
	}
}
