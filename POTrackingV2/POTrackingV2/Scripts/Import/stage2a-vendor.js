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
            url: "/Import/VendorUploadProformaInvoice",
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

// Vendor Skip Proforma
$(".st2a-skip-proforma").on("click", function (obj) {
    obj.preventDefault();

    var buttonSkipProforma = $(this);
    var buttonUploadProformaInvoice = $(this).closest(".po-form-item-st2a").find(".st2a-upload-proforma-invoice");
    var inputFileProformaInvoice = $(this).closest(".po-form-item-st2a").find(".st2a-file-proforma-invoice");

    var inputPurchasingDocumentItemID = $(this).closest(".po-form-item-st2a").find(".st2a-item-id").val();

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 13;
    var donutProgress = 75.39822368615503 - 4 * donutProgressUnit;
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    // Next stage Controller
    cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

    if (inputPurchasingDocumentItemID !== '') {
        $.ajax({
            type: "POST",
            url: "/Import/VendorSkipProformaInvoice",
            data: JSON.stringify({ 'inputPurchasingDocumentItemID': inputPurchasingDocumentItemID }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                buttonUploadProformaInvoice.attr("disabled", "disabled");
                inputFileProformaInvoice.attr("disabled", "disabled");
                buttonSkipProforma.attr("disabled", "disabled").addClass("selected-negative");

                donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("3");

                nextDataContent.find(".st3-checkbox-item").first().removeAttr("disabled");
                //nextDataContent.find(".st3-checkbox-item").first().attr("checked", "checked");
                //nextDataContent.find(".st3-item-confirm-payment-date").first().removeAttr("disabled");
                //nextDataContent.find(".st3-confirm-payment-submit").first().removeAttr("disabled");
                //nextDataContent.find(".st3-skip-confirm-payment").first().removeAttr("disabled");

            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
});