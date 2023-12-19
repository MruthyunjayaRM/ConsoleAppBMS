using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibBMS
{
    public class BatteryPack
    {
        public int BMSPackID { get; set; }
        public int CellCount { get; set; } = 16;
        public float CellVoltage { get; set; }
        public int CellAh {  get; set; }
        public float PresentPackVoltage { get; set; }
        public float PresentPackCurrent { get; set; }
        public int CRating { get; set; }
        public int CellMinTemp { get; set; }
        public int CellMaxTemp { get; set; }
        public float CellOVAlarm { get; set; }
        public float CellOVProtext { get; set; }
        public float CellUVAlarm { get; set; }
        public float CellUVProtect { get; set; }

        public float PackOVAlarm { get; set; }
        public float PackOVProtect { get; set; }
        public float PackUVAlarm { get; set; }
        public float PackUVProtect { get; set; }
        public float SOC { get; set; }
        public float SOCLowAlarm { get; set; }
        public float SOH { get; set; }
        public float DsgOCALarm { get; set; }
        public float DsgOCProtect { get; set; }
        public float ChgOCAlarm { get; set; }
        public float ChgOCProtext { get; set; }
        public sbyte PresentPackTemp { get; set; }  // Range -128 to 127 (sbyte)
        public sbyte ChgOTAlarm { get; set; }
        public sbyte ChgOTProtect { get; set; }
        public sbyte ChgUTAlarm { get; set; }
        public sbyte ChgUTProtext { get; set; }
        public sbyte DsgOTAlarm { get; set; }
        public sbyte DsgOTProtect { get; set; }
        public sbyte DsgUTAlarm { get; set; }
        public sbyte DsgUTProtext { get; set; }
        public sbyte PresentEnvTemp { get; set; }
        public sbyte EnvOTAlarm { get; set; }
        public sbyte EnvOTProtect {  get; set; }
        public sbyte EnvUTAlarm { get; set; }
        public sbyte EnvUTProtect { get; set; }
        public sbyte PresentMosTemp { get; set; }
        public sbyte MosOTAlarm { get; set; }
        public sbyte MosOTProtect { get; set; }

        public BatteryPack(int BMSPackID, int CellCount, float CellVoltage, int CellAh, int CRating)
        {
            this.BMSPackID = BMSPackID;
            this.CellCount = CellCount;
            this.CellVoltage = CellVoltage;
            this.CellAh = CellAh;
            this.CRating = CRating;

            // Had to write the code for other data members
        }
        public BatteryPack() { }
        public bool CellOVAlarmStatus
        {
            get
            {
                if (PresentPackVoltage > CellOVAlarm) return true; else return false;
            }
        }
        public bool CellUVAlarmStatus
        {
            get
            {
                if (PresentPackVoltage < CellUVAlarm) return true; else return false;
            }
        }
        public bool PackOVAlarmStatus
        {
            get
            {
                if (PresentPackVoltage > PackOVAlarm) return true; else return false;
            }
        }
        public bool PackUVAlarmStatus
        {
            get
            {
                if (PresentPackVoltage < PackUVAlarm) return true; else return false;
            }
        }
        public bool DsgOCALarmStatus
        {
            get
            {
                if (PresentPackCurrent > DsgOCALarm) return true; else return false;
            }
        }
        public bool ChgOCAlarmStatus
        {
            get
            {
                if (PresentPackCurrent > ChgOCAlarm) return true; else return false;
            }
        }
        public bool ChgOTAlarmStatus
        {
            get
            {
                if (PresentPackTemp > ChgOTAlarm) return true; else return false;
            }
        }
        public bool ChgUTAlarmStatus
        {
            get
            {
                if (PresentPackTemp < ChgUTAlarm) return true; else return false;
            }
        }
        public bool DsgOTAlarmStatus
        {
            get
            {
                if (PresentPackTemp > DsgOTAlarm) return true; else return false;
            }
        }
        public bool DsgUTAlarmStatus
        {
            get
            {
                if (PresentPackTemp < DsgUTAlarm) return true; else return false;
            }
        }
        public bool SOCLowAlarmStatus
        {
            get
            {
                if(SOC < SOCLowAlarm) return true; else return false;
            }
        }
        public bool EnvOTAlarmStatus
        {
            get
            {
                if (PresentEnvTemp > EnvOTAlarm) return true; else return false;
            }
        }
        public bool EnvUTAlarmStatus
        {
            get
            {
                if(PresentEnvTemp < EnvUTAlarm) return true; else return false;
            }
        }
        public bool MosOTAlarmStatus
        {
            get
            {
                if(PresentMosTemp > MosOTAlarm)return true; else return false;
            }
        }
    }

}
