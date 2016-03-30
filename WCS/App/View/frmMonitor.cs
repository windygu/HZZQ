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
        private System.Timers.Timer tmMotor = new System.Timers.Timer();
        BLL.BLLBase bll = new BLL.BLLBase();
        Dictionary<int, string> dicCraneFork = new Dictionary<int, string>();
        Dictionary<int, string> dicCraneTaskType = new Dictionary<int, string>();
        Dictionary<int, string> dicCraneAction = new Dictionary<int, string>();
        Dictionary<int, string> dicCraneError = new Dictionary<int, string>();
        Dictionary<int, string> dicCraneOver = new Dictionary<int, string>();
        Dictionary<int, string> dicWorkMode = new Dictionary<int, string>();
        Dictionary<int, string> dicProductNo = new Dictionary<int, string>();

        public frmMonitor()
        {
            InitializeComponent();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            //Point P2 = picCrane.Location;
            //P2.X = P2.X - 90;

            //this.picCrane.Location = P2;
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
            Signals.OnSignal += new SignalEventHandler(Monitor_OnSignal);
            AddDicKeyValue();

            InitialP1 = this.btnCrane.Location;
            
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

            tmMotor.Interval = 3000;
            tmMotor.Elapsed += new System.Timers.ElapsedEventHandler(tmMotorWorker1);
            tmMotor.Start();
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

            dicProductNo.Add(0, "A");
            dicProductNo.Add(1, "B");
            dicProductNo.Add(2, "C");
            dicProductNo.Add(3, "D");
            dicProductNo.Add(4, "E");
            dicProductNo.Add(5, "F");

        }
        private void GetWorkMode()
        {
            DataTable dt = bll.FillDataTable("WCS.SelectWorkMode");
            if (dt.Rows.Count > 0)
            {
                this.txtWorkMode.Text = dicWorkMode[int.Parse(dt.Rows[0]["WorkMode"].ToString())];
                if (dt.Rows[0]["WorkMode"].ToString() == "2" || dt.Rows[0]["WorkMode"].ToString() == "3")
                {
                    this.txtProductName.Text = dt.Rows[0]["ProductName"].ToString();
                    this.txtOutQty.Text = dt.Rows[0]["OutQty"].ToString();
                    Program.mainForm.ProductCode = dt.Rows[0]["ProductCode"].ToString();
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

                //if (crane.WalkCode > 0)
                //{
                //    txt = GetTextBox("txtBpWalkCode", crane.CraneNo);
                //    if (txt != null)
                //    {
                //        txt.Text = crane.WalkCode.ToString();
                //        txt.BackColor = Color.Red;
                //    }
                //}
                //else
                //{
                //    txt = GetTextBox("txtBpWalkCode", crane.CraneNo);
                //    if (txt != null)
                //    {
                //        txt.Text = "";
                //        txt.BackColor = Control.DefaultBackColor;
                //    }
                //}

                //if (crane.UpDownCode > 0)
                //{
                //    txt = GetTextBox("txtBpUpDownCode", crane.CraneNo);
                //    if (txt != null)
                //    {
                //        txt.Text = crane.UpDownCode.ToString();
                //        txt.BackColor = Color.Red;
                //    }
                //}
                //else
                //{
                //    txt = GetTextBox("txtBpUpDownCode", crane.CraneNo);
                //    if (txt != null)
                //    {
                //        txt.Text = "";
                //        txt.BackColor = Control.DefaultBackColor;
                //    }
                //}

                //堆垛机位置
                if (crane.CraneNo == 1)
                {                    
                    //this.picCrane.Visible = true;
                    Point P1 = InitialP1;
                    P1.Y = P1.Y + (int)((crane.Column-1) * 30);
                    this.btnCrane.Location = P1;
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

                this.txtInProductName.Text = crane.PalletCode;

                //更新错误代码、错误描述
                //更新任务状态为执行中
                if(crane.TaskNo.Length>0)
                    bll.ExecNonQuery("WCS.UpdateTaskError", new DataParameter[] { new DataParameter("@CraneErrCode", crane.ErrCode.ToString()), new DataParameter("@CraneErrDesc", dicCraneError[crane.ErrCode]), new DataParameter("@TaskNo", crane.TaskNo) });
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
        //void Monitor_OnConveyor(ConveyorEventArgs args)
        //{
        //    if (InvokeRequired)
        //    {
        //        BeginInvoke(new ConveyorEventHandler(Monitor_OnConveyor), args);
        //    }
        //    else
        //    {
        //        Conveyor[] conveyor = args.conveyor;

        //        for (int i = 0; i < conveyor.Length; i++)
        //        {
        //            if (conveyor[i] != null)
        //            {
        //                Button btn = GetButton("button", conveyor[i].ConveyorNo);
        //                if (btn != null)
        //                {
        //                    btn.Text = "■";
        //                    btn.BackColor = Color.Red;
        //                }
        //            }
        //        }
        //    }
        //}
        void Monitor_OnSignal(SignalEventArgs args)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new SignalEventHandler(Monitor_OnSignal), args);
            }
            else
            {
                bool[,] motors1 = args.obj1;
                bool[,] signal1 = args.obj2;
                bool[,] motors2 = args.obj3;
                bool[,] signal2 = args.obj4;

                //M1
                if (motors2[0, 0])
                    this.btnConveyor01.Text = "↓";
                else if (motors2[0, 1])
                    this.btnConveyor01.Text = "↑";
                else
                    this.btnConveyor01.Text = "";
                if (signal2[0, 0])
                    this.btnConveyor01.Text += "■";
                else
                    this.btnConveyor01.Text += "";

                //M2
                if (motors2[0, 2])
                    this.btnConveyor02.Text = "△";
                else if (motors2[0, 3])
                    this.btnConveyor02.Text = "▽";
                else
                    this.btnConveyor02.Text = "";
                if (signal2[0, 2])
                    this.btnConveyor02.Text = "◎";
                else if (signal2[0, 5])
                    this.btnConveyor02.Text = "●";
                //else
                //    this.btnConveyor02.Text = "";
                //M3
                if (motors2[3, 0])
                    this.btnConveyor03.Text = "↑";                
                else
                    this.btnConveyor03.Text = "";
                if (signal2[0, 6])
                    this.btnConveyor03.Text += "■";                
                else
                    this.btnConveyor03.Text += "";

                //M28 JK62没看到信号点
                if (motors2[4, 0])
                    this.btnConveyor28.Text = "↓";
                else
                    this.btnConveyor28.Text = "";
                if (signal2[2, 7])
                    this.btnConveyor28.Text += "■";
                else
                    this.btnConveyor28.Text += "";
                //M4
                if (motors2[1, 0])
                    this.btnConveyor04.Text = "↑";
                else if (motors2[1, 1])
                    this.btnConveyor04.Text = "↓";
                else
                    this.btnConveyor04.Text = "";
                if (signal2[0, 7])
                    this.btnConveyor04.Text += "■";
                else
                    this.btnConveyor04.Text += "";
                //M27 JK60 JK61没有信号点
                if (motors2[2, 3])
                    this.btnConveyor27.Text = "↓";
                else if (motors2[2, 4])
                    this.btnConveyor27.Text = "↑";
                else
                    this.btnConveyor27.Text = "";
                if (signal2[2, 6])
                    this.btnConveyor27.Text += "■";
                else
                    this.btnConveyor27.Text += "";
                //M5
                if (motors2[1, 3])
                    this.btnConveyor05.Text = "△";
                else if (motors2[1, 4])
                    this.btnConveyor05.Text = "▽";
                else
                    this.btnConveyor05.Text = "";
                if (signal2[1, 1])
                    this.btnConveyor05.Text = "◎";
                else if (signal2[1, 4])
                    this.btnConveyor05.Text = "●";
                else if (signal2[1, 3])
                    this.btnConveyor05.Text = "○";
                //else
                //    this.btnConveyor05.Text = "";
                //M6
                if (motors2[3, 3])
                    this.btnConveyor06.Text = "↑";
                else
                    this.btnConveyor06.Text = "";
                if (signal2[1, 6])
                    this.btnConveyor06.Text += "■";
                else
                    this.btnConveyor06.Text += "";

                //M26
                if (motors2[3, 7])
                    this.btnConveyor26.Text = "↓";
                else
                    this.btnConveyor26.Text = "";
                if (signal2[2, 4])
                    this.btnConveyor26.Text += "■";
                else
                    this.btnConveyor26.Text += "";
                //M7
                if (motors2[3, 4])
                    this.btnConveyor07.Text = "↑";
                else
                    this.btnConveyor07.Text = "";
                if (signal2[2, 0])
                    this.btnConveyor07.Text += "■";
                else
                    this.btnConveyor07.Text += "";

                //M25
                if (motors2[3, 6])
                    this.btnConveyor25.Text = "↓";
                else
                    this.btnConveyor25.Text = "";
                if (signal2[2, 1])
                    this.btnConveyor25.Text += "■";
                else
                    this.btnConveyor25.Text += "";

                //M8 旋转
                if (motors2[2, 0])
                    this.btnConveyor08.Text = "正";
                else if (motors2[2, 1])
                    this.btnConveyor08.Text = "反";
                else
                    this.btnConveyor08.Text = "";
                if (signal2[21, 1])
                    this.btnConveyor08.Text = "⊃";
                else if (signal2[20, 6])
                    this.btnConveyor08.Text = "∪";
                //else
                //    this.btnConveyor08.Text = "";

                //M9
                if (motors2[20, 0])
                    this.btnConveyor09.Text = "↑";
                else if (motors2[20, 1])
                    this.btnConveyor09.Text = "←";
                else
                    this.btnConveyor09.Text = "";
                if (signal2[20, 0])
                    this.btnConveyor09.Text += "■";
                else
                    this.btnConveyor09.Text += "";
                //M24
                if (motors2[22, 3])
                    this.btnConveyor24.Text = "→";
                else if (motors2[22, 4])
                    this.btnConveyor24.Text = "↓";
                else
                    this.btnConveyor24.Text = "";
                if (signal2[24, 5])
                    this.btnConveyor24.Text += "■";
                else
                    this.btnConveyor24.Text += "";
                //M10
                if (motors2[20, 3])
                    this.btnConveyor10.Text = "△";
                else if (motors2[20, 4])
                    this.btnConveyor10.Text = "▽";
                else
                    this.btnConveyor10.Text = "";
                
                if (signal2[20, 2])
                    this.btnConveyor10.Text = "◎";
                else if (signal2[20, 5])
                    this.btnConveyor10.Text = "●";
                //else
                //    this.btnConveyor10.Text = "";

                //M11
                if (motors2[23, 0])
                    this.btnConveyor11.Text = "←";
                else
                    this.btnConveyor11.Text = "";
                if (signal2[30, 0])
                    this.btnConveyor11.Text += "■";
                else
                    this.btnConveyor11.Text += "";
                //M23
                if (motors2[24, 2])
                    this.btnConveyor23.Text = "→";
                else
                    this.btnConveyor23.Text = "";
                if (signal2[24, 2] || signal2[24, 3])
                    this.btnConveyor23.Text += "■";
                else
                    this.btnConveyor23.Text += "";
                //M100
                if (motors2[26, 0])
                    this.btnConveyor100.Text = "←";
                else
                    this.btnConveyor100.Text = "";
                if (signal2[21, 2])
                    this.btnConveyor100.Text += "■";
                else
                    this.btnConveyor100.Text += "";
                //M101
                if (motors2[26, 1])
                    this.btnConveyor101.Text = "←";
                else
                    this.btnConveyor101.Text = "";
                if (signal2[30, 2])
                    this.btnConveyor101.Text += "■";
                else
                    this.btnConveyor101.Text += "";
                //M102
                if (motors2[26, 2])
                    this.btnConveyor102.Text = "←";
                else
                    this.btnConveyor102.Text = "";
                if (signal2[30, 4])
                    this.btnConveyor102.Text += "■";
                else
                    this.btnConveyor102.Text += "";
                //M22
                if (motors2[24, 1])
                    this.btnConveyor22.Text = "→";
                else
                    this.btnConveyor22.Text = "";
                if (signal2[24, 0] || signal2[24, 1])
                    this.btnConveyor22.Text += "■";
                else
                    this.btnConveyor22.Text += "";
                //M12
                if (motors2[23, 1])
                    this.btnConveyor12.Text = "←";
                else
                    this.btnConveyor12.Text = "";
                if (signal2[21, 4])
                    this.btnConveyor12.Text += "■";
                else
                    this.btnConveyor12.Text += "";
                //M103
                if (motors2[26, 3])
                    this.btnConveyor103.Text = "←";
                else
                    this.btnConveyor103.Text = "";
                if (signal2[30, 6])
                    this.btnConveyor103.Text += "■";
                else
                    this.btnConveyor103.Text += "";
                //M104
                if (motors2[26, 4])
                    this.btnConveyor104.Text = "←";
                else
                    this.btnConveyor104.Text = "";
                if (signal2[31, 0])
                    this.btnConveyor104.Text += "■";
                else
                    this.btnConveyor104.Text += "";
                //M21
                if (motors2[24, 0])
                    this.btnConveyor21.Text = "→";
                else
                    this.btnConveyor21.Text = "";
                if (signal2[23, 6] || signal2[23, 7])
                    this.btnConveyor21.Text += "■";
                else
                    this.btnConveyor21.Text += "";
                //M105
                if (motors2[26, 5])
                    this.btnConveyor105.Text = "←";
                else
                    this.btnConveyor105.Text = "";
                if (signal2[21, 7])
                    this.btnConveyor105.Text += "■";
                else
                    this.btnConveyor105.Text += "";
                //M13
                if (motors2[23, 2])
                    this.btnConveyor13.Text = "←";
                else
                    this.btnConveyor13.Text = "";
                if (signal2[31, 2])
                    this.btnConveyor13.Text += "■";
                else
                    this.btnConveyor13.Text += "";
                //M14
                if (motors2[21, 0])
                    this.btnConveyor14.Text = "←";
                else if (motors2[21, 1])
                    this.btnConveyor14.Text = "→";
                else
                    this.btnConveyor14.Text = "";
                if (signal2[22, 0])
                    this.btnConveyor14.Text += "■";
                else
                    this.btnConveyor14.Text += "";
                //M20
                if (motors2[23, 7])
                    this.btnConveyor20.Text = "→";
                else
                    this.btnConveyor20.Text = "";
                if (signal2[23, 4] || signal2[23, 5])
                    this.btnConveyor20.Text += "■";
                else
                    this.btnConveyor20.Text += "";
                //M15
                if (motors2[23, 3])
                    this.btnConveyor15.Text = "←";
                else
                    this.btnConveyor15.Text = "";
                if (signal2[22, 2])
                    this.btnConveyor15.Text += "■";
                else
                    this.btnConveyor15.Text += "";
                //M19
                if (motors2[23, 6])
                    this.btnConveyor19.Text = "→";
                else
                    this.btnConveyor19.Text = "";
                if (signal2[23, 2] || signal2[23, 3])
                    this.btnConveyor19.Text += "■";
                else
                    this.btnConveyor19.Text += "";
                //M16
                if (motors2[23, 4])
                    this.btnConveyor16.Text = "←";
                else
                    this.btnConveyor16.Text = "";
                if (signal2[22, 4])
                    this.btnConveyor16.Text += "■";
                else
                    this.btnConveyor16.Text += "";

                //M17
                if (motors2[21, 3])
                    this.btnConveyor17.Text = "←";
                else if (motors2[21, 4])
                    this.btnConveyor17.Text = "→";
                else
                    this.btnConveyor17.Text = "";
                if (signal2[22, 5])
                    this.btnConveyor17.Text += "■";
                else
                    this.btnConveyor17.Text += "";
                //M18
                if (motors2[21, 6])
                    this.btnConveyor18.Text = "△";
                else if (motors2[21, 7])
                    this.btnConveyor18.Text = "▽";
                else
                    this.btnConveyor18.Text = "";
                if (signal2[23, 1])
                    this.btnConveyor18.Text = "●";
                else if (signal2[22, 6])
                    this.btnConveyor18.Text = "◎";

                //MS1
                if (motors1[2, 6])
                    this.btnMS1.Text = "△";
                else if (motors2[2, 7])
                    this.btnMS1.Text = "▽";
                else
                    this.btnMS1.Text = "";
                if (signal1[7, 1])
                    this.btnMS1.Text = "●";
                else if (signal1[7, 2])
                    this.btnMS1.Text = "○";
                else if (signal1[6, 6])
                    this.btnMS1.Text = "◎";
                //else
                //    this.btnMS1.Text = "";
                //ML1
                if (motors1[2, 3])
                    this.btnML1.Text = "↑";
                else if (motors1[2, 4])
                    this.btnML1.Text = "↓";
                else
                    this.btnML1.Text = "";
                if (signal1[6, 5])
                    this.btnML1.Text += "■";
                else
                    this.btnML1.Text += "";
                //MG1
                if (motors1[2, 0])
                    this.btnMG1.Text = "→";
                else if (motors1[2, 1])
                    this.btnMG1.Text = "←";
                else
                    this.btnMG1.Text = "";
                if (signal1[6, 2])
                    this.btnMG1.Text += "■";
                else
                    this.btnMG1.Text += "";
                //M1
                if (motors1[0, 0])
                    this.btnM1.Text = "→";
                else
                    this.btnM1.Text = "";
                if (signal1[5, 0])
                    this.btnM1.Text += "■";
                else
                    this.btnM1.Text += "";
                //M2
                if (motors1[0, 1])
                    this.btnM2.Text = "→";
                else
                    this.btnM2.Text = "";
                if (signal1[5, 1])
                    this.btnM2.Text += "■";
                else
                    this.btnM2.Text += "";
                //M3
                if (motors1[0, 2])
                    this.btnM3.Text = "→";
                else
                    this.btnM3.Text = "";
                if (signal1[5, 2])
                    this.btnM3.Text += "■";
                else
                    this.btnM3.Text += "";
                //M4
                if (motors1[0, 3])
                    this.btnM4.Text = "→";
                else
                    this.btnM4.Text = "";
                if (signal1[5, 3])
                    this.btnM4.Text += "■";
                else
                    this.btnM4.Text += "";
                //M5
                if (motors1[0, 4])
                    this.btnM5.Text = "→";
                else
                    this.btnM5.Text = "";
                if (signal1[5,4])
                    this.btnM5.Text += "■";
                else
                    this.btnM5.Text += "";
                //M9
                if (motors1[1, 0])
                    this.btnM9.Text = "→";
                else
                    this.btnM9.Text = "";
                if (signal1[7, 4])
                    this.btnM9.Text += "■";
                else
                    this.btnM9.Text += "";
                //MD1
                if (motors1[3, 6])
                    this.btnMD1.Text = "△";
                else if (motors1[3, 7])
                    this.btnMD1.Text = "▽";
                else
                    this.btnMD1.Text = "";
                if (signal1[7, 5])
                    this.btnMD1.Text = "●";
                else if (signal1[8, 4])
                    this.btnMD1.Text = "◎";
                //else
                //    this.btnMS1.Text = "";
                //MY1
                if (motors1[3, 2])
                    this.btnMY1.Text = "↓";
                else
                    this.btnMY1.Text = "";
                if (signal1[7, 4])
                    this.btnMY1.Text += "■";
                else
                    this.btnMY1.Text += "";

                //MYL1
                if (motors1[3, 3])
                    this.btnMYL1.Text = "↓";
                else
                    this.btnMYL1.Text = "";
                if (signal1[8, 2])
                    this.btnMYL1.Text += "■";
                else
                    this.btnMYL1.Text += "";
                //MYS1
                if (motors1[4, 2])
                    this.btnMYS1.Text = "△";
                else if (motors1[5, 2])
                    this.btnMYS1.Text = "▽";
                else
                    this.btnMYS1.Text = "";
                if (signal1[8, 3])
                    this.btnMYS1.Text = "●";
                else if (signal1[25, 1])
                    this.btnMYS1.Text = "◎";
                //else
                //    this.btnMYS1.Text = "";

                //MD2
                if (motors1[4, 0])
                    this.btnMD2.Text = "△";
                else if (motors1[4, 1])
                    this.btnMD2.Text = "▽";
                else
                    this.btnMD2.Text = "";
                if (signal1[8, 0])
                    this.btnMD2.Text = "●";
                else if (signal1[24, 0])
                    this.btnMD2.Text = "◎";
                //else
                //    this.btnMD2.Text = "";
                //MY2
                if (motors1[3, 4])
                    this.btnMY2.Text = "↓";
                else
                    this.btnMY2.Text = "";
                if (signal1[7, 6])
                    this.btnMY2.Text += "■";
                else
                    this.btnMY2.Text += "";

                //MYL2
                if (motors1[3, 5])
                    this.btnMYL2.Text = "↓";
                else
                    this.btnMYL2.Text = "";
                if (signal1[8, 5])
                    this.btnMYL2.Text += "■";
                else
                    this.btnMYL2.Text += "";
                //MYS2
                if (motors1[4, 3])
                    this.btnMYS2.Text = "△";
                else if (motors1[5, 3])
                    this.btnMYS2.Text = "▽";
                else
                    this.btnMYS2.Text = "";
                if (signal1[8, 7])
                    this.btnMYS2.Text = "●";
                else if (signal1[25, 0])
                    this.btnMYS2.Text = "◎";
                //else
                //    this.btnMYS2.Text = "";
                //M6
                if (motors1[0, 5])
                    this.btnM6.Text = "←";
                else
                    this.btnM6.Text = "";
                if (signal1[5, 5])
                    this.btnM6.Text += "■";
                else
                    this.btnM6.Text += "";
                //M7
                if (motors1[0, 6])
                    this.btnM7.Text = "←";
                else
                    this.btnM7.Text = "";
                if (signal1[5, 6])
                    this.btnM7.Text += "■";
                else
                    this.btnM7.Text += "";
                //M8
                if (motors1[0, 7])
                    this.btnM8.Text = "←";
                else
                    this.btnM8.Text = "";
                if (signal1[5, 7])
                    this.btnM8.Text += "■";
                else
                    this.btnM8.Text += "";
                
            }
        }
        private Button GetButton(string name, string Motor)
        {
            Control[] ctl = this.Controls.Find("button1", true);
            if (ctl.Length > 0)
                return (Button)ctl[0];
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

                DataRow[] dr = dt.Select("TaskType='12'");
                if (dr.Length > 0)
                    return;
                //如果是先进先出模式，出库任务完成后，再产生一笔出库任务
                if (Program.mainForm.WorkMode > 1 && Program.mainForm.Run)
                {
                    //string CellCode = "";
                    //string ProductCode = Program.mainForm.ProductCode;
                    //string ProductNo = "";
                    //if (Program.mainForm.WorkMode == 2)
                    //{
                    //    int key = int.Parse(ObjectUtil.GetObject(Context.ProcessDispatcher.WriteToService("ConveyorPLC1", "ProductNo")).ToString());
                    //    ProductNo = dicProductNo[key];

                    //    dt = bll.FillDataTable("WCS.SelectProduct", new DataParameter[] { new DataParameter("{0}", string.Format("ProductNo='{0}'", ProductNo)) });
                    //    if (dt.Rows.Count > 0)
                    //        ProductCode = dt.Rows[0]["ProductCode"].ToString();
                    //}


                    //DataParameter[] param = new DataParameter[] 
                    //{
                    //    new DataParameter("@CraneNo", "01"), 
                    //    new DataParameter("@ProductCode", ProductCode),
                    //    new DataParameter("@CellCode",CellCode)
                    //};

                    //bll.FillDataTable("WCS.Sp_CreateOutTask", param);
                }
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

                //string palletCode = Util.ConvertStringChar.BytesToString(ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "CranePalletCode")));
                obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "CraneTaskNo"));
                string plcTaskNo = Util.ConvertStringChar.BytesToString(obj);

                //obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "CraneSpeed"));
                string ProductName = "";

                //int key = int.Parse(ObjectUtil.GetObject(Context.ProcessDispatcher.WriteToService("ConveyorPLC1", "ProductNo")).ToString());
                //string ProductNo = dicProductNo[key];

                //DataTable dt = bll.FillDataTable("WCS.SelectProduct", new DataParameter[] { new DataParameter("{0}", string.Format("ProductNo='{0}'", ProductNo)) });
                //if (dt.Rows.Count > 0)
                //    ProductName = dt.Rows[0]["ProductName"].ToString();


                Crane crane = new Crane();
                crane.CraneNo = 1;
                crane.Column = location[0];
                crane.Height = location[1];
                crane.ForkStatus = craneInfo[1];
                crane.Action = craneInfo[2];
                crane.TaskType = craneInfo[3];
                crane.ErrCode = craneInfo[4];
                crane.PalletCode = ProductName;
                crane.TaskNo = plcTaskNo;
                if (crane.Column == 100)
                    crane.Column = 0;
                //crane.WalkCode = int.Parse(obj[3].ToString());
                //crane.UpDownCode = int.Parse(obj[4].ToString());
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
        private void tmMotorWorker1(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                tmMotor.Stop();

                //bool[] signal = new bool[44];
                string serviceName = "ConveyorPLC1";
                //object[] obj1 = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "MotorSignal1"));
                //object[] obj2 = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "SensorSignal"));
                serviceName = "ConveyorPLC2";
                
                object[] obj3 = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "MotorSignal1"));
                object[] obj4 = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "SensorSignal"));

                //string s = Util.ConvertStringChar.BytesToString(obj3);
                //string d16 = Convert.ToString(long.Parse(obj4[6].ToString()), 16);
                //string dd = Convert.ToString(byte.Parse(obj3[4].ToString()), 2).PadLeft(8, '0');
                
                //定义四个二维数组
                bool[,] motors1 = new bool[32, 8];
                bool[,] signal1 = new bool[32, 8];
                bool[,] motors2 = new bool[32, 8];
                bool[,] signal2 = new bool[32, 8];
                //GetSignal(obj3, ref motors2);
                //ConvSignal(obj4, ref signal2);
                //MonitorSignal(obj1, ref motors1);
                //MonitorSignal(obj2, ref signal1);
                MonitorSignal(obj3, ref motors2);
                MonitorSignal(obj4, ref signal2);
                Signals.SignalInfo(motors1, signal1, motors2, signal2);
                //Conveyors.ConveyorInfo(conveyor);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
            finally
            {
                tmMotor.Start();
            }
        }
        private void ConvSignal(object[] obj, ref bool[,] signal)
        {
            for (int i = 0; i < obj.Length; i++)
            {
                signal[i / 8, i % 8] = bool.Parse(obj[i].ToString());
            }
        }
        //BYTE
        private void MonitorSignal(object[] obj, ref bool[,] signal)
        {
            for (int i = 0; i < obj.Length; i++)
            {
                if (obj[i].ToString() == "0")
                    continue;
                string obj2str = Convert.ToString(byte.Parse(obj[i].ToString()), 2).PadLeft(8, '0');

                //高低位转换
                string s = conv(obj2str);
                for (int j = 0; j < s.Length; j++)
                {
                    if (s[j] == '1')
                        signal[i, j] = true;
                }

            }
        }
        //DINT
        private void GetSignal(object[] obj, ref bool[,] signal)
        {
            for (int i = 0; i < obj.Length; i++)
            {
                if (obj[i].ToString() == "0")
                    continue;
                string obj2str = Convert.ToString(long.Parse(obj[i].ToString()), 2).PadLeft(32, '0');

                int m = 0;
                for (int k = i * 4; k < i * 4 + 4; k++)
                {
                    //高低位转换
                    string s = conv(obj2str.Substring(m, 8));
                    for (int j = 0; j < s.Length; j++)
                    {
                        if(s[j]=='1')
                            signal[k, j] = true;
                    }
                    m += 8;
                }
            }
        }
        private string conv(string a)
        {
            string b = string.Empty;
            for (int i = a.Length - 1; i >= 0; i--)
            {
                b += a[i];
            }
            return b;
        }
        //private void tmConveyorWorker1(object sender, System.Timers.ElapsedEventArgs e)
        //{
        //    try
        //    {
        //        tmConveyor.Stop();

        //        bool[] signal = new bool[44];
        //        string serviceName = "ConveyorPLC1";
        //        object[] obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "MotorSignal1"));
        //        for (int j = 0; j < obj.Length; j++)
        //            signal[j] = Convert.ToBoolean(obj[j]);


        //        //obj = ObjectUtil.GetObjects(Context.ProcessDispatcher.WriteToService(serviceName, "MotorSignal2"));
        //        //string plcTaskNo = Util.ConvertStringChar.BytesToString(obj);

        //        Conveyor[] conveyor = new Conveyor[20];
        //        conveyor[0] = new Conveyor();
        //        conveyor[0].ConveyorNo = 1;
        //        conveyor[0].Fwd = signal[0];
        //        conveyor[0].Rev = signal[1];
        //        conveyor[0].Up = signal[1];
        //        conveyor[0].Down = signal[2];
        //        conveyor[0].Load = signal[3];
        //        conveyor[0].Status = 1;

        //        Conveyors.ConveyorInfo(conveyor);
        //    }
        //    catch (Exception ex)
        //    {
        //        Logger.Error(ex.Message);
        //    }
        //    finally
        //    {
        //        tmConveyor.Start();
        //    }
        //}
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
                //string CraneErrCode = this.dgvMain.Rows[this.dgvMain.CurrentCell.RowIndex].Cells["colErrCode"].Value.ToString();
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
            //Task.frmSelectCell f = new Task.frmSelectCell();
            //f.ShowDialog();
            try
            {
                frmChangeMode f = new frmChangeMode(Context);
                if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    GetWorkMode();
                    //写入PLC
                    Context.ProcessDispatcher.WriteToService("ConveyorPLC1", "WorkMode", Program.mainForm.WorkMode);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
            }
        }
        
    }
}
