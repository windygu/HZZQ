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

namespace App.View.Report
{
    public partial class InStock : BaseForm
    {
        BLL.BLLBase bll = new BLL.BLLBase();
        private string TaskType = "11";

        public InStock()
        {
            InitializeComponent();
            Filter.EnableFilter(dgvMain);
        }
        public InStock(string TaskType)
        {
            InitializeComponent();
            this.TaskType = TaskType;
            Filter.EnableFilter(dgvMain);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            InStockDialog f = new InStockDialog(TaskType);
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pnlProgress.Top = (pnlMain.Height - pnlProgress.Height) / 3;
                pnlProgress.Left = (pnlMain.Width - pnlProgress.Width) / 2;
                pnlProgress.Visible = true;
                Application.DoEvents();

                string filter = string.Format("WCS_TASK.TaskType='{0}' and WCS_TASK.State='2' and {1}", TaskType, f.filter);
                DataTable dt = bll.FillDataTable("WCS.SelectTaskTotal", new DataParameter[] { new DataParameter("{0}", filter) });
                bsMain.DataSource = dt;

                if (dt.Rows.Count <= 0)
                {
                    filter = string.Format("1=2");
                    dt = bll.FillDataTable("WCS.SelectTaskDetail", new DataParameter[] { new DataParameter("{0}", filter) });
                    this.bsDetail.DataSource = dt;
                }
                pnlProgress.Visible = false;
            }
        }

        private void InStock_Load(object sender, EventArgs e)
        {
            this.BindData();
        }
        private void BindData()
        {
            string filter = string.Format("Convert(varchar(10),WCS_TASK.TaskDate,120)=convert(varchar(10),getdate(),120) and WCS_TASK.State='2' and WCS_TASK.TaskType='{0}'",TaskType);

            DataTable dt = bll.FillDataTable("WCS.SelectTaskTotal", new DataParameter[] { new DataParameter("{0}", filter) });
            bsMain.DataSource = dt;
            
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (bsMain.DataSource == null)
                return;            
            View.ExcelHelper.DoExport((DataTable)bsMain.DataSource);
        }

        private void dgvMain_RowEnter(object sender, DataGridViewCellEventArgs e)
        {
            try
            {
                string TaskDate = dgvMain.Rows[e.RowIndex].Cells[0].Value.ToString();
                string ProductCode = dgvMain.Rows[e.RowIndex].Cells[1].Value.ToString();
                string filter = string.Format("WCS_TASK.TaskType='{0}' and WCS_TASK.State='2' and WCS_TASK.ProductCode='{1}' and convert(varchar(10),WCS_TASK.TaskDate,120)='{2}'", TaskType, ProductCode, TaskDate);
                DataTable dt = bll.FillDataTable("WCS.SelectTaskDetail", new DataParameter[] { new DataParameter("{0}", filter) });
                this.bsDetail.DataSource = dt;
            }
            catch { }
        }

        private void dgvDetail_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(dgvDetail.RowHeadersDefaultCellStyle.ForeColor);
            int index = e.RowIndex + 1;
            e.Graphics.DrawString(index.ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 12, e.RowBounds.Location.Y + 4);
        }

        private void dgvMain_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            
        }
    }
}
