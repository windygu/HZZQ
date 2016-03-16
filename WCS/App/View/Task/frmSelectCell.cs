using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace App.View.Task
{
    public partial class frmSelectCell : Form
    {
        BLL.BLLBase bll = new BLL.BLLBase();

        private Dictionary<int, DataRow[]> shelf = new Dictionary<int, DataRow[]>();
        private Dictionary<int, string> ShelfCode = new Dictionary<int, string>();

        private DataTable cellTable = null;
        private int racks = 4;
        private int columns = 5;
        private int rows = 11;
        private int cellWidth = 0;
        private int cellHeight = 0;
        private int[] top = new int[4];
        private int left = 5;
        string CellCode = "";

        public frmSelectCell()
        {
            InitializeComponent();
            //设置双缓冲
            SetStyle(ControlStyles.DoubleBuffer |
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint, true);
        }
        private void pnlChart_Paint(object sender, PaintEventArgs e)
        {
            try
            {
                Font font = new Font("微软雅黑", 8);
                SizeF size = e.Graphics.MeasureString("第1排第5层", font);
                float adjustHeight = Math.Abs(size.Height - cellHeight) / 2;
                size = e.Graphics.MeasureString("13", font);

                float adjustWidth = (cellWidth - size.Width) / 2;

                for (int i = 0; i < racks; i++)
                {
                    if (!shelf.ContainsKey(i))
                    {
                        DataRow[] rows = cellTable.Select(string.Format("ShelfCode='{0}'", ShelfCode[i]), "CellCode desc");
                        shelf.Add(i, rows);
                    }

                    DrawShelf(shelf[i], e.Graphics, top[i], font, adjustWidth);

                    int tmpLeft = left + columns * cellWidth + 5;

                    for (int j = 0; j < rows; j++)
                    {
                        string s = string.Format("{0}-{1}", i+1, Convert.ToString(rows - j).PadLeft(2, '0'));
                        e.Graphics.DrawString(s, font, Brushes.Orange, tmpLeft, top[i] + j * cellHeight);
                    }                    
                }                
            }
            catch (Exception ex)
            {
                string str = ex.Message;
            }
        }

        private void DrawShelf(DataRow[] cellRows, Graphics g, int top, Font font, float adjustWidth)
        {
            string shelfCode = "001";
            foreach (DataRow cellRow in cellRows)
            {
                shelfCode = cellRow["ShelfCode"].ToString();
                int column = Convert.ToInt32(cellRow["CellColumn"]);


                int row = rows - Convert.ToInt32(cellRow["CellRow"]) + 1;
                int quantity = ReturnColorFlag(cellRow["ProductCode"].ToString(), cellRow["IsActive"].ToString(), cellRow["IsLock"].ToString(), cellRow["ErrorFlag"].ToString());

                int x = left + (column - 1) * cellWidth;
                int y = top + (row-1) * cellHeight;

                Pen pen = new Pen(Color.DarkCyan, 2);
                g.DrawRectangle(pen, new Rectangle(x, y, cellWidth, cellHeight));
                
                FillCell(g, top, row, column, quantity, shelfCode);
                
            }
            for (int j = 1; j <= columns; j++)
            {
                if (j == 1 && cellRows.Length < columns * rows)
                    continue;
                g.DrawString(Convert.ToString(j), new Font("微软雅黑", 10), Brushes.Orange, left + (j - 1) * cellWidth + adjustWidth, top + (rows*cellHeight));
            }
        }
        private int ReturnColorFlag(string ProductCode, string IsActive, string IsLock, string ErrFlag)
        {
            int Flag = 0;
            if (ProductCode == "")
            {
                if (IsLock == "1")
                {
                    Flag = 1;
                }
            }
            else
            {
                if (IsLock == "0")
                {
                    if (ProductCode == "0001")
                        Flag = 6;
                    else
                        Flag = 2;
                }
                else
                {
                    if (ProductCode == "0001")
                        Flag = 7;
                    else
                        Flag = 3;
                }
            }
            if (IsActive == "0")
                Flag = 4;
            if (ErrFlag == "1")
                Flag = 5;
            return Flag;
        }
        
        private void FillCell(Graphics g, int top, int row, int column, int quantity, string shelfCode)
        {
            int x = left + (column - 1) * cellWidth;

            int y = top + (row-1) * cellHeight;
            if (quantity == 1)  //空货位锁定
                g.FillRectangle(Brushes.Yellow, new Rectangle(x + 2, y + 2, cellWidth - 3, cellHeight - 3));
            else if (quantity == 2) //有货未锁定
                g.FillRectangle(Brushes.Blue, new Rectangle(x + 2, y + 2, cellWidth - 3, cellHeight - 3));
            else if (quantity == 3) //有货且锁定
                g.FillRectangle(Brushes.Green, new Rectangle(x + 2, y + 2, cellWidth - 3, cellHeight - 3));
            else if (quantity == 4) //禁用
                g.FillRectangle(Brushes.Gray, new Rectangle(x + 2, y + 2, cellWidth - 3, cellHeight - 3));
            else if (quantity == 5) //有问题
                g.FillRectangle(Brushes.Red, new Rectangle(x + 2, y + 2, cellWidth - 3, cellHeight - 3));
            else if (quantity == 6) //托盘
                g.FillRectangle(Brushes.Orange, new Rectangle(x + 2, y + 2, cellWidth - 3, cellHeight - 3));
            else if (quantity == 7) //托盘锁定
                g.FillRectangle(Brushes.Gold, new Rectangle(x + 2, y + 2, cellWidth - 3, cellHeight - 3));
        }

        private void frmSelectCell_Load(object sender, EventArgs e)
        {
            ShelfCode.Clear();

            DataTable dtShelf = bll.FillDataTable("CMD.SelectShelf");
            for (int i = 0; i < dtShelf.Rows.Count; i++)
            {
                ShelfCode.Add(i, dtShelf.Rows[i]["ShelfCode"].ToString());
            }
            cellTable = bll.FillDataTable("WCS.SelectCell");

            cellWidth = (pnlChart.Width-40) / columns;
            cellHeight = (pnlChart.Height / (racks*rows))-2;
            //cellWidth = cellHeight;

            top[0] = 2;
            top[1] = pnlChart.Height / racks + 2;
            top[2] = 2*pnlChart.Height / racks + 4;
            top[3] = 3*pnlChart.Height / racks + 6;  
        }
        private void pnlChart_MouseClick(object sender, MouseEventArgs e)
        {
            int shelf = 1;
            if (e.Y > top[1] && e.Y < top[2])
                shelf = 2;
            else if (e.Y > top[2] && e.Y < top[3])
                shelf = 3;
            else if (e.Y > top[3])
                shelf = 4;

            int column = (e.X - left) / cellWidth + 1;
            int row = rows - (e.Y - top[shelf - 1]) / cellHeight;
            if (column <= columns && row <= rows)
            {                
                if (e.Button == System.Windows.Forms.MouseButtons.Left)
                {
                    CellCode = (1000 + shelf).ToString().Substring(1, 3) + (1000 + column).ToString().Substring(1, 3) + (1000 + row).ToString().Substring(1, 3);
                    MessageBox.Show("货位:" + CellCode);
                }
            }

        }
        private int X, Y;
        private void pnlChart_MouseMove(object sender, MouseEventArgs e)
        {
            if (X != e.X || Y != e.Y)
            {
                int shelf = 1;
                if (e.Y > top[1] && e.Y < top[2])
                    shelf = 2;
                else if(e.Y > top[2] && e.Y < top[3])
                    shelf = 3;
                else if (e.Y > top[3])
                    shelf = 4;

                int column = (e.X - left) / cellWidth + 1;
                int row = rows - (e.Y - top[shelf-1]) / cellHeight;
                if (column <= columns && row <= rows && row > 0 && column > 0)
                {
                    string tip = "货架:" + shelf.ToString() + ";列:" + column.ToString() + ";层:" + row.ToString();
                    toolTip1.SetToolTip(pnlChart, tip);
                }
                else
                    toolTip1.SetToolTip(pnlChart, null);

                X = e.X;
                Y = e.Y;
            }
        }     
    }
}
