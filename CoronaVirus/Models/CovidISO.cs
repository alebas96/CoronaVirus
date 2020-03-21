using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CoronaVirus.Models
{
    public class CovidISO:Covid
    {
        public string id { get; set; }
        public string longitude { get; set; }
        public string latitude { get; set; }

        public CovidISO(Covid c,string id, string longitude, string latitude)
        {
            base.province = c.province;
            base.country = c.country;
            base.lastUpdate = c.lastUpdate;
            base.confirmed = c.confirmed;
            base.deaths = c.deaths;
            base.recovered = c.recovered;
            this.id = id;
            this.longitude = longitude;
            this.latitude = latitude;
        }
    }
}
