﻿using System;
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

        public StockTotal()
        {
            InitializeComponent();
            Filter.EnableFilter(dgvMain);
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            this.BindData();
        }

        private void StockQuery_Load(object sender, EventArgs e)
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
            string filter = string.Format("CMD_Cell.ProductCode='{0}' and CMD_Cell.InDate is not null", ProductCode);
            DataTable dt = bll.FillDataTable("WCS.SelectStockDetail", new DataParameter[] { new DataParameter("{0}", filter) });
            this.bsDetail.DataSource = dt;
        }
    }
}
