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
        public frmChangeMode()
        {
            InitializeComponent();
        }

        private void frmChangeMode_Load(object sender, EventArgs e)
        {
            ProductFields.Add("ProductCode", "产品编号");
            ProductFields.Add("ProductName", "产品名称");
            ProductFields.Add("ProductTypeName", "产品类型");

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
                    this.txtProductName.Text = dt.Rows[0]["ProductName"].ToString();
                    this.txtQuantity.Text = dt.Rows[0]["OutQty"].ToString();
                }
                else
                {
                    this.txtProductName.Text = "";
                    this.txtQuantity.Text = "";
                }
            }
        }

        private void btnProductCode_Click(object sender, EventArgs e)
        {
            DataTable dt = bll.FillDataTable("CMD.SelectProduct", new DataParameter[] { new DataParameter("{0}", "ProductCode!='0001'") });

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

        private void radioButton4_CheckedChanged(object sender, EventArgs e)
        {
            this.btnProductCode.Enabled = true;
            this.txtQuantity.ReadOnly = false;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            int WorkMode = 0;
            if (this.radioButton2.Checked)
                WorkMode = 1;
            else if (this.radioButton3.Checked)
                WorkMode = 2;
            else if (this.radioButton4.Checked)
                WorkMode = 3;

            int OutQty = 0;
            //更新任务状态
            if (this.radioButton4.Checked)
            {
                if (this.txtProductCode.Text.Length <= 0)
                {
                    MessageBox.Show("请先选择产品", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.btnProductCode.Focus();
                    return;
                }

                int.TryParse(this.txtQuantity.Text.Trim(), out OutQty);

                if (OutQty <= 0)
                {
                    MessageBox.Show("请输入出库数量", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    this.txtQuantity.Focus();
                    return;
                }
                try
                {
                    for (int i = 0; i < OutQty; i++)
                    {
                        string CellCode = "";

                        DataParameter[] param = new DataParameter[] 
                        { 
                            new DataParameter("@CraneNo", "01"), 
                            new DataParameter("@ProductCode", this.txtProductCode.Text),
                            new DataParameter("@CellCode",CellCode)
                        };
                        bll.FillDataTable("WCS.Sp_CreateOutTask", param);
                    }                    
                }
                catch (System.Data.SqlClient.SqlException ex)
                {
                    //如果是存储过程名称是PROC_NAME，而且State是数据库中设置的一个值 如：66
                    //则该异常就是我们需要特殊处理的一个异常
                    if (ex.Procedure.Equals("Sp_CreateOutTask") && ex.State == 1)
                    {
                        MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }                                
            }
            //保存状态
            bll.ExecNonQuery("WCS.UpdateWorkMode", new DataParameter[] {    
                            new DataParameter("@WorkMode", WorkMode),
                            new DataParameter("@ProductCode", this.txtProductCode.Text),
                            new DataParameter("@ProductName", this.txtProductName.Text),
                            new DataParameter("@OutQty", OutQty)});

            //写入输送机PLC

            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
