using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    [DataContract]
    public class DroneSample
    {
        private double linearAccelerationX;
        private double linearAccelerationY;
        private double linearAccelerationZ;
        private double windSpeed;
        private double windAngle;
        private double dateTime;

        [DataMember]
        public double LinearAccelerationX { get => linearAccelerationX; set => linearAccelerationX = value; }

        [DataMember]
        public double LinearAccelerationY { get => linearAccelerationY; set => linearAccelerationY = value; }

        [DataMember]
        public double LinearAccelerationZ { get => linearAccelerationZ; set => linearAccelerationZ = value; }

        [DataMember]
        public double WindSpeed { get => windSpeed; set => windSpeed = value; }

        [DataMember]
        public double WindAngle { get => windAngle; set => windAngle = value; }

        [DataMember]
        public double Time { get => dateTime; set => dateTime = value; }
    }
}
