using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MCP;
using Util;

namespace App.View
{
    public partial class frmChangeMode : Form
    {
        private Dictionary<string, string> ProductFields = new Dictionary<string, string>();
        BLL.BLLBase bll = new BLL.BLLBase();
        Context context;
        Dictionary<int, string> dicProductNo = new Dictionary<int, string>();
        public frmChangeMode()
        {
            InitializeComponent();
        }
        public frmChangeMode(Context context)
        {
            InitializeComponent();
            this.context = context;
        }
        private void frmChangeMode_Load(object sender, EventArgs e)
        {
            dicProductNo.Add(0, "A");
            dicProductNo.Add(1, "B");
            dicProductNo.Add(2, "C");
            dicProductNo.Add(3, "D");
            dicProductNo.Add(4, "E");
            dicProductNo.Add(5, "F");

            ProductFields.Add("ProductNo", "产品代号");
            ProductFields.Add("ProductCode", "产品编号");
            ProductFields.Add("ProductName", "产品名称");
            ProductFields.Add("Spec", "规格");

            DataTable dt = bll.FillDataTable("WCS.SelectWorkMode");
            if (dt.Rows.Count > 0)
            {
                if (dt.Rows[0]["WorkMode"].ToString() == "0")
                    this.radioButton1.Checked = true;
                else if (dt.Rows[0]["WorkMode"].ToString() == "1")
                    this.radioButton2.Checked = true;
                else if (dt.Rows[0]["WorkMode"].ToString() == "2")
                    this.radioButton3.Checked = true;
                else if (dt.Rows[0]["WorkMode"].ToString() == "3")
                    this.radioButton4.Checked = true;

                if (this.radioButton4.Checked)
                {
                    this.txtProductCode.Text = dt.Rows[0]["ProductCode"].ToString();
                    this.txtProductName.Text = dt.Rows[0]["ProductName"].ToString();
                    this.txtQuantity.Text = dt.Rows[0]["OutQty"].ToString();
                }
                else
                {
                    this.txtProductCode.Text = "";
                    this.txtProductName.Text = "";
                    this.txtQuantity.Text = "";
                }
            }
        }

        private void btnProductCode_Click(object sender, EventArgs e)
        {
            DataTable dt = bll.FillDataTable("CMD.SelectProduct");
            //DataTable dt = bll.FillDataTable("CMD.SelectProduct", new DataParameter[] { new DataParameter("{0}", "ProductCode!='0001'") });

            SelectDialog selectDialog = new SelectDialog(dt, ProductFields, false);
            if (selectDialog.ShowDialog() == DialogResult.OK)
            {
                this.txtProductCode.Text = selectDialog["ProductCode"];
                this.txtProductName.Text = selectDialog["ProductName"];
            }
        }

        private void radioButton1_CheckedChanged(object sender, EventArgs e)
        {
            this.btnProductCode.Enabled = false;
            this.txtQuantity.ReadOnly = true;
        }
        private void radioButton3_CheckedChanged(object sender, EventArgs e)
        {
            this.btnProductCode.Enabled = true;            
            this.txtQuantity.ReadOnly = false;
        }
        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            this.btnProductCode.Enabled = true;            
            this.txtQuantity.ReadOnly = false;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            if (this.radioButton1.Checked)
                Program.mainForm.WorkMode =0;
            else if (this.radioButton2.Checked)
                Program.mainForm.WorkMode = 1;
            else if (this.radioButton3.Checked)
                Program.mainForm.WorkMode = 2;
            else if (this.radioButton4.Checked)
                Program.mainForm.WorkMode = 3;

            int OutQty = 0;
            string ProductCode = "";
            string ProductName = "";
            string ProductNo = "";
            //更新任务状态
            if (this.radioButton3.Checked)
            {
                //int.TryParse(this.txtQuantity.Text.Trim(), out OutQty);

                //if (OutQty <= 0)
                //{
                //    MessageBox.Show("请输入出库数量", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                //    this.txtQuantity2.Focus();
                //    return;
                //}
                //int key = int.Parse(ObjectUtil.GetObject(context.ProcessDispatcher.WriteToService("ConveyorPLC1", "ProductNo")).ToString());
                //ProductNo = dicProductNo[key];

                //DataTable dt = bll.FillDataTable("WCS.SelectProduct", new DataParameter[] { new DataParameter("{0}", string.Format("ProductNo='{0}'", ProductNo)) });
                //if (dt.Rows.Count > 0)
                //{
                //    ProductCode = dt.Rows[0]["ProductCode"].ToString();
                //    ProductName = dt.Rows[0]["ProductName"].ToString();
                //}
            }
            if (this.radioButton3.Checked || this.radioButton4.Checked)
            {
                if (this.txtProductCode.Text.Length <= 0)
                {
                    MessageBox.Show("请先选择产品", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.btnProductCode.Focus();
                    return;
                }

                ProductCode = this.txtProductCode.Text;
                ProductName = this.txtProductName.Text;
                int.TryParse(this.txtQuantity.Text.Trim(), out OutQty);

                if (OutQty <= 0)
                {
                    MessageBox.Show("请输入出库数量", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtQuantity.Focus();
                    return;
                }
            }
            int OutQuantity = 0;
            int Valid = 1;
            if (this.radioButton5.Checked)
                Valid = 1;
            else if (this.radioButton6.Checked)
                Valid = 0;
            else
                Valid = 2;
            try
            {
                for (int i = 0; i < OutQty; i++)
                {
                    string CellCode = "";

                    DataParameter[] param = new DataParameter[] 
                    { 
                        new DataParameter("@CraneNo", "01"), 
                        new DataParameter("@ProductCode",ProductCode),
                        new DataParameter("@CellCode",CellCode),
                        new DataParameter("@Valid",Valid),
                        new DataParameter("@WorkMode",Program.mainForm.WorkMode)
                    };
                    bll.FillDataTable("WCS.Sp_CreateOutTask", param);
                    OutQuantity++;
                }

                //保存状态
                bll.ExecNonQuery("WCS.UpdateWorkMode", new DataParameter[] {    
                            new DataParameter("@WorkMode", Program.mainForm.WorkMode),
                            new DataParameter("@ProductCode", ProductCode),
                            new DataParameter("@ProductName", ProductName),
                            new DataParameter("@OutQty", OutQuantity)});

                //写入输送机PLC
                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
            catch (System.Data.SqlClient.SqlException ex)
            {
                //如果是存储过程名称是PROC_NAME，而且State是数据库中设置的一个值 如：66
                //则该异常就是我们需要特殊处理的一个异常
                if (ex.Procedure.Equals("Sp_CreateOutTask") && ex.State == 1)
                {
                    if (OutQuantity <= 0)
                        MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    else
                    {
                        MessageBox.Show(ex.Message + ",已产生" + OutQuantity.ToString() + "笔出库任务", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        //保存状态
                        bll.ExecNonQuery("WCS.UpdateWorkMode", new DataParameter[] {    
                                                                new DataParameter("@WorkMode", Program.mainForm.WorkMode),
                                                                new DataParameter("@ProductCode", ProductCode),
                                                                new DataParameter("@ProductName", ProductName),
                                                                new DataParameter("@OutQty", OutQuantity)});
                    }
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
