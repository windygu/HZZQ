using System;
using System.Collections.Generic;
using System.Text;
using MCP;
using System.Data;
using Util;

namespace Dispatching.Process
{
    public class StockInToStationProcess : AbstractProcess
    {
        int TimeDiff;
        Dictionary<int, string> dicProductNo = new Dictionary<int, string>();
        BLL.BLLBase bll = new BLL.BLLBase();

        public override void Initialize(Context context)
        {
            try
            {
                base.Initialize(context);
                TimeDiff = int.Parse(context.Attributes["TimeDiff"].ToString());
                dicProductNo.Add(0, "A");
                dicProductNo.Add(1, "B");
                dicProductNo.Add(2, "C");
                dicProductNo.Add(3, "D");
                dicProductNo.Add(4, "E");
                dicProductNo.Add(5, "F");
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("StockInToStationProcess 初始化失败！原因：{0}！", e.Message));
            }
        }
        protected override void StateChanged(StateItem stateItem, IProcessDispatcher dispatcher)
        {
            try
            {
                object obj = ObjectUtil.GetObject(stateItem.State);
                string TaskFinish = obj.ToString();
                if (TaskFinish.Equals("True") || TaskFinish.Equals("1"))
                {
                    //读取产品品种

                    string CellCode = "";
                    string ProductCode = "";
                    string ProductNo = "";
                    string ProductType = ObjectUtil.GetObject(Context.ProcessDispatcher.WriteToService(stateItem.Name, "ProductType")).ToString();
                    if (ProductType.Equals("True") || ProductType.Equals("1"))
                    {
                        int key = int.Parse(ObjectUtil.GetObject(Context.ProcessDispatcher.WriteToService(stateItem.Name, "ProductNo")).ToString());
                        ProductNo =dicProductNo[key];
                        
                        DataTable dt = bll.FillDataTable("WCS.SelectProduct", new DataParameter[] { new DataParameter("{0}", string.Format("ProductNo='{0}'",ProductNo)) });
                        if (dt.Rows.Count > 0)
                            ProductCode = dt.Rows[0]["ProductCode"].ToString();
                    }
                    else
                    {
                        ProductCode = "0001";
                    }
                    DataParameter[] param = new DataParameter[] 
                                            {
                                                new DataParameter("@CraneNo", "01"), 
                                                new DataParameter("@ProductCode", ProductCode),
                                                new DataParameter("@TimeDiff", TimeDiff),
                                                new DataParameter("@CellCode",CellCode)
                                            };

                    bll.FillDataTable("WCS.Sp_CreateInTask", param);

                    
                    //如果不是托盘组入库，需产生一个托盘组出库任务
                    if (ProductCode != "0001")
                    {
                        CellCode = "";
                        param = new DataParameter[] 
                                            {
                                                new DataParameter("@CraneNo", "01"), 
                                                new DataParameter("@ProductCode", "0001"),
                                                new DataParameter("@TimeDiff", TimeDiff),
                                                new DataParameter("@CellCode",CellCode)
                                            };

                        bll.FillDataTable("WCS.Sp_CreateInTask", param);
                    }
                    //清除申请标识
                    //WriteToService("ConveyorPLC", "_CarReply", 50);
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
    }
}