using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Util;

namespace App.View.Task
{
    public partial class frmCraneTask : BaseForm
    {
        BLL.BLLBase bll = new BLL.BLLBase();
        DataRow dr;

        public frmCraneTask()
        {
            InitializeComponent();
        }

        private void frmCraneTask_Load(object sender, EventArgs e)
        {
            DataTable dt = bll.FillDataTable("CMD.SelectCrane", new DataParameter[] { new DataParameter("{0}", "CMD_Crane.State='1'") });
            this.cmbCraneNo.DataSource = dt.DefaultView;
            this.cmbCraneNo.ValueMember = "CraneNo";
            this.cmbCraneNo.DisplayMember = "CraneNo";

            this.cmbTaskType.SelectedIndex = 0;
            this.txtTaskNo1.Text = DateTime.Now.ToString("yyMMdd") + "0000";
            
        }        

        private void cmbTaskType_SelectedIndexChanged(object sender, EventArgs e)
        {
            BindShelf();
            //if (this.cmbTaskType.SelectedIndex == 1)
            //    this.checkBox1.Visible = true;
            //else
            //{
            this.checkBox1.Visible = false;
            this.checkBox1.Checked = false;
            //}
        }
        private void BindShelf()
        {
            DataParameter[] param = new DataParameter[] 
            { 
                new DataParameter("{0}", string.Format("CraneNo='{0}'", this.cmbCraneNo.Text))
            };
            if (this.cmbTaskType.SelectedIndex == 0)
            {
                DataTable dt = new DataTable("dt");
                dt.Columns.Add("dtText");
                dt.Columns.Add("dtValue");
                DataRow dr = dt.NewRow();

                dr["dtText"] = "002";
                dr["dtValue"] = "002";

                dt.Rows.Add(dr);

                this.cbFromRow.DataSource = dt;
                this.cbFromRow.DisplayMember = "dtText";
                this.cbFromRow.ValueMember = "dtValue";

                //DataTable dt = bll.FillDataTable("CMD.SelectShelf", param);
                //this.cbFromRow.DataSource = dt.DefaultView;
                //this.cbFromRow.ValueMember = "shelfcode";
                //this.cbFromRow.DisplayMember = "ShelfRow";
            }
            else
            {
                DataTable dt = bll.FillDataTable("CMD.SelectShelf", param);
                this.cbFromRow.DataSource = dt.DefaultView;
                this.cbFromRow.ValueMember = "shelfcode";
                this.cbFromRow.DisplayMember = "ShelfRow";
            }

            if (this.cmbTaskType.SelectedIndex == 1)
            {
                DataTable dt = new DataTable("dt");
                dt.Columns.Add("dtText");
                dt.Columns.Add("dtValue");
                DataRow dr = dt.NewRow();

                dr["dtText"] = "003";
                dr["dtValue"] = "003";

                dt.Rows.Add(dr);

                this.cbToRow.DataSource = dt;
                this.cbToRow.DisplayMember = "dtText";
                this.cbToRow.ValueMember = "dtValue";

                //DataTable dt = bll.FillDataTable("CMD.SelectShelf", param);
                //this.cbToRow.DataSource = dt.DefaultView;
                //this.cbToRow.ValueMember = "shelfcode";
                //this.cbToRow.DisplayMember = "ShelfRow";
            }
            else
            {
                DataTable dtt = bll.FillDataTable("CMD.SelectShelf", param);
                this.cbToRow.DataSource = dtt.DefaultView;
                this.cbToRow.ValueMember = "shelfcode";
                this.cbToRow.DisplayMember = "ShelfRow";
            }
        }
        private void cbFromRow_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cbFromRow.Text == "System.Data.DataRowView")
                return;

            DataParameter[] param = new DataParameter[] 
            { 
                new DataParameter("{0}", string.Format("ShelfCode='{0}'",this.cbFromRow.SelectedValue.ToString()))
            };


            if (this.cmbTaskType.SelectedIndex == 0)
            {
                DataTable dt = new DataTable("dt");
                dt.Columns.Add("dtText");
                dt.Columns.Add("dtValue");
                DataRow dr = dt.NewRow();

                dr["dtText"] = "100";
                dr["dtValue"] = "100";

                dt.Rows.Add(dr);
                this.cbFromColumn.DataSource = dt;
                this.cbFromColumn.DisplayMember = "dtText";
                this.cbFromColumn.ValueMember = "dtValue";
            }
            else
            {
                DataTable dt = bll.FillDataTable("CMD.SelectColumn", param);
                this.cbFromColumn.DataSource = dt.DefaultView;
                this.cbFromColumn.ValueMember = "CellColumn";
                this.cbFromColumn.DisplayMember = "CellColumn";
            }
        }
        private void cbToRow_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cbToRow.Text == "System.Data.DataRowView")
                return;

            DataParameter[] param = new DataParameter[] 
            { 
                new DataParameter("{0}", string.Format("ShelfCode='{0}'",this.cbToRow.SelectedValue.ToString()))
            };

            if (this.cmbTaskType.SelectedIndex == 1)
            {
                DataTable dt = new DataTable("dt");
                dt.Columns.Add("dtText");
                dt.Columns.Add("dtValue");
                DataRow dr = dt.NewRow();


                dr["dtText"] = "100";
                dr["dtValue"] = "100";


                dt.Rows.Add(dr);
                this.cbToColumn.DataSource = dt;
                this.cbToColumn.DisplayMember = "dtText";
                this.cbToColumn.ValueMember = "dtValue";
            }
            else
            {
                DataTable dt = bll.FillDataTable("CMD.SelectColumn", param);

                this.cbToColumn.DataSource = dt.DefaultView;
                this.cbToColumn.ValueMember = "CellColumn";
                this.cbToColumn.DisplayMember = "CellColumn";
            }
        }

        private void cbFromColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cbFromRow.Text == "System.Data.DataRowView")
                return;
            if (this.cbFromColumn.Text == "System.Data.DataRowView")
                return;

            DataParameter[] param = new DataParameter[] 
            { 
                new DataParameter("{0}", string.Format("ShelfCode='{0}' and CellColumn={1}",this.cbFromRow.SelectedValue.ToString(),this.cbFromColumn.Text))
            };

            if (this.cmbTaskType.SelectedIndex == 0)
            {
                DataTable dt = new DataTable("dt");
                dt.Columns.Add("dtText");
                dt.Columns.Add("dtValue");
                DataRow dr = dt.NewRow();
                dr["dtText"] = "2";
                dr["dtValue"] = "2";
                dt.Rows.Add(dr);
                this.cbFromHeight.DataSource = dt;
                this.cbFromHeight.DisplayMember = "dtText";
                this.cbFromHeight.ValueMember = "dtValue";
            }
            else
            {
                DataTable dt = bll.FillDataTable("CMD.SelectCell", param);
                DataView dv = dt.DefaultView;
                dv.Sort = "CellRow";
                this.cbFromHeight.DataSource = dv;
                this.cbFromHeight.ValueMember = "CellRow";
                this.cbFromHeight.DisplayMember = "CellRow";
            }
        }
        private void cbToColumn_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.cbToRow.Text == "System.Data.DataRowView")
                return;
            if (this.cbToColumn.Text == "System.Data.DataRowView")
                return;

            DataParameter[] param = new DataParameter[] 
            { 
                new DataParameter("{0}", string.Format("ShelfCode='{0}' and CellColumn={1}",this.cbToRow.SelectedValue.ToString(),this.cbToColumn.Text))
            };

            if (this.cmbTaskType.SelectedIndex == 1)
            {
                DataTable dt = new DataTable("dt");
                dt.Columns.Add("dtText");
                dt.Columns.Add("dtValue");
                DataRow dr = dt.NewRow();


                dr["dtText"] = "1";
                dr["dtValue"] = "1";


                dt.Rows.Add(dr);
                this.cbToHeight.DataSource = dt;
                this.cbToHeight.DisplayMember = "dtText";
                this.cbToHeight.ValueMember = "dtValue";
            }
            else
            {
                DataTable dt = bll.FillDataTable("CMD.SelectCell", param);
                DataView dv = dt.DefaultView;
                dv.Sort = "CellRow";
                this.cbToHeight.DataSource = dv;
                this.cbToHeight.ValueMember = "CellRow";
                this.cbToHeight.DisplayMember = "CellRow";
            }
        }

        private void btnAction_Click(object sender, EventArgs e)
        {
            string serviceName = "CranePLC" + this.cmbCraneNo.Text.Substring(1,1);
            byte[] cellAddr = new byte[8];
            cellAddr[0] = byte.Parse((this.cmbTaskType.SelectedIndex+1).ToString());
            cellAddr[1] = 0;  //0-不允许伸叉，1-允许伸叉
            cellAddr[2] = byte.Parse(this.cbFromRow.Text);
            cellAddr[3] = byte.Parse(this.cbFromColumn.Text);
            cellAddr[4] = byte.Parse(this.cbFromHeight.Text);
            cellAddr[5] = byte.Parse(this.cbToRow.Text);
            cellAddr[6] = byte.Parse(this.cbToColumn.Text);
            cellAddr[7] = byte.Parse(this.cbToHeight.Text);

            //for (int i = 0; i < cellAddr.Length; i++)
            //    cellAddr[i] += 48;
            sbyte[] palletBarcode = new sbyte[8];
            Util.ConvertStringChar.stringToBytes("", 8).CopyTo(palletBarcode, 0);

            sbyte[] taskNo = new sbyte[10];
            Util.ConvertStringChar.stringToBytes(this.txtTaskNo1.Text, 10).CopyTo(taskNo, 0);

            Context.ProcessDispatcher.WriteToService(serviceName, "TaskAddress", cellAddr);
            Context.ProcessDispatcher.WriteToService(serviceName, "PalletCode", palletBarcode);
            Context.ProcessDispatcher.WriteToService(serviceName, "TaskNo", taskNo);
            Context.ProcessDispatcher.WriteToService(serviceName, "ProductType", 1);
            Context.ProcessDispatcher.WriteToService(serviceName, "WriteFinished", 1);

            string fromStation = this.cbFromRow.Text + (1000 + int.Parse(this.cbFromColumn.Text)).ToString().Substring(1, 3) + (1000 + int.Parse(this.cbFromHeight.Text)).ToString().Substring(1, 3);
            string toStation = this.cbToRow.Text + (1000 + int.Parse(this.cbToColumn.Text)).ToString().Substring(1, 3) + (1000 + int.Parse(this.cbToHeight.Text)).ToString().Substring(1, 3);
            MCP.Logger.Info("测试任务已下发给" + this.cmbCraneNo.Text + "堆垛机;起始地址:" + fromStation + ",目标地址:" + toStation);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string serviceName = "CranePLC" + this.cmbCraneNo.Text.Substring(1, 1);
            byte[] cellAddr = new byte[8];
            cellAddr[0] = 1;
            cellAddr[1] = 0;
            cellAddr[5] = byte.Parse(this.cbFromRow.Text.Substring(3, 3));
            cellAddr[6] = byte.Parse(this.cbFromColumn.Text);
            cellAddr[7] = byte.Parse(this.cbFromHeight.Text);
            cellAddr[2] = byte.Parse(this.cbToRow.Text.Substring(3, 3));
            cellAddr[3] = byte.Parse(this.cbToColumn.Text);
            cellAddr[4] = byte.Parse(this.cbToHeight.Text);

            for (int i = 0; i < cellAddr.Length; i++)
                cellAddr[i] += 48;
            sbyte[] palletBarcode = new sbyte[8];
            Util.ConvertStringChar.stringToBytes("", 8).CopyTo(palletBarcode, 0);

            sbyte[] taskNo = new sbyte[10];
            Util.ConvertStringChar.stringToBytes(this.txtTaskNo1.Text, 10).CopyTo(taskNo, 0);

            Context.ProcessDispatcher.WriteToService(serviceName, "TaskAddress", cellAddr);
            Context.ProcessDispatcher.WriteToService(serviceName, "PalletCode", palletBarcode);
            Context.ProcessDispatcher.WriteToService(serviceName, "TaskNo", taskNo);
            Context.ProcessDispatcher.WriteToService(serviceName, "ProductType", 49);
            Context.ProcessDispatcher.WriteToService(serviceName, "WriteFinished", 49);

            string fromStation = this.cbFromRow.Text.Substring(3, 3) + (1000 + int.Parse(this.cbFromColumn.Text)).ToString().Substring(1, 3) + (1000 + int.Parse(this.cbFromHeight.Text)).ToString().Substring(1, 3);
            string toStation = this.cbToRow.Text.Substring(3, 3) + (1000 + int.Parse(this.cbToColumn.Text)).ToString().Substring(1, 3) + (1000 + int.Parse(this.cbToHeight.Text)).ToString().Substring(1, 3);
            MCP.Logger.Info("测试任务已下发给" + this.cmbCraneNo.Text + "堆垛机;起始地址:" + toStation + ",目标地址:" + fromStation);
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            if (this.checkBox1.Checked)
                this.button1.Visible = true;
            else
                this.button1.Visible = false;
        }

        private void btnDelTask_Click(object sender, EventArgs e)
        {
            sbyte[] taskNo = new sbyte[10];
            Util.ConvertStringChar.stringToBytes("", 10).CopyTo(taskNo, 0);

            Context.ProcessDispatcher.WriteToService("CranePLC1", "TaskNo", taskNo);
        }
    }
}
