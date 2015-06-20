using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FSUIPC;
using System.Collections;

namespace FSUIPCServerLib
{
    public class Offset_C
    {
        public Offset_C(int Address, Datatype datatype, string datagroup)
        {
            this.address = Address;
            this.datagroup = datagroup;
            this.datatype = datatype;

            switch (this.datatype)
            {
                case Datatype.Int16:
                    {
                        offInt16 = new Offset<Int16>(this.datagroup, this.address);
                        break;
                    }
                case Datatype.Int32:
                    {
                        offInt32 = new Offset<Int32>(this.datagroup, this.address);
                        break;
                    }
                case Datatype.Int64:
                    {
                        offInt64 = new Offset<Int64>(this.datagroup, this.address);
                        break;
                    }
                case Datatype.Byte:
                    {
                        offByte = new Offset<SByte>(this.datagroup, this.address);
                        break;
                    }
                case Datatype.Double:
                    {
                        offDouble = new Offset<double>(this.datagroup, this.address);
                        break;
                    }
                case Datatype.Single:
                    {
                        offSingle = new Offset<Single>(this.datagroup, this.address);
                        break;
                    }
                case Datatype.String:
                    {
                        offString = new Offset<string>(this.datagroup, this.address, 100);
                        break;
                    }
                case Datatype.BitArray:
                    {
                        offBitArray = new Offset<BitArray>(this.datagroup, this.address);
                        break;
                    }
                case Datatype.ByteArray:
                    {
                        offByteArray = new Offset<byte[]>(this.datagroup, this.address);
                        break;
                    }
            }

            //offInt32 = new Offset<int>(this.Datagroup, this.address);
        }

        private Datatype datatype;
        private int address;
        public int Address { get {return address;} }
        public string Value {
            get
            {
                switch (this.datatype)
                {
                    case Datatype.Int16:
                        {

                            return offInt16.Value.ToString();
                        }
                    case Datatype.Int32:
                        {
                            return offInt32.Value.ToString();
                        }
                    case Datatype.Int64:
                        {
                            return offInt64.Value.ToString();
                        }
                    case Datatype.Byte:
                        {
                            return offByte.Value.ToString();
                        }
                    case Datatype.Double:
                        {
                            return offDouble.Value.ToString();
                        }
                    case Datatype.Single:
                        {
                            return offSingle.Value.ToString();
                        }
                    case Datatype.String:
                        {
                            return offString.Value;
                        }
                    case Datatype.BitArray:
                        {
                            return offBitArray.Value.ToString();
                        }
                    case Datatype.ByteArray:
                        {
                            return offByteArray.Value.ToString();
                        }
                }

                return "";
            }
            //set
            //{
                
            //}
        }
        private string datagroup;
        public string Datagroup { get { return datagroup; } }
        public Datatype DataType { get { return datatype; } }

        private Offset<SByte> offByte;
        private Offset<Int16> offInt16;
        private Offset<Int32> offInt32;
        private Offset<Single> offSingle;
        private Offset<Int64> offInt64;
        private Offset<Double> offDouble;
        private Offset<String> offString;
        private Offset<BitArray> offBitArray;
        private Offset<Byte[]> offByteArray;
    }

    public enum Datatype
    {
        Byte,
        Int16,
        Int32,
        Single,
        Int64,
        Double,
        String,
        BitArray,
        ByteArray
    }
}
