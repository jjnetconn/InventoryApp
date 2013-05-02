using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;

namespace LagerMan_v2
{
    public partial class MainForm : Form
    {
        private AppCore _appCore;
        private AppService_ExcelImport _appServiceExcel;
        private AppEventLogger _appEventlog;
        Thread workerThread = null;
        String FileName = "";
        inventoryBaseEntities baseDB;

        public MainForm()
        {
            InitializeComponent();
            toolStripStatusLabel4.Text = "Klar";
            baseDB = new inventoryBaseEntities();
            _appCore = new AppCore();
            _appServiceExcel = new AppService_ExcelImport();
            _appCore.OnUpdateStatus += new AppCore.StatusUpdateHandler(ShowLoadDBUpdate);
            _appServiceExcel.OnUpdateStatus += new AppService_ExcelImport.StatusUpdateHandler(ShowLoadExcelUpdate);
            _appEventlog = new AppEventLogger();

            tabControl1.SelectedIndex = 1;

            try
            {
                baseDB.Database.Connection.Open();
            }
            catch (Exception ex)
            {
                _appEventlog.writeError(ex.Message, ex.StackTrace);
                MessageBox.Show("Fejl i forbindelse til databasen! Se eventuelt eventlog", "Fejl", MessageBoxButtons.OK);
                System.Threading.Thread.Sleep(2000);   
                EndApplication();
            }

            //Preloading suppliers and productCatalog to Dictonaries in _appCore
            _appCore.preloadSuppliers();
            _appCore.preloadProducCatalog();
        }

        private void EndApplication()
        {
            Application.ExitThread();
            Application.Exit();
        }

        private void ShowLoadDBUpdate(object sender, ProgressEventArgs e)
        {
            SetStatus(e.Status);
        }

        private void ShowLoadExcelUpdate(object sender, ProgressEventArgs e)
        {
            SetStatus(e.Status);
        }

        private void SetStatus(string status)
        {
            toolStripStatusLabel4.Text = status;
        }

        private void startWorker(string fileName, string panelMfg)
        {
            this.FileName = fileName;
            ThreadStart param_loadExcel = delegate { loadExcel(panelMfg); };

            workerThread = new Thread(param_loadExcel);
            workerThread.Start();
            
            while (!workerThread.IsAlive)
            {

            }
        }

        private void loadExcel(string panelMfg){
            switch (panelMfg)
            {
                case "Sunpower": _appCore.dbLoadExcel(_appServiceExcel.GetExcel(FileName)); break;
                default: break;
            }
        }

        private void getFileNameDialog(string panelMfg)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            Stream xlsStream = null;

            openFileDialog1.InitialDirectory = "Desktop";
            openFileDialog1.Filter = "Excel 97-2003 files (*.xls)|*.xls|Excel files (*.xlsx)|*.xlsx";
            openFileDialog1.FilterIndex = 1;
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Multiselect = false;
            openFileDialog1.SupportMultiDottedExtensions = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                if (openFileDialog1.FileNames.Length > 1 && openFileDialog1.Multiselect)
                {
                    foreach (string fileName in openFileDialog1.FileNames)
                    {
                        try
                        {
                            do
                            {
                                xlsStream = File.Open(fileName, FileMode.Open, FileAccess.Read);
                                startWorker(fileName, panelMfg);
                                xlsStream.Close();
                            }
                            while (!workerThread.IsAlive);
                        }
                        catch (Exception ex)
                        {
                            _appEventlog.writeError(ex.Message, ex.StackTrace);
                        }
                    }
                }
                else
                {
                    try
                    {
                        if ((xlsStream = openFileDialog1.OpenFile()) != null)
                        {
                            startWorker(openFileDialog1.FileName, panelMfg);
                        }
                        xlsStream.Close();
                    }
                    catch (Exception ex)
                    {
                        _appEventlog.writeError(ex.Message, ex.StackTrace);
                    }
                }
            }
        }

        private void excelImportSunpowerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            getFileNameDialog("Sunpower");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
                toolStripStatusLabel2.Text = baseDB.Database.Connection.State.ToString();

        }

        private void textBox4_keyPress(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AppCore core = new AppCore();
                string city = core.getCity(textBox4.Text);
                if (!city.Equals(string.Empty))
                {
                    textBox5.ReadOnly = true;
                    textBox5.Text = city;
                    textBox6.Select();
                }
            }
        }

        private void aflustToolStripMenuItem_Click(object sender, EventArgs e)
        {
            EndApplication();
        }

       /* private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            int prodGrp = 0;

            string searchTable;

            if (e.KeyCode == Keys.Enter)
            {
                AppCore core = new AppCore();



                
            }
        }*/

    }
}
