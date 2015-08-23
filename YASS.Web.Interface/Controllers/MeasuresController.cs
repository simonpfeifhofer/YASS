using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Mvc;
using YASS.Web.Interface.Models;
using NLog;
using Npgsql;
using System.Configuration;
using System.Data.Common;
using System.Text.RegularExpressions;

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

            if(!Regex.Match(measure.SensorName, "[a-zA-Z]+").Success){
                throw new Exception("Invalid SensorName. Only letter-combinations are supported.");
            }

            using (var conn = new NpgsqlConnection(ConfigurationManager.ConnectionStrings["dbConnection"].ConnectionString))
            {
                conn.Open();

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        @"INSERT INTO ""Measure""(""Timestamp"", ""ActivityIdentifier"", ""GroupIdentifier"", ""EntityIdentifier"", ""SensorName"", ""SensorType"", ""ValueNumeric"")
                          VALUES(@Timestamp, @ActivityIdentifier, @GroupIdentifier, @EntityIdentifier, @SensorName, @SensorType, @ValueNumeric);";
                    cmd.Parameters.AddWithValue("@Timestamp", measure.Timestamp);
                    cmd.Parameters.AddWithValue("@ActivityIdentifier", measure.ActivityIdentifier);
                    cmd.Parameters.AddWithValue("@GroupIdentifier", measure.GroupIdentifier);
                    cmd.Parameters.AddWithValue("@EntityIdentifier", measure.EntityIdentifier);
                    cmd.Parameters.AddWithValue("@SensorName", measure.SensorName);
                    cmd.Parameters.AddWithValue("@SensorType", measure.SensorType.ToString());
                    cmd.Parameters.AddWithValue("@ValueNumeric", Convert.ToDouble(measure.Value));
                    cmd.ExecuteNonQuery();

                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        @"  CREATE OR REPLACE VIEW public.""Fact_Measure"" AS
                            SELECT  ""Timestamp"" AS ""Timestamp"",
                                    ""ActivityIdentifier"" AS ""ActivityName"",
                                    ""GroupIdentifier"" AS ""GroupName"",
                                    ""EntityIdentifier"" AS ""EntityName"",
                                    ""SensorName"" AS ""SensorName"",
                                    ""ValueNumeric"" AS ""Value""
                            FROM    public.""Measure""
                            WHERE	""ValueNumeric"" IS NOT NULL";
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        @"  CREATE OR REPLACE VIEW public.""Dim_Date"" AS
                            SELECT EXTRACT(year FROM ""Timestamp"") AS ""Year"",
                                EXTRACT(quarter FROM ""Timestamp"") AS ""Quarter"",
                                EXTRACT(month FROM ""Timestamp"") AS ""Month"",
                                EXTRACT(week FROM ""Timestamp"") AS ""Week"",
                                EXTRACT(day FROM ""Timestamp"") AS ""Day"",
                                EXTRACT(hour FROM ""Timestamp"") AS ""Hour"",
                                EXTRACT(minute FROM ""Timestamp"") AS ""Minute"",
                                EXTRACT(second FROM ""Timestamp"") AS ""Second"",
	                            ""Timestamp"" AS ""Timestamp""
                            FROM public.""Measure""
                            ORDER BY ""Timestamp"" DESC";
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        @"  CREATE OR REPLACE VIEW public.""Dim_Activity"" AS
                            SELECT  ""ActivityIdentifier"" AS ""ActivityName"",
	                            COUNT(*) AS ""Weight""
                            FROM    public.""Measure""
                            WHERE	""ActivityIdentifier"" IS NOT NULL AND ""ActivityIdentifier"" <> '' 
                            GROUP BY ""ActivityIdentifier""
                            ORDER BY COUNT(*) DESC";
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        @"  CREATE OR REPLACE VIEW public.""Dim_Group"" AS
                            SELECT  ""GroupIdentifier"" AS ""GroupName"",
	                            COUNT(*) AS ""Weight""
                            FROM    public.""Measure""
                            WHERE	""GroupIdentifier"" IS NOT NULL AND ""GroupIdentifier"" <> '' 
                            GROUP BY ""GroupIdentifier""
                            ORDER BY COUNT(*) DESC";
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = conn.CreateCommand())
                {
                    cmd.CommandText =
                        @"  CREATE OR REPLACE VIEW public.""Dim_Entity"" AS
                            SELECT  ""EntityIdentifier"" AS ""EntityName"",
	                            COUNT(*) AS ""Weight""
                            FROM    public.""Measure""
                            WHERE	""EntityIdentifier"" IS NOT NULL AND ""EntityIdentifier"" <> '' 
                            GROUP BY ""EntityIdentifier""
                            ORDER BY COUNT(*) DESC";
                    cmd.ExecuteNonQuery();
                }

                using (var cmd = conn.CreateCommand())
                {

                    var commandBuilder = DbProviderFactories.GetFactory(conn).CreateCommandBuilder();
                    cmd.CommandText =
                        string.Format(
                            @"  CREATE OR REPLACE VIEW public.{0} AS
                                SELECT  ""SensorName"" AS ""SensorName"",
	                                COUNT(*) AS ""Weight""
                                FROM    public.""Measure""
                                WHERE	""SensorName"" = '{1}' 
                                GROUP BY ""SensorName""
                                ORDER BY COUNT(*) DESC",
                            commandBuilder.QuoteIdentifier(string.Format("Dim_Sensor_{0}", measure.SensorName)),
                            measure.SensorName
                        );
                    cmd.ExecuteNonQuery();
                }


                // ToDo: recreate schema for slicer 

            }


        }

    }
}