using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using POTrackingV2.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace SyncDataPOTracking
{

    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                string[] POHistoryAccept = { "E", "Q" };
                int[] MovementTypeAccept = { 105, 101 };
                int[] MovementTypeAlert = { 124 };
                string costIsDelete = "L";
                string costIsClose = "X";

                POTrackingEntities db = new POTrackingEntities();
                AlertToolsEntities dbAlert = new AlertToolsEntities();

                Console.WriteLine("Start");

                #region initiate dictionary and field
                int row = 6;

                Dictionary<string, POTemp> listPO = new Dictionary<string, POTemp>();
                List<POItemTemp> listPOItem = new List<POItemTemp>();
                Dictionary<string, POTemp> listPOAlert = new Dictionary<string, POTemp>();
                Dictionary<int, List<POItemTemp>> listPOItemHistory = new Dictionary<int, List<POItemTemp>>();

                #endregion

                //WebClient request = new WebClient();
                //string url = "ftp://10.48.10.116/MCS/" + "zemes.csv";
                //string fileName = "test\\zemes.csv";
                //request.Credentials = new NetworkCredential("administrator", "P@s5w0rd");
                //// Download the Web resource and save it into the current filesystem folder.
                //request.DownloadFile(url, fileName);

                List<CSVModel> dataFromCSV = File.ReadAllLines("Data\\test.csv").Select(x => CSVModel.FromCSV(x)).ToList();
                foreach (var dataItem in dataFromCSV)
                {
                    //Console.WriteLine("PONumber : " + dataItem.Number);
                    //Console.WriteLine("Type : " + dataItem.Type);
                    //Console.WriteLine("POReleaseDate : " + dataItem.POReleaseDate);
                    //Console.WriteLine("PODate : " + dataItem.PODate);
                    //Console.WriteLine("VendorCode : " + dataItem.VendorCode);
                    //Console.WriteLine("PurchaseOrderCreator : " + dataItem.PurchaseOrderCreator);
                    //Console.WriteLine("ItemNumber : " + dataItem.ItemNumber);
                    //Console.WriteLine("Material : " + dataItem.Material);
                    //Console.WriteLine("Description : " + dataItem.Description);
                    //Console.WriteLine("NetPrice : " + dataItem.NetPrice);
                    //Console.WriteLine("Currency : " + dataItem.Currency);
                    //Console.WriteLine("Quantity : " + dataItem.Quantity);
                    //Console.WriteLine("MovementType : " + dataItem.MovementType);
                    //Console.WriteLine("IsDelete : " + dataItem.IsDelete);
                    //Console.WriteLine("IsClose : " + dataItem.IsClose);
                    //Console.WriteLine("DeliveryDate : " + dataItem.DeliveryDate);
                    //Console.WriteLine("GRDate : " + dataItem.GRDate);
                    //Console.WriteLine("GRQuantity : " + dataItem.GRQuantity);
                    //Console.WriteLine("POHistoryCategory : " + dataItem.POHistoryCategory);
                    //Console.WriteLine("DocumentNumber : " + dataItem.DocumentNumber);
                    //Console.WriteLine("HasInbound : " + dataItem.HasInbound);

                    //if (POHistoryAccept.Contains(dataItem.POHistoryCategory))
                    //{
                    var poItemTempKey = dataItem.Number + "-" + dataItem.ItemNumber;
                    POTemp poTemp = new POTemp();
                    poTemp.Number = dataItem.Number;
                    poTemp.Type = dataItem.Type;
                    poTemp.PurchaseOrderCreator = dataItem.PurchaseOrderCreator;
                    poTemp.POCreator = dataItem.POCreator;
                    poTemp.PODate = dataItem.PODate;
                    poTemp.POReleaseDate = dataItem.POReleaseDate;
                    poTemp.VendorCode = dataItem.VendorCode;

                    if (!listPO.ContainsKey(poItemTempKey))
                    {
                        listPO.Add(poItemTempKey, poTemp);
                    }

                    POItemTemp poItemTemp = new POItemTemp();
                    poItemTemp.PONumber = dataItem.Number;
                    poItemTemp.ItemNumber = dataItem.ItemNumber;
                    poItemTemp.Material = dataItem.Material;
                    poItemTemp.MaterialVendor = dataItem.MaterialVendor;
                    poItemTemp.Description = dataItem.Description;
                    poItemTemp.NetPrice = dataItem.NetPrice;
                    poItemTemp.Currency = dataItem.Currency;
                    poItemTemp.Quantity = dataItem.Quantity;
                    poItemTemp.MovementType = dataItem.MovementType;
                    poItemTemp.IsClose = dataItem.IsClose;
                    poItemTemp.IsDelete = dataItem.IsDelete;
                    poItemTemp.DeliveryDate = dataItem.DeliveryDate;
                    poItemTemp.GRQuantity = dataItem.GRQuantity;
                    poItemTemp.GRDate = dataItem.GRDate;
                    poItemTemp.POHistoryCategory = dataItem.POHistoryCategory;
                    poItemTemp.DocumentNumber = dataItem.DocumentNumber;
                    poItemTemp.InboundNumber = dataItem.InboundNumber;
                    poItemTemp.PayTerm = dataItem.PayTerm;
                    poItemTemp.PRNumber = dataItem.PRNumber;
                    poItemTemp.PRCreateDate = dataItem.PRCreateDate;
                    poItemTemp.PRReleaseDate = dataItem.PRReleaseDate;
                    poItemTemp.ProgressDay = dataItem.ProgressDay;

                    listPOItem.Add(poItemTemp);

                    if (listPO.SingleOrDefault(x => x.Key == poItemTempKey).Value.listItem == null)
                    {

                        listPO.SingleOrDefault(x => x.Key == poItemTempKey).Value.listItem = new List<POItemTemp>();
                    }


                    listPO.SingleOrDefault(x => x.Key == poItemTempKey).Value.listItem.Add(poItemTemp);

                    if (poItemTemp.MovementType != null)
                    {
                        if (MovementTypeAlert.Contains((int)poItemTemp.MovementType))
                        {
                            if (!listPOAlert.ContainsKey(poItemTempKey))
                            {
                                listPOAlert.Add(poItemTempKey, poTemp);
                            }

                            if (listPOAlert.SingleOrDefault(x => x.Key == poItemTempKey).Value.listItem == null)
                            {
                                listPOAlert.SingleOrDefault(x => x.Key == poItemTempKey).Value.listItem = new List<POItemTemp>();
                            }

                            listPOAlert.SingleOrDefault(x => x.Key == poItemTempKey).Value.listItem.Add(poItemTemp);
                        }
                    }

                    //}

                    row++;
                }

                foreach (var po in listPO)
                {
                    PO poValue = new PO();
                    var poNumberKey = po.Key.Split('-')[0].ToString();
                    var itemNumberKey = Convert.ToInt32(po.Key.Split('-')[1].ToString());

                    var poExist = db.POes.Where(x => x.Number == poNumberKey).SingleOrDefault();
                    var ID = 0;

                    if (poExist == null)
                    {
                        poValue.Number = po.Value.Number;
                        poValue.Type = string.IsNullOrWhiteSpace(po.Value.Type) ? null : po.Value.Type;
                        poValue.Date = po.Value.PODate;
                        poValue.ReleaseDate = po.Value.POReleaseDate;
                        poValue.VendorCode = string.IsNullOrWhiteSpace(po.Value.VendorCode) ? null : po.Value.VendorCode;
                        poValue.Information = string.Empty;
                        poValue.ProductGroup = string.Empty;
                        poValue.NumberPostedInvoice = string.Empty;
                        poValue.PurchaseOrderCreator = string.IsNullOrWhiteSpace(po.Value.PurchaseOrderCreator) ? "" : po.Value.PurchaseOrderCreator;
                        poValue.Status = string.Empty;
                        poValue.Reference = string.Empty;
                        //poValue.Created = DateTime.Now.Date;
                        poValue.Created = DateTime.Now;
                        poValue.CreatedBy = string.IsNullOrWhiteSpace(po.Value.POCreator) ? null : po.Value.POCreator;
                        //poValue.LastModified = DateTime.Now.Date;
                        poValue.LastModified = DateTime.Now;
                        poValue.LastModifiedBy = "SyncDataSAP";
                        db.POes.Add(poValue);
                        db.SaveChanges();
                        ID = poValue.ID;
                        Console.WriteLine("PO with ID : " + ID + " is added");
                    }
                    else
                    {
                        poExist.Type = string.IsNullOrWhiteSpace(po.Value.Type) ? null : po.Value.Type;
                        poExist.Date = po.Value.PODate;
                        poExist.ReleaseDate = po.Value.POReleaseDate;
                        poExist.VendorCode = string.IsNullOrWhiteSpace(po.Value.VendorCode) ? "" : po.Value.VendorCode;
                        poExist.Information = string.Empty;
                        poExist.ProductGroup = string.Empty;
                        poExist.NumberPostedInvoice = string.Empty;
                        poExist.PurchaseOrderCreator = string.IsNullOrWhiteSpace(po.Value.PurchaseOrderCreator) ? "" : po.Value.PurchaseOrderCreator;
                        poExist.Status = string.Empty;
                        poExist.Reference = string.Empty;
                        poExist.CreatedBy = string.IsNullOrWhiteSpace(po.Value.POCreator) ? null : po.Value.POCreator;
                        //poExist.LastModified = DateTime.Now.Date;
                        poExist.LastModified = DateTime.Now;
                        poExist.LastModifiedBy = "SyncDataSAP";
                        db.SaveChanges();

                        ID = poExist.ID;
                        Console.WriteLine("PO with ID " + ID + " is updated");

                    }

                    var itemExist = db.PurchasingDocumentItems.Where(x => x.POID == ID && x.ItemNumber == itemNumberKey).ToList();
                    var itemExistCount = itemExist.Count();
                    //int? itemExistParentID = itemExistCount > 0 ? itemExist.SingleOrDefault(x => x.ParentID == null && (string.IsNullOrEmpty(x.IsClosed) ? true : (x.IsClosed.Contains(costIsDelete) ? false : true))).ID : (int?)null;
                    int? itemExistParentID = itemExistCount > 0 ? itemExist.SingleOrDefault(x => x.ParentID == null).ID : (int?)null;
                    var isExistItemPOHistory = new List<PurchasingDocumentItemHistory>();

                    if (itemExistParentID != null)
                    {
                        isExistItemPOHistory = db.PurchasingDocumentItemHistories.Where(x => x.PurchasingDocumentItemID == itemExistParentID).ToList();
                    }

                    var itemClose = po.Value.listItem.Any(x => x.IsClose.Contains(costIsClose));

                    var flagItem = 0;
                    int? idItem = null;

                    foreach (var itemPO in po.Value.listItem.OrderBy(x => x.GRDate))
                    {
                        PurchasingDocumentItem poItem = new PurchasingDocumentItem();
                        PurchasingDocumentItemHistory poHistoryAdd = new PurchasingDocumentItemHistory();

                        if (itemExistCount > 0)
                        {
                            var flagItemExist = 0;
                            foreach (var itemExistVar in itemExist)
                            {
                                int itemQuantityTemp = itemExist[flagItemExist].Quantity;

                                itemExist[flagItemExist].ItemNumber = itemPO.ItemNumber;
                                itemExist[flagItemExist].Material = itemPO.Material;
                                //itemExist[flagItemExist].ProgressDay = itemPO.ProgressDay;
                                //itemExist[flagItemExist].MaterialVendor = itemPO.MaterialVendor;
                                itemExist[flagItemExist].Description = itemPO.Description;
                                itemExist[flagItemExist].NetPrice = itemPO.NetPrice;
                                itemExist[flagItemExist].Currency = itemPO.Currency;
                                itemExist[flagItemExist].Quantity = itemPO.Quantity;
                                //itemExist[flagItem].NetValue = 0;
                                //itemExist[flagItem].WorkTime = 0;
                                //itemExist[flagItemExist].ConfirmedQuantity = null;
                                //itemExist[flagItemExist].ConfirmedDate = null;
                                //itemExist[flagItemExist].ConfirmedItem = null;
                                itemExist[flagItemExist].IsClosed = itemPO.IsDelete + (itemClose ? costIsClose : itemPO.IsClose);
                                itemExist[flagItemExist].DeliveryDate = itemPO.DeliveryDate;
                                //itemExist[flagItem].LeadTimeItem = Convert.ToDecimal("0.00");
                                //itemExist[flagItem].ActiveStage = "2";
                                //itemExist[flagItemExist].LastModified = DateTime.Now.Date;
                                itemExist[flagItemExist].PRNumber = itemPO.PRNumber;
                                itemExist[flagItemExist].PRCreateDate = itemPO.PRCreateDate;
                                itemExist[flagItemExist].PRReleaseDate = itemPO.PRReleaseDate;
                                itemExist[flagItemExist].LastModified = DateTime.Now;
                                itemExist[flagItemExist].LastModifiedBy = "SyncDataSAP";

                                int itemID = itemExist[flagItemExist].ID > 0 ? itemExist[flagItemExist].ID : 0;
                                List<int> notificationIDs = db.PurchasingDocumentItems.Where(x => x.ParentID == itemID || x.ID == itemID).Select(x => x.ID).ToList();

                                #region deactivate Canceled or Closed Notification 
                                if (itemExist[flagItemExist].IsClosed != null && itemExist[flagItemExist].IsClosed != "")
                                {
                                    if (notificationIDs.Count > 0)
                                    {
                                        List<Notification> notificationsCloseds = db.Notifications.Where(x => notificationIDs.Contains(x.PurchasingDocumentItemID) && x.NotificationStatu.ID == 2).ToList();
                                        foreach (Notification notificationsClosed in notificationsCloseds)
                                        {
                                            notificationsClosed.isActive = false;
                                            notificationsClosed.Modified = DateTime.Now;
                                            notificationsClosed.ModifiedBy = "SyncSAP";
                                            Console.WriteLine("Notification with ID : " + itemID + "is updated");
                                        }
                                    }
                                }
                                #endregion

                                #region Deactivate Notification IsNeededUpdateFromSAP
                                if (itemQuantityTemp != itemPO.Quantity && itemExist[flagItemExist].Quantity == itemExist[flagItemExist].ConfirmedQuantity)
                                {
                                    if (notificationIDs.Count > 0)
                                    {
                                        List<Notification> notificationsIsNeededUpdateSAPs = db.Notifications.Where(x => notificationIDs.Contains(x.PurchasingDocumentItemID) && x.NotificationStatu.ID == 4).ToList();
                                        foreach (Notification notificationsIsNeededUpdateSAP in notificationsIsNeededUpdateSAPs)
                                        {
                                            notificationsIsNeededUpdateSAP.isActive = false;
                                            notificationsIsNeededUpdateSAP.Modified = DateTime.Now;
                                            notificationsIsNeededUpdateSAP.ModifiedBy = "SyncSAP";
                                            Console.WriteLine("Notification with ID : " + itemID + " (change quantity) is updated from SAP");
                                        }
                                    }
                                }
                                #endregion

                                db.SaveChanges();
                                idItem = itemExist[flagItemExist].ID;

                                Console.WriteLine("POItem with ID : " + itemExist[flagItemExist].ID + "is updated");

                                flagItemExist++;
                            }
                        }
                        else
                        {
                            if (flagItem == 0)
                            {
                                poItem.POID = ID;
                                poItem.ItemNumber = itemPO.ItemNumber;
                                poItem.Material = itemPO.Material;
                                //poItem.MaterialVendor = itemPO.MaterialVendor;
                                //poItem.ProgressDay = itemPO.ProgressDay;
                                poItem.Description = itemPO.Description;
                                poItem.NetPrice = itemPO.NetPrice;
                                poItem.Currency = itemPO.Currency;
                                poItem.Quantity = itemPO.Quantity;
                                poItem.NetValue = 0;
                                poItem.WorkTime = 0;
                                poItem.IsClosed = itemPO.IsDelete + (itemClose ? costIsClose : itemPO.IsClose);
                                poItem.LeadTimeItem = Convert.ToDecimal("0.00");
                                //poItem.ConfirmedDate = null;
                                //poItem.ConfirmedItem = null;
                                //poItem.ConfirmedQuantity = null;
                                poItem.ActiveStage = "0";
                                poItem.DeliveryDate = itemPO.DeliveryDate;
                                //poItem.Created = DateTime.Now.Date;
                                poItem.PRNumber = itemPO.PRNumber;
                                poItem.PRCreateDate = itemPO.PRCreateDate;
                                poItem.PRReleaseDate = itemPO.PRReleaseDate;
                                poItem.Created = DateTime.Now;
                                poItem.CreatedBy = "SyncDataSAP";
                                //poItem.LastModified = DateTime.Now.Date;
                                poItem.LastModified = DateTime.Now;
                                poItem.LastModifiedBy = "SyncDataSAP";
                                db.PurchasingDocumentItems.Add(poItem);
                                db.SaveChanges();
                                idItem = poItem.ID;
                                itemExistParentID = idItem;
                                Console.WriteLine("POItem with ID : " + idItem + "is added");
                            }

                        }
                        #region ga dipake
                        //if (itemExistCount > 0)
                        //{
                        //    if (itemExistCount > flagItem)
                        //    {
                        //        itemExist[flagItem].ItemNumber = itemPO.ItemNumber;
                        //        itemExist[flagItem].Material = itemPO.Material;
                        //        itemExist[flagItem].Description = "description";//itemPO.Description;
                        //        itemExist[flagItem].NetPrice = itemPO.NetPrice;
                        //        itemExist[flagItem].Currency = itemPO.Currency;
                        //        itemExist[flagItem].Quantity = itemPO.Quantity;
                        //        //itemExist[flagItem].NetValue = 0;
                        //        //itemExist[flagItem].WorkTime = 0;
                        //        itemExist[flagItem].ConfirmedQuantity = itemPO.GRQuantity;
                        //        itemExist[flagItem].ConfirmedDate = itemPO.GRDate;
                        //        itemExist[flagItem].IsClosed = itemPO.IsDelete + (itemClose ? costIsClose : itemPO.IsClose);
                        //        //itemExist[flagItem].Shipment_InboundNumber = !string.IsNullOrWhiteSpace(itemPO.InboundNumber) ? itemPO.InboundNumber : null;
                        //        itemExist[flagItem].DeliveryDate = itemPO.DeliveryDate;
                        //        //itemExist[flagItem].LeadTimeItem = Convert.ToDecimal("0.00");
                        //        itemExist[flagItem].ConfirmedDate = itemPO.GRDate == null ? itemPO.DeliveryDate : itemPO.GRDate;
                        //        //itemExist[flagItem].ActiveStage = "2";
                        //        itemExist[flagItem].LastModified = DateTime.Now.Date;
                        //        itemExist[flagItem].LastModifiedBy = "SyncDataSAP";
                        //        db.SaveChanges();
                        //        idItem = itemExist[flagItem].ID;

                        //        Console.WriteLine("POItem with ID : " + itemExist[flagItem].ID + "is updated");

                        //    }
                        //    else
                        //    {
                        //        poItem.POID = (int)itemExistPOID;
                        //        poItem.ParentID = itemExistParentID;
                        //        poItem.ItemNumber = itemPO.ItemNumber;
                        //        poItem.Material = itemPO.Material;
                        //        poItem.Description = "description";//itemPO.Description;
                        //        poItem.NetPrice = itemPO.NetPrice;
                        //        poItem.Currency = itemPO.Currency;
                        //        poItem.Quantity = itemPO.Quantity;
                        //        poItem.NetValue = 0;
                        //        poItem.WorkTime = 0;
                        //        poItem.IsClosed = itemPO.IsDelete + (itemClose ? costIsClose : itemPO.IsClose);
                        //        poItem.LeadTimeItem = Convert.ToDecimal("0.00");
                        //        poItem.ConfirmedDate = itemPO.GRDate == null ? itemPO.DeliveryDate : itemPO.GRDate;
                        //        poItem.ConfirmedItem = null;
                        //        poItem.ActiveStage = "0";
                        //        poItem.DeliveryDate = itemPO.DeliveryDate;
                        //        //poItem.Shipment_InboundNumber = !string.IsNullOrWhiteSpace(itemPO.InboundNumber) ? itemPO.InboundNumber : null;
                        //        poItem.Created = DateTime.Now.Date;
                        //        poItem.CreatedBy = "SyncDataSAP";
                        //        poItem.LastModified = DateTime.Now.Date;
                        //        poItem.LastModifiedBy = "SyncDataSAP";
                        //        db.PurchasingDocumentItems.Add(poItem);
                        //        db.SaveChanges();

                        //        idItem = poItem.ID;
                        //        Console.WriteLine("POItem with ID : " + idItem + "is added");

                        //    }

                        //    //if (!listPOItemHistory.ContainsKey((int)itemExistParentID))
                        //    //{
                        //    //    listPOItemHistory.Add((int)itemExistParentID, listPOItem.Where(x => x.PONumber == itemPO.PONumber && x.ItemNumber == itemPO.ItemNumber).ToList());
                        //    //}
                        //}
                        //else
                        //{
                        //    poItem.POID = ID;
                        //    if (flagItem > 0)
                        //    {
                        //        poItem.ParentID = itemExistParentID;
                        //    }
                        //    poItem.ItemNumber = itemPO.ItemNumber;
                        //    poItem.Material = itemPO.Material;
                        //    poItem.Description = "description";//itemPO.Description;
                        //    poItem.NetPrice = itemPO.NetPrice;
                        //    poItem.Currency = itemPO.Currency;
                        //    poItem.Quantity = itemPO.Quantity;
                        //    poItem.NetValue = 0;
                        //    poItem.WorkTime = 0;
                        //    poItem.IsClosed = itemPO.IsDelete + (itemClose ? costIsClose : itemPO.IsClose);
                        //    poItem.LeadTimeItem = Convert.ToDecimal("0.00");
                        //    poItem.ConfirmedDate = itemPO.GRDate == null ? itemPO.DeliveryDate : itemPO.GRDate;
                        //    poItem.ConfirmedItem = null;
                        //    poItem.ActiveStage = "0";
                        //    poItem.DeliveryDate = itemPO.DeliveryDate;
                        //    //poItem.Shipment_InboundNumber = !string.IsNullOrWhiteSpace(itemPO.InboundNumber) ? itemPO.InboundNumber : null;
                        //    poItem.Created = DateTime.Now.Date;
                        //    poItem.CreatedBy = "SyncDataSAP";
                        //    poItem.LastModified = DateTime.Now.Date;
                        //    poItem.LastModifiedBy = "SyncDataSAP";
                        //    db.PurchasingDocumentItems.Add(poItem);
                        //    db.SaveChanges();
                        //    idItem = poItem.ID;

                        //    Console.WriteLine("POItem with ID : " + idItem + "is added");


                        //}
                        //if (flagItem == 0)
                        //{
                        //    if (!listPOItemHistory.ContainsKey((int)idItem))
                        //    {
                        //        itemExistParentID = idItem;
                        //        listPOItemHistory.Add((int)idItem, listPOItem.Where(x => x.PONumber == itemPO.PONumber && x.ItemNumber == itemPO.ItemNumber).ToList());
                        //    }
                        //}
                        #endregion

                        if (flagItem == 0 && itemExistParentID == null)
                        {
                            itemExistParentID = idItem;
                        }


                        //if ((itemPO.MovementType != null && (POHistoryAccept.Contains(itemPO.POHistoryCategory))) || itemPO.POHistoryCategory == "T" || itemPO.POHistoryCategory == "T")
                        if ((itemPO.MovementType != null && itemPO.POHistoryCategory == "E") || itemPO.POHistoryCategory == "T" || itemPO.POHistoryCategory == "Q")
                        {
                            if (isExistItemPOHistory.Count() > flagItem)
                            {
                                isExistItemPOHistory[flagItem].PurchasingDocumentItemID = (int)itemExistParentID;
                                isExistItemPOHistory[flagItem].DeliveryDate = itemPO.DeliveryDate;
                                isExistItemPOHistory[flagItem].GoodsReceiptDate = itemPO.GRDate;
                                isExistItemPOHistory[flagItem].GoodsReceiptQuantity = itemPO.GRQuantity;
                                isExistItemPOHistory[flagItem].MovementType = itemPO.MovementType;
                                isExistItemPOHistory[flagItem].POHistoryCategory = !string.IsNullOrWhiteSpace(itemPO.POHistoryCategory) ? itemPO.POHistoryCategory : null;
                                isExistItemPOHistory[flagItem].DocumentNumber = !string.IsNullOrWhiteSpace(itemPO.DocumentNumber) ? itemPO.DocumentNumber : null;
                                isExistItemPOHistory[flagItem].PayTerms = !string.IsNullOrWhiteSpace(itemPO.PayTerm) ? itemPO.PayTerm : null;
                                isExistItemPOHistory[flagItem].Shipment_InboundNumber = !string.IsNullOrWhiteSpace(itemPO.InboundNumber) ? itemPO.InboundNumber : null;
                                //isExistItemPOHistory[flagItem].LastModified = DateTime.Now.Date;
                                isExistItemPOHistory[flagItem].LastModified = DateTime.Now;
                                isExistItemPOHistory[flagItem].LastModifiedBy = "SyncDataSAP";

                                Console.WriteLine("POHistory is updated");
                            }
                            else
                            {
                                poHistoryAdd.PurchasingDocumentItemID = (int)itemExistParentID;
                                poHistoryAdd.DeliveryDate = itemPO.DeliveryDate;
                                poHistoryAdd.GoodsReceiptDate = itemPO.GRDate;
                                poHistoryAdd.GoodsReceiptQuantity = itemPO.GRQuantity;
                                poHistoryAdd.MovementType = itemPO.MovementType;
                                poHistoryAdd.POHistoryCategory = !string.IsNullOrWhiteSpace(itemPO.POHistoryCategory) ? itemPO.POHistoryCategory : null;
                                poHistoryAdd.DocumentNumber = !string.IsNullOrWhiteSpace(itemPO.DocumentNumber) ? itemPO.DocumentNumber : null;
                                poHistoryAdd.PayTerms = !string.IsNullOrWhiteSpace(itemPO.PayTerm) ? itemPO.PayTerm : null;
                                poHistoryAdd.Shipment_InboundNumber = !string.IsNullOrWhiteSpace(itemPO.InboundNumber) ? itemPO.InboundNumber : null;
                                //poHistoryAdd.Created = DateTime.Now.Date;
                                poHistoryAdd.Created = DateTime.Now;
                                poHistoryAdd.CreatedBy = "SyncDataSAP";
                                //poHistoryAdd.LastModified = DateTime.Now.Date;
                                poHistoryAdd.LastModified = DateTime.Now;
                                poHistoryAdd.LastModifiedBy = "SyncDataSAP";

                                db.PurchasingDocumentItemHistories.Add(poHistoryAdd);

                                Console.WriteLine("POHistory is added");

                            }
                        }


                        flagItem++;
                    }
                    db.SaveChanges();
                    #region ga kepake
                    //foreach (var poHistory in listPOItemHistory)
                    //{
                    //    var isExistItemPOHistory = db.PurchasingDocumentItemHistories.Where(x => x.PurchasingDocumentItemID == poHistory.Key).ToList();
                    //    var itemPOHistoryFlag = 0;

                    //    foreach (var itemPOHistory in poHistory.Value)
                    //    {
                    //        PurchasingDocumentItemHistory poHistoryAdd = new PurchasingDocumentItemHistory();
                    //        if (isExistItemPOHistory.Count() > itemPOHistoryFlag)
                    //        {
                    //            isExistItemPOHistory[itemPOHistoryFlag].PurchasingDocumentItemID = poHistory.Key;
                    //            isExistItemPOHistory[itemPOHistoryFlag].DeliveryDate = itemPOHistory.DeliveryDate;
                    //            isExistItemPOHistory[itemPOHistoryFlag].GoodsReceiptDate = itemPOHistory.GRDate;
                    //            isExistItemPOHistory[itemPOHistoryFlag].GoodsReceiptQuantity = itemPOHistory.GRQuantity;
                    //            isExistItemPOHistory[itemPOHistoryFlag].MovementType = itemPOHistory.MovementType;
                    //            isExistItemPOHistory[itemPOHistoryFlag].POHistoryCategory = !string.IsNullOrWhiteSpace(itemPOHistory.POHistoryCategory) ? itemPOHistory.POHistoryCategory : null;
                    //            isExistItemPOHistory[itemPOHistoryFlag].DocumentNumber = !string.IsNullOrWhiteSpace(itemPOHistory.DocumentNumber) ? itemPOHistory.DocumentNumber : null;
                    //            isExistItemPOHistory[itemPOHistoryFlag].PayTerms = !string.IsNullOrWhiteSpace(itemPOHistory.PayTerm) ? itemPOHistory.PayTerm : null;
                    //            isExistItemPOHistory[itemPOHistoryFlag].Shipment_InboundNumber = !string.IsNullOrWhiteSpace(itemPOHistory.InboundNumber) ? itemPOHistory.InboundNumber : null;
                    //            isExistItemPOHistory[itemPOHistoryFlag].LastModified = DateTime.Now.Date;
                    //            isExistItemPOHistory[itemPOHistoryFlag].LastModifiedBy = "SyncDataSAP";

                    //            Console.WriteLine("POHistory is updated");
                    //        }
                    //        else
                    //        {
                    //            poHistoryAdd.PurchasingDocumentItemID = poHistory.Key;
                    //            poHistoryAdd.DeliveryDate = itemPOHistory.DeliveryDate;
                    //            poHistoryAdd.GoodsReceiptDate = itemPOHistory.GRDate;
                    //            poHistoryAdd.GoodsReceiptQuantity = itemPOHistory.GRQuantity;
                    //            poHistoryAdd.MovementType = itemPOHistory.MovementType;
                    //            poHistoryAdd.POHistoryCategory = !string.IsNullOrWhiteSpace(itemPOHistory.POHistoryCategory) ? itemPOHistory.POHistoryCategory : null;
                    //            poHistoryAdd.DocumentNumber = !string.IsNullOrWhiteSpace(itemPOHistory.DocumentNumber) ? itemPOHistory.DocumentNumber : null;
                    //            poHistoryAdd.PayTerms = !string.IsNullOrWhiteSpace(itemPOHistory.PayTerm) ? itemPOHistory.PayTerm : null;
                    //            poHistoryAdd.Shipment_InboundNumber = !string.IsNullOrWhiteSpace(itemPOHistory.InboundNumber) ? itemPOHistory.InboundNumber : null;
                    //            poHistoryAdd.Created = DateTime.Now.Date;
                    //            poHistoryAdd.CreatedBy = "SyncDataSAP";
                    //            poHistoryAdd.LastModified = DateTime.Now.Date;
                    //            poHistoryAdd.LastModifiedBy = "SyncDataSAP";

                    //            db.PurchasingDocumentItemHistories.Add(poHistoryAdd);

                    //            Console.WriteLine("POHistory is added");

                    //        }

                    //        itemPOHistoryFlag++;
                    //    }
                    //}
                }

                #endregion

                #region data excel
                //FileInfo fileInfo = new FileInfo("Olahan utk PO Tracking 10052019.xlsx");

                //Stream stream = File.Open(fileInfo.FullName, FileMode.Open);
                //SpreadsheetDocument spreadsheetDocument = SpreadsheetDocument.Open(stream, false);

                //WorkbookPart workbookPart = spreadsheetDocument.WorkbookPart;
                //WorksheetPart worksheetPart = workbookPart.WorksheetParts.ElementAt(0);

                //SharedStringTablePart stringTablePart = workbookPart.GetPartsOfType<SharedStringTablePart>().FirstOrDefault();
                //SharedStringItem[] arrayOfSharedStringItems = stringTablePart.SharedStringTable.Descendants<SharedStringItem>().ToArray();

                //OpenXmlReader reader = OpenXmlReader.Create(worksheetPart);

                //while (reader.Read())
                //{
                //    if (reader.ElementType == typeof(Row))
                //    {
                //        OpenXmlAttribute attribute = reader.Attributes.FirstOrDefault();
                //        if (attribute != null && attribute.Value == row.ToString())
                //        {
                //            Row rowElement = reader.LoadCurrentElement() as Row;
                //            bool tidakAdaData = rowElement.All(x => string.IsNullOrEmpty(x.InnerText));
                //            var cells = rowElement.Descendants<Cell>();
                //            Dictionary<string, Cell> indexedCells = new Dictionary<string, Cell>();
                //            foreach (Cell cell in cells)
                //            {
                //                indexedCells.Add(cell.CellReference.Value, cell);
                //            }


                //            if (!tidakAdaData)
                //            {
                //                #region
                //                string cellPONumber = "D" + row;
                //                string cellItemNumber = "E" + row;
                //                string cellPOReleaseDate = "H" + row;
                //                string cellPODate = "I" + row;
                //                string cellVendorCode = "M" + row;
                //                string cellPOCreator = "K" + row;
                //                string cellMaterial = "AK" + row;
                //                string cellPOType = "C" + row;
                //                string cellMaterialDesc = "AQ" + row;
                //                string cellNetPrice = "Y" + row;
                //                string cellCurrency = "Q" + row;
                //                string cellQuantity = "L" + row;
                //                string cellDocumentNumber = "S" + row;
                //                string cellDeliveryDate = "AR" + row;
                //                string cellGRDate = "W" + row;
                //                string cellGRQuantity = "X" + row;
                //                string cellMovementType = "V" + row;
                //                string cellIsComplete = "AA" + row;
                //                string cellIsDelete = "G" + row;
                //                string cellPOHistory = "U" + row;
                //                string cellInboundDelivery = "AO" + row;
                //                string cellInboundQuantity = "AP" + row;
                //                string cellInboundPayTerm = "O" + row;

                //                var PONumber = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellPONumber);
                //                var ItemNumber = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellItemNumber);
                //                var PODate = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellPODate);
                //                var POReleaseDate = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellPOReleaseDate);
                //                var VendorCode = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellVendorCode);
                //                var POCreator = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellPOCreator);
                //                var Material = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellMaterial);
                //                var POType = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellPOType);
                //                var MaterialDesc = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellMaterialDesc);
                //                var NetPrice = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellNetPrice);
                //                var Currency = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellCurrency);
                //                var Quantity = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellQuantity);
                //                var DeliveryDate = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellDeliveryDate);
                //                var GRDate = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellGRDate);
                //                var GRQuantity = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellGRQuantity);
                //                var MovementType = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellMovementType);
                //                var IsComplete = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellIsComplete);
                //                var IsDelete = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellIsDelete);
                //                var POHistory = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellPOHistory);
                //                var DocumentNumber = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellDocumentNumber);
                //                var InboundDelivery = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellInboundDelivery);
                //                var InboundQuantity = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellInboundQuantity);
                //                var PayTerm = ExcelFileHelper.GetCellValue(arrayOfSharedStringItems, indexedCells, cellInboundPayTerm);


                //                if (POHistoryAccept.Contains(POHistory))
                //                {

                //                    var poItemTempKey = PONumber + "-" + ItemNumber;
                //                    POTemp poTemp = new POTemp();
                //                    poTemp.Number = PONumber;
                //                    poTemp.Type = POType;
                //                    poTemp.PurchaseOrderCreator = POCreator;
                //                    if (!string.IsNullOrWhiteSpace(PODate))
                //                    {
                //                        poTemp.PODate = DateTime.FromOADate(long.Parse(PODate));
                //                    }
                //                    if (!string.IsNullOrWhiteSpace(POReleaseDate))
                //                    {
                //                        poTemp.POReleaseDate = DateTime.FromOADate(long.Parse(POReleaseDate));
                //                    }
                //                    poTemp.VendorCode = VendorCode;

                //                    if (!listPO.ContainsKey(poItemTempKey))
                //                    {
                //                        listPO.Add(poItemTempKey, poTemp);
                //                    }

                //                    POItemTemp poItemTemp = new POItemTemp();
                //                    poItemTemp.PONumber = PONumber;
                //                    if (!string.IsNullOrWhiteSpace(ItemNumber))
                //                    {
                //                        poItemTemp.ItemNumber = Convert.ToInt32(ItemNumber);
                //                    }
                //                    poItemTemp.Material = Material;

                //                    poItemTemp.Description = MaterialDesc;

                //                    if (!string.IsNullOrWhiteSpace(NetPrice))
                //                    {
                //                        poItemTemp.NetPrice = Convert.ToInt32(Math.Round(Convert.ToDouble(NetPrice)));
                //                    }
                //                    poItemTemp.Currency = Currency;

                //                    if (!string.IsNullOrWhiteSpace(Quantity))
                //                    {
                //                        poItemTemp.Quantity = Convert.ToInt32(Math.Floor(double.Parse(Quantity)));
                //                    }
                //                    if (!string.IsNullOrWhiteSpace(MovementType))
                //                    {
                //                        poItemTemp.MovementType = Convert.ToInt32(MovementType);
                //                    }
                //                    if ((!string.IsNullOrWhiteSpace(IsComplete) && IsComplete == costIsClose))
                //                    {
                //                        poItemTemp.IsClose = IsComplete.Trim();
                //                    }
                //                    else
                //                    {
                //                        poItemTemp.IsClose = string.Empty;
                //                    }
                //                    if ((!string.IsNullOrWhiteSpace(IsDelete) && IsDelete == costIsDelete))
                //                    {
                //                        poItemTemp.IsDelete = IsDelete.Trim();
                //                    }
                //                    else
                //                    {
                //                        poItemTemp.IsDelete = string.Empty;
                //                    }

                //                    if (!string.IsNullOrWhiteSpace(DeliveryDate))
                //                    {
                //                        poItemTemp.DeliveryDate = DateTime.FromOADate(long.Parse(DeliveryDate));
                //                    }

                //                    if (!string.IsNullOrWhiteSpace(GRQuantity))
                //                    {
                //                        poItemTemp.GRQuantity = Convert.ToInt32(Math.Floor(double.Parse(GRQuantity)));
                //                    }

                //                    if (!string.IsNullOrWhiteSpace(GRDate))
                //                    {
                //                        poItemTemp.GRDate = DateTime.FromOADate(long.Parse(GRDate));
                //                    }
                //                    poItemTemp.POHistoryCategory = POHistory;
                //                    poItemTemp.DocumentNumber = DocumentNumber;
                //                    poItemTemp.InboundNumber = InboundDelivery;
                //                    poItemTemp.PayTerm = PayTerm;

                //                    listPOItem.Add(poItemTemp);

                //                    if (listPO.SingleOrDefault(x => x.Key == poItemTempKey).Value.listItem == null)
                //                    {
                //                        listPO.SingleOrDefault(x => x.Key == poItemTempKey).Value.listItem = new List<POItemTemp>();
                //                    }

                //                    if (poItemTemp.MovementType != null)
                //                    {
                //                        if (MovementTypeAccept.Contains((int)poItemTemp.MovementType))
                //                        {                                          
                //                            listPO.SingleOrDefault(x => x.Key == poItemTempKey).Value.listItem.Add(poItemTemp);
                //                        }

                //                        if (MovementTypeAlert.Contains((int)poItemTemp.MovementType))
                //                        {
                //                            if (!listPOAlert.ContainsKey(poItemTempKey))
                //                            {
                //                                listPOAlert.Add(poItemTempKey, poTemp);
                //                            }

                //                            if (listPOAlert.SingleOrDefault(x => x.Key == poItemTempKey).Value.listItem == null)
                //                            {
                //                                listPOAlert.SingleOrDefault(x => x.Key == poItemTempKey).Value.listItem = new List<POItemTemp>();
                //                            }

                //                            listPOAlert.SingleOrDefault(x => x.Key == poItemTempKey).Value.listItem.Add(poItemTemp);
                //                        }
                //                    }
                //                }
                //                #endregion

                //            }
                //            row++;

                //        }
                //    }
                //}

                //Console.WriteLine("start sync po");
                //foreach (var po in listPO)
                //{
                //    PO poValue = new PO();
                //    var poNumberKey = po.Key.Split('-')[0].ToString();
                //    var itemNumberKey = Convert.ToInt32(po.Key.Split('-')[1].ToString());

                //    var poExist = db.POes.Where(x => x.Number == poNumberKey).SingleOrDefault();
                //    var ID = 0;

                //    if (poExist == null)
                //    {
                //        poValue.Number = po.Value.Number;
                //        poValue.Type = string.IsNullOrWhiteSpace(po.Value.Type) ? null : po.Value.Type;
                //        poValue.Date = po.Value.PODate;
                //        poValue.ReleaseDate = po.Value.POReleaseDate;
                //        poValue.VendorCode = string.IsNullOrWhiteSpace(po.Value.VendorCode) ? null : po.Value.VendorCode;
                //        poValue.Information = string.Empty;
                //        poValue.ProductGroup = string.Empty;
                //        poValue.NumberPostedInvoice = string.Empty;
                //        poValue.PurchaseOrderCreator = string.IsNullOrWhiteSpace(po.Value.PurchaseOrderCreator) ? null : po.Value.PurchaseOrderCreator;
                //        poValue.Status = string.Empty;
                //        poValue.Reference = string.Empty;
                //        poValue.Created = DateTime.Now.Date;
                //        poValue.CreatedBy = "SyncDataSAP";
                //        poValue.LastModified = DateTime.Now.Date;
                //        poValue.LastModifiedBy = "SyncDataSAP";
                //        db.POes.Add(poValue);
                //        db.SaveChanges();
                //        ID = poValue.ID;
                //        Console.WriteLine("PO with ID : " + ID + " is added");
                //    }
                //    else
                //    {
                //        poExist.Type = string.IsNullOrWhiteSpace(po.Value.Type) ? null : po.Value.Type;
                //        poExist.Date = po.Value.PODate;
                //        poExist.ReleaseDate = po.Value.POReleaseDate;
                //        poExist.VendorCode = string.IsNullOrWhiteSpace(po.Value.VendorCode) ? null : po.Value.VendorCode;
                //        poExist.Information = string.Empty;
                //        poExist.ProductGroup = string.Empty;
                //        poExist.NumberPostedInvoice = string.Empty;
                //        poExist.PurchaseOrderCreator = string.IsNullOrWhiteSpace(po.Value.PurchaseOrderCreator) ? null : po.Value.PurchaseOrderCreator;
                //        poExist.Status = string.Empty;
                //        poExist.Reference = string.Empty;
                //        poExist.Created = DateTime.Now.Date;
                //        poExist.CreatedBy = "SyncDataSAP";
                //        poExist.LastModified = DateTime.Now.Date;
                //        poExist.LastModifiedBy = "SyncDataSAP";
                //        db.SaveChanges();

                //        ID = poExist.ID;
                //        Console.WriteLine("PO with ID " + ID + " is updated");

                //    }

                //    var itemExist = db.PurchasingDocumentItems.Where(x => x.POID == ID && x.ItemNumber == itemNumberKey).ToList();
                //    var itemExistCount = itemExist.Count();

                //    int? itemExistParentID = itemExistCount > 0 ? itemExist.SingleOrDefault(x => x.ParentID == null && (string.IsNullOrEmpty(x.IsClosed) ? true : (x.IsClosed.Contains(costIsDelete) ? false : true))).ID : (int?)null;
                //    int? itemExistPOID = itemExistCount > 0 ? itemExist.FirstOrDefault().POID : (int?)null;
                //    var itemClose = po.Value.listItem.Any(x => x.IsClose.Contains(costIsClose));

                //    var flagItem = 0;
                //    int? idItem = null;

                //    foreach (var itemPO in po.Value.listItem)
                //    {
                //        PurchasingDocumentItem poItem = new PurchasingDocumentItem();

                //        if (itemExistCount > 0)
                //        {
                //            if (itemExistCount > flagItem)
                //            {
                //                itemExist[flagItem].ItemNumber = itemPO.ItemNumber;
                //                itemExist[flagItem].Material = itemPO.Material;
                //                itemExist[flagItem].Description = "description";//itemPO.Description;
                //                itemExist[flagItem].NetPrice = itemPO.NetPrice;
                //                itemExist[flagItem].Currency = itemPO.Currency;
                //                itemExist[flagItem].Quantity = itemPO.Quantity;
                //                //itemExist[flagItem].NetValue = 0;
                //                //itemExist[flagItem].WorkTime = 0;
                //                itemExist[flagItem].ConfirmedQuantity = itemPO.GRQuantity;
                //                itemExist[flagItem].ConfirmedDate = itemPO.GRDate;
                //                itemExist[flagItem].IsClosed = itemPO.IsDelete + (itemClose ? costIsClose : itemPO.IsClose);
                //                itemExist[flagItem].DeliveryDate = itemPO.DeliveryDate;
                //                //itemExist[flagItem].Shipment_InboundNumber = !string.IsNullOrWhiteSpace(itemPO.InboundNumber) ? itemPO.InboundNumber : null;
                //                //itemExist[flagItem].LeadTimeItem = Convert.ToDecimal("0.00");
                //                itemExist[flagItem].ConfirmedDate = itemPO.GRDate == null ? itemPO.DeliveryDate : itemPO.GRDate;
                //                itemExist[flagItem].ConfirmedItem = null;
                //                itemExist[flagItem].ActiveStage = "2";
                //                itemExist[flagItem].LastModified = DateTime.Now.Date;
                //                itemExist[flagItem].LastModifiedBy = "SyncDataSAP";
                //                db.SaveChanges();
                //                idItem = itemExist[flagItem].ID;

                //                Console.WriteLine("POItem with ID : " + itemExist[flagItem].ID + "is updated");

                //            }
                //            else
                //            {
                //                poItem.POID = (int)itemExistPOID;
                //                poItem.ParentID = itemExistParentID;
                //                poItem.ItemNumber = itemPO.ItemNumber;
                //                poItem.Material = itemPO.Material;
                //                poItem.Description = "description";//itemPO.Description;
                //                poItem.NetPrice = itemPO.NetPrice;
                //                poItem.Currency = itemPO.Currency;
                //                poItem.Quantity = itemPO.Quantity;
                //                poItem.NetValue = 0;
                //                poItem.WorkTime = 0;
                //                poItem.IsClosed = itemPO.IsDelete + (itemClose ? costIsClose : itemPO.IsClose);
                //                poItem.LeadTimeItem = Convert.ToDecimal("0.00");
                //                poItem.ConfirmedDate = itemPO.GRDate == null ? itemPO.DeliveryDate : itemPO.GRDate;
                //                poItem.ConfirmedItem = null;
                //                poItem.ActiveStage = "0";
                //                poItem.DeliveryDate = itemPO.DeliveryDate;
                //                //poItem.Shipment_InboundNumber = !string.IsNullOrWhiteSpace(itemPO.InboundNumber) ? itemPO.InboundNumber : null;
                //                poItem.Created = DateTime.Now.Date;
                //                poItem.CreatedBy = "SyncDataSAP";
                //                poItem.LastModified = DateTime.Now.Date;
                //                poItem.LastModifiedBy = "SyncDataSAP";
                //                db.PurchasingDocumentItems.Add(poItem);
                //                db.SaveChanges();

                //                idItem = poItem.ID;
                //                Console.WriteLine("POItem with ID : " + idItem + "is added");

                //            }

                //            //if (!listPOItemHistory.ContainsKey((int)itemExistParentID))
                //            //{
                //            //    listPOItemHistory.Add((int)itemExistParentID, listPOItem.Where(x => x.PONumber == itemPO.PONumber && x.ItemNumber == itemPO.ItemNumber).ToList());
                //            //}
                //        }
                //        else
                //        {
                //            poItem.POID = ID;
                //            if (flagItem > 0)
                //            {
                //                poItem.ParentID = itemExistParentID;
                //            }
                //            poItem.ItemNumber = itemPO.ItemNumber;
                //            poItem.Material = itemPO.Material;
                //            poItem.Description = "description";//itemPO.Description;
                //            poItem.NetPrice = itemPO.NetPrice;
                //            poItem.Currency = itemPO.Currency;
                //            poItem.Quantity = itemPO.Quantity;
                //            poItem.NetValue = 0;
                //            poItem.WorkTime = 0;
                //            poItem.IsClosed = itemPO.IsDelete + (itemClose ? costIsClose : itemPO.IsClose);
                //            poItem.LeadTimeItem = Convert.ToDecimal("0.00");
                //            poItem.ConfirmedDate = itemPO.GRDate == null ? itemPO.DeliveryDate : itemPO.GRDate;
                //            poItem.ConfirmedItem = null;
                //            poItem.ActiveStage = null;
                //            poItem.DeliveryDate = itemPO.DeliveryDate;
                //            //poItem.Shipment_InboundNumber = !string.IsNullOrWhiteSpace(itemPO.InboundNumber) ? itemPO.InboundNumber : null;
                //            poItem.Created = DateTime.Now.Date;
                //            poItem.CreatedBy = "SyncDataSAP";
                //            poItem.LastModified = DateTime.Now.Date;
                //            poItem.LastModifiedBy = "SyncDataSAP";
                //            db.PurchasingDocumentItems.Add(poItem);
                //            db.SaveChanges();
                //            idItem = poItem.ID;

                //            Console.WriteLine("POItem with ID : " + idItem + "is added");


                //        }
                //        if (flagItem == 0)
                //        {
                //            if (!listPOItemHistory.ContainsKey((int)idItem))
                //            {
                //                itemExistParentID = idItem;
                //                listPOItemHistory.Add((int)idItem, listPOItem.Where(x => x.PONumber == itemPO.PONumber && x.ItemNumber == itemPO.ItemNumber).ToList());
                //            }
                //        }

                //        flagItem++;
                //    }
                //    foreach (var poHistory in listPOItemHistory)
                //    {
                //        var isExistItemPOHistory = db.PurchasingDocumentItemHistories.Where(x => x.PurchasingDocumentItemID == poHistory.Key).ToList();
                //        var itemPOHistoryFlag = 0;

                //        foreach (var itemPOHistory in poHistory.Value)
                //        {
                //            PurchasingDocumentItemHistory poHistoryAdd = new PurchasingDocumentItemHistory();
                //            if (isExistItemPOHistory.Count() > itemPOHistoryFlag)
                //            {
                //                isExistItemPOHistory[itemPOHistoryFlag].PurchasingDocumentItemID = poHistory.Key;
                //                isExistItemPOHistory[itemPOHistoryFlag].DeliveryDate = itemPOHistory.DeliveryDate.Date;
                //                isExistItemPOHistory[itemPOHistoryFlag].GoodsReceiptDate = itemPOHistory.GRDate.Date;
                //                isExistItemPOHistory[itemPOHistoryFlag].GoodsReceiptQuantity = itemPOHistory.GRQuantity;
                //                isExistItemPOHistory[itemPOHistoryFlag].MovementType = itemPOHistory.MovementType;
                //                isExistItemPOHistory[itemPOHistoryFlag].POHistoryCategory = !string.IsNullOrWhiteSpace(itemPOHistory.POHistoryCategory) ? itemPOHistory.POHistoryCategory : null;
                //                isExistItemPOHistory[itemPOHistoryFlag].DocumentNumber = !string.IsNullOrWhiteSpace(itemPOHistory.DocumentNumber) ? itemPOHistory.DocumentNumber : null;
                //                isExistItemPOHistory[itemPOHistoryFlag].PayTerms = !string.IsNullOrWhiteSpace(itemPOHistory.PayTerm) ? itemPOHistory.PayTerm : null;
                //                isExistItemPOHistory[itemPOHistoryFlag].Shipment_InboundNumber = !string.IsNullOrWhiteSpace(itemPOHistory.InboundNumber) ? itemPOHistory.InboundNumber : null;
                //                isExistItemPOHistory[itemPOHistoryFlag].LastModified = DateTime.Now.Date;
                //                isExistItemPOHistory[itemPOHistoryFlag].LastModifiedBy = "SyncDataSAP";

                //                Console.WriteLine("POHistory is updated");
                //            }
                //            else
                //            {
                //                poHistoryAdd.PurchasingDocumentItemID = poHistory.Key;
                //                poHistoryAdd.DeliveryDate = itemPOHistory.DeliveryDate.Date;
                //                poHistoryAdd.GoodsReceiptDate = itemPOHistory.GRDate.Date;
                //                poHistoryAdd.GoodsReceiptQuantity = itemPOHistory.GRQuantity;
                //                poHistoryAdd.MovementType = itemPOHistory.MovementType;
                //                poHistoryAdd.POHistoryCategory = !string.IsNullOrWhiteSpace(itemPOHistory.POHistoryCategory) ? itemPOHistory.POHistoryCategory : null;
                //                poHistoryAdd.DocumentNumber = !string.IsNullOrWhiteSpace(itemPOHistory.DocumentNumber) ? itemPOHistory.DocumentNumber : null;
                //                poHistoryAdd.PayTerms = !string.IsNullOrWhiteSpace(itemPOHistory.PayTerm) ? itemPOHistory.PayTerm : null;
                //                poHistoryAdd.Shipment_InboundNumber = !string.IsNullOrWhiteSpace(itemPOHistory.InboundNumber) ? itemPOHistory.InboundNumber : null;
                //                poHistoryAdd.Created = DateTime.Now.Date;
                //                poHistoryAdd.CreatedBy = "SyncDataSAP";
                //                poHistoryAdd.LastModified = DateTime.Now.Date;
                //                poHistoryAdd.LastModifiedBy = "SyncDataSAP";

                //                db.PurchasingDocumentItemHistories.Add(poHistoryAdd);

                //                Console.WriteLine("POHistory is added");

                //            }

                //            itemPOHistoryFlag++;
                //        }
                //    }
                //}
                //db.SaveChanges();
                //Console.WriteLine("end sync po");

                #endregion

                Console.WriteLine("start sync po alert");
                if (listPOAlert.Count() > 0)
                {
                    IssueHeader issueHeader = new IssueHeader();

                    issueHeader.MasterIssueID = 8;
                    issueHeader.RaisedBy = "SyncDataSAP";
                    issueHeader.DateOfIssue = DateTime.Now.Date;
                    issueHeader.IssueDescription = dbAlert.MasterIssues.SingleOrDefault(x => x.ID == 8).Name;
                    issueHeader.Created = DateTime.Now.Date;
                    issueHeader.CreatedBy = "SyncDataSAP";
                    issueHeader.LastModified = DateTime.Now.Date;
                    issueHeader.LastModifiedBy = "SyncDataSAP";
                    dbAlert.IssueHeaders.Add(issueHeader);
                    dbAlert.SaveChanges();

                    foreach (var poAlert in listPOAlert)
                    {
                        QualityReceivePOTracking qrPOTracking = new QualityReceivePOTracking();
                        qrPOTracking.IssueHeaderID = issueHeader.ID;
                        qrPOTracking.PONumber = poAlert.Value.listItem[0].PONumber;
                        qrPOTracking.PartNumber = poAlert.Value.listItem[0].Material.ToString();
                        qrPOTracking.MaterialName = poAlert.Value.listItem[0].Description.ToString();
                        qrPOTracking.MovementType = poAlert.Value.listItem[0].MovementType;
                        qrPOTracking.Quantity = poAlert.Value.listItem.Count();
                        qrPOTracking.PRNumber = poAlert.Value.listItem[0].PRNumber.ToString();
                        qrPOTracking.Created = DateTime.Now.Date;
                        qrPOTracking.CreatedBy = "SyncDataSAP";
                        qrPOTracking.LastModified = DateTime.Now.Date;
                        qrPOTracking.LastModifiedBy = "SyncDataSAP";
                        dbAlert.QualityReceivePOTrackings.Add(qrPOTracking);
                    }
                    dbAlert.SaveChanges();

                }


                Console.WriteLine("end sync po alert");

                Console.WriteLine("finish");
                //Console.Read();

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message + " \n " + ex.StackTrace);
                Console.Read();
            }
        }
    }

    public static class ExcelFileHelper
    {
        public static string GetCellValue(SharedStringItem[] arrayOfSharedStringItems, Cell cell)
        {
            if (cell.DataType != null && cell.DataType == CellValues.SharedString)
            {
                int sharedStringTableIndex = -1;
                if (int.TryParse(cell.CellValue.InnerText, out sharedStringTableIndex))
                {
                    return arrayOfSharedStringItems[sharedStringTableIndex].InnerText;
                }
                else
                {
                    return string.Empty;
                }
            }
            else
            {
                return cell.CellValue.InnerText;
            }
        }
        public static string GetCellValue(SharedStringItem[] arrayOfSharedStringItems, Dictionary<string, Cell> cells, string cellAddress)
        {
            if (cells.ContainsKey(cellAddress))
            {
                Cell cell = cells[cellAddress];
                if (cell.DataType != null && cell.DataType == CellValues.SharedString)
                {
                    int sharedStringTableIndex = -1;
                    if (int.TryParse(cell.CellValue.InnerText, out sharedStringTableIndex))
                    {
                        return arrayOfSharedStringItems[sharedStringTableIndex].InnerText;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
                else
                {
                    if (cell.CellValue != null)
                    {
                        return cell.CellValue.InnerText;
                    }
                    else
                    {
                        return string.Empty;
                    }
                }
            }
            else
            {
                return string.Empty;
            }
        }

    }

    public class POTemp
    {
        public string Number { get; set; }
        public string Type { get; set; }
        public DateTime PODate { get; set; }
        public DateTime? POReleaseDate { get; set; }
        public string VendorCode { get; set; }
        public string PurchaseOrderCreator { get; set; }
        public string POCreator { get; set; }
        public List<POItemTemp> listItem { get; set; }
    }

    public class POItemTemp
    {
        public string PONumber { get; set; }
        public int ItemNumber { get; set; }
        public string Material { get; set; }
        public string MaterialVendor { get; set; }

        public string Description { get; set; }
        public decimal NetPrice { get; set; }
        public string Currency { get; set; }
        public int Quantity { get; set; }
        public int? MovementType { get; set; }
        public string IsClose { get; set; }
        public string IsDelete { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? GRDate { get; set; }
        public int GRQuantity { get; set; }
        public string POHistoryCategory { get; set; }
        public string DocumentNumber { get; set; }
        public string InboundNumber { get; set; }
        public string PayTerm { get; set; }
        public string PRNumber { get; set; }
        public DateTime? PRCreateDate { get; set; }
        public DateTime? PRReleaseDate { get; set; }
        public int ProgressDay { get; set; }



    }

    public class CSVModel
    {
        public string Number { get; set; }
        public string Type { get; set; }
        public DateTime PODate { get; set; }
        public DateTime? POReleaseDate { get; set; }
        public string VendorCode { get; set; }
        public string PurchaseOrderCreator { get; set; }
        public string POCreator { get; set; }
        public int ItemNumber { get; set; }
        public string Material { get; set; }
        public string MaterialVendor { get; set; }

        public string Description { get; set; }
        public decimal NetPrice { get; set; }
        public string Currency { get; set; }
        public int Quantity { get; set; }
        public int? MovementType { get; set; }
        public string IsClose { get; set; }
        public string IsDelete { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public DateTime? GRDate { get; set; }
        public int GRQuantity { get; set; }
        public string POHistoryCategory { get; set; }
        public string DocumentNumber { get; set; }
        public string InboundNumber { get; set; }
        public string PayTerm { get; set; }
        public string PRNumber { get; set; }
        public DateTime? PRCreateDate { get; set; }
        public DateTime? PRReleaseDate { get; set; }
        public int ProgressDay { get; set; }


        public static CSVModel FromCSV(string csv)
        {
            string[] val = csv.Split(';');
            CultureInfo culture = new CultureInfo("en-US");
            CSVModel csvModel = new CSVModel();

            csvModel.Number = val[1].ToString().TrimStart(new Char[] { '0' });
            csvModel.Type = val[0].ToString();
            csvModel.POReleaseDate = val[43].ToString() == "00.00.0000" ? (DateTime?)null : DateTime.ParseExact(val[43].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            csvModel.PODate = DateTime.ParseExact(val[5].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            csvModel.PRReleaseDate = DateTime.ParseExact(val[29].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            csvModel.PRCreateDate = DateTime.ParseExact(val[46].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            csvModel.VendorCode = val[9].ToString().TrimStart(new Char[] { '0' });
            csvModel.POCreator = val[6].ToString().TrimStart(new Char[] { '0' });
            csvModel.PurchaseOrderCreator = string.IsNullOrWhiteSpace(val[7].ToString()) ? string.Empty : val[7].ToString();
            csvModel.ItemNumber = Convert.ToInt32(val[2].ToString().TrimStart(new Char[] { '0' }));
            csvModel.Material = val[33].ToString();
            csvModel.MaterialVendor = string.IsNullOrWhiteSpace(val[45].ToString()) ? string.Empty : val[45].ToString();
            csvModel.Description = val[39].ToString();
            //csvModel.NetPrice = Convert.ToInt32(val[21].ToString().Split(',')[0].Trim().Replace(".", ""));
            //csvModel.NetPrice = Convert.ToInt32(val[44].ToString().Split(',')[0].Trim().Replace(".", ""));
            csvModel.NetPrice = Convert.ToDecimal(val[44].ToString().Trim().Replace(".", "").Replace(",","."), culture);
            csvModel.Currency = val[13].ToString();
            csvModel.Quantity = Convert.ToInt32(val[8].ToString().Split(',')[0].Trim().Replace(".", ""));
            csvModel.MovementType = string.IsNullOrWhiteSpace(val[18].ToString()) ? (int?)null : Convert.ToInt32(val[18].ToString().TrimStart(new Char[] { '0' }));
            csvModel.IsDelete = !string.IsNullOrWhiteSpace(val[3].ToString()) && val[3].ToString() == "L" ? val[3].ToString() : string.Empty;
            csvModel.IsClose = !string.IsNullOrWhiteSpace(val[23].ToString()) && val[23].ToString() == "X" ? val[23].ToString() : string.Empty;
            csvModel.DeliveryDate = val[40].ToString() == "00.00.0000" ? (DateTime?)null : DateTime.ParseExact(val[40].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            csvModel.GRDate = val[19].ToString() == "00.00.0000" ? (DateTime?)null : DateTime.ParseExact(val[19].ToString(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
            csvModel.GRQuantity = Convert.ToInt32(val[20].ToString().Split(',')[0].Trim().Replace(".", ""));
            csvModel.POHistoryCategory = val[17].ToString();
            csvModel.DocumentNumber = val[15].ToString().TrimStart(new Char[] { '0' });
            csvModel.InboundNumber = val[37].ToString();
            csvModel.PayTerm = val[11].ToString();
            csvModel.PRNumber = string.IsNullOrWhiteSpace(val[27].ToString()) ? "0" : val[27].ToString();
            csvModel.ProgressDay = string.IsNullOrWhiteSpace(val[26].ToString()) ? 0 : Convert.ToInt32(val[26].ToString());
            return csvModel;
        }
    }

}
