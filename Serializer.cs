using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Runtime.Serialization.Formatters.Binary;
using System.Runtime.Serialization;
using System.Reflection;
using System.Security.Permissions;

namespace PSV_Server
{

    public enum PSVKeyType : uint
    {
        Left = 1u,
        Up = 2u,
        Right = 4u,
        Down = 8u,
        Square = 16u,
        Triangle = 32u,
        Circle = 64u,
        Cross = 128u,
        Start = 256u,
        Select = 512u,
        L = 1024u,
        R = 2048u,
        Enter = 65536u, // Not used refers to either x or circle I believe
        Back = 131072u,

        //Additional Buttons 
        B1 = 0x1 << 18,
        B2 = 0x1 << 19,
        B3 = 0x1 << 20,
        B4 = 0x1 << 21,
        B5 = 0x1 << 22,
        B6 = 0x1 << 23,
        B7 = 0x1 << 24,
        B8 = 0x1 << 25,
        B9 = 0x1 << 26,
        B10 = 0x1 << 27,
        B11 = 0x1 << 28,
        B12 = 0x1 << 29,
        B13 = 0x1 << 30,
        B14 = 0x1 << 12

    }


    [Serializable]
    public class VitaInputData
    {
        public uint keyData;
        //left anologue
        public float lx;
        public float ly;
        //right anologue
        public float rx;
        public float ry;

        //Motion Data from vita.
        public float motionX = 0;
        public float motionY = 0;
        public float motionZ = 0;

        public byte keyboardDat;

        // Holds rear touch data.
        public byte rearTouch = 0;
    };

    //Rear touch enumeration for clarity 
    //NOTE -> Actions on the rear touch will translate to joy buttons...
    //		  as such theres no variation in regards to swipe speed etc... 
    /// <summary>
    /// Rear touch button enumeration.
    /// SWIPE_UP --> User swipes up on rear touch panel, either side 
    /// SWIPE_DOWN --> User swipes down of the rear touch panel, either side 
    /// LEFT_TOUCH --> User touches the rear touch panel on the left hand side of the vita
    /// RIGHT_TOUCH --> User touched the rear touch panel on the right hand side of the vita 
    /// </summary>
    public enum PSVRearTouch : uint
    {
        SWIPE_UP = 0x1 << 0,
        SWIPE_DOWN = 0x1 << 1,
        LEFT_TOUCH = 0x1 << 2,
        RIGHT_TOUCH = 0x1 << 3
    }

    [Serializable]
    public class InputData : ISerializable
    {
        public uint keyData;
        //left anologue
        public float lx;
        public float ly;
        //right anologue
        public float rx;
        public float ry;

        //Motion Data from vita.
        public float motionX = 0;
        public float motionY = 0;
        public float motionZ = 0;

        public byte keyboardDat;

        // Holds rear touch data.
        public byte rearTouch = 0;

        // The security attribute demands that code that calls 
        // this method have permission to perform serialization.
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("keyData", keyData);
            info.AddValue("lx", lx);
            info.AddValue("ly", ly);
            info.AddValue("rx", rx);
            info.AddValue("ry", ry);
            info.AddValue("motionX", motionX);
            info.AddValue("motionY", motionY);
            info.AddValue("motionZ", motionZ);
            info.AddValue("keyboardDat", keyboardDat);
            info.AddValue("rearTouch", rearTouch); 
        }

        // The security attribute demands that code that calls   
        // this method have permission to perform serialization.
        [SecurityPermissionAttribute(SecurityAction.Demand, SerializationFormatter = true)]
        public InputData(SerializationInfo info, StreamingContext context)
        {
            keyData = info.GetUInt32("keyData");
            lx = info.GetSingle("lx");
            ly = info.GetSingle("ly");
            rx = info.GetSingle("rx");
            ry = info.GetSingle("ry");
            motionX = info.GetSingle("motionX");
            motionY = info.GetSingle("motionY");
            motionZ = info.GetSingle("motionZ");
            keyboardDat = info.GetByte("keyboardDat");
            rearTouch = info.GetByte("rearTouch");
           
        }
    }


    sealed class vitaAssemblyBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            Type typeToDeserialize = null;

            // For each assemblyName/typeName that you want to deserialize to 
            // a different type, set typeToDeserialize to the desired type.
            String assemVer1 = Assembly.GetExecutingAssembly().FullName;
            //String typeVer1 = "VitaInputData";

            assemblyName = assemVer1;
            typeName = "PSV_Server.InputData";
            /*if (assemblyName == assemVer1 && typeName == typeVer1)
            {
                // To use a type from a different assembly version,  
                // change the version number. 
                // To do this, uncomment the following line of code. 
                assemblyName = assemblyName.Replace("1.0.0.0", "2.0.0.0");

                // To use a different type from the same assembly,  
                // change the type name.
                typeName = "InputData";
            }*/

            // The following line of code returns the type.
            typeToDeserialize = Type.GetType(String.Format("{0}, {1}",
                typeName, assemblyName));

            return typeToDeserialize;
        }
    }
}
