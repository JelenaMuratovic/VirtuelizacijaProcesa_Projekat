using Common;
using Service.Subscriber;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class DroneService : IDroneService
    {
        private StreamWriter measurementsWriter;
        private StreamWriter rejectsWriter;

        private double previousAnorm = 0;
        private double Amean = 0;
        private double Anorm = 0;
        private double deltaA = 0;
        private double AnormSum = 0;
        private int samplesCount = 0;

        public delegate void TransferEventHandler(object sender, EventArgs e);
        public event TransferEventHandler TransferStartedEvent;
        public event TransferEventHandler TransferCompletedEvent;
        public event TransferEventHandler SampleReceivedEvent;


        public delegate void WarningEventHandler(object sender, WarningEventArguments e);
        public event WarningEventHandler WarningRaisedEvent;

        TransferCompletedObserver transferCompleted = new TransferCompletedObserver();
        TransferStartedObserver transferStarted = new TransferStartedObserver();
        SampleReceiveObserver sampleReceived = new SampleReceiveObserver();

        public DroneService()
        {
            TransferStartedEvent += transferStarted.OnTransferStarted;
            TransferCompletedEvent += transferCompleted.OnTransferCompleted;
            SampleReceivedEvent += sampleReceived.OnSampleReceived;
        }

        public string EndSession()
        {
            TransferCompletedEvent(this, EventArgs.Empty);

            measurementsWriter.Dispose();
            rejectsWriter.Dispose();

            return "ACK: COMPLETED";
        }

        public string PushSample(DroneSample sample)
        {
            SampleReceivedEvent(this, EventArgs.Empty);
            //validacija polja
            bool isValidFormat = ValidateFormat(sample);
            bool isValidValues = ValidateValues(sample);
            if (!isValidFormat)
            {
                throw new FaultException<DataFormatFault>(
                   new DataFormatFault($"Polja nisu validnog formata."),
                   "DataFormatFault");
            }
            if (!isValidValues)
            {

                //upisi u rejects.csv
                rejectsWriter.WriteLine(sample.ToString());

                throw new FaultException<DataValidationFault>(
                   new DataValidationFault($"Vrednosti polja nisu u validnom opsegu."),
                   "DataValidationFault");
            }
            //Console.WriteLine("Prenos u toku...");

            //upis u measurements.csv
            measurementsWriter.WriteLine(sample.ToString());

            samplesCount++;
            Anorm = Math.Sqrt(Math.Pow((double)sample.LinearAccelerationX, 2) + Math.Pow((double)sample.LinearAccelerationY, 2) + Math.Pow((double)sample.LinearAccelerationZ, 2));
            AnormSum += Anorm;
            Amean = AnormSum / samplesCount;
            
            if(samplesCount > 1)
            {
                deltaA = Anorm - previousAnorm;
                //Console.WriteLine("deltaA: " + deltaA);
            }
            previousAnorm = Anorm;


            //Console.WriteLine("Prenos zavrsen");
            return "ACK: COMPLETED";
        }
        private bool ValidateFormat(DroneSample sample)
        {
            if (sample.LinearAccelerationX == null || sample.LinearAccelerationY == null || sample.LinearAccelerationZ == null ||
                sample.WindSpeed == null || sample.WindAngle == null || sample.Time == null)
                return false;
            else
                return true;
        }
        private bool ValidateValues(DroneSample sample)
        {
            //dodaj validaciju za time
            if (sample.WindSpeed <= 0 || sample.WindAngle <= 0 || sample.Time < 0)
                return false;
            else
                return true;
        }

        public string StartSession(string metaHeader)
        {
            TransferStartedEvent(this, EventArgs.Empty);

            //prebaciti u app.config
            string filePathMeasurements = "measurements_session.csv";
            string filePathReject = "rejects.csv";

            measurementsWriter = new StreamWriter(filePathMeasurements);
            measurementsWriter.WriteLine(metaHeader);

            rejectsWriter = new StreamWriter(filePathReject);
            rejectsWriter.WriteLine(metaHeader);

            //resetuj
            previousAnorm = 0;
            Amean = 0;
            samplesCount = 0;

            return "ACK: IN_PROGRESS";
        }
    }
}
