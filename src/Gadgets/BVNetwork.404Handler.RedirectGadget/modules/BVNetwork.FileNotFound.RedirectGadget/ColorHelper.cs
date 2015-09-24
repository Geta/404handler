using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BVNetwork.FileNotFound.RedirectGadget
{
    public class ColorHelper
    {
        public static string GetRedTone(int maxValue, int minValue, int value)
        {

            if (maxValue != 0)
            {
                int calculatedValue = value * 255 / maxValue;


                return calculatedValue.ToString("x");
            }
            else
                return "00";

        }

    }
}