using System;
using System.Collections.Generic;
using System.Text;
using MCP;
using System.Data;
using Util;
using System.Timers;

namespace App.Dispatching.Process
{
    public class CraneProcess : AbstractProcess
    {
        private class rCrnStatus
        {
            public string TaskNo { get; set; }
            public int Status { get; set; }
            public int Action { get; set; }
            public int ErrCode { get; set; }
            public int TaskStatus { get; set; }
            public int io_flag { get; set; }

            public rCrnStatus()
            {
                TaskNo = "";
                Status = 0;
                Action = 0;
                ErrCode = 0;
                TaskStatus = 0;
                io_flag = 0;
            }
        }

        // 记录堆垛机当前状态及任务相关信息
        BLL.BLLBase bll = new BLL.BLLBase();        
        private Dictionary<int, rCrnStatus> dCrnStatus = new Dictionary<int, rCrnStatus>();
        private Timer tmWorkTimer = new Timer();
        private bool blRun = false;


        public override void Initialize(Context context)
        {
            try
            {
                //获取堆垛机信息
                DataTable dt = bll.FillDataTable("CMD.SelectCrane", new DataParameter[] { new DataParameter("{0}", "1=1") });
                for (int i = 1; i <= dt.Rows.Count; i++)
                {
                    if (!dCrnStatus.ContainsKey(i))
                    {
                        rCrnStatus crnsta = new rCrnStatus();
                        dCrnStatus.Add(i, crnsta);

                        dCrnStatus[i].TaskNo = "";
                        dCrnStatus[i].Status = int.Parse(dt.Rows[i-1]["State"].ToString());
                        dCrnStatus[i].TaskStatus = 0;
                        dCrnStatus[i].ErrCode = 0;
                        dCrnStatus[i].Action = 0;
                    }
                }
                

                tmWorkTimer.Interval = 1000;
                tmWorkTimer.Elapsed += new ElapsedEventHandler(tmWorker);
                

                base.Initialize(context);
            }
            catch (Exception ex)
            {
                Logger.Error("Dispatching.Process.Process_Crane.CraneProcess堆垛机初始化出错，原因：" + ex.Message);
            }
        }
        protected override void StateChanged(StateItem stateItem, IProcessDispatcher dispatcher)
        {
            try
            {
                object obj = ObjectUtil.GetObject(stateItem.State);
                //if (obj == null)
                //    return;

                switch (stateItem.ItemName)
                {
                    case "CraneTaskFinished":
                        obj = ObjectUtil.GetObject(stateItem.State);
                        if (obj.ToString() == "2")
                        {
                            string TaskNo = Util.ConvertStringChar.BytesToString(ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(stateItem.Name, "CraneTaskNo")));
                            //存储过程处理
                            Logger.Info(stateItem.ItemName + "完成标志,任务号:" + TaskNo);
                            sbyte[] taskNo = new sbyte[10];
                            Util.ConvertStringChar.stringToBytes("", 10).CopyTo(taskNo, 0);

                            Context.ProcessDispatcher.WriteToService(stateItem.Name, "TaskNo", taskNo);
                            //更新任务状态
                            DataParameter[] param = new DataParameter[] { new DataParameter("@TaskNo", TaskNo) };
                            bll.ExecNonQueryTran("WCS.Sp_TaskProcess", param);


                            WriteToService(stateItem.Name, "ReplyFinished", 1);
                        }
                        break;
                    case "Run":
                        blRun = (int)stateItem.State == 1;
                        if (blRun)
                        {
                            tmWorkTimer.Start();
                            Logger.Info("堆垛机联机");
                        }
                        else
                        {
                            tmWorkTimer.Stop();
                            Logger.Info("堆垛机脱机");
                        }
                        break;
                    default:
                        break;
                }
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                //如果是存储过程名称是PROC_NAME，而且State是数据库中设置的一个值 如：66
                //则该异常就是我们需要特殊处理的一个异常
                if (e.Procedure.Equals("Sp_CreateInTask") && e.State == 1)
                {
                    Logger.Error("Dispatching.Process.StockInToStationProcess：" + e.Message);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Dispatching.Process.StockInToStationProcess：" + e.Message);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tmWorker(object sender, ElapsedEventArgs e)
        {
            try
            {
                tmWorkTimer.Stop();

                DataTable dt = bll.FillDataTable("CMD.SelectCrane", new DataParameter[] { new DataParameter("{0}", "1=1") });
                for (int i = 1; i <= dt.Rows.Count; i++)
                {
                    if (!dCrnStatus.ContainsKey(i))
                    {
                        dCrnStatus[i].Status = int.Parse(dt.Rows[i - 1]["State"].ToString());
                    }
                }

                for (int i = 1; i <= dt.Rows.Count; i++)
                {
                    if (dCrnStatus[i].Status != 1)
                        continue;
                    if (dCrnStatus[i].io_flag == 0)
                    {
                        CraneOut(i);
                    }
                    else
                    {
                        CraneIn(i);
                    }
                }
                
            }
            finally
            {
                tmWorkTimer.Start();
            }
        }
        /// <summary>
        /// 检查堆垛机入库状态
        /// </summary>
        /// <param name="piCrnNo"></param>
        /// <returns></returns>
        private bool Check_Crane_Status_IsOk(int craneNo)
        {
            try
            {
                string serviceName = "CranePLC" + craneNo;

                string plcTaskNo = Util.ConvertStringChar.BytesToString(ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "CraneTaskNo")));

                //craneInfo 0 
                int[] craneInfo = new int[7];
                object[] obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "CraneInfo"));
                for (int j = 0; j < obj.Length; j++)
                    craneInfo[j] = Convert.ToInt16(obj[j]);
                //0 0-无故障货叉在原位无货
                //2 3-联机自动
                //3 0-待机 1-入库 2-出库
                //4 0-无故障 5-满货位入库，6-空货位出库
                if (craneInfo[0] == 0 && craneInfo[2] == 3 && (craneInfo[3] == 0 || (craneInfo[3] == 1 && (craneInfo[4] == 5 || craneInfo[4]==15) || (craneInfo[3] == 2 && (craneInfo[4] == 6 || craneInfo[4] == 16)))))
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            }            
        }

        /// <summary>
        /// 小车出库是否准备好
        /// </summary>
        /// <param name="piCrnNo"></param>
        /// <returns></returns>
        private bool Check_Car_Out_Status_IsOk(string carNo)
        {
            try
            {
                string itemName = carNo + "_CarInfo";

                int[] carInfo = new int[6];
                object[] obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService("CarPLC", itemName));
                for (int j = 0; j < 6; j++)
                    carInfo[j] = Convert.ToInt16(obj[j])-48;
                //object[] taskNo = new object[10];
                //for (int j = 0; j < 10; j++)
                //    taskNo[j] = obj[j];

                if (carInfo[0] == 0 && carInfo[2] == 2 && carInfo[3] == 0 && carInfo[4] == 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            }  
        }
        /// <summary>
        /// 小车出库是否准备好
        /// </summary>
        /// <param name="piCrnNo"></param>
        /// <returns></returns>
        private bool Check_Car_In_Status_IsOk(string carNo)
        {
            try
            {
                string itemName = carNo + "_CarInfo";

                int[] carInfo = new int[6];
                object[] obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService("CarPLC", itemName));
                for (int j = 0; j < 6; j++)
                    carInfo[j] = Convert.ToInt16(obj[j])-48;

                itemName = carNo + "_CarTaskInfo";
                obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService("CarPLC", itemName));
                object[] taskNo = new object[10];
                for (int j = 0; j < 10; j++)
                    taskNo[j] = obj[j];

                if (carInfo[0] == 1 && carInfo[2] == 2 && carInfo[4] == 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                return false;
            } 
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="craneNo"></param>
        private void CraneOut(int craneNo)
        {
            // 判断堆垛机的状态 自动  空闲
            //Logger.Debug("判断堆垛机" + piCrnNo.ToString() + "能否出库");
            try
            {
                //切换入库优先
                dCrnStatus[craneNo].io_flag = 1;
                //判断堆垛机
                if (!Check_Crane_Status_IsOk(craneNo))
                {
                    //Logger.Info("堆垛机状态不符合出库");
                    return;
                }                
            }
            catch (Exception e)
            {
                Logger.Debug("Crane out 状态检查错误:" + e.Message.ToString());
                return;
            }

            string serviceName = "CranePLC" + craneNo;

            int[] craneInfo = new int[6];
            object[] obj = ObjectUtil.GetObjects(WriteToService(serviceName, "CraneInfo"));

            obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "CraneTaskNo"));
            string plcTaskNo = Util.ConvertStringChar.BytesToString(obj);

            string StationLoad = ObjectUtil.GetObject(Context.ProcessDispatcher.WriteToService("ConveyorPLC1", "OutStationLoad")).ToString();
            //判断出库站台无货
            if (StationLoad.Equals("False") || StationLoad.Equals("0"))
            {
                Logger.Info("出库站台有货不符合堆垛机出库");
                return;
            }
            string CraneNo = "0" + craneNo.ToString();
            //获取任务，排序优先等级、任务时间
            DataParameter[] parameter = new DataParameter[] { new DataParameter("{0}", string.Format("WCS_Task.TaskType in ('12','13','14') and WCS_Task.State='0' and WCS_Task.CraneNo='{0}'",CraneNo)) };
            DataTable dt = bll.FillDataTable("WCS.SelectTask", parameter);

            //出库
            if (dt.Rows.Count>0)
            {
                DataRow dr = dt.Rows[0];
                //string carNo = dr["CarNo"].ToString();
                string TaskNo = dr["TaskNo"].ToString();
                string BillID = dr["BillID"].ToString();
                int CraneErrCode = byte.Parse(dr["CraneErrCode"].ToString());
                byte taskType = byte.Parse(dt.Rows[0]["TaskType"].ToString().Substring(1, 1));
                
                string fromStation = dt.Rows[0]["FromStation"].ToString();
                string toStation = dt.Rows[0]["ToStation"].ToString();

                if (plcTaskNo != "" && TaskNo != plcTaskNo.Trim())
                    return;

                

                if (plcTaskNo.Length > 0)
                {
                    if (CraneErrCode == 6 || CraneErrCode == 16)
                        taskType = 6;                    
                }

                byte[] cellAddr = new byte[8];

                cellAddr[0] = taskType;
                cellAddr[1] = 0;  //0-不允许伸叉，1-允许伸叉
                cellAddr[2] = byte.Parse(fromStation.Substring(0, 3));
                cellAddr[3] = byte.Parse(fromStation.Substring(3, 3));
                cellAddr[4] = byte.Parse(fromStation.Substring(6, 3));
                cellAddr[5] = byte.Parse(toStation.Substring(0, 3));
                cellAddr[6] = byte.Parse(toStation.Substring(3, 3));
                cellAddr[7] = byte.Parse(toStation.Substring(6, 3));

                sbyte[] palletBarcode = new sbyte[8];
                Util.ConvertStringChar.stringToBytes(dr["PalletCode"].ToString(), 8).CopyTo(palletBarcode, 0);

                sbyte[] taskNo = new sbyte[10];
                Util.ConvertStringChar.stringToBytes(dr["TaskNo"].ToString(), 10).CopyTo(taskNo, 0);

                
                WriteToService(serviceName, "TaskAddress", cellAddr);
                WriteToService(serviceName, "PalletCode", palletBarcode);
                WriteToService(serviceName, "TaskNo", taskNo);
                WriteToService(serviceName, "ProductType", 1);
                if (WriteToService(serviceName, "WriteFinished", 1))
                {
                    //更新任务状态为执行中
                    bll.ExecNonQuery("WCS.UpdateTaskTimeByTaskNo", new DataParameter[] { new DataParameter("@State", 1), new DataParameter("@TaskNo", TaskNo) });
                    //bll.ExecNonQuery("WCS.UpdateBillStateByBillID", new DataParameter[] { new DataParameter("@State", 3), new DataParameter("@BillID", BillID) });
                }
                Logger.Info("任务:" + dr["TaskNo"].ToString() + "已下发给" + craneNo + "堆垛机;起始地址:" + fromStation + ",目标地址:" + toStation);
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="craneNo"></param>
        private void CraneIn(int craneNo)
        {
            // 判断堆垛机的状态 自动  空闲
            try
            {
                //切换入库优先
                dCrnStatus[craneNo].io_flag = 0;
                //判断堆垛机
                if (!Check_Crane_Status_IsOk(craneNo))
                    return;

                
            }
            catch (Exception e)
            {
                //Logger.Debug("Crane out 状态检查错误:" + e.Message.ToString());
                return;
            }

            string serviceName = "CranePLC" + craneNo;

            int[] craneInfo = new int[6];
            object[] obj = ObjectUtil.GetObjects(WriteToService(serviceName, "CraneInfo"));

            obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "CraneTaskNo"));
            string plcTaskNo = Util.ConvertStringChar.BytesToString(obj);

            string CraneNo = "0" + craneNo.ToString();
            //获取任务，排序优先等级、任务时间
            DataParameter[] parameter = new DataParameter[] { new DataParameter("{0}", string.Format("((WCS_Task.TaskType='11' and WCS_Task.State='0') or (WCS_Task.TaskType='14' and WCS_Task.State='5')) and WCS_Task.CraneNo='{0}'", CraneNo)) };
            DataTable dt = bll.FillDataTable("WCS.SelectTask", parameter);

            //出库
            if (dt.Rows.Count > 0)
            {
                DataRow dr = dt.Rows[0];

                string TaskNo = dr["TaskNo"].ToString();
                string BillID = dr["BillID"].ToString();
                int CraneErrCode = byte.Parse(dr["CraneErrCode"].ToString());
                byte taskType = byte.Parse(dt.Rows[0]["TaskType"].ToString().Substring(1, 1));
                string fromStation = dt.Rows[0]["FromStation"].ToString();
                string toStation = dt.Rows[0]["ToStation"].ToString();

                //如果重入，需要重新下任务需判断堆垛机任务号跟当前要下的任务号是否一致，如不一致不能下达
                if (plcTaskNo != "" && TaskNo != plcTaskNo.Trim())
                    return;

                if (plcTaskNo.Length > 0)
                {
                    if (CraneErrCode == 5 || CraneErrCode == 15)
                        taskType = 5;
                }

                byte[] cellAddr = new byte[8];

                cellAddr[0] = taskType;
                cellAddr[1] = 0;  //0-不允许伸叉，1-允许伸叉
                cellAddr[2] = byte.Parse(fromStation.Substring(0, 3));
                cellAddr[3] = byte.Parse(fromStation.Substring(3, 3));
                cellAddr[4] = byte.Parse(fromStation.Substring(6, 3));
                cellAddr[5] = byte.Parse(toStation.Substring(0, 3));
                cellAddr[6] = byte.Parse(toStation.Substring(3, 3));
                cellAddr[7] = byte.Parse(toStation.Substring(6, 3));

                sbyte[] palletBarcode = new sbyte[8];
                Util.ConvertStringChar.stringToBytes(dr["PalletCode"].ToString(), 8).CopyTo(palletBarcode, 0);

                sbyte[] taskNo = new sbyte[10];
                Util.ConvertStringChar.stringToBytes(dr["TaskNo"].ToString(), 10).CopyTo(taskNo, 0);

                WriteToService(serviceName, "TaskAddress", cellAddr);
                WriteToService(serviceName, "PalletCode", palletBarcode);
                WriteToService(serviceName, "TaskNo", taskNo);
                WriteToService(serviceName, "ProductType", 1);
                if (WriteToService(serviceName, "WriteFinished", 1))
                {
                    string State = "1";
                    if (taskType == 4)
                        State = "6";
                    //更新任务状态为执行中
                    bll.ExecNonQuery("WCS.UpdateTaskTimeByTaskNo", new DataParameter[] { new DataParameter("@State", State), new DataParameter("@TaskNo", TaskNo) });
                    //bll.ExecNonQuery("WCS.UpdateBillStateByBillID", new DataParameter[] { new DataParameter("@State", 3), new DataParameter("@BillID", BillID) });
                }
                Logger.Info("任务:" + dr["TaskNo"].ToString() + "已下发给" + craneNo + "堆垛机;起始地址:" + fromStation + ",目标地址:" + toStation);
            }
        }
    }
}