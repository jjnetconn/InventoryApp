using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Threading.Tasks;
using Excel = Microsoft.Office.Interop.Excel;
using Marshal = System.Runtime.InteropServices.Marshal;
using System.Reflection;
using System.Diagnostics;


namespace LagerMan_v2
{
    class AppService_ExcelImport
    {
        public delegate void StatusUpdateHandler(object sender, ProgressEventArgs e);
        public event StatusUpdateHandler OnUpdateStatus;
        private AppEventLogger _appEventlog = new AppEventLogger();
        private Excel.Application oXL = null;
        private Excel.Workbook oWB = null;
        
        public List<DataSet> GetExcel(string fileName)
        {
            List<DataSet> excelWorkBook = new List<DataSet>();
            Stopwatch queryTimer = new Stopwatch();
            if (Properties.Settings.Default.debug)
            {
                queryTimer.Start();
            }
            try
            {    
                UpdateStatus("Excel indlæsning startet");

                //  creat a Application object
                oXL = new Excel.Application();
                //   get   WorkBook  object
                oWB = oXL.Workbooks.Open(fileName, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                        Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value, Missing.Value,
                        Missing.Value, Missing.Value);

                for (int i = 0; i < oWB.Sheets.Count; i++)
                {
                    excelWorkBook.Add(GetExcelSheet(fileName, (i + 1), this.oXL, this.oWB));
                }
            }
            catch (Exception ex)
            {
                _appEventlog.writeError(ex.Message, ex.StackTrace);
            }
            finally
            {
                releaseObject(ref oWB);
                if (oXL != null)
                {
                    oXL.Quit();
                }
                releaseObject(ref oXL);
            }
            if (Properties.Settings.Default.debug)
            {
                queryTimer.Stop();
                _appEventlog.writeInfo("Indlæsning af Excelark tog: " + queryTimer.Elapsed.ToString());
            }
            return excelWorkBook;
        }

        private void UpdateStatus(string status)
        {
            // Make sure someone is listening to event
            if (OnUpdateStatus == null) return;

            ProgressEventArgs args = new ProgressEventArgs(status);
            OnUpdateStatus(this, args);
        }

        private DataSet GetExcelSheet(string fileName, int sheetNo, Excel.Application oXL, Excel.Workbook oWB)
        {
            DataTable dt = new DataTable("dtExcel");
            DataSet ds = new DataSet();

            Excel.Worksheet oSheet = null;
            Excel.Range oRng = null;

            Stopwatch queryTimer = new Stopwatch();
            if (Properties.Settings.Default.debug){
                queryTimer.Start();
            }
            try
            {
                //   get   WorkSheet object 
                oSheet = (Excel.Worksheet)oWB.Sheets[sheetNo];
                
                ds.Tables.Add(dt);
                DataRow dr;

                int jValue = oSheet.UsedRange.Cells.Columns.Count;
                int iValue = oSheet.UsedRange.Cells.Rows.Count;
                //  get data columns
                for (int j = 1; j <= jValue; j++)
                {
                    dt.Columns.Add("column" + j, System.Type.GetType("System.String"));
                }

                //  get data in cell
                for (int i = 1; i <= iValue; i++)
                {
                    dr = ds.Tables["dtExcel"].NewRow();
                    
                    for (int j = 1; j <= jValue; j++)
                    {
                        oRng = (Microsoft.Office.Interop.Excel.Range)oSheet.Cells[i, j];
                        string strValue = oRng.Text.ToString();
                        dr["column" + j] = strValue;
                    }
                    
                    ds.Tables["dtExcel"].Rows.Add(dr);
                }
                
                //releaseObject(ref oSheet);
                //releaseObject(ref oWB);
                // The Quit is done in the finally because we always
                // want to quit. It is no different than releasing RCWs. 
            }
            catch (Exception ex)
            {
                _appEventlog.writeError(ex.Message, ex.StackTrace);
            }

            UpdateStatus("Excel indlæsning afsluttet");
            if (Properties.Settings.Default.debug)
            {
                queryTimer.Stop();
                _appEventlog.writeInfo("Indlæsning af Exceldata tog: " + queryTimer.Elapsed.ToString());
            }
            return ds;
        }

        private void releaseObject(ref Excel.Application obj)
        {
            // Do not catch an exception from this.
            // You may want to remove these guards depending on
            // what you think the semantics should be.
            if (obj != null && Marshal.IsComObject(obj))
            {
                Marshal.ReleaseComObject(obj);
            }
            // Since passed "by ref" this assingment will be useful
            // (It was not useful in the original, and neither was the
            //  GC.Collect.)
            obj = null;
        }

        private void releaseObject(ref Excel.Workbook obj)
        {
            // Do not catch an exception from this.
            // You may want to remove these guards depending on
            // what you think the semantics should be.
            if (obj != null && Marshal.IsComObject(obj))
            {
                Marshal.ReleaseComObject(obj);
            }
            // Since passed "by ref" this assingment will be useful
            // (It was not useful in the original, and neither was the
            //  GC.Collect.)
            obj = null;
        }

        private void releaseObject(ref Excel.Worksheet obj)
        {
            // Do not catch an exception from this.
            // You may want to remove these guards depending on
            // what you think the semantics should be.
            if (obj != null && Marshal.IsComObject(obj))
            {
                Marshal.ReleaseComObject(obj);
            }
            // Since passed "by ref" this assingment will be useful
            // (It was not useful in the original, and neither was the
            //  GC.Collect.)
            obj = null;
        }
    }
}
