// Procurement Ask Proforma
$(".st2a-ask-proforma").on("click", function (obj) {

    var stage2aProcurementAskProformaInvoice = $("#stage2aProcurementAskProformaInvoice").val();
    obj.preventDefault();

    var buttonAskProforma = $(this);
    var buttonSkipProforma = $(this).closest(".po-form-item-st2a").find(".st2a-skip-proforma");
    var buttonUploadProformaInvoice = $(this).closest(".po-form-item-st2a").find(".st2a-upload-proforma-invoice");
    var inputFileProformaInvoice = $(this).closest(".po-form-item-st2a").find(".st2a-file-proforma-invoice");

    var inputPurchasingDocumentItemID = $(this).closest(".po-form-item-st2a").find(".st2a-item-id").val();

    //// Donut Progress
    //var donutProgressUnit = 75.39822368615503 / 13;
    //var donutProgress = 75.39822368615503 - 4 * donutProgressUnit;
    //var cssRow = $(this).closest(".po-item-data-content").prop("class");
    //cssRow = cssRow.replace(" ", ".");
    //cssRow = "." + cssRow;
    //var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    //// Next stage Controller
    //cssRow = $(this).closest(".po-item-data-content").prop("class");
    //cssRow = cssRow.replace(" ", ".");
    //cssRow = "." + cssRow;
    //var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

    if (inputPurchasingDocumentItemID !== '') {
        $.ajax({
            type: "POST",
            url: stage2aProcurementAskProformaInvoice,
            data: JSON.stringify({ 'inputPurchasingDocumentItemID': inputPurchasingDocumentItemID }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                buttonUploadProformaInvoice.attr("disabled", "disabled");
                inputFileProformaInvoice.attr("disabled", "disabled");
                buttonSkipProforma.attr("disabled", "disabled");
                buttonAskProforma.attr("disabled", "disabled").addClass("selected");

                //donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                //donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("3");

                //nextDataContent.find(".st3-checkbox-item").first().removeAttr("disabled");
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

// Procurement Skip Proforma
$(".st2a-skip-proforma").on("click", function (obj) {

    var stage2aProcurementSkipProformaInvoice = $("#stage2aProcurementSkipProformaInvoice").val();
    obj.preventDefault();

    var buttonSkipProforma = $(this);
    var buttonAskProforma = $(this).closest(".po-form-item-st2a").find(".st2a-ask-proforma");
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
            url: stage2aProcurementSkipProformaInvoice,
            data: JSON.stringify({ 'inputPurchasingDocumentItemID': inputPurchasingDocumentItemID }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                buttonUploadProformaInvoice.attr("disabled", "disabled");
                inputFileProformaInvoice.attr("disabled", "disabled");
                buttonSkipProforma.attr("disabled", "disabled").addClass("selected-negative");
                buttonAskProforma.attr("disabled", "disabled");

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