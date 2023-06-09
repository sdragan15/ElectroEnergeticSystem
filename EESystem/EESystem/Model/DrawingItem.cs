﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EESystem.Model
{
    public class DrawingItem
    {
        public string Uid { get; set; }
        public string? Name { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double StrokeThickness { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
    }
}
