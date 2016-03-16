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
namespace App.View.Task
{
    public partial class frmProducePlans : BaseForm
    {
        BLL.BLLBase bll = new BLL.BLLBase();
        

        public frmProducePlans()
        {
            InitializeComponent();
        }
        
        private void toolStripButton_Close_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void toolStripButton_Refresh_Click(object sender, EventArgs e)
        {
            BindData();
        }

        private void toolStripButton_Delete_Click(object sender, EventArgs e)
        {
            if (this.dgvMain.CurrentRow == null)
                return;
            if (this.dgvMain.CurrentRow.Index >= 0)
            {
                if (this.dgvMain.SelectedRows[0].Cells["colState"].Value.ToString() == "保存")
                {
                    if (DialogResult.Yes == MessageBox.Show("您确定要删除此生产计划吗？", "询问", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        string BillID = this.dgvMain.SelectedRows[0].Cells["colBillID"].Value.ToString();
                        //bll.ExecNonQuery("WCS.UpdateTaskStateByTaskNo", new DataParameter[] { new DataParameter("@State", 9), new DataParameter("@TaskNo", TaskNo) });
                        DataParameter[] param = new DataParameter[] { new DataParameter("@BillID", BillID) };
                        bll.ExecNonQueryTran("WCS.DeleteProducePlan", param);

                        this.BindData();
                    }
                }
                else
                {
                    MessageBox.Show("选中的状态非[保存],请确认！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }
        private void BindData()
        {
            //DataTable dt = bll.FillDataTable("WCS.SelectProducePlan", new DataParameter[] { new DataParameter("{0}", "WCS_TASK.State in('0','1','2','3') and WCS_TASK.TaskType='11'") });
            DataTable dt = bll.FillDataTable("WCS.SelectProducePlan");
            bsMain.DataSource = dt;
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
        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            UpdatedgvMainState("1");
        }

        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            UpdatedgvMainState("2");
        }
        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            UpdatedgvMainState("0");
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {
            UpdatedgvMainState("3");
        }

        
        private void UpdatedgvMainState(string State)
        {
            if (this.dgvMain.CurrentCell != null)
            {
                BLL.BLLBase bll = new BLL.BLLBase();
                string BillID = this.dgvMain.SelectedRows[0].Cells["colBillID"].Value.ToString();
                bll.ExecNonQuery("WCS.UpdatePlanStateByBillID", new DataParameter[] { new DataParameter("@State", State), new DataParameter("@BillID", BillID) });

                ////堆垛机完成执行
                //if (State == "7")
                //{
                //    DataParameter[] param = new DataParameter[] { new DataParameter("@TaskNo", TaskNo) };
                //    bll.ExecNonQueryTran("WCS.Sp_TaskProcess", param);
                //}
                BindData();
            }
        }

        

        private void frmProducePlans_Activated(object sender, EventArgs e)
        {
            this.BindData();
        }

        private void toolStripButton_Add_Click(object sender, EventArgs e)
        {
            frmProducePlanEdit f = new frmProducePlanEdit();
            if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                this.BindData();
            }
        }

        private void toolStripButton_Check_Click(object sender, EventArgs e)
        {
            if (this.dgvMain.CurrentRow == null)
                return;
            if (this.dgvMain.CurrentRow.Index >= 0)
            {
                if (this.dgvMain.SelectedRows[0].Cells["colState"].Value.ToString() == "保存")
                {
                    string BillID = this.dgvMain.SelectedRows[0].Cells["colBillID"].Value.ToString();
                    bll.ExecNonQuery("WCS.UpdatePlanStateByBillID", new DataParameter[] { new DataParameter("@State", 1), new DataParameter("@BillID", BillID) });

                    this.BindData();

                }
                else
                {
                    MessageBox.Show("选中的状态非[保存],请确认！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }

        private void toolStripButton_Adjust_Click(object sender, EventArgs e)
        {
            if (this.dgvMain.CurrentRow == null)
                return;
            if (this.dgvMain.CurrentRow.Index >= 0)
            {
                if (this.dgvMain.SelectedRows[0].Cells["colState"].Value.ToString() == "保存")
                {
                    string BillID = this.dgvMain.SelectedRows[0].Cells["colBillID"].Value.ToString();
                    DataRowView drv = dgvMain.SelectedRows[0].DataBoundItem as DataRowView;
                    DataRow dr = drv.Row;

                    frmProducePlanEdit f = new frmProducePlanEdit(dr);
                    if (f.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        this.BindData();
                    };

                }
                else
                {
                    MessageBox.Show("选中的状态非[保存],请确认！", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }        
    }
}

