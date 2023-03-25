﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EESystem.Model
{
    public class Coordinates
    {
        public double X { get; set; }
        public double Y { get; set; }

        public override bool Equals(object? obj)
        {
            return obj is Coordinates coordinates &&
                   X == coordinates.X &&
                   Y == coordinates.Y;
        }
    }
}
