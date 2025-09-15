using Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class DroneService : IDroneService
    {
        private StreamWriter measurementsWriter;
        private StreamWriter rejectsWriter;

        private double previousAnorm = 0;
        private double runningMeanA = 0;
        private int samplesCount = 0;
        public string EndSession()
        {
            throw new NotImplementedException();
        }

        public string PushSample(DroneSample sample)
        {
            throw new NotImplementedException();
        }

        public string StartSession(string metaHeader)
        {
            string filePathMeasurements = "measurements_session.csv";
            string filePathReject = "rejects.csv";

            measurementsWriter = new StreamWriter(File.Create(filePathMeasurements));
            measurementsWriter.WriteLine(metaHeader);
            measurementsWriter.Dispose();

            rejectsWriter = new StreamWriter(File.Create(filePathReject));
            rejectsWriter.WriteLine(metaHeader);
            rejectsWriter.Dispose();

            //resetuj
            previousAnorm = 0;
            runningMeanA = 0;
            samplesCount = 0;

            Console.WriteLine("Session started...");
            return "ACK: IN_PROGRESS";
        }
    }
}
