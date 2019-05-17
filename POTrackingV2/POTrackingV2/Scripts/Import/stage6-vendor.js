// DatePicker modal st6
$(".st6-copy-bl-date").datepicker({
    autoHide: true, format: 'dd/mm/yyyy', beforeShow: function () {
        setTimeout(function () {
            $('.datepicker-container').css('z-index', 99999999999999);
        }, 0);
    }
});
$(".st6-docs-acceptance-lc-date").datepicker({
    autoHide: true, format: 'dd/mm/yyyy', beforeShow: function () {
        setTimeout(function () {
            $('.datepicker-container').css('z-index', 99999999999999);
        }, 0);
    }
});

// Open Modal
$(document).on("click", ".st6-fill-in-the-form", function () {
    var myPurchasingDocumentItemId = $(this).data('id');
    $("#fillForm .modal-content #modalPurchasingDocumentItemId").val(myPurchasingDocumentItemId);
    var modalContent = $("#fillForm .modal-content");

    $.ajax({
        type: "POST",
        url: "/Import/GetShippingInformation",
        data: JSON.stringify({ 'myPurchasingDocumentItemId': myPurchasingDocumentItemId }),
        contentType: "application/json; charset=utf-8",
        success: function (response) {

            if (response.isCompleted === true) {

                modalContent.find(".st6-copy-bl-date").val(response.copyBLDate);
                modalContent.find(".st6-copy-bl-date").attr("disabled", "disabled");

                modalContent.find(".st6-group-copy-bl-document").attr("hidden", true);
                modalContent.find(".st6-group-copy-bl-document-uploaded").removeAttr("hidden");
                modalContent.find(".st6-copy-bl-document-download").attr("href", response.dokumenCopyBL);

                modalContent.find(".st6-group-packing-list-document").attr("hidden", true);
                modalContent.find(".st6-group-packing-list-document-uploaded").removeAttr("hidden");
                modalContent.find(".st6-packing-list-document-download").attr("href", response.dokumenPackingList);

                modalContent.find(".st6-group-invoice-document").attr("hidden", true);
                modalContent.find(".st6-group-invoice-document-uploaded").removeAttr("hidden");
                modalContent.find(".st6-invoice-document-download").attr("href", response.dokumenInvoice);

                modalContent.find(".st6-AWB").val(response.awb);
                modalContent.find(".st6-AWB").attr("disabled", "disabled");
                modalContent.find(".st6-courier-name").val(response.courierName);
                modalContent.find(".st6-courier-name").attr("disabled", "disabled");

                modalContent.find(".st6-fill-the-form").attr("disabled", "disabled");
                modalContent.find(".st6-fill-the-form").addClass("selected");
            }
            else {
                modalContent.find(".st6-copy-bl-date").val("");
                modalContent.find(".st6-copy-bl-date").removeAttr("disabled");

                modalContent.find(".st6-group-copy-bl-document").removeAttr("hidden");
                modalContent.find(".st6-group-copy-bl-document-uploaded").attr("hidden", true);
                modalContent.find(".st6-copy-bl-document-download").attr("href", "");

                modalContent.find(".st6-group-packing-list-document").removeAttr("hidden");
                modalContent.find(".st6-group-packing-list-document-uploaded").attr("hidden", true);
                modalContent.find(".st6-packing-list-document-download").attr("href", "");

                modalContent.find(".st6-group-invoice-document").removeAttr("hidden");
                modalContent.find(".st6-group-invoice-document-uploaded").attr("hidden", true);
                modalContent.find(".st6-invoice-document-download").attr("href", "");

                modalContent.find(".st6-AWB").val("");
                modalContent.find(".st6-AWB").removeAttr("disabled");
                modalContent.find(".st6-courier-name").val("");
                modalContent.find(".st6-courier-name").removeAttr("disabled");

                modalContent.find(".st6-fill-the-form").removeAttr("disabled");
            }
        },
        error: function (xhr, status, error) {
            alert(xhr.status + " : " + error);
        }
    });
});

// Validation
$(".st6-copy-bl-date").on('input focus', function (e) {
    this.reportValidity();
});

$(".st6-AWB").on('input focus', function (e) {
    this.reportValidity();
});

$(".st6-courier-name").on('input focus', function (e) {
    this.reportValidity();
});

//Vendor click Shipment Book Date
$(".st6-fill-the-form").on("click", function (obj) {

    var buttonFillTheForm = $(this);
    var rowModal = $(this).closest(".modal.fade");
    var inputCopyBLDate = rowModal.find(".st6-copy-bl-date");
    var inputCopyBLDocument = rowModal.find(".st6-copy-bl-document");
    var inputCopyBLDocumentDOM = inputCopyBLDocument.get(0);
    var inputPackingListDocument = rowModal.find(".st6-packing-list-document");
    var inputPackingListDocumentDOM = inputPackingListDocument.get(0);
    var inputPackingInvoiceDocument = rowModal.find(".st6-invoice-document");
    var inputPackingInvoiceDocumentDOM = inputPackingInvoiceDocument.get(0);
    var inputAWB = rowModal.find(".st6-AWB");
    var inputCourierName = rowModal.find(".st6-courier-name");

    var itemID = rowModal.find(".st6-item-id").val();
    var copyBLDate = reverseDayMonth(inputCopyBLDate.val());
    var awb = inputAWB.val();
    var courierName = inputCourierName.val();

    var donutProgressUnit = 75.39822368615503 / 13;
    var donutProgress = 75.39822368615503 - 8 * donutProgressUnit;
    var cssRow;
    var donutRow;
    var nextDataContent;


    $(".st6-fill-in-the-form").each(function (index) {
        if ($(this).attr("data-id") === itemID) {

            // Donut Progress
            cssRow = $(this).closest(".po-item-data-content").prop("class");
            cssRow = cssRow.replace(" ", ".");
            cssRow = "." + cssRow;
            donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

            // Next stage Controller
            cssRow = $(this).closest(".po-item-data-content").prop("class");
            cssRow = cssRow.replace(" ", ".");
            cssRow = "." + cssRow;
            nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

        }
    });


    var formData = new FormData();

    for (var i = 0; i < 1; i++) {
        var fileCopyBL = inputCopyBLDocumentDOM.files[i];
        var filePackingList = inputPackingListDocumentDOM.files[i];
        var fileInvoice = inputPackingInvoiceDocumentDOM.files[i];

        formData.append("inputPurchasingDocumentItemID", itemID);
        formData.append("inputCopyBLDate", copyBLDate.toUTCString());
        formData.append("fileCopyBL", fileCopyBL);
        formData.append("filePackingList", filePackingList);
        formData.append("fileInvoice", fileInvoice);
        formData.append("inputAWB", awb);
        formData.append("inputCourierName", courierName);
    }

    //for (var key of formData.entries()) {
    //    console.log(key[0] + ', ' + key[1]);
    //}

    if (!isNaN(copyBLDate.getTime()) && inputCopyBLDate.val() !== '') {
        if (inputCopyBLDocumentDOM.files.length > 0) {
            if (inputPackingListDocumentDOM.files.length > 0) {
                if (inputPackingInvoiceDocumentDOM.files.length > 0) {
                    if (awb !== '' && inputAWB.val() !== '') {
                        if (courierName !== '' && inputCourierName.val() !== '') {
                            $.ajax({
                                type: "POST",
                                url: "/Import/VendorFillInShipmentForm",
                                data: formData,
                                cache: false,
                                contentType: false,
                                processData: false,
                                success: function (response) {
                                    alert(response.responseText);

                                    buttonFillTheForm.attr("disabled", "disabled").addClass("selected");

                                    donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                                    donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("7");

                                },
                                error: function (xhr, status, error) {
                                    alert(xhr.status + " : " + error);
                                }
                            });
                        }
                        else {
                            alert("Courier Name cannot be empty");
                            inputCourierName.focus();
                        }
                    }
                    else {
                        alert("AWB cannot be empty");
                        inputAWB.focus();
                    }
                }
                else {
                    alert("Dokumen Invoice cannot be empty");
                    inputPackingInvoiceDocument.focus();
                }
            }
            else {
                alert("Dokumen Packing List cannot be empty");
                inputPackingListDocument.focus();
            }
        }
        else {
            alert("Dokumen Copy BL cannot be empty");
            inputCopyBLDocument.focus();
        }
    }
    else {
        alert("Tanggal Copy BL is not valid");
        inputCopyBLDate.focus();
    }
});