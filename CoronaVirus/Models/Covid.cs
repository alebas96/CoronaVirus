﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaVirus.Models
{
    public class Covid
    {
        public string city { get; set; }
        public string province { get; set; }
        public string country { get; set; }
        public string lastUpdate { get; set; }
        public string keyId { get; set; }
        public int    confirmed { get; set; }
        public int    deaths { get; set; }
        public int    recovered { get; set; }


    }
}
