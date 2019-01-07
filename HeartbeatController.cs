using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using YamlDotNet.RepresentationModel;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using System.IO;
using Microsoft.Extensions.Configuration;

namespace BeatConfig.Controllers
{
    [Route("api/[controller]")]
    public class HeartbeatMonitorsController : Controller
    {
        //// GET api/values
        //[HttpGet]
        //public IEnumerable<string> Get()
        //{
        //    return new string[] { "value1", "value2" };
        //}

        //// GET api/values/5
        //[HttpGet("{id}")]
        //public string Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody]string value)
        //{
        //}

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody]string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}

        private readonly IConfiguration Configuration;
        public HeartbeatMonitorsController(IConfiguration config)
        {
            this.Configuration = config;
        }


        /// <summary>
        /// Get all monitors.
        /// </summary>
        /// <returns>Monitors</returns>
        [HttpGet]
        public List<Monitor> Get()
        {
            List<Monitor> monitorList = new List<Monitor>();

            var MonitorsPath = this.Configuration.GetSection("MySettings").GetSection("MonitorsPath").Value;

            DirectoryInfo d = new DirectoryInfo(MonitorsPath);

            try
            {
                foreach (var file in d.GetFiles("*.yml"))
                {
                    string alltext = System.IO.File.ReadAllText(file.FullName);
                    string prependedtext = alltext.Insert(0, "Monitor: " + Environment.NewLine);

                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(new CamelCaseNamingConvention())
                        .IgnoreUnmatchedProperties()
                        .Build();

                    var heartbeat1 = deserializer.Deserialize<Heartbeat>(prependedtext);
                    monitorList.Add(heartbeat1.Monitors[0]);
                }
            }
            catch (Exception)
            {

                throw;
            }


            return monitorList;

        }

        /// <summary>
        /// Delete a monitor.
        /// </summary>
        /// <returns></returns>
        [HttpDelete("{name}")]
        public void Delete(string name)
        {
            List<Monitor> monitorList = new List<Monitor>();
            var MonitorsPath = this.Configuration.GetSection("MySettings").GetSection("MonitorsPath").Value;
            DirectoryInfo d = new DirectoryInfo(MonitorsPath);

            try
            {
                foreach (var file in d.GetFiles("*.yml"))
                {
                    string alltext = System.IO.File.ReadAllText(file.FullName);
                    string prependedtext = alltext.Insert(0, "Monitor: " + Environment.NewLine);

                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(new CamelCaseNamingConvention())
                        .IgnoreUnmatchedProperties()
                        .Build();

                    var heartbeat1 = deserializer.Deserialize<Heartbeat>(prependedtext);

                    if (heartbeat1.Monitors[0].Name.Equals(name)) file.Delete();
                }
            }
            catch (Exception)
            {

                throw;
            }

        }



        /// <summary>
        /// Create new monitor.
        /// </summary>
        /// <param name="monitor"></param>
        /// <returns></returns>
        [HttpPost]
        public Heartbeat Post([FromBody]Heartbeat heartbeat)
        {
            try
            {
                Monitor mon0 = heartbeat.Monitors[0];
                //mon0.Request.Body = Base64Decode(mon0.Request.Body).Replace('"', '\"'); 
                // mon0.Request.Body = mon0.Request.Body.Replace('"', '\"'); 


                var serializer = new SerializerBuilder()
                       .Build();

                var yaml = serializer.Serialize(heartbeat);

                string MonitorName = mon0.Name;

                var MonitorsPath = this.Configuration.GetSection("MySettings").GetSection("MonitorsPath").Value;
                using (StreamWriter sw = System.IO.File.CreateText(MonitorsPath + MonitorName + ".yml"))
                {

                    sw.Write(yaml);
                }

                var lines = System.IO.File.ReadAllLines(MonitorsPath + MonitorName + ".yml");
                System.IO.File.WriteAllLines(MonitorsPath + MonitorName + ".yml", lines.Skip(1).Take(lines.Count() - 1));
            }
            catch (Exception)
            {

                throw;
            }

            return heartbeat;

        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }


    public class Heartbeat
    {
        [YamlMember(Alias = "Monitor", ApplyNamingConventions = false)]
        public List<Monitor> Monitors { get; set; }
    }


    public class Monitor
    {

        [YamlMember(Alias = "type", ApplyNamingConventions = false)]
        public string Type { get; set; }

        [YamlMember(Alias = "schedule", ApplyNamingConventions = false)]
        public string Schedule { get; set; }

        [YamlMember(Alias = "name", ApplyNamingConventions = false)]
        public string Name { get; set; }

        [YamlMember(Alias = "fields", ApplyNamingConventions = false)]
        public Fields Fields { get; set; }

        [YamlMember(Alias = "urls", ApplyNamingConventions = false)]
        public List<string> Urls { get; set; }

        [YamlMember(Alias = "ssl", ApplyNamingConventions = false)]
        public SSL SSL { get; set; }

        [YamlMember(Alias = "check.request", ApplyNamingConventions = false)]
        public Request Request { get; set; }

        [YamlMember(Alias = "check.response.status", ApplyNamingConventions = false)]
        public int ResponseStatus { get; set; }

    }

    public class Request
    {
        [YamlMember(Alias = "headers", ApplyNamingConventions = false)]
        public List<string> Headers { get; set; }

        [YamlMember(Alias = "body", ApplyNamingConventions = false)]
        public string Body { get; set; }

        [YamlMember(Alias = "method", ApplyNamingConventions = false)]
        public string Method { get; set; }

    }

    public class Fields
    {
        [YamlMember(Alias = "monitor.http.product", ApplyNamingConventions = false)]
        public string MonitorHttpProduct { get; set; }
    }

    public class SSL
    {
        [YamlMember(Alias = "enabled", ApplyNamingConventions = false)]
        public bool Enabled { get; set; }

        [YamlMember(Alias = "certificate_authorities", ApplyNamingConventions = false)]
        public List<string> CertificateAuthorities { get; set; }

        [YamlMember(Alias = "supported_protocols", ApplyNamingConventions = false)]
        public List<string> SupportedProtocols { get; set; }

        [YamlMember(Alias = "certificate", ApplyNamingConventions = false)]
        public string Certificate { get; set; }

        [YamlMember(Alias = "key", ApplyNamingConventions = false)]
        public string Key { get; set; }

        [YamlMember(Alias = "key_passphrase", ApplyNamingConventions = false)]
        public string KeyPassphrase { get; set; }

    }
}
