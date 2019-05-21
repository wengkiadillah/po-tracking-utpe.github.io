using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace POTrackingV2.Models
{
    public partial class PurchasingDocumentItem
    {
        POTrackingEntities db = new POTrackingEntities();
        public DateTime LatestDeliveryDate
        {
            get
            {
                return this.PurchasingDocumentItemHistories.OrderByDescending(x => x.Created).Select(x => x.DeliveryDate.GetValueOrDefault()).FirstOrDefault();
            }
        }

        public int TotalGR
        {
            get
            {
                //var duration = Level3Data.AsQueryable().Any(d => d.DurationMonths.HasValue)
                //? Level3Data.AsQueryable().Sum(d => d.DurationMonths)
                //: null;
                //return this.PurchasingDocumentItemHistories.Sum(x => x.GoodsReceiptQuantity ?? 0);
                if (this.ParentID == null)
                {
                    return this.PurchasingDocumentItemHistories.Where(po => po.PurchasingDocumentItem.PurchasingDocumentItemHistories.Any(pdih => pdih.MovementType == 101 || pdih.MovementType == 105)).Sum(x => x.GoodsReceiptQuantity ?? 0);
                }
                else
                {
                    return db.PurchasingDocumentItemHistories.Where(po => po.PurchasingDocumentItem.PurchasingDocumentItemHistories.Any(pdih => (pdih.MovementType == 101 || pdih.MovementType == 105) && pdih.PurchasingDocumentItemID == this.ParentID)).Sum(x => x.GoodsReceiptQuantity ?? 0);
                }
            }
        }

        public PurchasingDocumentItemHistory LatestPurchasingDocumentItemHistories
        {
            get
            {
                List<PurchasingDocumentItem> purchasingDocumentItems = new List<PurchasingDocumentItem>();
                //List<PurchasingDocumentItemHistory> purchasingDocumentItemHistories = this.PurchasingDocumentItemHistories.Where(po => po.PurchasingDocumentItem.PurchasingDocumentItemHistories.Any(pdih => pdih.MovementType == 101 || pdih.MovementType == 105)).ToList();
                List<PurchasingDocumentItemHistory> purchasingDocumentItemHistories = new List<PurchasingDocumentItemHistory>();

                PurchasingDocumentItemHistory purchasingDocumentItemHistory = new PurchasingDocumentItemHistory();

                if (this.ParentID == null)
                {
                    purchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ID == this.ID || x.ParentID == this.ID).OrderBy(x => x.ConfirmedDate).ToList();
                    purchasingDocumentItemHistories = db.PurchasingDocumentItemHistories.Where(pdih => (pdih.MovementType == 101 || pdih.MovementType == 105) && pdih.PurchasingDocumentItemID == this.ID).ToList();
                }
                else
                {
                    purchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ID == this.ParentID || x.ParentID == this.ParentID).OrderBy(x => x.ConfirmedDate).ToList();
                    purchasingDocumentItemHistories = db.PurchasingDocumentItemHistories.Where(pdih => (pdih.MovementType == 101 || pdih.MovementType == 105) && pdih.PurchasingDocumentItemID == this.ParentID).ToList();
                }

                if (purchasingDocumentItems.Count > 0 && purchasingDocumentItemHistories.Count > 0 && this.TotalGR > 0 && this.ConfirmedQuantity > 0)
                {
                    int otherConfirmedQty = 0;
                    int currentGRQty = 0;
                    DateTime? currentGRDate = null;
                    string documentNumber = null;

                    foreach (PurchasingDocumentItem purchasingDocumentItem in purchasingDocumentItems)
                    {
                        if (this.ID == purchasingDocumentItem.ID)
                        {
                            if (otherConfirmedQty > 0)
                            {
                                foreach (var pdih in purchasingDocumentItemHistories)
                                {
                                    int grQty = pdih.GoodsReceiptQuantity.HasValue ? pdih.GoodsReceiptQuantity.Value : 0;
                                    if (otherConfirmedQty > 0)
                                    {
                                        otherConfirmedQty -= pdih.GoodsReceiptQuantity.HasValue ? pdih.GoodsReceiptQuantity.Value : 0;
                                        if (otherConfirmedQty < 0)
                                        {
                                            currentGRDate = pdih.GoodsReceiptDate;
                                            documentNumber = pdih.DocumentNumber;
                                            if (Math.Abs(otherConfirmedQty) < this.ConfirmedQuantity)
                                            {
                                                currentGRQty += Math.Abs(otherConfirmedQty);
                                            }
                                            else
                                            {
                                                currentGRQty = this.ConfirmedQuantity.HasValue ? this.ConfirmedQuantity.Value : 0;
                                            }

                                        }
                                    }
                                    else
                                    {
                                        currentGRDate = pdih.GoodsReceiptDate;
                                        documentNumber = pdih.DocumentNumber;
                                        if ((currentGRQty + grQty) < this.ConfirmedQuantity)
                                        {
                                            currentGRQty += grQty;
                                        }
                                        else
                                        {
                                            currentGRQty = this.ConfirmedQuantity.HasValue ? this.ConfirmedQuantity.Value : 0;
                                        }
                                    }
                                }

                                purchasingDocumentItemHistory.DocumentNumber = documentNumber;
                                purchasingDocumentItemHistory.GoodsReceiptDate = currentGRDate;
                                purchasingDocumentItemHistory.GoodsReceiptQuantity = currentGRQty;
                            }
                            else
                            {
                                foreach (var pdih in purchasingDocumentItemHistories)
                                {
                                    int grQty = pdih.GoodsReceiptQuantity.HasValue ? pdih.GoodsReceiptQuantity.Value : 0;
                                    currentGRDate = pdih.GoodsReceiptDate;
                                    documentNumber = pdih.DocumentNumber;
                                    if ((currentGRQty + grQty) < this.ConfirmedQuantity)
                                    {
                                        currentGRQty += grQty;
                                    }
                                    else
                                    {
                                        currentGRQty = this.ConfirmedQuantity.HasValue ? this.ConfirmedQuantity.Value : 0;
                                    }
                                }

                                purchasingDocumentItemHistory.DocumentNumber = documentNumber;
                                purchasingDocumentItemHistory.GoodsReceiptDate = currentGRDate;
                                purchasingDocumentItemHistory.GoodsReceiptQuantity = currentGRQty;
                            }
                        }
                        else
                        {
                            otherConfirmedQty += purchasingDocumentItem.ConfirmedQuantity.HasValue ? purchasingDocumentItem.ConfirmedQuantity.Value : 0;
                        }
                    }
                    return purchasingDocumentItemHistory;
                }
                else
                {
                    return purchasingDocumentItemHistory;
                }
            }
        }

        public PurchasingDocumentItemHistory LatestDocumentInvoices
        {
            get
            {
                List<PurchasingDocumentItem> purchasingDocumentItems = new List<PurchasingDocumentItem>();
                //List<PurchasingDocumentItemHistory> purchasingDocumentItemHistories = this.PurchasingDocumentItemHistories.Where(po => po.PurchasingDocumentItem.PurchasingDocumentItemHistories.Any(pdih => pdih.MovementType == 101 || pdih.MovementType == 105)).ToList();
                List<PurchasingDocumentItemHistory> purchasingDocumentItemHistories = new List<PurchasingDocumentItemHistory>();

                PurchasingDocumentItemHistory purchasingDocumentItemHistory = new PurchasingDocumentItemHistory();

                if (this.ParentID == null)
                {
                    purchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ID == this.ID || x.ParentID == this.ID).OrderBy(x => x.ConfirmedDate).ToList();
                    purchasingDocumentItemHistories = db.PurchasingDocumentItemHistories.Where(pdih => (pdih.POHistoryCategory.ToLower() == "q") && pdih.PurchasingDocumentItemID == this.ID).ToList();
                }
                else
                {
                    purchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ID == this.ParentID || x.ParentID == this.ParentID).OrderBy(x => x.ConfirmedDate).ToList();
                    purchasingDocumentItemHistories = db.PurchasingDocumentItemHistories.Where(pdih => (pdih.POHistoryCategory.ToLower() == "q") && pdih.PurchasingDocumentItemID == this.ParentID).ToList();
                }

                if (purchasingDocumentItems.Count > 0 && purchasingDocumentItemHistories.Count > 0 && this.TotalGR > 0 && this.ConfirmedQuantity > 0)
                {
                    DateTime? currentGRDate = null;
                    string documentNumber = null;
                    string payTerms = null;
                    int indexParent = 0;

                    foreach (PurchasingDocumentItem purchasingDocumentItem in purchasingDocumentItems)
                    {
                        if (this.ID == purchasingDocumentItem.ID)
                        {
                            int indexChild = 0;
                            foreach (var pdih in purchasingDocumentItemHistories)
                            {
                                if (indexParent == indexChild)
                                {
                                    currentGRDate = pdih.GoodsReceiptDate;
                                    payTerms = pdih.PayTerms;
                                }
                                indexChild++;
                            }
                            purchasingDocumentItemHistory.DocumentNumber = documentNumber;
                            purchasingDocumentItemHistory.PayTerms = payTerms;
                            purchasingDocumentItemHistory.GoodsReceiptDate = currentGRDate;
                        }
                        indexParent++;
                    }
                    return purchasingDocumentItemHistory;
                }
                else
                {
                    return purchasingDocumentItemHistory;
                }
            }
        }

        public List<PurchasingDocumentItemHistory> PurchasingDocumentItemHistoriesDetail
        {
            get
            {
                var confirmQty = this.ConfirmedQuantity;
                List<PurchasingDocumentItem> purchasingDocumentItems = new List<PurchasingDocumentItem>();
                //List<PurchasingDocumentItemHistory> purchasingDocumentItemHistories = this.PurchasingDocumentItemHistories.Where(po => po.PurchasingDocumentItem.PurchasingDocumentItemHistories.Any(pdih => pdih.MovementType == 101 || pdih.MovementType == 105)).ToList();
                List<PurchasingDocumentItemHistory> purchasingDocumentItemHistories = new List<PurchasingDocumentItemHistory>();

                List<PurchasingDocumentItemHistory> listPurchasingDocumentItemHistory = new List<PurchasingDocumentItemHistory>();

                if (this.ParentID == null)
                {
                    purchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ID == this.ID || x.ParentID == this.ID).OrderBy(x => x.ConfirmedDate).ToList();
                    purchasingDocumentItemHistories = db.PurchasingDocumentItemHistories.Where(pdih => (pdih.MovementType == 105 || pdih.MovementType == 101) && pdih.PurchasingDocumentItemID == this.ID).ToList();
                }
                else
                {
                    purchasingDocumentItems = db.PurchasingDocumentItems.Where(x => x.ID == this.ParentID || x.ParentID == this.ParentID).OrderBy(x => x.ConfirmedDate).ToList();
                    //purchasingDocumentItemHistories = db.PurchasingDocumentItemHistories.Where(po => po.PurchasingDocumentItem.PurchasingDocumentItemHistories.Any(pdih => (pdih.MovementType == 101 || pdih.MovementType == 105) && pdih.PurchasingDocumentItemID == this.ParentID)).ToList();
                    purchasingDocumentItemHistories = db.PurchasingDocumentItemHistories.Where(pdih => (pdih.MovementType == 105 || pdih.MovementType == 101) && pdih.PurchasingDocumentItemID == this.ParentID).ToList();

                }

                if (purchasingDocumentItems.Count > 0 && purchasingDocumentItemHistories.Count > 0 && this.TotalGR > 0 && this.ConfirmedQuantity > 0)
                {
                    int otherConfirmedQty = 0;
                    bool isMatch = false;
                    var totalGR = 0;
                    var currentGR = 0;
                    //int index = 0;
                    //double totalGR = this.TotalGR;
                    //double availableGR = purchasingDocumentItemHistories[index].GoodsReceiptQuantity.HasValue? purchasingDocumentItemHistories[index].GoodsReceiptQuantity.Value : 0;
                    foreach (PurchasingDocumentItem purchasingDocumentItem in purchasingDocumentItems)
                    {
                        //if (this.ID == purchasingDocumentItemHistories[index].PurchasingDocumentItemID || this.ParentID == purchasingDocumentItemHistories[index].PurchasingDocumentItemID)
                        if (this.ID == purchasingDocumentItem.ID && isMatch == false)
                        {
                            if (otherConfirmedQty > 0)
                            {
                                foreach (var pdih in purchasingDocumentItemHistories)
                                {
                                    if (otherConfirmedQty > 0)
                                    {
                                        //if (otherConfirmedQty > pdih.GoodsReceiptQuantity)
                                        //{
                                        otherConfirmedQty -= pdih.GoodsReceiptQuantity.HasValue ? pdih.GoodsReceiptQuantity.Value : 0;
                                        if (otherConfirmedQty < 0)
                                        {
                                            if (Math.Abs(otherConfirmedQty) < confirmQty)
                                            {
                                                totalGR += Math.Abs(otherConfirmedQty);
                                                PurchasingDocumentItemHistory newPurchasingDocumentItemHistory = new PurchasingDocumentItemHistory();
                                                newPurchasingDocumentItemHistory.GoodsReceiptDate = pdih.GoodsReceiptDate;
                                                newPurchasingDocumentItemHistory.GoodsReceiptQuantity = Math.Abs(otherConfirmedQty);
                                                listPurchasingDocumentItemHistory.Add(newPurchasingDocumentItemHistory);
                                            }
                                            else
                                            {
                                                PurchasingDocumentItemHistory newPurchasingDocumentItemHistory = new PurchasingDocumentItemHistory();
                                                newPurchasingDocumentItemHistory.GoodsReceiptDate = pdih.GoodsReceiptDate;
                                                newPurchasingDocumentItemHistory.GoodsReceiptQuantity = confirmQty;
                                                listPurchasingDocumentItemHistory.Add(newPurchasingDocumentItemHistory);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        totalGR += pdih.GoodsReceiptQuantity.HasValue ? pdih.GoodsReceiptQuantity.Value : 0;
                                        currentGR = pdih.GoodsReceiptQuantity.HasValue ? pdih.GoodsReceiptQuantity.Value : 0;

                                        if (totalGR > confirmQty)
                                        {
                                            PurchasingDocumentItemHistory newPurchasingDocumentItemHistory = new PurchasingDocumentItemHistory();
                                            newPurchasingDocumentItemHistory.GoodsReceiptDate = pdih.GoodsReceiptDate;
                                            newPurchasingDocumentItemHistory.GoodsReceiptQuantity = confirmQty - (totalGR - currentGR);
                                            listPurchasingDocumentItemHistory.Add(newPurchasingDocumentItemHistory);
                                        }
                                        else
                                        {
                                            PurchasingDocumentItemHistory newPurchasingDocumentItemHistory = new PurchasingDocumentItemHistory();
                                            newPurchasingDocumentItemHistory.GoodsReceiptDate = pdih.GoodsReceiptDate;
                                            newPurchasingDocumentItemHistory.GoodsReceiptQuantity = currentGR;
                                            listPurchasingDocumentItemHistory.Add(newPurchasingDocumentItemHistory);
                                        }
                                        if (totalGR >= confirmQty)
                                        {
                                            break;
                                        }
                                    }
                                }
                                //isMatch = true;
                                break;
                            }
                            else
                            {
                                totalGR = 0;
                                currentGR = 0;

                                foreach (var pdih in purchasingDocumentItemHistories)
                                {
                                    totalGR += pdih.GoodsReceiptQuantity.HasValue ? pdih.GoodsReceiptQuantity.Value : 0;
                                    currentGR = pdih.GoodsReceiptQuantity.HasValue ? pdih.GoodsReceiptQuantity.Value : 0;
                                    if (totalGR > confirmQty)
                                    {
                                        PurchasingDocumentItemHistory newPurchasingDocumentItemHistory = new PurchasingDocumentItemHistory();
                                        newPurchasingDocumentItemHistory.GoodsReceiptDate = pdih.GoodsReceiptDate;
                                        newPurchasingDocumentItemHistory.GoodsReceiptQuantity = confirmQty - (totalGR - currentGR);
                                        listPurchasingDocumentItemHistory.Add(newPurchasingDocumentItemHistory);
                                    }
                                    else
                                    {
                                        PurchasingDocumentItemHistory newPurchasingDocumentItemHistory = new PurchasingDocumentItemHistory();
                                        newPurchasingDocumentItemHistory.GoodsReceiptDate = pdih.GoodsReceiptDate;
                                        newPurchasingDocumentItemHistory.GoodsReceiptQuantity = currentGR;
                                        listPurchasingDocumentItemHistory.Add(newPurchasingDocumentItemHistory);
                                    }
                                    if (totalGR >= confirmQty)
                                    {
                                        //isMatch = true;
                                        break;
                                    }
                                }
                            }
                        }
                        else //if(isMatch==false)
                        {
                            otherConfirmedQty += purchasingDocumentItem.ConfirmedQuantity.HasValue ? purchasingDocumentItem.ConfirmedQuantity.Value : 0;
                        }
                    }
                    return listPurchasingDocumentItemHistory;
                }
                else
                {
                    return listPurchasingDocumentItemHistory;
                }
            }
        }


        #region Subcont

        public int ActiveStageToNumber
        {
            get
            {
                int donutProgress;

                if (!String.IsNullOrEmpty(this.ActiveStage))
                {
                    //donutProgress = Convert.ToDouble(this.ActiveStage) * 8.33;
                    donutProgress = Convert.ToInt32(Regex.Replace(this.ActiveStage, "[^.0-9]", ""));
                }
                else
                {
                    donutProgress = 0;
                }

                return donutProgress;
            }
        }

        #endregion

        #region Import

        public string ConfirmedDateView
        {
            get
            {
                if (this.ConfirmedDate.HasValue)
                {
                    return ConfirmedDate.Value.ToString("dd/MM/yyyy");
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        public bool HasProgressPhotoes
        {
            get
            {
                bool hasProgressPhotoes;

                if (this.ProgressPhotoes.Count < 1)
                {
                    hasProgressPhotoes = false;
                }
                else
                {
                    hasProgressPhotoes = true;
                }

                return hasProgressPhotoes;
            }
        }

        public bool HasETAHistory
        {
            get
            {
                bool hasETAHistory;

                if (this.ETAHistories.Count < 1)
                {
                    hasETAHistory = false;
                }
                else
                {
                    hasETAHistory = true;
                }

                return hasETAHistory;
            }
        }

        public bool HasPDIHistory
        {
            get
            {
                bool hasPDIHistory;

                if (this.PurchasingDocumentItemHistories.Count < 1)
                {
                    hasPDIHistory = false;
                }
                else
                {
                    hasPDIHistory = true;
                }

                return hasPDIHistory;
            }
        }

        public PurchasingDocumentItemHistory FirstPDIHistory
        {
            get
            {
                PurchasingDocumentItemHistory firstPDIHistory = new PurchasingDocumentItemHistory();

                if (this.HasPDIHistory)
                {
                    firstPDIHistory = this.PurchasingDocumentItemHistories.OrderBy(x => x.Created).FirstOrDefault();
                }
                else
                {
                    firstPDIHistory.PurchasingDocumentItem = this;
                }

                return firstPDIHistory;
            }
        }

        public bool IsDelayed
        {
            get
            {
                if (this.FirstETAHistory.ETADate == this.LastETAHistory.ETADate)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool IsComplete
        {
            get
            {
                if (this.FirstPDIHistory.GoodsReceiptQuantity == this.ConfirmedQuantity)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public ETAHistory FirstETAHistory
        {
            get
            {
                ETAHistory firstETAHistory = new ETAHistory();

                if (this.HasETAHistory)
                {
                    firstETAHistory = this.ETAHistories.OrderBy(x => x.Created).FirstOrDefault();
                }
                else
                {
                    firstETAHistory.PurchasingDocumentItem = this;
                }

                return firstETAHistory;
            }
        }

        public ETAHistory LastETAHistory
        {
            get
            {
                ETAHistory firstETAHistory = new ETAHistory();

                if (this.HasETAHistory)
                {
                    firstETAHistory = this.ETAHistories.OrderBy(x => x.Created).LastOrDefault();
                }
                else
                {
                    firstETAHistory.PurchasingDocumentItem = this;
                }

                return firstETAHistory;
            }
        }

        public Shipment FirstShipment
        {
            get
            {
                Shipment firstShipment = new Shipment();

                if (this.Shipments.Count > 0)
                {
                    firstShipment = this.Shipments.OrderBy(x => x.Created).FirstOrDefault();
                }

                return firstShipment;
            }
        }

        public double DonutProgress
        {
            get
            {
                double donutProgress;

                if (String.IsNullOrEmpty(this.ActiveStage) || this.ActiveStage == "0")
                {
                    donutProgress = 0;
                }
                else if (this.ActiveStage == "2a")
                {
                    donutProgress = Convert.ToDouble(3) * 8.33;
                }
                else if (this.ActiveStage == "1" || this.ActiveStage == "2")
                {
                    donutProgress = Convert.ToDouble(this.ActiveStage) * 8.33;
                }
                else
                {
                    donutProgress = (Convert.ToDouble(this.ActiveStage) + 1) * 8.33;
                }

                return donutProgress;
            }
        }

        public string ConfirmReceivedPaymentDateView
        {
            get
            {
                string confirmReceivedPaymentDateView = string.Empty;

                if (this.ConfirmReceivedPaymentDate != null)
                {
                    confirmReceivedPaymentDateView = this.ConfirmReceivedPaymentDate.GetValueOrDefault().ToString("dd/MM/yyyy");
                }

                return confirmReceivedPaymentDateView;
            }
        }

        public string ActiveStageView
        {
            get
            {
                if (string.IsNullOrEmpty(this.ActiveStage))
                {
                    return "0";
                }
                else
                {
                    return this.ActiveStage;
                }
            }
        }

        public string NetPriceView
        {
            get
            {
                int length = this.NetPrice.ToString().Length;
                int count = 0;

                string netPriceView = this.NetPrice.ToString();

                for (int i = length; i > 0; i--)
                {
                    if (count == 3)
                    {
                        netPriceView = netPriceView.Insert(i, ".");
                        count = 0;
                    }
                    count++;
                }

                netPriceView = netPriceView.Insert(netPriceView.Length, ",-");

                return netPriceView;
            }
        }

        public List<string> ShipmentInboundNumbers
        {
            get
            {
                if (this.ParentID == null || this.ParentID == this.ID)
                {
                    List<PurchasingDocumentItemHistory> purchasingDocumentItemHistories = db.PurchasingDocumentItemHistories.Where(x => x.PurchasingDocumentItemID == this.ID).ToList();
                    purchasingDocumentItemHistories = purchasingDocumentItemHistories.Where(x => x.POHistoryCategory == "E").ToList();

                    string empty = "-";

                    List<string> shipmentInboundNumbers = new List<string>();

                    foreach (var purchasingDocumentItemHistory in purchasingDocumentItemHistories)
                    {
                        if (string.IsNullOrEmpty(purchasingDocumentItemHistory.Shipment_InboundNumber))
                        {
                            shipmentInboundNumbers.Add(empty);
                        }
                        else
                        {
                            shipmentInboundNumbers.Add(purchasingDocumentItemHistory.Shipment_InboundNumber);
                        }
                    }

                    return shipmentInboundNumbers;
                }
                else
                {
                    return null;
                }
            }
        }

        public List<string> ImportParkingDateViews
        {
            get
            {
                if (this.ParentID == null || this.ParentID == this.ID)
                {
                    List<PurchasingDocumentItemHistory> purchasingDocumentItemHistories = db.PurchasingDocumentItemHistories.Where(x => x.PurchasingDocumentItemID == this.ID).ToList();
                    purchasingDocumentItemHistories = purchasingDocumentItemHistories.Where(x => x.POHistoryCategory == "E").ToList();

                    string empty = "-";

                    List<string> importParkingDateViews = new List<string>();

                    foreach (var purchasingDocumentItemHistory in purchasingDocumentItemHistories)
                    {
                        if (string.IsNullOrEmpty(purchasingDocumentItemHistory.GoodsReceiptDate.GetValueOrDefault().ToString("dd/MM/yyyy")))
                        {
                            importParkingDateViews.Add(empty);
                        }
                        else
                        {
                            importParkingDateViews.Add(purchasingDocumentItemHistory.GoodsReceiptDate.GetValueOrDefault().ToString("dd/MM/yyyy"));
                        }
                        
                    }

                    return importParkingDateViews;
                }
                else
                {
                    return null;
                }
            }
        }

        public List<string> TermDateViews
        {
            get
            {
                if (this.ParentID == null || this.ParentID == this.ID)
                {
                    List<PurchasingDocumentItemHistory> purchasingDocumentItemHistories = db.PurchasingDocumentItemHistories.Where(x => x.PurchasingDocumentItemID == this.ID).ToList();
                    purchasingDocumentItemHistories = purchasingDocumentItemHistories.Where(x => x.POHistoryCategory == "E").ToList();

                    List<string> TermDateViews = new List<string>();

                    foreach (var purchasingDocumentItemHistory in purchasingDocumentItemHistories)
                    {
                        if (string.IsNullOrEmpty(purchasingDocumentItemHistory.PayTerms))
                        {
                            TermDateViews.Add(purchasingDocumentItemHistory.GoodsReceiptDate.GetValueOrDefault().ToString("dd/MM/yyyy"));
                        }
                        else
                        {
                            if (purchasingDocumentItemHistory.PayTerms.Length >= 2)
                            {
                                var result = purchasingDocumentItemHistory.PayTerms.Substring(purchasingDocumentItemHistory.PayTerms.Length - 2);
                                int number;
                                int days;
                                bool success = Int32.TryParse(result, out number);

                                if (success)
                                {
                                    days = number;
                                }
                                else
                                {
                                    days = 0;
                                }

                                DateTime grDate = purchasingDocumentItemHistory.GoodsReceiptDate.GetValueOrDefault();
                                grDate.AddDays(days);

                                TermDateViews.Add(grDate.ToString("dd/MM/yyyy"));
                            }
                        }
                        
                    }

                    return TermDateViews;
                }
                else
                {
                    return null;
                }
            }
        }

        #endregion
    }
}