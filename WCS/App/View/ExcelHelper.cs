using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.OleDb;
using Microsoft.Office.Interop.Excel;
using System.Reflection;

namespace App.View
{
    public class ExcelHelper
    {
        /// <summary>
        ///  从excel2007文件中读出dt
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static System.Data.DataTable ExcelToDataTable(string fileName)
        {
            System.Data.DataTable dt;
            string conStr = @"Provider=Microsoft.ACE.OLEDB.12.0;Data Source=" + fileName + ";Extended Properties='Excel 12.0;HDR=YES;IMEX=1'";
            OleDbConnection myConn = new OleDbConnection(conStr);
            string strCom = " SELECT * FROM [Sheet1$]";
            myConn.Open();
            OleDbDataAdapter myCommand = new OleDbDataAdapter(strCom, myConn);
            dt = new System.Data.DataTable();
            myCommand.Fill(dt);
            myConn.Close();
            return dt;
        }


        public static void DataTabletoExcel(System.Data.DataTable tmpDataTable, string strFileName)
        {
            if (tmpDataTable == null)
                return;

            int rowNum = tmpDataTable.Rows.Count;
            int columnNum = tmpDataTable.Columns.Count;
            int rowIndex = 1;
            int columnIndex = 0;

            Application xlApp = new ApplicationClass();
            xlApp.DefaultFilePath = "";
            xlApp.DisplayAlerts = true;
            xlApp.SheetsInNewWorkbook = 1;
            Workbook xlBook = xlApp.Workbooks.Add(true);

            //将DataTable的列名导入Excel表第一行
            foreach (DataColumn dc in tmpDataTable.Columns)
            {
                columnIndex++;
                xlApp.Cells[rowIndex, columnIndex] = dc.ColumnName;
            }

            //将DataTable中的数据导入Excel中

            for (int i = 0; i < rowNum; i++)
            {
                rowIndex++;
                columnIndex = 0;
                for (int j = 0; j < columnNum; j++)
                {
                    columnIndex++;
                    xlApp.Cells[rowIndex, columnIndex] = tmpDataTable.Rows[i][j].ToString();
                }
            }
            //xlBook.SaveCopyAs(HttpUtility.UrlDecode(strFileName, System.Text.Encoding.UTF8));
            xlBook.SaveCopyAs(strFileName);
            xlBook.Close(false);
        }
        /// <summary>
        /// DataTable直接导出Excel,此方法会把DataTable的数据用Excel打开,再自己手动去保存到确切的位置
        /// </summary>
        /// <param name="dt">要导出Excel的DataTable</param>
        /// <returns></returns>
        public static bool DoExport(System.Data.DataTable dt)
        {
            Application app = new ApplicationClass();
            if (app == null)
            {
                throw new Exception("Excel无法启动");
            }
            app.Visible = true;
            Workbooks wbs = app.Workbooks;
            Workbook wb = wbs.Add(Missing.Value);
            Worksheet ws = (Worksheet)wb.Worksheets[1];

            int cnt = dt.Rows.Count;
            int columncnt = dt.Columns.Count;

            // *****************获取数据********************
            object[,] objData = new Object[cnt + 1, columncnt];  // 创建缓存数据
            // 获取列标题
            for (int i = 0; i < columncnt; i++)
            {
                string columnName = GetColumnName(dt.Columns[i].ColumnName);
                objData[0, i] = columnName;
            }
            // 获取具体数据
            for (int i = 0; i < cnt; i++)
            {
                System.Data.DataRow dr = dt.Rows[i];
                for (int j = 0; j < columncnt; j++)
                {
                    objData[i + 1, j] = dr[j];
                }
            }
            //********************* 写入Excel******************
            Range r = ws.get_Range(app.Cells[1, 1], app.Cells[cnt + 1, columncnt]);
            r.NumberFormat = "@";
            //r = r.get_Resize(cnt+1, columncnt);
            r.Value2 = objData;
            r.EntireColumn.AutoFit();

            app = null;
            return true;

        }
        private static string GetColumnName(string columnName)
        {
            string chnName = columnName;
            if (columnName == "TaskNo")
                chnName = "任务编号";
            else if (columnName == "StateDesc")
                chnName = "状态";
            else if (columnName == "ProductCode")
                chnName = "产品编号";
            else if (columnName == "ProductNo")
                chnName = "产品代号";
            else if (columnName == "BillTypeName")
                chnName = "单据类型";
            else if (columnName == "ProductName")
                chnName = "产品名称";
            else if (columnName == "Spec")
                chnName = "产品规格";
            else if (columnName == "CellCode")
                chnName = "货位编号";
            else if (columnName == "StartDate")
                chnName = "开始时间";
            else if (columnName == "FinishDate")
                chnName = "结束时间";
            else if (columnName == "TaskDate")
                chnName = "作业时间";
            else if (columnName == "InDate")
                chnName = "入库时间";
            else if (columnName == "StockDays")
                chnName = "存放时数(小时)";
            else if (columnName == "BillNo")
                chnName = "单据号码";
            else if (columnName == "IsActive")
                chnName = "启用";
            else if (columnName == "IsLock")
                chnName = "锁定";
            else if (columnName == "Quantity")
                chnName = "数量";
            else if (columnName == "Total")
                chnName = "数量";
            else if (columnName == "ValidPeriod")
                chnName = "有效期限";
            return chnName;
        }
    }
}
