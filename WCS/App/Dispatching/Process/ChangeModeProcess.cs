using System;
using System.Collections.Generic;
using System.Text;
using MCP;
using System.Data;
using Util;

namespace App.Dispatching.Process
{
    public class ChangeModeProcess : AbstractProcess
    {
        BLL.BLLBase bll = new BLL.BLLBase();
        
        protected override void StateChanged(StateItem stateItem, IProcessDispatcher dispatcher)
        {
            try
            {
                object obj = ObjectUtil.GetObject(stateItem.State);
                if (obj == null)
                    return;

                string TaskFinish = obj.ToString();
                if (TaskFinish.Equals("True") || TaskFinish.Equals("1"))
                {
                    //保存状态
                    string WorkModeId = "";
                    DataTable dt = bll.FillDataTable("WCS.GetWorkModeId");
                    if (dt.Rows.Count > 0)
                        WorkModeId = dt.Rows[0][0].ToString();

                    bll.ExecNonQuery("WCS.InsertWorkMode", new DataParameter[] {  
                                                           new DataParameter("@WorkModeId", WorkModeId),
                                                           new DataParameter("@WorkMode", 1),
                                                           new DataParameter("@ProductCode", ""),
                                                           new DataParameter("@ProductName", ""),
                                                           new DataParameter("@OutQty", 0)});


                    //出空盘
                    string CellCode = "";
                    Logger.Info("直供切换储存后，开始产生空托盘出库");
                    DataParameter[] param = new DataParameter[] 
                                            {
                                                new DataParameter("@CraneNo", "01"), 
                                                new DataParameter("@ProductCode", "0001"),
                                                new DataParameter("@CellCode",CellCode),
                                                new DataParameter("@Valid",2),
                                                new DataParameter("@WorkMode",1),
                                                new DataParameter("@WorkModeId",WorkModeId)
                                            };

                    bll.FillDataTable("WCS.Sp_CreateOutTask", param);
                    
                    //下发给PLC
                    WriteToService("ConveyorPLC2", "WorkMode", 1);
                }
                
            }
            catch (System.Data.SqlClient.SqlException e)
            {
                //如果是存储过程名称是PROC_NAME，而且State是数据库中设置的一个值 如：66
                //则该异常就是我们需要特殊处理的一个异常
                if (e.Procedure.Equals("Sp_CreateInTask") && e.State == 1)
                {
                    Logger.Error("Dispatching.Process.ChangeModeProcess：" + e.Message);
                }
                else if (e.Procedure.Equals("Sp_CreateOutTask") && e.State == 1)
                {
                    Logger.Error("Dispatching.Process.ChangeModeProcess：" + e.Message);
                }
            }
            catch (Exception e)
            {
                Logger.Error("Dispatching.Process.ChangeModeProcess：" + e.Message);
            }
        }
    }
}