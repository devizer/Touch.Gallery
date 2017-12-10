﻿using System;
using System.Collections.Generic;

namespace Gallery.MVC.Utils
{
    public static class MyListExtenstions
    {
        private static Random rng = new Random();
        public static void Shuffle<T>(this IList<T> array, int seed)
        {
            rng = new Random(seed);
            int n = array.Count;
            while (n > 1)
            {
                int k = rng.Next(n);
                n--;
                T temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
}