﻿//Vendor upload one Proforma Invoice
$(".st2a-upload-proforma-invoice").on("click", function (obj) {
    var stage2aVendorUploadProformaInvoice = $("#stage2aVendorUploadProformaInvoice").val();

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

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 8;
    var donutProgress = 75.39822368615503 - 4 * donutProgressUnit;
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    //// Next stage Controller
    //cssRow = $(this).closest(".po-item-data-content").prop("class");
    //cssRow = cssRow.replace(" ", ".");
    //cssRow = "." + cssRow;
    //var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

    if (inputFileProformaInvoiceDOM.files.length > 0) {
        $.ajax({
            type: "POST",
            url: stage2aVendorUploadProformaInvoice,
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                alert(response.responseText);

                buttonUploadProformaInvoice.attr("disabled", "disabled");
                inputFileProformaInvoice.attr("disabled", "disabled");
                formUploading.attr("hidden", true);
                formUploaded.removeAttr("hidden");

                formUploaded.find(".st2a-download-proforma").attr("href", response.proformaInvoiceUrl);
                //formUploaded.find(".st2a-remove-proforma-invoice").attr("href", "");

                donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("3");

                //nextDataContent.find(".st3-checkbox-item").first().removeAttr("disabled");

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

//Vendor remove Proforma Invoice
$(".st2a-remove-proforma-invoice").on("click", function (obj) {

    var stage2aVendorRemoveProformaInvoice = $("#stage2aVendorRemoveProformaInvoice").val();
    obj.preventDefault();

    var buttonUploadProformaInvoice = $(this).closest(".form-inline").prev().find(".st2a-upload-proforma-invoice");
    var inputFileProformaInvoice = $(this).closest(".form-inline").prev().find(".st2a-file-proforma-invoice");
    var formUploading = $(this).closest(".form-inline").prev();
    var formUploaded = $(this).closest(".form-inline");

    var itemID = $(this).closest(".form-inline").prev().find(".st2a-item-id").val();

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 8;
    var donutProgress = 75.39822368615503 - 3 * donutProgressUnit;
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    $.ajax({
        type: "POST",
        url: stage2aVendorRemoveProformaInvoice,
        data: JSON.stringify({ 'inputPurchasingDocumentItemID': itemID }),
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            alert(response.responseText);

            buttonUploadProformaInvoice.removeAttr("disabled");
            inputFileProformaInvoice.removeAttr("disabled");
            formUploading.removeAttr("hidden");
            formUploaded.attr("hidden", true);

            formUploaded.find(".st2a-download-proforma").attr("href", "");

            donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
            donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("2a");

            //nextDataContent.find(".st3-checkbox-item").first().removeAttr("disabled");
        },
        error: function (xhr, status, error) {
            alert(xhr.status + " : " + error);
        }
    });
});
// Vendor Skip Proforma
/*$(".st2a-vendor-skip-PI").on("click", function (obj) {
    var stage2aVendorSkipPI = $("#stage2aVendorSkipPI").val();

    obj.preventDefault();

    var buttonSkipPI = $(this);
    var buttonUploadPI = $(this).closest(".po-form-item-st2a").find(".st2a-upload-proforma-invoice");
    var inputFileProformaInvoice = $(this).closest(".po-form-item-st2a").find(".st2a-file-proforma-invoice");

    var inputPurchasingDocumentItemID = $(this).closest(".po-form-item-st2a").find(".st2a-item-id").val();

    //var inputPurchasingDocumentItem = {
    //    ID: itemID
    //};

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 8;
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
            url: stage2aVendorSkipPI,
            data: JSON.stringify({ 'inputPurchasingDocumentItemID': inputPurchasingDocumentItemID }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                //buttonSkipPI.addClass("selected").attr("disabled", "disabled");
                buttonUploadPI.attr("disabled", "disabled");
                inputFileProformaInvoice.attr("disabled", "disabled");
                buttonSkipPI.attr("disabled", "disabled").addClass("selected-negative");

                donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("3");
                nextDataContent.find(".st3-checkbox-item").first().removeAttr("disabled");

            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
});*/