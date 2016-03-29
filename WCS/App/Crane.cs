﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace App
{
    public class Crane
    {
        public int CraneNo { get; set; }
        public string TaskNo { get; set;}
        public string PalletCode { get; set; }
        public int TaskType { get; set;}
        public int Action { get; set; }
        public int Column { get; set; }
        public int Height { get; set; }
        public int ForkStatus { get; set; }
        public int ErrCode { get; set; }
        public int WalkCode { get; set; }
        public int UpDownCode { get; set; }
    }

    public delegate void CraneEventHandler(CraneEventArgs args);
    public class CraneEventArgs
    {
        private Crane _crane;
        public Crane crane
        {
            get
            {
                return _crane;
            }
        }
        public CraneEventArgs(Crane crane)
        {
            this._crane = crane;
        }
    }
    public class Cranes
    {
        public static event CraneEventHandler OnCrane = null;

        public static void CraneInfo(Crane crane)
        {
            if (OnCrane != null)
            {
                OnCrane(new CraneEventArgs(crane));
            }
        }
    }

    public class Car
    {
        public int CarNo { get; set; }
        public string TaskNo { get; set; }
        public string PalletCode { get; set; }
        public int TaskType { get; set; }
        public int Action { get; set; }
        public int Load { get; set; }
        public int Status { get; set; }
        public int ErrCode { get; set; }
    }

    public delegate void CarEventHandler(CarEventArgs args);
    public class CarEventArgs
    {
        private Car _car;
        public Car car
        {
            get
            {
                return _car;
            }
        }
        public CarEventArgs(Car car)
        {
            this._car = car;
        }
    }
    public class Cars
    {
        public static event CarEventHandler OnCar = null;

        public static void CarInfo(Car car)
        {
            if (OnCar != null)
            {
                OnCar(new CarEventArgs(car));
            }
        }
    }
    public class Conveyor
    {
        public int ConveyorNo { get; set; }
        public string TaskNo { get; set; }
        public string PalletCode { get; set; }
        public bool Fwd { get; set; }  //正转
        public bool Rev { get; set; }  //反转
        public bool Up { get; set; }  //上升
        public bool Down { get; set; }  //下降
        public bool Load { get; set; }  //有无货
        public int Status { get; set; }
        public int ErrCode { get; set; }
    }

    public delegate void ConveyorEventHandler(ConveyorEventArgs args);
    public class ConveyorEventArgs
    {
        private Conveyor[] _conveyor;
        public Conveyor[] conveyor
        {
            get
            {
                return _conveyor;
            }
        }
        public ConveyorEventArgs(Conveyor[] conveyor)
        {
            this._conveyor = conveyor;
        }
    }
    public class Conveyors
    {
        public static event ConveyorEventHandler OnConveyor = null;

        public static void ConveyorInfo(Conveyor[] conveyor)
        {
            if (OnConveyor != null)
            {
                OnConveyor(new ConveyorEventArgs(conveyor));
            }
        }
    }

    public delegate void SignalEventHandler(SignalEventArgs args);
    public class SignalEventArgs
    {
        private bool[,] _obj1;
        private bool[,] _obj2;
        private bool[,] _obj3;
        private bool[,] _obj4;

        public bool[,] obj1
        {
            get
            {
                return _obj1;
            }
        }
        public bool[,] obj2
        {
            get
            {
                return _obj2;
            }
        }
        public bool[,] obj3
        {
            get
            {
                return _obj3;
            }
        }
        public bool[,] obj4
        {
            get
            {
                return _obj4;
            }
        }

        public SignalEventArgs(bool[,] obj1, bool[,] obj2, bool[,] obj3, bool[,] obj4)
        {
            this._obj1 = obj1;
            this._obj2 = obj2;
            this._obj3 = obj3;
            this._obj4 = obj4;
        }
    }
    public class Signals
    {
        public static event SignalEventHandler OnSignal = null;

        public static void SignalInfo(bool[,] obj1, bool[,] obj2, bool[,] obj3, bool[,] obj4)
        {
            if (OnSignal != null)
            {
                OnSignal(new SignalEventArgs(obj1, obj2, obj3, obj4));
            }
        }
    }
}
