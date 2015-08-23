using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using YASS.Batch.FitImporter.Models;

namespace YASS.Batch.FitImporter
{
    class Program
    {

        static void Main(string[] args)
        {

            try { 

            string endpoint = args[0];
            string file = args[1];

            var serializer = new XmlSerializer(typeof(TrainingCenterDatabase_t));

            TrainingCenterDatabase_t database = null;

            using (var sr = new StreamReader(file))
            {
                database = serializer.Deserialize(sr) as TrainingCenterDatabase_t;
            }

            if(database.Activities == null || database.Activities.Activity == null)
            {
                return;
            }

            foreach(var ac in database.Activities.Activity)
            {
                foreach(var lap in ac.Lap)
                {
                    foreach(var tp in lap.Track)
                    {
                        SubmitTrackPoint(ac, lap, tp, endpoint);
                        Console.WriteLine("Trackpoint {0} submitted!", tp.Time);
                    }
                }
            }

            }
            catch(Exception e)
            {
                Console.WriteLine(e);
            }

        }

        private static void SubmitTrackPoint(Activity_t activity, ActivityLap_t lap, Trackpoint_t tp, string url)
        {

            string requestString = JsonConvert.SerializeObject(
                new Measure()
                {
                    ActivityIdentifier = activity.Id.ToString(),
                    GroupIdentifier = "Team Dowe Sportswear",
                    Timestamp = tp.Time,
                    EntityIdentifier = "Simon Pfeifhofer",
                    SensorName = "HeartRate",
                    SensorType = SensorType.Numeric,
                    Value = Convert.ToDouble(tp.HeartRateBpm.Value).ToString()
                }
            );

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "POST";
            request.ContentType = "application/json";
            request.ContentLength = requestString.Length;
            using (StreamWriter requestWriter = new StreamWriter(request.GetRequestStream()))
            {
                requestWriter.Write(requestString);
            }

        }

    }
}
