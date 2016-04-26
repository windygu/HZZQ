using System;
using System.Collections.Generic;
using System.Text;
using MCP;
using System.Data;
using Util;

namespace App.Dispatching.Process
{
    public class StockOutPalletProcess : AbstractProcess
    {
        int TimeDiff;
        Dictionary<int, string> dicProductNo = new Dictionary<int, string>();
        BLL.BLLBase bll = new BLL.BLLBase();

        public override void Initialize(Context context)
        {
            try
            {
                base.Initialize(context);                
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("StockOutPalletProcess 初始化失败！原因：{0}！", e.Message));
            }
        }
        protected override void StateChanged(StateItem stateItem, IProcessDispatcher dispatcher)
        {
            try
            {
                object obj = ObjectUtil.GetObject(stateItem.State);
                if (obj == null)
                    return;

                string PalletOut = obj.ToString();
                if (PalletOut.Equals("True") || PalletOut.Equals("1"))
                {
                    //如果工作模式是储存且不是托盘组入库，需产生一个托盘组出库任务
                    object workmode = ObjectUtil.GetObject(Context.ProcessDispatcher.WriteToService("ConveyorPLC2", "GetWorkMode"));

                    Logger.Info("触发出托盘");
                    //if (Program.mainForm.WorkMode == 1)
                    if (workmode.ToString() == "1")
                    {
                        string CellCode = "";
                        Logger.Info("开始产生空托盘出库");
                        DataParameter[] param = new DataParameter[] 
                                            {
                                                new DataParameter("@CraneNo", "01"), 
                                                new DataParameter("@ProductCode", "0001"),
                                                new DataParameter("@CellCode",CellCode),
                                                new DataParameter("@Valid",2),
                                                new DataParameter("@WorkMode",Program.mainForm.WorkMode),
                                                new DataParameter("@WorkModeId",Program.mainForm.WorkModeId)
                                            };

                        bll.FillDataTable("WCS.Sp_CreateOutTask", param);
                        Logger.Info("空托盘出库已产生");
                    }
                }
                
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                //如果是存储过程名称是PROC_NAME，而且State是数据库中设置的一个值 如：66
                //则该异常就是我们需要特殊处理的一个异常
                if (e.Procedure.Equals("Sp_CreateInTask") && e.State == 1)
                {
                    Logger.Error("Dispatching.Process.StockOutPalletProcess：" + e.Message);
                }
                else if (e.Procedure.Equals("Sp_CreateOutTask") && e.State == 1)
                {
                    Logger.Error("Dispatching.Process.StockOutPalletProcess：" + e.Message);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Dispatching.Process.StockOutPalletProcess：" + e.Message);
            }
        }
    }
}