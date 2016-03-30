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
    public partial class frmClearTask : Form
    {
        BLL.BLLBase bll = new BLL.BLLBase();
        public frmClearTask()
        {
            InitializeComponent();
        }

        private void btnRequest_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("此清理功能会把小于当前日期的任务全部删除，请操作前最好备份数据库", "询问", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                string filter = string.Format("convert(varchar(10),WCS_TASK.TaskDate,120)<='{0}'", this.dtpFromDate.Value.ToString("yyyy-MM-dd"));
                bll.ExecNonQuery("WCS.DeleteHistoryTask", new DataParameter[] { new DataParameter("{0}", filter) });
                MessageBox.Show("清理完成", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = System.Windows.Forms.DialogResult.OK;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        }
    }
}
