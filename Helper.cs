using System;

namespace Mobile_Framework
{
	public enum VALUE
	{
		INVALID = -1000000
	}

	/**
	 * The class that represents the directions that platoons can take at any intersection.
	 */
	public class Direction
	{
		public static readonly int NS = 0;
		public static readonly int SN = 1;
		public static readonly int EW = 2;
		public static readonly int WE = 4;
	}

	/*
	 * This class represents the orientation of the roads - going in NS/SN or EW/WE directions.
	 */
	public class RoadOrientation
	{
		public static readonly int NS = 0;
		public static readonly int EW = 1;
	}
	/// <summary>
	/// Class to compute LCM and GCD
	/// </summary>
	public class ComputeLCM
	{
		/// <summary>
		/// Class constructor
		/// </summary>
		public ComputeLCM()
		{

		}

		/// <summary>
		/// Returns LCM of the input array
		/// </summary>
		/// <param name="input">An array of integers whose LCM is needed</param>
		/// <returns>LCM of the input array</returns>
		public static int LCM(int[] input)
		{
			int lcm, i;

			lcm = input[0];
 
			i = 1;
 
			do
			{
				lcm = (lcm * input[i]) / GCD(lcm, input[i]);
  
				i += 1;      
 
			} while(i < input.Length);
 
			return lcm;
 		}

		/// <summary>
		/// Returns GCD of input
		/// </summary>
		/// <param name="m">Input 1</param>
		/// <param name="n">Input 2</param>
		/// <returns>GCD of Input 1 and 2</returns>
		public static int GCD(int m, int n)
		{
			int r;
 
			while (true) 
			{
				r = m % n;
				if (r == 0) 
					break;
				
				m = n;
				n = r;
			}
			return n;
		}
	}

//	/// <summary>
//	/// Possible directions of platoon travel
//	/// </summary>
//	public class Direction
//	{
//		public static int NS = 0;
//		public static int SN = 1;
//		public static int EW = 2;
//		public static int WE = 4;
//	}

	
}
