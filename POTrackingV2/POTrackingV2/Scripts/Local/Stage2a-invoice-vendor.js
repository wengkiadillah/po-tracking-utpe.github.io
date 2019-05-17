﻿//Vendor upload one Proforma Invoice
$(".st2a-upload-proforma-invoice").on("click", function (obj) {
    obj.preventDefault();

    var buttonUploadProformaInvoice = $(this);
    var inputFileProformaInvoice = $(this).closest(".form-inline").find(".st2a-file-proforma-invoice");
    var inputFileProformaInvoiceDOM = inputFileProformaInvoice.get(0);
    var formUploading = $(this).closest(".form-inline");
    var formUploaded = $(this).closest(".form-inline").next();

    var itemID = $(this).closest(".form-inline").find(".st2a-item-id").val();

    var formData = new FormData();

    for (var i = 0; i < inputFileProformaInvoiceDOM.files.length; i++) {
        var file = inputFileProformaInvoiceDOM.files[i];

        formData.append("fileProformaInvoice", file);
        formData.append("inputPurchasingDocumentItemID", itemID);
    }

    if (inputFileProformaInvoiceDOM.files.length > 0) {
        $.ajax({
            type: "POST",
            url: "VendorUploadInvoice",
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                alert(response.responseText);

                buttonUploadProformaInvoice.attr("disabled", "disabled");
                inputFileProformaInvoice.attr("disabled", "disabled");
                formUploading.attr("hidden", true);
                formUploaded.removeAttr("hidden");
                formUploaded.find(".st2a-approve-proforma-proc").attr("hidden", true);
                formUploaded.find(".st2a-disapprove-proforma-proc").attr("hidden", true);

                formUploaded.find(".st2a-download-proforma").attr("href", response.proformaInvoiceUrl);

            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
    else {
        alert("File failed to upload");
    }
});

$(".st2a-vendor-skip-PI").on("click", function (obj) {
    obj.preventDefault();

    var buttonSkipPI = $(this);
    var buttonUploadPI = $(this).closest(".form-inline").find(".st2a-upload-proforma-invoice");
    var inputFileProformaInvoice = $(this).closest(".form-inline").find(".st2a-file-proforma-invoice");

    var itemID = $(this).closest(".form-inline").find(".st2a-item-id").val();

    var inputPurchasingDocumentItem = {
        ID: itemID
    };

    //Donut
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);
    var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

    if (itemID !== null) {
        $.ajax({
            type: "POST",
            url: "VendorSkipPI",
            data: JSON.stringify({ 'inputPurchasingDocumentItem': inputPurchasingDocumentItem }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);


                buttonSkipPI.addClass("selected").attr("disabled", "disabled");
                buttonUploadPI.attr("disabled", "disabled");
                inputFileProformaInvoice.attr("disabled","disabled");

                donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("3");
                nextDataContent.find(".st3-checkbox-item").first().removeAttr("disabled");
            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
});