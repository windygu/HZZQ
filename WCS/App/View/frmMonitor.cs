using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Util;
using DataGridViewAutoFilter;
using MCP;

namespace App.View
{
    public partial class frmMonitor : BaseForm
    {
        private Point InitialP1;
        private Point InitialP2;

        float colDis = 20.75f;
        float rowDis = 54.4f;
        
        float colIndex = 1;
        float rowIndex = 1;
        private System.Timers.Timer tmWorkTimer = new System.Timers.Timer();
        private System.Timers.Timer tmCrane1 = new System.Timers.Timer();
        private System.Timers.Timer tmCar = new System.Timers.Timer();
        BLL.BLLBase bll = new BLL.BLLBase();
        Dictionary<int, string> dicCraneFork = new Dictionary<int, string>();
        Dictionary<int, string> dicCraneTaskType = new Dictionary<int, string>();
        Dictionary<int, string> dicCraneAction = new Dictionary<int, string>();
        Dictionary<int, string> dicCraneError = new Dictionary<int, string>();
        Dictionary<int, string> dicCraneOver = new Dictionary<int, string>();
        Dictionary<int, string> dicWorkMode = new Dictionary<int, string>();

        public frmMonitor()
        {
            InitializeComponent();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            Point P2 = picCrane.Location;
            P2.X = P2.X - 90;

            this.picCrane.Location = P2;
        }

        //private void btnBack_Click(object sender, EventArgs e)
        //{
        //    this.picCrane11.Location = InitialP1;
        //    this.picCrane2.Location = InitialP2;
        //    this.picCrane12.Location = InitialP3;
        //}

        private void frmMonitor_Load(object sender, EventArgs e)
        {
            MainData.OnTask += new TaskEventHandler(Data_OnTask);
            Cranes.OnCrane += new CraneEventHandler(Monitor_OnCrane);
            AddDicKeyValue();

            InitialP1 = picCrane.Location;
            picCrane.Parent = pictureBox1;
            picCrane.BackColor = Color.Transparent;

            //InitialP2 = picCar.Location;
            //picCar.Parent = pictureBox1;
            //picCar.BackColor = Color.Transparent;

            GetWorkMode();
            this.BindData();
            for (int i = 0; i < this.dgvMain.Columns.Count - 1; i++)
                ((DataGridViewAutoFilterTextBoxColumn)this.dgvMain.Columns[i]).FilteringEnabled = true;

            tmWorkTimer.Interval = 3000;
            tmWorkTimer.Elapsed += new System.Timers.ElapsedEventHandler(tmWorker);
            tmWorkTimer.Start();

            tmCrane1.Interval = 3000;
            tmCrane1.Elapsed += new System.Timers.ElapsedEventHandler(tmCraneWorker1);
            tmCrane1.Start();

            
        }
        private void AddDicKeyValue()
        {
            dicCraneFork.Add(0, "叉原位");
            dicCraneFork.Add(1, "叉在1排");
            dicCraneFork.Add(2, "叉在2排");
            dicCraneFork.Add(3, "叉在3排");
            dicCraneFork.Add(4, "叉在4排");


            dicCraneTaskType.Add(0, "待机");
            dicCraneTaskType.Add(1, "入库");
            dicCraneTaskType.Add(2, "出库");
            dicCraneTaskType.Add(3, "移库");
            dicCraneTaskType.Add(4, "转库");
            dicCraneTaskType.Add(5, "双重入库");
            dicCraneTaskType.Add(6, "出库空取");
            dicCraneTaskType.Add(7, "召回");
            dicCraneTaskType.Add(8, "急停");

            dicCraneAction.Add(0, "维修");
            dicCraneAction.Add(1, "手动(脱机)");
            dicCraneAction.Add(2, "单机自动");
            dicCraneAction.Add(3, "联机自动");

            dicCraneError.Add(0, "");
            dicCraneError.Add(1, "货物左超限");
            dicCraneError.Add(2, "货物右超限");
            dicCraneError.Add(3, "左侧外形超限");
            dicCraneError.Add(4, "右侧外形超限");
            dicCraneError.Add(5, "满货位入库");
            dicCraneError.Add(6, "空货位出库");
            dicCraneError.Add(7, "运行变频器报警");
            dicCraneError.Add(8, "起升变频器报警");
            dicCraneError.Add(9, "传送地址错");
            dicCraneError.Add(10, "通讯故障");
            dicCraneError.Add(11, "运行超时");
            dicCraneError.Add(12, "起升超时");
            dicCraneError.Add(13, "货叉超时");
            dicCraneError.Add(14, "超高");
            dicCraneError.Add(15, "入外侧内测货位有货");
            dicCraneError.Add(16, "出外侧货位内侧有货");
            dicCraneError.Add(17, "水平激光数据错误");
            dicCraneError.Add(18, "起升激光数据错误");
            dicCraneError.Add(19, "超速");
            dicCraneError.Add(20, "超载");
            dicCraneError.Add(21, "送绳");
            dicCraneError.Add(22, "货叉数据错误");
            dicCraneError.Add(23, "货叉变频器报警");

            dicWorkMode.Add(0, "直供");
            dicWorkMode.Add(1, "储存");
            dicWorkMode.Add(2, "入库(先入先出)");
            dicWorkMode.Add(3, "出库");

        }
        private void GetWorkMode()
        {
            DataTable dt = bll.FillDataTable("WCS.SelectWorkMode");
            if (dt.Rows.Count > 0)
            {
                this.txtWorkMode.Text = dicWorkMode[int.Parse(dt.Rows[0]["WorkMode"].ToString())];
                if (dt.Rows[0]["WorkMode"].ToString() == "3")
                {
                    this.txtProductName.Text = dt.Rows[0]["ProductName"].ToString();
                    this.txtOutQty.Text = dt.Rows[0]["OutQty"].ToString();
                }
                else
                {
                    this.txtProductName.Text = "";
                    this.txtOutQty.Text = "";
                }
            }
        }
        void Data_OnTask(TaskEventArgs args)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TaskEventHandler(Data_OnTask), args);
            }
            else
            {
                lock (this.dgvMain)
                {
                    DataTable dt = args.datatTable;
                    this.bsMain.DataSource = dt;
                    for (int i = 0; i < this.dgvMain.Rows.Count; i++)
                    {
                        if (dgvMain.Rows[i].Cells["colErrCode"].Value.ToString() != "0")
                            this.dgvMain.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                        else
                        {
                            if(i%2==0)
                                this.dgvMain.Rows[i].DefaultCellStyle.BackColor = Color.White;
                            else
                                this.dgvMain.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(192, 255, 192);

                        }
                    }
                }
            }
        }
        void Monitor_OnCrane(CraneEventArgs args)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new CraneEventHandler(Monitor_OnCrane), args);
            }
            else
            {
                Crane crane = args.crane;
                TextBox txt = GetTextBox("txtTaskNo", crane.CraneNo);
                if (txt != null)
                    txt.Text = crane.TaskNo;

                txt = GetTextBox("txtTaskType", crane.CraneNo);
                if (txt != null && dicCraneTaskType.ContainsKey(crane.TaskType))
                    txt.Text = dicCraneTaskType[crane.TaskType];

                txt = GetTextBox("txtCraneAction", crane.CraneNo);
                if (txt != null && dicCraneAction.ContainsKey(crane.Action))
                    txt.Text = dicCraneAction[crane.Action];

                txt = GetTextBox("txtColumn", crane.CraneNo);
                if (txt != null)
                    txt.Text = crane.Column.ToString();

                if (crane.WalkCode > 0)
                {
                    txt = GetTextBox("txtBpWalkCode", crane.CraneNo);
                    if (txt != null)
                    {
                        txt.Text = crane.WalkCode.ToString();
                        txt.BackColor = Color.Red;
                    }
                }
                else
                {
                    txt = GetTextBox("txtBpWalkCode", crane.CraneNo);
                    if (txt != null)
                    {
                        txt.Text = "";
                        txt.BackColor = Control.DefaultBackColor;
                    }
                }

                if (crane.UpDownCode > 0)
                {
                    txt = GetTextBox("txtBpUpDownCode", crane.CraneNo);
                    if (txt != null)
                    {
                        txt.Text = crane.UpDownCode.ToString();
                        txt.BackColor = Color.Red;
                    }
                }
                else
                {
                    txt = GetTextBox("txtBpUpDownCode", crane.CraneNo);
                    if (txt != null)
                    {
                        txt.Text = "";
                        txt.BackColor = Control.DefaultBackColor;
                    }
                }

                //堆垛机位置
                if (crane.CraneNo == 1)
                {
                    this.picCrane.Visible = true;
                    Point P1 = InitialP1;
                    P1.X = P1.X + (int)((crane.Column-1) * colDis);
                }                

                txt = GetTextBox("txtHeight", crane.CraneNo);
                if (txt != null)
                    txt.Text = crane.Height.ToString();

                txt = GetTextBox("txtForkStatus", crane.CraneNo);
                if (txt != null && dicCraneFork.ContainsKey(crane.ForkStatus))
                    txt.Text = dicCraneFork[crane.ForkStatus];
                txt = GetTextBox("txtErrorNo", crane.CraneNo);
                if (txt != null)
                    txt.Text = crane.ErrCode.ToString();

                txt = GetTextBox("txtErrorDesc", crane.CraneNo);
                if (txt != null && dicCraneError.ContainsKey(crane.ErrCode))
                    txt.Text = dicCraneError[crane.ErrCode];

                this.button1.BackColor = Color.Red;
                //更新错误代码、错误描述
                //更新任务状态为执行中
                //bll.ExecNonQuery("WCS.UpdateTaskError", new DataParameter[] { new DataParameter("@CraneErrCode", crane.ErrCode.ToString()), new DataParameter("@CraneErrDesc", dicCraneError[crane.ErrCode]), new DataParameter("@TaskNo", crane.TaskNo) });
                if(crane.ErrCode>0)
                    Logger.Error(crane.CraneNo.ToString() + "堆垛机执行时出现错误,代码:"+ crane.ErrCode.ToString() + ",描述:" + dicCraneError[crane.ErrCode]);
            }
        }
        TextBox GetTextBox(string name, int CraneNo)
        {
            Control[] ctl = this.Controls.Find(name + CraneNo.ToString(), true);
            if (ctl.Length > 0)
                return (TextBox)ctl[0];
            else
                return null;
        }
        
        private void tmWorker(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                tmWorkTimer.Stop();
                DataTable dt = GetMonitorData();
                MainData.TaskInfo(dt);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            finally
            {
                tmWorkTimer.Start();
            }
        }
        private void tmCraneWorker1(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                tmCrane1.Stop();

                int[] location = new int[2];
                string serviceName = "CranePLC1";
                object[] obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "CraneLocation"));
                for (int j = 0; j < obj.Length; j++)
                    location[j] = Convert.ToInt16(obj[j]);

                int[] craneInfo = new int[6];
                obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "CraneInfo"));
                for (int j = 0; j < obj.Length; j++)
                    craneInfo[j] = Convert.ToInt16(obj[j]);

                string palletCode = Util.ConvertStringChar.BytesToString(ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "CranePalletCode")));
                obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "CraneTaskNo"));
                string plcTaskNo = Util.ConvertStringChar.BytesToString(obj);

                obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "CraneSpeed"));

                Crane crane = new Crane();
                crane.CraneNo = 1;
                crane.Column = location[0];
                crane.Height = location[1];
                crane.ForkStatus = craneInfo[1];
                crane.Action = craneInfo[2];
                crane.TaskType = craneInfo[3];
                crane.ErrCode = craneInfo[4];
                crane.PalletCode = palletCode;
                crane.TaskNo = plcTaskNo;
                crane.WalkCode = int.Parse(obj[3].ToString());
                crane.UpDownCode = int.Parse(obj[4].ToString());
                Cranes.CraneInfo(crane);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            finally
            {
                tmCrane1.Start();
            }
        }
        
        
        private void BindData()
        {
            bsMain.DataSource = GetMonitorData();
        }
        private DataTable GetMonitorData()
        {
            DataTable dt = bll.FillDataTable("WCS.SelectTask", new DataParameter[] { new DataParameter("{0}", "(WCS_TASK.TaskType='11' and WCS_TASK.State in('0','1')) OR (WCS_TASK.TaskType in('12','13') and WCS_TASK.State in('0','1')) OR (WCS_TASK.TaskType in('14') and WCS_TASK.State in('0','1'))") });
            return dt;
        }      

        
        private void PutCommand(string craneNo, byte TaskType)
        {
            byte[] cellAddr = new byte[8];
            cellAddr[0] = TaskType;
            cellAddr[1] = 0;

            for (int i = 0; i < cellAddr.Length; i++)
                cellAddr[i] += 48;

            string serviceName = "CranePLC" + craneNo;
            Context.ProcessDispatcher.WriteToService(serviceName, "TaskAddress", cellAddr);

            Context.ProcessDispatcher.WriteToService(serviceName, "WriteFinished", 49);
        }       


        private void dgvMain_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (e.RowIndex >= 0 && e.ColumnIndex >= 0)
                {
                    //若行已是选中状态就不再进行设置
                    if (dgvMain.Rows[e.RowIndex].Selected == false)
                    {
                        dgvMain.ClearSelection();
                        dgvMain.Rows[e.RowIndex].Selected = true;
                    }
                    //只选中一行时设置活动单元格
                    if (dgvMain.SelectedRows.Count == 1)
                    {
                        dgvMain.CurrentCell = dgvMain.Rows[e.RowIndex].Cells[e.ColumnIndex];
                    }
                    
                    //弹出操作菜单
                    contextMenuStrip1.Show(MousePosition.X, MousePosition.Y);
                }
            }
        }

        private void ToolStripMenuItemCellCode_Click(object sender, EventArgs e)
        {
            if (this.dgvMain.CurrentCell != null)
            {
                BLL.BLLBase bll = new BLL.BLLBase();
                string TaskNo = this.dgvMain.Rows[this.dgvMain.CurrentCell.RowIndex].Cells[0].Value.ToString();
                string TaskType = this.dgvMain.Rows[this.dgvMain.CurrentCell.RowIndex].Cells["colTaskType"].Value.ToString();
                string ErrCode = this.dgvMain.Rows[this.dgvMain.CurrentCell.RowIndex].Cells["colErrCode"].ToString();

                if (TaskType=="11")
                {
                    DataGridViewSelectedRowCollection rowColl = dgvMain.SelectedRows;
                    if (rowColl == null)
                        return;
                    DataRow dr = (rowColl[0].DataBoundItem as DataRowView).Row;
                    frmReassignEmptyCell f = new frmReassignEmptyCell(dr);
                    if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        this.BindData(); 
                }
                else if (TaskType == "12" || TaskType == "14")
                {
                    DataGridViewSelectedRowCollection rowColl = dgvMain.SelectedRows;
                    if (rowColl == null)
                        return;
                    DataRow dr = (rowColl[0].DataBoundItem as DataRowView).Row;
                    frmReassign f = new frmReassign(dr);
                    if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                        this.BindData(); 
                }
                else if (TaskType == "13")
                {
                    DataGridViewSelectedRowCollection rowColl = dgvMain.SelectedRows;
                    if (rowColl == null)
                        return;
                    DataRow dr = (rowColl[0].DataBoundItem as DataRowView).Row;

                    frmReassignOption fo = new frmReassignOption();
                    if (fo.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (fo.option == 0)
                        {
                            frmReassign f = new frmReassign(dr);
                            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                this.BindData(); 
                        }
                        else
                        {
                            frmReassignEmptyCell fe = new frmReassignEmptyCell(dr);
                            if (fe.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                                this.BindData(); 
                        }
                    }                    
                }
            }
        }

        private void ToolStripMenuItemReassign_Click(object sender, EventArgs e)
        {
            if (this.dgvMain.CurrentCell != null)
            {
                BLL.BLLBase bll = new BLL.BLLBase();
                string TaskNo = this.dgvMain.Rows[this.dgvMain.CurrentCell.RowIndex].Cells[0].Value.ToString();
                string TaskType = this.dgvMain.Rows[this.dgvMain.CurrentCell.RowIndex].Cells["colTaskType"].Value.ToString();

                if (TaskType == "11")
                    bll.ExecNonQuery("WCS.UpdateTaskStateByTaskNo", new DataParameter[] { new DataParameter("@State", 0), new DataParameter("@TaskNo", TaskNo) });
                else if (TaskType == "12" || TaskType == "13")
                    bll.ExecNonQuery("WCS.UpdateTaskStateByTaskNo", new DataParameter[] { new DataParameter("@State", 0), new DataParameter("@TaskNo", TaskNo) });
                else if (TaskType == "14")
                {
                    frmTaskOption f = new frmTaskOption();
                    if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if(f.option==0)
                            bll.ExecNonQuery("WCS.UpdateTaskStateByTaskNo", new DataParameter[] { new DataParameter("@State", 2), new DataParameter("@TaskNo", TaskNo) });
                        else
                            bll.ExecNonQuery("WCS.UpdateTaskStateByTaskNo", new DataParameter[] { new DataParameter("@State", 5), new DataParameter("@TaskNo", TaskNo) });

                    }
                }
                this.BindData();
            }
        }

        private void ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string ItemName = ((ToolStripMenuItem)sender).Name;
            string State = ItemName.Substring(ItemName.Length-1, 1);

            if (this.dgvMain.CurrentCell != null)
            {
                BLL.BLLBase bll = new BLL.BLLBase();
                string TaskNo = this.dgvMain.Rows[this.dgvMain.CurrentCell.RowIndex].Cells[0].Value.ToString();

                DataParameter[] param = new DataParameter[] { new DataParameter("@TaskNo", TaskNo), new DataParameter("@State", State) };
                bll.ExecNonQueryTran("WCS.Sp_UpdateTaskState", param);
                
                BindData();
            }
        }

        private void btnChangeMode_Click(object sender, EventArgs e)
        {
            Task.frmSelectCell f = new Task.frmSelectCell();
            f.ShowDialog();
            
            //frmChangeMode f = new frmChangeMode();
            //if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            //    GetWorkMode();
        }
        
    }
}
