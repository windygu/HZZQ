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
    public partial class StockQuery : BaseForm
    {
        BLL.BLLBase bll = new BLL.BLLBase();

        public StockQuery()
        {
            InitializeComponent();
            Filter.EnableFilter(dgvMain);
        }        

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            InStockDialog f = new InStockDialog();
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                pnlProgress.Top = (pnlMain.Height - pnlProgress.Height) / 3;
                pnlProgress.Left = (pnlMain.Width - pnlProgress.Width) / 2;
                pnlProgress.Visible = true;
                Application.DoEvents();

                string filter = f.filter;
                DataTable dt = bll.FillDataTable("WCS.SelectStockDetail", new DataParameter[] { new DataParameter("{0}", filter) });
                bsMain.DataSource = dt;

                pnlProgress.Visible = false;
            }
        }

        private void StockQuery_Load(object sender, EventArgs e)
        {
            this.BindData();
        }
        private void BindData()
        {
            string filter = string.Format("CMD_Cell.InDate is not null");

            DataTable dt = bll.FillDataTable("WCS.SelectStockDetail", new DataParameter[] { new DataParameter("{0}", filter) });
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

        private void dgvMain_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            for (int i = 0; i < this.dgvMain.Rows.Count; i++)
            {
                int ValidPeriod = 0;
                int.TryParse(dgvMain.Rows[i].Cells["Column8"].Value.ToString(), out ValidPeriod);
                int StockHours = int.Parse(dgvMain.Rows[i].Cells["Column2"].Value.ToString());
                if (StockHours > ValidPeriod)
                    this.dgvMain.Rows[i].DefaultCellStyle.BackColor = Color.Red;
                else
                {
                    if (i % 2 == 0)
                        this.dgvMain.Rows[i].DefaultCellStyle.BackColor = Color.White;
                    else
                        this.dgvMain.Rows[i].DefaultCellStyle.BackColor = Color.FromArgb(192, 255, 192);

                }
            }
        }

        private void dgvMain_RowPostPaint(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            //foreach (DataGridViewRow dr in dgvMain.Rows)
            //    dr.Cells[0].Value = dr.Index + 1;
            SolidBrush solidBrush = new SolidBrush(dgvMain.RowHeadersDefaultCellStyle.ForeColor);
            int index = e.RowIndex + 1;
            e.Graphics.DrawString(index.ToString(), e.InheritedRowStyle.Font, solidBrush, e.RowBounds.Location.X + 12, e.RowBounds.Location.Y + 4);
        }
    }
}
