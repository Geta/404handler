// Copyright (c) Geta Digital. All rights reserved.
// Licensed under Apache-2.0. See the LICENSE file in the project root for more information

namespace BVNetwork.NotFound.Core
{
    public class ColorHelper
    {
        public static string GetRedTone(int maxValue, int minValue, int value)
        {
            if (maxValue == 0) return "00";

            var calculatedValue = value * 255 / maxValue;
            return calculatedValue.ToString("x");
        }
    }
}