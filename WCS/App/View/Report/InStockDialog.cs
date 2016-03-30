using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Util;

namespace App.View.Report
{
    public partial class InStockDialog : Form
    {
        private Dictionary<string, string> ProductFields = new Dictionary<string, string>();
        BLL.BLLBase bll = new BLL.BLLBase();
        public string filter = "";
        private string TaskType = "";

        public InStockDialog()
        {
            InitializeComponent();
        }
        public InStockDialog(string TaskType)
        {
            InitializeComponent();
            this.TaskType = TaskType;
        }

        private void InStockDialog_Load(object sender, EventArgs e)
        {
            this.dataGridView1.AutoGenerateColumns = false;
            DataTable dt = bll.FillDataTable("CMD.SelectProduct");
            this.dataGridView1.DataSource = dt.DefaultView;
            if (TaskType == "11" || TaskType=="")
                this.label2.Text = "入库日期";
            else
                this.label2.Text = "出库日期";
        }

        private void btnRequest_Click(object sender, EventArgs e)
        {
            string ProductCode = "(''";

            for (int i = 0; i < this.dataGridView1.Rows.Count; i++)
            {
                if (dataGridView1.Rows[i].Cells[0].Value != null)
                {
                    if (bool.Parse(this.dataGridView1.Rows[i].Cells[0].Value.ToString()))
                        ProductCode += ",'" + this.dataGridView1.Rows[i].Cells[1].Value.ToString() + "'";                    
                }
            }
            ProductCode += ")";
            if (TaskType.Length > 0)
            {
                filter = string.Format("WCS_TASK.ProductCode in {0}", ProductCode);
                filter += string.Format(" and Convert(varchar(10),WCS_TASK.TaskDate,120) between '{0}' and '{1}'", this.dtpFromDate.Value.ToString("yyyy-MM-dd"), this.dtpToDate.Value.ToString("yyyy-MM-dd"));
            }
            else
            {
                filter = string.Format("CMD_Cell.ProductCode in {0}", ProductCode);
                filter += string.Format(" and Convert(varchar(10),CMD_Cell.InDate,120) between '{0}' and '{1}'", this.dtpFromDate.Value.ToString("yyyy-MM-dd"), this.dtpToDate.Value.ToString("yyyy-MM-dd"));
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < dataGridView1.Rows.Count; i++)  
            {
                dataGridView1.Rows[i].Cells[0].Value = ((System.Windows.Forms.CheckBox)sender).Checked;  
            }
        }
    }
}
