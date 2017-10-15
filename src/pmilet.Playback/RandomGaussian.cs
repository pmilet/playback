// Copyright (c) 2017 Pierre Milet. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
using System;

namespace pmilet.Playback
{
    internal class RandomGaussian
    {
        private static Random random = new Random();
        private static bool haveNextNextGaussian;
        private static double nextNextGaussian;

        public static double NextInRange(double from, double mean, double to)
        {
            if (!(from <= mean && mean <= to))
                throw new ArgumentOutOfRangeException();

            int p = Convert.ToInt32(random.NextDouble() * 100);
            double retval;
            if (p < (mean * Math.Abs(from - to)))
            {
                double interval1 = (NextGaussian() * (mean - from));
                retval = from + (float)(interval1);
            }
            else
            {
                double interval2 = (NextGaussian() * (to - mean));
                retval = mean + (float)(interval2);
            }
            while (retval < from || retval > to)
            {
                if (retval < from)
                    retval = (from - retval) + from;
                if (retval > to)
                    retval = to - (retval - to);
            }
            return retval;
        }

        private static double NextGaussian()
        {
            if (haveNextNextGaussian)
            {
                haveNextNextGaussian = false;
                return nextNextGaussian;
            }
            else
            {
                double v1, v2, s;
                do
                {
                    v1 = 2 * random.NextDouble() - 1;
                    v2 = 2 * random.NextDouble() - 1;
                    s = v1 * v1 + v2 * v2;
                } while (s >= 1 || s == 0);
                double multiplier = Math.Sqrt(-2 * Math.Log(s) / s);
                nextNextGaussian = v2 * multiplier;
                haveNextNextGaussian = true;
                return v1 * multiplier;
            }
        }
    }
}