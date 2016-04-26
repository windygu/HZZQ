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
    public partial class StockTotal : BaseForm
    {
        BLL.BLLBase bll = new BLL.BLLBase();
        int ValidPeriod = 0;

        public StockTotal()
        {
            InitializeComponent();
            Filter.EnableFilter(dgvMain);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.BindData();
        }        
        private void BindData()
        {
            string filter = "CMD_Cell.ProductCode!='' and CMD_Cell.InDate is not null";

            DataTable dt = bll.FillDataTable("WCS.SelectStockTotal", new DataParameter[] { new DataParameter("{0}", filter) });
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
            string ProductCode = dgvMain.Rows[e.RowIndex].Cells[0].Value.ToString();
            int.TryParse(dgvMain.Rows[e.RowIndex].Cells["Column2"].Value.ToString(), out ValidPeriod);
            string filter = string.Format("CMD_Cell.ProductCode='{0}' and CMD_Cell.InDate is not null", ProductCode);
            DataTable dt = bll.FillDataTable("WCS.SelectStockDetail", new DataParameter[] { new DataParameter("{0}", filter) });
            this.bsDetail.DataSource = dt;
        }

        private void dgvDetail_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            if (this.dgvMain.CurrentCell == null)
                return;            
            
            for (int i = 0; i < this.dgvDetail.Rows.Count; i++)
            {
                int StockHours = int.Parse(this.dgvDetail.Rows[i].Cells["colStockHours"].Value.ToString());
                if (StockHours > ValidPeriod && ValidPeriod > 0)
                    this.dgvDetail.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                else
                {
                    if (i % 2 == 0)
                        this.dgvDetail.Rows[i].DefaultCellStyle.BackColor = Color.White;
                    else
                        this.dgvDetail.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(192, 255, 192);

                }
            }
        }

        private void StockTotal_Load(object sender, EventArgs e)
        {
            this.BindData();
        }

        private void dgvDetail_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            SolidBrush solidBrush = new SolidBrush(dgvDetail.RowHeadersDefaultCellStyle.ForeColor);
            int index = e.RowIndex + 1;
            e.Graphics.DrawString(index.ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 12, e.RowBounds.Location.Y + 4);
        }        
    }
}
