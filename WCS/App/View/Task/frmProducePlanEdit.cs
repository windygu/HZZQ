using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using IDAL;

namespace App.View.Task
{
    public partial class frmProducePlanEdit : Form
    {
        BLL.BLLBase bll = new BLL.BLLBase();
        string BillID = "";
        DataRow dr;
        private Dictionary<string, string> ProductFields = new Dictionary<string, string>();

        public frmProducePlanEdit()
        {
            InitializeComponent();
        }
        public frmProducePlanEdit(DataRow dr)
        {
            InitializeComponent();
            this.dr = dr;
            this.BillID = dr["BillID"].ToString();
        }
        private void dtpBillDate_ValueChanged(object sender, EventArgs e)
        {
            if (BillID.Length <= 0)
                this.txtBillID.Text = bll.GetAutoCode("PP", this.dtpBillDate.Value, "1=1");
        }

        private void frmProducePlanEdit_Load(object sender, EventArgs e)
        {
            if (this.BillID.Length > 0)
            {
                this.dtpBillDate.Value = DateTime.Parse(dr["BillDate"].ToString());
                this.txtBillID.Text = dr["BillID"].ToString();
                this.txtProductCode.Text = dr["ProductCode"].ToString();
                this.txtProductName.Text = dr["ProductName"].ToString();
                this.txtPallets.Text = dr["Pallets"].ToString();
                
                this.txtMemo.Text = dr["Memo"].ToString();

                this.Text = "生产计划调整";
                this.btnProductCode.Enabled = false;
                this.dtpBillDate.Enabled = false;
                this.txtBillID.ReadOnly = true;
            }

            ProductFields.Add("ProductCode", "产品编号");
            ProductFields.Add("ProductName", "产品名称");
            ProductFields.Add("CategoryName", "产品类型");
            ProductFields.Add("Spec", "规格");
            ProductFields.Add("ValidPeriod", "有效期");
            
            if (BillID.Length <= 0)
            this.txtBillID.Text = bll.GetAutoCode("PP", this.dtpBillDate.Value, "1=1");
        }

        private void btnProductCode_Click(object sender, EventArgs e)
        {
            DataTable dt = bll.FillDataTable("CMD.SelectProduct", new DataParameter[] { new DataParameter("{0}", "IsProduce='1'") });

            SelectDialog selectDialog = new SelectDialog(dt, ProductFields, false);
            if (selectDialog.ShowDialog() == DialogResult.OK)
            {
                this.txtProductCode.Text = selectDialog["ProductCode"];
                this.txtProductName.Text = selectDialog["ProductName"];
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            int Pallets = 0;
            if (this.txtProductCode.Text.Trim().Length <= 0)
            {
                MessageBox.Show("产品编号不能为空,请选取！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.btnProductCode.Focus();
                return;
            }
            if (this.txtBillID.Text.Trim().Length <= 0)
            {
                MessageBox.Show("单号不能为空,请输入！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.txtBillID.Focus();
                return;
            }
            int.TryParse(this.txtPallets.Text.Trim(), out Pallets);
            if (Pallets <= 0)
            {
                MessageBox.Show("计划盘数不正确,请重输！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.txtPallets.Focus();
                return;
            }

            

            if (BillID.Length <= 0) //新增
            {
                int Count = bll.GetRowCount("WCS_ProducePlan", string.Format("BillID='{0}'", this.txtBillID.Text.Trim()));
                if (Count > 0)
                {
                    this.txtBillID.Text = bll.GetAutoCode("PP", this.dtpBillDate.Value, "1=1");
                }

                bll.ExecNonQuery("WCS.InsertProducePlan", new DataParameter[] {                            
                            new DataParameter("@BillDate", this.dtpBillDate.Value),                            
                            new DataParameter("@BillID", this.txtBillID.Text),
                            new DataParameter("@ProductCode", this.txtProductCode.Text.Trim()),
                            new DataParameter("@Pallets", Pallets),
                            new DataParameter("@AdjustedPallets", Pallets),
                            new DataParameter("@State", "0"),
                            new DataParameter("@Memo", this.txtMemo.Text.Trim()),
                            new DataParameter("@Creator", "admin"),
                            new DataParameter("@Adjuster", "admin")});
            }
            else
            {
                bll.ExecNonQuery("WCS.UpdateProducePlan", new DataParameter[] {                               
                            new DataParameter("@BillID", this.txtBillID.Text),                            
                            new DataParameter("@AdjustedPallets", Pallets),                            
                            new DataParameter("@Memo", this.txtMemo.Text.Trim()),                            
                            new DataParameter("@Adjuster", "admin")});
            }
            this.DialogResult = System.Windows.Forms.DialogResult.OK;
        }
    }
}
