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
        Thread workerThread;
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

            try
            {
                baseDB.Database.Connection.Open();
            }
            catch (Exception ex)
            {
                AppEventLogger log = new AppEventLogger();
                log.writeError(ex.Message, ex.StackTrace);
                if (MessageBox.Show("Fejl i forbindelse til databasen! Se eventuelt eventlog", "Fejl", MessageBoxButtons.OKCancel) == DialogResult.OK)
                {
                    Application.Exit();
                }
            }
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
                case "Sunpower": _appCore.dbLoadExcel(_appServiceExcel.GetExcel(FileName), Properties.Settings.Default.SP_StartRow, Properties.Settings.Default.SP_cNameRow, Properties.Settings.Default.SP_cNameCol, Properties.Settings.Default.SP_mfgBy); break;
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
                try
                {
                    if ((xlsStream = openFileDialog1.OpenFile()) != null)
                    {
                        startWorker(openFileDialog1.FileName, panelMfg);
                    }
                    xlsStream.Close();
                }
                catch(Exception ex)
                {
                    AppEventLogger log = new AppEventLogger();
                    log.writeError(ex.Message, ex.StackTrace);
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
