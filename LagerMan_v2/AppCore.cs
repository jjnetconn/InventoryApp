using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LagerMan_v2
{
    class AppCore
    {
        public delegate void StatusUpdateHandler(object sender, ProgressEventArgs e);
        public event StatusUpdateHandler OnUpdateStatus;
        private AppEventLogger _appEventlog = new AppEventLogger();
        public Dictionary<string, int> PreloadedSuppliers { get; set; }
        public Dictionary<string, int> PreloadedProductCatalog { get; set; }

        public void preloadSuppliers()
        {
            Dictionary<string, int> _preSuppilers = new Dictionary<string, int>();

            using (inventoryBaseEntities ivb = new inventoryBaseEntities())
            {
                try
                {
                    var query = (from q in ivb.suppliers
                                 select q);

                    foreach (suppliers itm in query)
                    {
                        _preSuppilers.Add(itm.name, itm.id);
                    }
                }
                catch (Exception ex)
                {
                    _appEventlog.writeError(ex.Message, ex.StackTrace);
                }
            }
            
            PreloadedSuppliers = _preSuppilers;
        }

        public void preloadProducCatalog()
        {
            Dictionary<string, int> _preProductCatalog = new Dictionary<string, int>();

            using (inventoryBaseEntities ivb = new inventoryBaseEntities())
            {
                try
                {
                    var query = (from q in ivb.productCatalog
                                 select q);

                    foreach (productCatalog itm in query)
                    {
                        if (!itm.prShortName.Equals(null))
                        {
                            _preProductCatalog.Add(itm.prShortName, (int)itm.prNumber);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _appEventlog.writeError(ex.Message, ex.StackTrace);
                }
            }

            PreloadedProductCatalog = _preProductCatalog;
        }

        public void dbLoadExcel(List<DataSet> excelList)
        {
            if (excelList[0].Tables[0].Rows[1].ItemArray[2].ToString().Contains("ASM"))
            {
                loadAlgatec(excelList, Properties.Settings.Default.AG_StartRow, Properties.Settings.Default.AG_cNameRow, Properties.Settings.Default.AG_cNameCol, Properties.Settings.Default.AG_mfgBy);
            }
            else
            {
                loadSunpower(excelList, Properties.Settings.Default.SP_StartRow, Properties.Settings.Default.SP_cNameRow, Properties.Settings.Default.SP_cNameCol, Properties.Settings.Default.SP_mfgBy);
            }

        }

        public void loadAlgatec(List<DataSet> excelList, int startRow, int cnameRow, int cnameCol, string mfgBy)
        {
            Stopwatch queryTimer = new Stopwatch();
            if (Properties.Settings.Default.debug)
            {
                queryTimer.Start();
            }
            UpdateStatus("Indlæser til Database");
            using (inventoryBaseEntities ivb = new inventoryBaseEntities())
            {
                try
                {
                    DataSet ds = excelList[0];
                    //foreach (DataSet ds in excelList)
                    //{
                    //if (ds.Tables[0].Rows.Count < 2 || ds.Tables[0].Columns.Count < 2)
                    //if (!ds.Tables[0].Rows[cnameRow].ItemArray[cnameCol].Equals(null))
                        {
                        string[] cName = (ds.Tables[0].Rows[cnameRow].ItemArray[cnameCol]).ToString().Split(' ');
                        string prShortName = cName[0] + " " + cName[1];
                        for (int i = startRow; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (((String)ds.Tables[0].Rows[i].ItemArray[0]).Length > 2)
                            {
                                string testSerial = (string)ds.Tables[0].Rows[i].ItemArray[0];
                                var query = (from q in ivb.panels
                                             where q.panelSerial.Equals(testSerial)
                                             select q.panelSerial);
                                if (query.Count() < 1)
                                {
                                    if (Properties.Settings.Default.debug)
                                    {
                                        _appEventlog.writeInfo("Opslag efter producent og produkt nr.: " + queryTimer.Elapsed.ToString());
                                        queryTimer.Reset();

                                        queryTimer.Start();
                                    }
                                    panels p = new panels();
                                    p.panelSupplier = (from q in PreloadedSuppliers where q.Key.Contains(mfgBy) select q).First().Value;
                                    p.panelCname = cName[0];
                                    p.prodNo = (from q in PreloadedProductCatalog where q.Key.Contains(prShortName) select q).First().Value;
                                    p.panelSerial = (string)ds.Tables[0].Rows[i].ItemArray[1];
                                    p.panelMFGDate = DateTime.Parse(ds.Tables[0].Rows[i].ItemArray[2].ToString(), CultureInfo.CurrentCulture);
                                    p.panelCellclass = Double.Parse(ds.Tables[0].Rows[i].ItemArray[3].ToString(), CultureInfo.CurrentCulture);
                                    p.panelEff = Double.Parse(ds.Tables[0].Rows[i].ItemArray[6].ToString(), CultureInfo.CurrentCulture);
                                    p.panelVmp = Double.Parse(ds.Tables[0].Rows[i].ItemArray[9].ToString(), CultureInfo.CurrentCulture);
                                    p.panelVoc = Double.Parse(ds.Tables[0].Rows[i].ItemArray[4].ToString(), CultureInfo.CurrentCulture);
                                    p.panelImp = Double.Parse(ds.Tables[0].Rows[i].ItemArray[10].ToString(), CultureInfo.CurrentCulture);
                                    p.panelIsc = Double.Parse(ds.Tables[0].Rows[i].ItemArray[5].ToString(), CultureInfo.CurrentCulture);
                                    p.panelFf = Double.Parse(ds.Tables[0].Rows[i].ItemArray[8].ToString(), CultureInfo.CurrentCulture);
                                    ivb.panels.Add(p);

                                    if (Properties.Settings.Default.debug)
                                    {
                                        queryTimer.Stop();
                                        _appEventlog.writeInfo("Oprettelse af panel i dataset " + queryTimer.Elapsed.ToString());
                                        queryTimer.Reset();
                                    }
                                }
                                else
                                {
                                    _appEventlog.writeWarning("Panel med serie nr.: " +
                                        (string)ds.Tables[0].Rows[i].ItemArray[0].ToString() +
                                        " findes allerede i databasen (dobbelt indlæsning)");

                                    if (Properties.Settings.Default.debug)
                                    {
                                        Console.WriteLine("Panel med serie nr.: " +
                                        (string)ds.Tables[0].Rows[i].ItemArray[0].ToString() +
                                        " findes allerede i databasen (dobbelt indlæsning)");
                                    }
                                }
                            }
                            }
                        }
                    //}
                    //Commit all panels to DB
                    ivb.SaveChanges();
                }
                catch (Exception ex)
                {
                    _appEventlog.writeError(ex.Message, ex.StackTrace);
                }
                finally
                {
                    if (Properties.Settings.Default.debug)
                    {
                        queryTimer.Stop();
                        UpdateStatus("Indlæst til database");
                        _appEventlog.writeInfo("Database indlæsning tog: " + queryTimer.Elapsed.ToString());
                        UpdateStatus("Klar");
                    }
                    //Disposeing database entity
                    ivb.Dispose();
                }
            }
        }

        public void loadSunpower(List<DataSet> excelList, int startRow, int cnameRow, int cnameCol, string mfgBy)
        {
            Stopwatch queryTimer = new Stopwatch();
            if (Properties.Settings.Default.debug)
            {
                queryTimer.Start();
            }
            UpdateStatus("Indlæser til Database");
            using (inventoryBaseEntities ivb = new inventoryBaseEntities())
            {
                try
                {
                    foreach (DataSet ds in excelList)
                    {
                        string[] cName = ((string)ds.Tables[0].Rows[cnameRow].ItemArray[cnameCol]).Split(' ');
                        string prShortName = cName[0];
                        for (int i = startRow; i < ds.Tables[0].Rows.Count; i++)
                        {
                            if (((String)ds.Tables[0].Rows[i].ItemArray[0]).Length > 2)
                            {
                                string testSerial = (string)ds.Tables[0].Rows[i].ItemArray[0];
                                var query = (from q in ivb.panels
                                             where q.panelSerial.Equals(testSerial)
                                             select q.panelSerial);
                                if (query.Count() < 1)
                                {
                                    if (Properties.Settings.Default.debug)
                                    {
                                        _appEventlog.writeInfo("Opslag efter producent og produkt nr.: " + queryTimer.Elapsed.ToString());
                                        queryTimer.Reset();

                                        queryTimer.Start();
                                    }
                                    panels p = new panels();
                                    p.panelSupplier = (from q in PreloadedSuppliers where q.Key.Contains(mfgBy) select q).First().Value;
                                    p.panelCname = cName[0];
                                    p.prodNo = (from q in PreloadedProductCatalog where q.Key.Contains(prShortName) select q).First().Value;
                                    p.panelSerial = (string)ds.Tables[0].Rows[i].ItemArray[0];
                                    p.panelEff = Double.Parse(ds.Tables[0].Rows[i].ItemArray[3].ToString(), CultureInfo.CurrentCulture);
                                    p.panelVmp = Double.Parse(ds.Tables[0].Rows[i].ItemArray[4].ToString(), CultureInfo.CurrentCulture);
                                    p.panelVoc = Double.Parse(ds.Tables[0].Rows[i].ItemArray[5].ToString(), CultureInfo.CurrentCulture);
                                    p.panelImp = Double.Parse(ds.Tables[0].Rows[i].ItemArray[6].ToString(), CultureInfo.CurrentCulture);
                                    p.panelIsc = Double.Parse(ds.Tables[0].Rows[i].ItemArray[7].ToString(), CultureInfo.CurrentCulture);
                                    p.panelFf = Double.Parse(ds.Tables[0].Rows[i].ItemArray[8].ToString(), CultureInfo.CurrentCulture);
                                    ivb.panels.Add(p);

                                    if (Properties.Settings.Default.debug)
                                    {
                                        queryTimer.Stop();
                                        _appEventlog.writeInfo("Oprettelse af panel i dataset " + queryTimer.Elapsed.ToString());
                                        queryTimer.Reset();
                                    }
                                }
                                else
                                {
                                    _appEventlog.writeWarning("Panel med serie nr.: " +
                                        (string)ds.Tables[0].Rows[i].ItemArray[0].ToString() +
                                        " findes allerede i databasen (dobbelt indlæsning)");

                                    if (Properties.Settings.Default.debug)
                                    {
                                        Console.WriteLine("Panel med serie nr.: " +
                                        (string)ds.Tables[0].Rows[i].ItemArray[0].ToString() +
                                        " findes allerede i databasen (dobbelt indlæsning)");
                                    }
                                }
                            }
                        }
                    }
                    //Commit all panels to DB
                    ivb.SaveChanges();
                }
                catch (Exception ex)
                {
                    _appEventlog.writeError(ex.Message, ex.StackTrace);
                }
                finally
                {
                    if (Properties.Settings.Default.debug)
                    {
                        queryTimer.Stop();
                        UpdateStatus("Indlæst til database");
                        _appEventlog.writeInfo("Database indlæsning tog: " + queryTimer.Elapsed.ToString());
                        UpdateStatus("Klar");
                    }
                    //Disposeing database entity
                    ivb.Dispose();
                }
            }
        }

        private void UpdateStatus(string status)
        {
            // Make sure someone is listening to event
            if (OnUpdateStatus == null) return;

            ProgressEventArgs args = new ProgressEventArgs(status);
            OnUpdateStatus(this, args);
        }

        public void findProductNr(string barcode)
        {
            using (inventoryBaseEntities ivb = new inventoryBaseEntities())
            {
                try
                {

                }
                catch(Exception ex)
                {
                    AppEventLogger log = new AppEventLogger();
                    log.writeError(ex.Message, ex.StackTrace);
                }
            }
        }

        public string getCity(string postCode)
        {
            int code = Int32.Parse(postCode);
            string city = "";
            using (inventoryBaseEntities ivb = new inventoryBaseEntities())
            {
                try
                {
                    var query = (from q in ivb.postCodes
                                 where q.postCode == code
                                 select q.cityName).FirstOrDefault();
                    city = query.ToString();
                }
                catch (Exception ex)
                {
                    AppEventLogger log = new AppEventLogger();
                    log.writeError(ex.Message, ex.StackTrace);
                }
                finally
                {
                    ivb.Dispose();
                }
            }
            return city;
        }

        public int getProdGrp(string barcode)
        {
            int prodGrp = 0;

            char[] charBarcode = barcode.ToCharArray();


            if (Char.IsLetter(charBarcode[0]) && Char.IsLetter(charBarcode[3]) && charBarcode.Length == 12)
            {

            }
            return prodGrp;
        }

        public object findProductByNumber(string barCodeIn)
        {
            object oProduct = null;
            using (inventoryBaseEntities ivb = new inventoryBaseEntities())
            {
                try
                {
                    var query = (from q in ivb.activeInventory
                                 where q.serialNo.Equals(barCodeIn)
                                 select q).FirstOrDefault();
                    oProduct = query;
                }
                catch (Exception ex)
                {
                    AppEventLogger log = new AppEventLogger();
                    log.writeError(ex.Message, ex.StackTrace);
                }
                finally
                {
                    ivb.Dispose();
                }
            }
            return oProduct;
        }
    }
}
