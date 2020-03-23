using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using CoronaVirus.Models;
using RestSharp;
using System.Text.Json;
using RestSharp.Extensions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json.Linq;

namespace CoronaVirus.Controllers
{
    public class InitialComparator : IEqualityComparer<string>
    {
        public bool Equals(string x, string y)
        {
            return x.Contains(y);
        }

        public int GetHashCode(string obj)
        {
            return obj[0].GetHashCode();
        }
    }
    /* l'attributo riassume parte di quanto si può fare con una rotta tradizionale
     * routes.MapRoute("covid", "api/covid/{country}}",
            defaults: new { controller = "Data", action = "GetCovidstat" });

     */
    [Route("api/[controller]")]
    public class CovidController : Controller
    {
        private readonly ILogger<CovidController> _logger;

        public CovidController(ILogger<CovidController> logger)
        {
            _logger = logger;
        }


        async Task<List<CovidISO>> GetCovidstatAsync(string country)
        { 


            List<Covid> CovidStats = new List<Covid>();
            var client = new RestClient("https://covid-19-coronavirus-statistics.p.rapidapi.com/v1/stats?country=" + country);
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "covid-19-coronavirus-statistics.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "74bcdb93f9mshccd0aa272253435p190b6cjsn6145df755185");
            _logger.LogInformation("Starting call to Api");
            // GetStringAsync returns a Task<IRestResponse>. That means that when you await the
            // task you'll get a IRestResponse (urlContents).
            Task<IRestResponse> getStringTask = client.ExecuteAsync(request);

            // You can do work here that doesn't rely on the string from GetStringAsync.


            // The await operator suspends AccessTheWebAsync.
            //  - AccessTheWebAsync can't continue until getStringTask is complete.
            //  - Meanwhile, control returns to the caller of AccessTheWebAsync.
            //  - Control resumes here when getStringTask is complete. 
            //  - The await operator then retrieves the string result from getStringTask.
            IRestResponse urlContents = await getStringTask;
            var ConvertedJSON = JsonDocument.Parse(urlContents.Content, default);
            var covid = ConvertedJSON.RootElement.GetProperty("data").GetProperty("covid19Stats");

            CovidStats = JsonSerializer.Deserialize<List<Covid>>(covid.ToString(), default);
            _logger.LogInformation("Got json from API "+Task.CurrentId);

            var covidISO = CombineIsoCountry(CovidStats, await GetISO(country));

            return covidISO;
            // The return statement specifies an  List<Covid> result.
            // Any methods that are awaiting GetCovidstatAsync retrieve the  List<Covid> objects.
        }
                //[HttpGet(Name = "GetCovid")]    //Route names can be used to generate a URL based on a specific route. 
                //              Route names have no impact on the URL matching behavior of routing and are only used for URL generation. 
                //              Route names must be unique application-wide.
        [Route("data/{country}")]
        
        public async Task<IActionResult> GetCovid(string country)
        {
            
            return Json(await GetCovidstatAsync(country));
        }



        private List<CovidISO> CombineIsoCountry(List<Covid> covids, List<Country> countries)
        {

            //var filterCountries = countries.Select(c => c.name).Intersect(covids.Select(s => s.country));
            //var filterContry = countries.Join(covids, c => c.name, s => s.country, (country, covid) => new { country.name, covid.country }, new InitialComparator()).Select(c => c.name);
            var filterCountries = from i in countries
                       from w in covids
                       where ((i.name.Contains(w.country) && w.province == "") )
                       select new 
                       {
                          w,
                           i.id,
                           i.longitude,
                           i.latitude
                       };
            var filterprov = from i in countries
                                  from w in covids
                                  where ((i.name.Contains(w.province) && w.province != ""))
                                  select new
                                  {
                                      w,
                                      i.id,
                                      i.longitude,
                                      i.latitude
                                  };
            var filterprov2 = from i in countries
                             from w in covids
                             where ((i.name.Contains(w.country) && w.province != ""))
                             select new
                             {
                                 w,
                                 i.id,
                                 i.longitude,
                                 i.latitude
                             };
            filterprov2 = filterprov2.GroupBy(c => c.id).Select(g => g.First());
            List<CovidISO> covISO = new List<CovidISO>();
            foreach (var t in filterCountries)
            {
                covISO.Add(new CovidISO(t.w, t.id, t.longitude, t.latitude));
            }
            foreach(var t in filterprov)
            {
                covISO.Add(new CovidISO(t.w, t.id, t.longitude, t.latitude));
            }
            
            //covids.ForEach(c=> {

            //    countries.ForEach(s => {

            //        if (s.name.Contains(c.country) && c.province == "" )
            //        {
            //            covISO.Add(new CovidISO(c, s.id, s.longitude, s.latitude));
            //        }
            //        if(c.province.Contains(s.name))
            //        {
            //            covISO.Add(new CovidISO(c, s.id, s.longitude, s.latitude));
            //        }
            //        if (c.country.Contains(","))
            //        {
            //            var state = c.country.Split(',');
            //            if (state[0].Contains(s.name))
            //            {
            //                covISO.Add(new CovidISO(c, s.id, s.longitude, s.latitude));
            //            }
            //        }
            //        if (c.province != "" && c.country.Contains(s.name))
            //        {
            //            covISO.Add(new CovidISO(c, s.id, s.longitude, s.latitude));
            //        }
            //        if (c.country.Trim() == s.iso2Code)
            //        {
            //            var us = countries.Where(co => co.iso2Code == "US").ToList();
            //            covISO.Add(new CovidISO(c, us[0].id, us[0].longitude, us[0].latitude));
            //        }
            //    });

            //});


            return covISO;
        }

         async Task<List<Country>> GetISO(string country)
        {
            var client = new RestClient("http://api.worldbank.org/v2/country/all?format=json&per_page=304");
            var request = new RestRequest(Method.GET);
            _logger.LogInformation("Starting call to Country Api for ISO");
            // GetStringAsync returns a Task<IRestResponse>. That means that when you await the
            // task you'll get a IRestResponse (urlContents).
            Task<IRestResponse> getStringTask = client.ExecuteAsync(request);

            // You can do work here that doesn't rely on the string from GetStringAsync.


            // The await operator suspends AccessTheWebAsync.
            //  - AccessTheWebAsync can't continue until getStringTask is complete.
            //  - Meanwhile, control returns to the caller of AccessTheWebAsync.
            //  - Control resumes here when getStringTask is complete. 
            //  - The await operator then retrieves the string result from getStringTask.
            IRestResponse urlContents = await getStringTask;
            JArray jarray = JArray.Parse(urlContents.Content);
            var countries = jarray[1].ToObject<List<Country>>();
            if (country.ToLower() != "all") {
                countries=countries.Where(c => c.name == country).ToList<Country>();
            }
           
            return countries;
        }

        [HttpGet("iso/{country}")]
        public async Task<IActionResult> GetISOCode(string country)
        {
            
            return Json(await GetISO(country));
        }


        //sync implement
       
        [HttpGet]
        [Route("stat/{country}")]
        public IActionResult Get(string country)
        {
            return Json(GetCovidstat(country));
        }
        
        public List<Covid> GetCovidstat(string country)
        {
            List<Covid> CovidStats = new List<Covid>();
            var client = new RestClient("https://covid-19-coronavirus-statistics.p.rapidapi.com/v1/stats?country=" + country);
            var request = new RestRequest(Method.GET);
            request.AddHeader("x-rapidapi-host", "covid-19-coronavirus-statistics.p.rapidapi.com");
            request.AddHeader("x-rapidapi-key", "74bcdb93f9mshccd0aa272253435p190b6cjsn6145df755185");
            IRestResponse response = client.Execute(request);

            var ConvertedJSON = JsonDocument.Parse(response.Content, default);
            var covid = ConvertedJSON.RootElement.GetProperty("data").GetProperty("covid19Stats");

            CovidStats = JsonSerializer.Deserialize<List<Covid>>(covid.ToString(), default);
            return CovidStats;
        }
        


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
