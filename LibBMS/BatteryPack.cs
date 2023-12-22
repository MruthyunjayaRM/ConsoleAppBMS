using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibBMS
{
    public class BatteryPack
    {
        public byte[] BMSPackID { get; set; } = new byte[2];                  //
        public byte[] MCUID { get; set; }                   // Introduction-Time only
        public int CellCount { get; set; } = DefineValue.DEFAULT_CELL_NUMBER;
        public float CellVoltage { get; set; }
        public float[] IndCellVoltage { get; set; } = new float[DefineValue.DEFAULT_CELL_NUMBER];
        public int CellAh {  get; set; }
        public int CRating { get; set; }
        public int CellMinTemp { get; set; }
        public int CellMaxTemp { get; set; }

        public float SleepCellVoltage { get; set; }
        public byte SleepCellDelay { get; set; }

        public float CellBalanceThreshold { get; set; }
        public short CellBalanceDeltaVoltage_mV { get; set; }

        public float PresentPackVoltage { get; set; }       //
        public float CellOVAlarm { get; set; }
        public float CellOVProtext { get; set; }
        public float CellOVRelease { get; set; }
        public short CellOVDelayTime { get; set; }
        public float CellUVAlarm { get; set; }
        public float CellUVProtect { get; set; }
        public float CellUVRelease { get; set; }
        public short CellUVDelay { get; set; }
        public float PackOVAlarm { get; set; }
        public float PackOVProtect { get; set; }
        public float PackOVRelease { get; set; }
        public short PackOVDelay { get; set; }
        public float PackUVAlarm { get; set; }
        public float PackUVProtect { get; set; }
        public float PackUVRelease { get; set; }
        public short PackUVDelay { get; set; }

        public float PackFullChargeVoltage { get; set; }
        public short PackFullChargeCurrent_mA {  get; set; }

        public byte SOC { get; set; }                       //
        public byte SOCLowAlarm { get; set; }
        public byte SOH { get; set; }                       //

        public float PresentPackCurrent { get; set; }    //
        public float DsgOCALarm { get; set; }
        public float DsgOC1Protect { get; set; }
        public short DsgOC1PDelay {  get; set; }
        public float DsgOC2Protect { get; set; }
        public short DsgOCP2Delay { get; set; }
        public float ChgOCAlarm { get; set; }
        public float ChgOC1Protext { get; set; }
        public short ChgOCP1Delay { get; set; }
        public float ChgOC2Protect { get; set; }
        public short ChgOCP2Delay { get; set; }

        public short PresentPackTemp { get; set; }          //   // Range -128 to 127 (sbyte)
        public byte ChgOTAlarm { get; set; }
        public byte ChgOTProtect { get; set; }
        public byte ChngOTRelease { get; set; }
        public sbyte ChgUTAlarm { get; set; }
        public sbyte ChgUTProtext { get; set; }
        public sbyte ChngUTRelease { get; set; }
        public byte DsgOTAlarm { get; set; }
        public byte DsgOTProtect { get; set; }
        public byte DsgOTRelease { get; set; }
        public sbyte DsgUTAlarm { get; set; }
        public sbyte DsgUTProtext { get; set; }
        public sbyte DsgUTRelease { get; set; }

        public short PresentEnvTemp { get; set; }           //
        public byte EnvOTAlarm { get; set; }
        public byte EnvOTProtect {  get; set; }
        public byte EnvOTRelease { get; set; }
        public sbyte EnvUTAlarm { get; set; }
        public sbyte EnvUTProtect { get; set; }
        public sbyte EnvUTRelease { get; set; }

        public short PresentMosTemp { get; set; }           //
        public short MosOTAlarm { get; set; }
        public short MosOTProtect { get; set; }
        public short MosOTRelease { get; set; }

        public bool ChargerState {  get; set; }             //
        public bool DischargeState { get; set; }            //
        public bool HeaterState {  get; set; }              //
        public bool AutoPolling { get; set; }


        public BatteryPack(byte[] BMSPackID, byte[] MCUID, float CellVoltage, int CellAh, int CRating){
            this.MCUID = MCUID;
            this.BMSPackID = BMSPackID;
            this.CellVoltage = CellVoltage;
            this.CellAh = CellAh;
            this.CRating = CRating;

            // Had to write the code for other data members
        }
        public BatteryPack() {
        }
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

public static class DefineValue
{
    public const int DEFAULT_CELL_NUMBER = 16;
    public const int MAX_BMS = 10;
}
