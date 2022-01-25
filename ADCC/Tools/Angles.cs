using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ASFramework
{
    public class Angles
    {
        /// <summary>
        /// convert 360 angle to 180 angle
        /// </summary>
        public static float C360TO180(float number)
        {
            if (number > 360)
                number -= 360;

            if (number > 180)
                return -360 + number;

            return number;
        }
    }
}