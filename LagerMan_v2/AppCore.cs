using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using System.Diagnostics;

namespace LagerMan_v2
{
    public delegate void ExcelEventHandler(string eventText);

    class AppCore
    {
        public event ExcelEventHandler excelEvent;
        
        public void dbLoadExcel(List<DataSet> excelList, int startRow, int cnameRow, int cnameCol, string mfgBy)
        {
            Stopwatch queryTimer = new Stopwatch();
            queryTimer.Start();

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
                                    var query2 = (from q in ivb.suppliers
                                                  where q.name.Equals(mfgBy)
                                                  select q.id).FirstOrDefault();
                                    var query3 = (from q in ivb.productCatalog
                                                  where q.prShortName.Equals(prShortName)
                                                  select q.prNumber).FirstOrDefault();

                                    panels p = new panels();

                                    p.panelSupplier = (int)query2;
                                    p.panelCname = cName[0];
                                    p.prodNo = (int)query3;
                                    p.panelSerial = (string)ds.Tables[0].Rows[i].ItemArray[0];
                                    p.panelEff = Double.Parse(ds.Tables[0].Rows[i].ItemArray[3].ToString(), CultureInfo.CurrentCulture);
                                    p.panelVmp = Double.Parse(ds.Tables[0].Rows[i].ItemArray[4].ToString(), CultureInfo.CurrentCulture);
                                    p.panelVoc = Double.Parse(ds.Tables[0].Rows[i].ItemArray[5].ToString(), CultureInfo.CurrentCulture);
                                    p.panelImp = Double.Parse(ds.Tables[0].Rows[i].ItemArray[6].ToString(), CultureInfo.CurrentCulture);
                                    p.panelIsc = Double.Parse(ds.Tables[0].Rows[i].ItemArray[7].ToString(), CultureInfo.CurrentCulture);
                                    p.panelFf = Double.Parse(ds.Tables[0].Rows[i].ItemArray[8].ToString(), CultureInfo.CurrentCulture);

                                    ivb.panels.Add(p);
                                    ivb.SaveChanges();

                                    //Console.WriteLine("" + i);
                                }
                                else
                                {
                                    Console.WriteLine("panel is already in DB");
                                }

                            }
                        }
                    }
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
            
            queryTimer.Stop();

            if (excelEvent != null)
            {
                ExcelEventHandler args = new ExcelEventHandler("Excel indlæsning færdig");
                excelEvent(args);
            }
            
            Console.WriteLine("Execution time: " + queryTimer.Elapsed);

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
