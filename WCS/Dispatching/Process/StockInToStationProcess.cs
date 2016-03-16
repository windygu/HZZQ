using System;
using System.Collections.Generic;
using System.Text;
using MCP;
using System.Data;
using Util;

namespace Dispatching.Process
{
    public class StockInToStation : AbstractProcess
    {
        protected override void StateChanged(StateItem stateItem, IProcessDispatcher dispatcher)
        {
            try
            {
                object obj = ObjectUtil.GetObject(stateItem.State);
                if (obj == null)
                    return;
                if (obj.ToString() == "48" || obj.ToString() == "0")
                    return;
                
                BLL.BLLBase bll = new BLL.BLLBase();
                //判断堆垛机状态是否可用

                //读取产品品种
                string filter = string.Format("WCS_Task.State='1' and WCS_TASK.TaskType='11' and WCS_TASK.CarNo='{0}'", "01");
                DataParameter[] parameter = new DataParameter[] { new DataParameter("{0}", filter) };
                DataTable dt = bll.FillDataTable("WCS.SelectTask", parameter);

                int rows = 0;
                
                WriteToService("CarPLC", "_CarReply", 50);
            }
            catch (Exception e)
            {
                Logger.Error("Dispatching.Process.CarProcess：" + e.Message);
            }
        }
    }
}