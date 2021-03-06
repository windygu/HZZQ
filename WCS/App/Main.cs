﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MCP;

namespace App
{
    public partial class Main : Form
    {
        private bool IsActiveForm = false;
        public bool IsActiveTab = false;
        private Context context = null;
        private System.Timers.Timer tmWorkTimer = new System.Timers.Timer();
        public int TimeDiff;
        public int WorkMode = 0;
        public string WorkModeId = "";
        public bool Run = false;
        public string ProductCode = "";

        public Main()
        {
            InitializeComponent();
        }

        private void inStockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.View.Task.frmInStock f = new View.Task.frmInStock();
            ShowForm(f);
        }

        private void Main_Shown(object sender, EventArgs e)
        {
            try
            {
                lbLog.Scrollable = true;
                Logger.OnLog += new LogEventHandler(Logger_OnLog);
                context = new Context();

                ContextInitialize initialize = new ContextInitialize();
                initialize.InitializeContext(context);

                this.TimeDiff = int.Parse(context.Attributes["TimeDiff"].ToString());
                string MonitorMode = context.Attributes["MonitorMode"].ToString();
                View.BaseForm f;
                if(MonitorMode=="1")
                    f = new View.frmMonitor();
                else
                    f = new View.frmMonitor1();
                ShowForm(f);

                //tmWorkTimer.Interval = 3000;
                //tmWorkTimer.Elapsed += new System.Timers.ElapsedEventHandler(tmWorker);
                //tmWorkTimer.Start();
                
            }
            catch (Exception ee)
            {
                Logger.Error("初始化处理失败请检查配置，原因：" + ee.Message);
            }
        }
        private void tmWorker(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                tmWorkTimer.Stop();

                //Sorting.Dispatching.Dal.OrderDal orderDal = new Dispatching.Dal.OrderDal();
                //DataTable dt = orderDal.GetSortingOrder().Tables[0];
                //for (int i = 0; i < dt.Rows.Count; i++)
                //{
                //    string requestNo = (i + 1).ToString();
                //    string batchNo = dt.Rows[i]["BATCHNO"].ToString();
                //    string SortNo = dt.Rows[i]["SORTNO"].ToString();
                //    DataSet ds = orderDal.GetOrder(batchNo, SortNo);
                //    Order.OrderInfo(requestNo, ds);
                //}
                //Logger.Debug("123");
                //Logger.Error("456");
            }
            finally
            {
                tmWorkTimer.Start();
            }
        }      
        void Logger_OnLog(MCP.LogEventArgs args)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new LogEventHandler(Logger_OnLog), args);
            }
            else
            {
                lock (lbLog)
                {
                    string msg1 = string.Format("[{0}]", args.LogLevel);
                    string msg2 = string.Format("{0}", DateTime.Now);
                    string msg3 = string.Format("{0} ", args.Message);
                    this.lbLog.BeginUpdate();
                    ListViewItem item = new ListViewItem(new string[] { msg1, msg2, msg3 });
                    
                    if (msg1.Contains("[ERROR]"))
                    {
                        //item.ForeColor = Color.Red;
                        item.BackColor = Color.Red;
                    }
                    lbLog.Items.Insert(0, item);
                    this.lbLog.EndUpdate();
                    WriteLoggerFile(msg1 + msg2 + msg3);
                }
            }
        }

        private void CreateDirectory(string directoryName)
        {
            if (!System.IO.Directory.Exists(directoryName))
                System.IO.Directory.CreateDirectory(directoryName);
        }

        private void WriteLoggerFile(string text)
        {
            try
            {
                string path = "";
                CreateDirectory("日志");
                path = "日志";
                path = path + @"/" + DateTime.Now.ToString().Substring(0, 4).Trim();
                CreateDirectory(path);
                path = path + @"/" + DateTime.Now.ToString("yyyy-MM-dd").Substring(0, 7).Trim();
                path = path.TrimEnd(new char[] { '-' });
                CreateDirectory(path);
                path = path + @"/" + DateTime.Now.ToString("yyyy-MM-dd") + ".txt";
                System.IO.File.AppendAllText(path, string.Format("{0} {1}", DateTime.Now, text + "\r\n"));
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
        }
        #region 公共方法
        /// <summary>
        /// 打开一个窗体

        /// </summary>
        /// <param name="frm"></param>
        private void ShowForm(Form frm)
        {
            if (OpenOnce(frm))
            {
                frm.MdiParent = this;
                ((View.BaseForm)frm).Context = context;
                frm.Show();
                frm.WindowState = FormWindowState.Maximized;
                AddTabPage(frm.Handle.ToString(), frm.Text);
            }
        }
        /// <summary>
        /// 判断窗体是否已打开
        /// </summary>
        /// <param name="frm"></param>
        /// <returns></returns>
        private bool OpenOnce(Form frm)
        {
            foreach (Form mdifrm in this.MdiChildren)
            {
                int index = mdifrm.Text.IndexOf(" ");
                if (index > 0)
                {
                    if (frm.Name == mdifrm.Name && frm.Text == mdifrm.Text.Substring(0, index))
                    {
                        mdifrm.Activate();
                        return false;
                    }
                }
                else
                {
                    if (frm.Name == mdifrm.Name && frm.Text == mdifrm.Text)
                    {
                        mdifrm.Activate();
                        return false;
                    }
                }
            }
            return true;

        }
        
        private void AddTabPage(string strKey, string strText)
        {
            IsActiveForm = true;
            TabPage tab = new TabPage();
            tab.Name = strKey.ToString();
            tab.Text = strText;
            tabForm.TabPages.Add(tab);
            tabForm.SelectedTab = tab;
            this.pnlTab.Visible = true;
            IsActiveForm = false;
        }
        
        public void SetActiveTab(string strKey, bool blnActive)
        {
            foreach (TabPage tab in this.tabForm.TabPages)
            {
                if (tab.Name == strKey)
                {
                    IsActiveForm = true;

                    if (blnActive)
                        tabForm.SelectedTab = tab;
                    else
                    {
                        tabForm.TabPages.Remove(tab);
                        if (this.MdiChildren.Length > 1)
                            this.pnlTab.Visible = true;
                        else
                            this.pnlTab.Visible = false;
                    }

                    IsActiveForm = false; ;
                }
            }
        }
        private void tabForm_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (IsActiveForm)
                return;
            foreach (Form mdifrm in this.MdiChildren)
            {
                if (mdifrm.Handle.ToInt32() == int.Parse(((TabControl)sender).SelectedTab.Name))
                {
                    IsActiveTab = true;
                    mdifrm.Activate();
                    IsActiveTab = false;
                }
            }
        }
        #endregion

        private void Main_Load(object sender, EventArgs e)
        {
            
        }

        private void ToolStripMenuItem_Cell_Click(object sender, EventArgs e)
        {
            App.View.Dispatcher.frmCellQuery f = new App.View.Dispatcher.frmCellQuery();
            ShowForm(f);
        }

        private void toolStripButton_Close_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton_Start.Text == "停止运行")
            {
                MessageBox.Show("请先停止运行后再退出系统!", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            if (DialogResult.Yes == MessageBox.Show("您确定要退出调度系统吗？", "询问", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                Logger.Info("退出系统");
                System.Environment.Exit(0);
            }
        }

        private void toolStripButton_InStockTask_Click(object sender, EventArgs e)
        {
            App.View.Task.frmInStock f = new View.Task.frmInStock();
            ShowForm(f);
        }

        private void toolStripButton_OutStock_Click(object sender, EventArgs e)
        {
            App.View.Task.frmOutStock f = new View.Task.frmOutStock();
            ShowForm(f);
        }

        private void OutStockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.View.Task.frmOutStock f = new View.Task.frmOutStock();
            ShowForm(f);
        }

        private void Main_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("您确定要退出调度系统吗？", "询问", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                Logger.Info("退出系统");
                System.Environment.Exit(0);
            }
            else
                e.Cancel = true;
        }

        private void MoveStockToolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.View.Task.frmMoveStock f = new View.Task.frmMoveStock();
            ShowForm(f);
        }

        private void toolStripButton_MoveStock_Click(object sender, EventArgs e)
        {
            App.View.Task.frmMoveStock f = new View.Task.frmMoveStock();
            ShowForm(f);
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            View.Task.frmCraneTask f = new View.Task.frmCraneTask();
            ShowForm(f);
        }

        private void toolStripButton_StartCrane_Click(object sender, EventArgs e)
        {
            if (this.toolStripButton_StartCrane.Text == "堆垛机联机")
            {
                context.ProcessDispatcher.WriteToProcess("CraneProcess", "Run", 1);
                this.toolStripButton_StartCrane.Image = App.Properties.Resources.process_accept;
                this.toolStripButton_StartCrane.Text = "堆垛机脱机";
            }
            else
            {
                context.ProcessDispatcher.WriteToProcess("CraneProcess", "Run", 0);
                this.toolStripButton_StartCrane.Image = App.Properties.Resources.process_remove;
                this.toolStripButton_StartCrane.Text = "堆垛机联机";
            }            
        }

        private void toolStripButton_Inventor_Click(object sender, EventArgs e)
        {
            App.View.Task.frmInventor f = new View.Task.frmInventor();
            ShowForm(f);
        }

        private void InventortoolStripMenuItem_Click(object sender, EventArgs e)
        {
            App.View.Task.frmInventor f = new View.Task.frmInventor();
            ShowForm(f);
        }

        private void toolStripButton_CellMonitor_Click(object sender, EventArgs e)
        {
            App.View.Dispatcher.frmCellQuery f = new App.View.Dispatcher.frmCellQuery();
            ShowForm(f);
        }

        private void ToolStripMenuItem_Param_Click(object sender, EventArgs e)
        {
            App.View.Param.ParameterForm f = new App.View.Param.ParameterForm();
            ShowForm(f);
        }

        private void toolStripButton_Plan_Click(object sender, EventArgs e)
        {
            App.View.Task.frmProducePlans f = new App.View.Task.frmProducePlans();
            ShowForm(f);
        }

        private void toolStripButton_Product_Click(object sender, EventArgs e)
        {
            App.View.Base.frmProducts f = new App.View.Base.frmProducts();
            ShowForm(f);
        }

        private void toolStripButton_Start_Click(object sender, EventArgs e)
        {
            try
            {

                if (this.toolStripButton_Start.Text == "开始运行")
                {
                    Run = true;
                    
                    context.ProcessDispatcher.WriteToProcess("CraneProcess", "Run", 1);
                    this.toolStripButton_Start.Image = App.Properties.Resources.stop;
                    this.toolStripButton_Start.Text = "停止运行";
                    context.ProcessDispatcher.WriteToService("ConveyorPLC2", "StartSignal", 1);
                }
                else
                {
                    Run = false;
                    
                    context.ProcessDispatcher.WriteToProcess("CraneProcess", "Run", 0);
                    this.toolStripButton_Start.Image = App.Properties.Resources.start;
                    this.toolStripButton_Start.Text = "开始运行";
                    context.ProcessDispatcher.WriteToService("ConveyorPLC2", "StartSignal", 0);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }

        private void toolStripButton_RptInStock_Click(object sender, EventArgs e)
        {
            App.View.Report.InStock f = new View.Report.InStock("11");
            ShowForm(f);            
        }

        private void toolStripButton_RptOutStock_Click(object sender, EventArgs e)
        {
            App.View.Report.InStock f = new View.Report.InStock("12");
            f.Text = "出库明细表";
            ShowForm(f);   
        }

        private void toolStripButton_Stock_Click(object sender, EventArgs e)
        {
            App.View.Report.StockQuery f = new View.Report.StockQuery();
            ShowForm(f);     
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            App.View.Report.StockTotal f = new View.Report.StockTotal();
            ShowForm(f);  
        }

        private void ToolStripMenuItem_ClearTask_Click(object sender, EventArgs e)
        {
            View.Task.frmClearTask f = new View.Task.frmClearTask();
            f.ShowDialog();
        }

        private void ToolStripMenuItem_ClearCell_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("此功能会把所有货位信息全部清空", "询问", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                BLL.BLLBase bll = new BLL.BLLBase();
                bll.ExecNonQuery("WCS.ClearCellInfo");
                MessageBox.Show("清理完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
