using System;

namespace Mobile_Framework
{
	/// <summary>
	/// 
	/// </summary>
	class Class1
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main(string[] args)
		{
			//
			// TODO: Add code to start application here
			//
			int seed = Environment.TickCount;

			seed = 2951;
			Random rN = new Random(seed);

			Console.WriteLine("Seed = " + seed);
			InputParameters ip = new InputParameters("input.txt", null);

            
		//	Algorithm algo = new FixedIntersectionTiming(ip);
			Algorithm  algo = new FixedPeriodVariableSize(ip);
        //    Algorithm algo = new FixedSizeVariablePeriod(ip);

			Network ns = new Network(ip, rN, algo);
			ns.runSimulation(7200);
            

 //           Console.WriteLine("Num Cars = " + ip.getNumCars(0, Direction.EW, 16));

//			PlatoonPerfCounter p1 = new PlatoonPerfCounter(1516, 16, 1500, 1, 3040, 0);
//			PlatoonPerfCounter p2 = new PlatoonPerfCounter(1500, 10, 1500, 1, 3040, 0);
//			PlatoonPerfCounter p3 = new PlatoonPerfCounter(1516, 16, 1500, 1, 3040, 0);
//			PlatoonPerfCounter p4 = new PlatoonPerfCounter(1510, 10, 1500, 1, 3040, 0);
//			
//
//			RoadPerfCounter rpf = new RoadPerfCounter();
//
//			rpf.addPlatoonCounter(p1);
//			rpf.addPlatoonCounter(p2);
//			rpf.addPlatoonCounter(p3);
//			rpf.addPlatoonCounter(p4);
//
//			for(int i = 0; i < 50; i++)
//			{
//				rpf.platoonCreated();
//			}
//
//			for(int i = 0; i < 4; i++)
//			{
//				rpf.platoonDeparted();
//			}
//
//			rpf.getStatistics();
		}
	}
}
