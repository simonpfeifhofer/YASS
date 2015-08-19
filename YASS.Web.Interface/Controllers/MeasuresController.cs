using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using YASS.Web.Interface.Models;
using NLog;
using Npgsql;
using System.Configuration;

namespace YASS.Web.Interface.Controllers
{
    [Route("api/[controller]")]
    public class MeasuresController : Controller
    {

        private Logger _logger = LogManager.GetCurrentClassLogger(); 

        // POST api/measures
        [HttpPost]
        public void Post([FromBody]Measure measure){

            _logger.Debug("New measure received");

            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString))
            {
                conn.Open();
                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        @"INSERT INTO public.Measure(Timestamp, ActivityIdentifier, GroupIdentifier, EntityIdentifier, SensorName, SensorType, ValueNumeric)
                          VALUES(@Timestamp, @ActivityIdentifier, @GroupIdentifier, @EntityIdentifier, @SensorName, @SensorType, @ValueNumeric);";
                    cmd.Parameters.AddWithValue("@Timestamp", measure.Timestamp);
                    cmd.Parameters.AddWithValue("@ActivityIdentifier", measure.ActivityIdentifier);
                    cmd.Parameters.AddWithValue("@GroupIdentifier", measure.GroupIdentifier);
                    cmd.Parameters.AddWithValue("@EntityIdentifier", measure.EntityIdentifier);
                    cmd.Parameters.AddWithValue("@SensorName", measure.SensorName);
                    cmd.Parameters.AddWithValue("@SensorType", measure.SensorType);
                    cmd.Parameters.AddWithValue("@ValueNumeric", Convert.ToDouble(measure.Value));
                    cmd.ExecuteNonQuery();

                }
            }


        }

    }
}