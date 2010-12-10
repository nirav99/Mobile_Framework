using System;
using System.Collections.Generic;
using System.Text;

namespace Mobile_Framework
{
    /// <summary>
    /// Class that represents how many cars arrived in which direction at specified time on a given road
    /// </summary>
    public class ArrivalLog
    {
        public int dirn;       // Direction
        public int numCars;    // number of cars

        public ArrivalLog(int _dirn, int _numCars)
        {
            dirn = _dirn;
            numCars = _numCars;
        }

       public override string  ToString()
       {
           string result = null;

           if (dirn == Direction.NS)
               result = "NS " + numCars;
           if (dirn == Direction.EW)
               result = "EW " + numCars;
           if (dirn == Direction.SN)
               result = "SN " + numCars;
           if (dirn == Direction.WE)
               result = "WE " + numCars;
           return result;
       }
    }


}
