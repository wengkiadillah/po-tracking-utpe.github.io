//Check / Unchecked All PO Items
$("input.st2a-checkbox-all").on("change", function (obj) {
    var checkboxAll = $(this);
    $(this).closest(".po-item-section").find("input.st2a-checkbox-item").each(function (index) {
        if (checkboxAll.prop("checked") === true && $(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true).change();
        } else if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", false).change();
        }
    });
});

//Checked Button Confirm PO -- Undisabling Controller
$(".st2a-checkbox-item").on("change", function (obj) {
    var row = $(this).closest(".po-form-item-st2a");

    if ($(this).prop("checked") === true) {

        row.find(".st2a-ask-proforma").removeAttr("disabled");
        row.find(".st2a-skip-proforma").removeAttr("disabled");
        row.find(".st2a-file-proforma-invoice").removeAttr("disabled");
        row.find(".st2a-upload-proforma-invoice").removeAttr("disabled");

    } else {
        row.find(".st2a-ask-proforma").attr("disabled", "disabled");
        row.find(".st2a-skip-proforma").attr("disabled", "disabled");
        row.find(".st2a-file-proforma-invoice").attr("disabled", "disabled");
        row.find(".st2a-upload-proforma-invoice").attr("disabled", "disabled");
    }
});


// Procurement ONE Ask Proforma
$(".st2a-ask-proforma").on("click", function (obj) {

    var stage2aProcurementAskProformaInvoice = $("#stage2aProcurementAskProformaInvoice").val();
    obj.preventDefault();

    var inputCheckboxItem = $(this).closest(".po-form-item-st2a").find(".st2a-checkbox-item");
    var buttonAskProforma = $(this);
    var buttonSkipProforma = $(this).closest(".po-form-item-st2a").find(".st2a-skip-proforma");
    var buttonUploadProformaInvoice = $(this).closest(".po-form-item-st2a").find(".st2a-upload-proforma-invoice");
    var inputFileProformaInvoice = $(this).closest(".po-form-item-st2a").find(".st2a-file-proforma-invoice");

    var inputPurchasingDocumentItemID = $(this).closest(".po-form-item-st2a").find(".st2a-item-id").val();

    var inputPurchasingDocumentItemIDs = [];

    inputPurchasingDocumentItemIDs.push(inputPurchasingDocumentItemID);

    if (inputPurchasingDocumentItemID !== '') {
        $.ajax({
            type: "POST",
            url: stage2aProcurementAskProformaInvoice,
            data: JSON.stringify({ 'inputPurchasingDocumentItemIDs': inputPurchasingDocumentItemIDs }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                inputCheckboxItem.attr("disabled", "disabled");
                buttonUploadProformaInvoice.attr("disabled", "disabled");
                inputFileProformaInvoice.attr("disabled", "disabled");
                buttonSkipProforma.attr("disabled", "disabled");
                buttonAskProforma.attr("disabled", "disabled").addClass("selected");

            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
});

// Procurement All Ask Proforma
$(".st2a-ask-first-eta-all").on("click", function (obj) {

    var stage2aProcurementAskProformaInvoice = $("#stage2aProcurementAskProformaInvoice").val();
    var inputPurchasingDocumentItemIDs = [];

    $(this).closest(".po-item-section.stage-3").find(".po-form-item-st2a").each(function (index) {

        var inputCheckboxItem = $(this).find(".st2a-checkbox-item");
        var buttonAskProforma = $(this).find(".st2a-ask-proforma");
        var buttonSkipProforma = $(this).find(".st2a-skip-proforma");
        var buttonUploadProformaInvoice = $(this).find(".st2a-upload-proforma-invoice");
        var inputFileProformaInvoice = $(this).find(".st2a-file-proforma-invoice");

        var inputPurchasingDocumentItemID = $(this).find(".st2a-item-id").val();

        if (inputCheckboxItem.prop("checked") === true && inputCheckboxItem.attr("disabled") !== "disabled") {
            inputPurchasingDocumentItemIDs.push(inputPurchasingDocumentItemID);

            inputCheckboxItem.addClass("row-updated");
            buttonUploadProformaInvoice.addClass("row-updated");
            inputFileProformaInvoice.addClass("row-updated");
            buttonSkipProforma.addClass("row-updated");
            buttonAskProforma.addClass("row-updated-button");
        }
    });

    if (inputPurchasingDocumentItemIDs.length > 0) {
        $.ajax({
            type: "POST",
            url: stage2aProcurementAskProformaInvoice,
            data: JSON.stringify({ 'inputPurchasingDocumentItemIDs': inputPurchasingDocumentItemIDs }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                alert(response.responseText);

                $(".row-updated").attr("disabled", "disabled");
                $(".row-updated").removeClass("row-updated");
                $(".row-updated-button").attr("disabled", "disabled").addClass("selected");
                $(".row-updated-button").removeClass("row-updated-button");
            }
        });
    }
    else {
        alert("0 Data affected");
    }
});

// Procurement ONE Skip Proforma
$(".st2a-skip-proforma").on("click", function (obj) {

    var stage2aProcurementSkipProformaInvoice = $("#stage2aProcurementSkipProformaInvoice").val();
    obj.preventDefault();

    var inputCheckboxItem = $(this).closest(".po-form-item-st2a").find(".st2a-checkbox-item");
    var buttonSkipProforma = $(this);
    var buttonAskProforma = $(this).closest(".po-form-item-st2a").find(".st2a-ask-proforma");
    var buttonUploadProformaInvoice = $(this).closest(".po-form-item-st2a").find(".st2a-upload-proforma-invoice");
    var inputFileProformaInvoice = $(this).closest(".po-form-item-st2a").find(".st2a-file-proforma-invoice");

    var inputPurchasingDocumentItemID = $(this).closest(".po-form-item-st2a").find(".st2a-item-id").val();

    var inputPurchasingDocumentItemIDs = [];

    inputPurchasingDocumentItemIDs.push(inputPurchasingDocumentItemID);

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
            data: JSON.stringify({ 'inputPurchasingDocumentItemIDs': inputPurchasingDocumentItemIDs }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                inputCheckboxItem.attr("disabled", "disabled");
                buttonUploadProformaInvoice.attr("disabled", "disabled");
                inputFileProformaInvoice.attr("disabled", "disabled");
                buttonSkipProforma.attr("disabled", "disabled").addClass("selected-negative");
                buttonAskProforma.attr("disabled", "disabled");

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


//Vendor Confirm All PO -- Save All buat item yang aktif (gak disabled)
$(".st2a-skip-first-eta-all").on("click", function (obj) {

    var stage2aProcurementSkipProformaInvoice = $("#stage2aProcurementSkipProformaInvoice").val();
    var inputPurchasingDocumentItemIDs = [];
    var donutProgress;

    $(this).closest(".po-item-section.stage-3").find(".po-form-item-st2a").each(function (index) {

        var inputCheckboxItem = $(this).find(".st2a-checkbox-item");
        var buttonSkipProforma = $(this).find(".st2a-skip-proforma");
        var buttonAskProforma = $(this).find(".st2a-ask-proforma");
        var buttonUploadProformaInvoice = $(this).find(".st2a-upload-proforma-invoice");
        var inputFileProformaInvoice = $(this).find(".st2a-file-proforma-invoice");

        var inputPurchasingDocumentItemID = $(this).find(".st2a-item-id").val();

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

        if (inputCheckboxItem.prop("checked") === true && inputCheckboxItem.attr("disabled") !== "disabled") {
            inputPurchasingDocumentItemIDs.push(inputPurchasingDocumentItemID);

            inputCheckboxItem.addClass("row-updated");
            buttonAskProforma.addClass("row-updated");
            buttonUploadProformaInvoice.addClass("row-updated");
            inputFileProformaInvoice.addClass("row-updated");
            buttonSkipProforma.addClass("row-updated-button");

            donutRow.find(".donut-chart").first().find("circle").next().addClass("row-updated-donut");
            donutRow.find(".donut-chart").first().next().find("span.mark-donut").addClass("row-updated-donut-text");

            nextDataContent.first().addClass("row-updated-next-content");
        }
    });

    if (inputPurchasingDocumentItemIDs.length > 0) {
        $.ajax({
            type: "POST",
            url: stage2aProcurementSkipProformaInvoice,
            data: JSON.stringify({ 'inputPurchasingDocumentItemIDs': inputPurchasingDocumentItemIDs }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                alert(response.responseText);

                $(".row-updated").attr("disabled", "disabled");
                $(".row-updated").removeClass("row-updated");
                $(".row-updated-button").attr("disabled", "disabled").addClass("selected-negative");
                $(".row-updated-button").removeClass("row-updated-button");

                $(".row-updated-donut").attr("stroke-dashoffset", donutProgress);
                $(".row-updated-donut-text").text("3");
                $(".row-updated-donut").removeClass("row-updated-donut");
                $(".row-updated-donut-text").removeClass("row-updated-donut-text");

                $(".row-updated-next-content").find(".st3-checkbox-item").removeAttr("disabled");
                $(".row-updated-next-content").removeClass("row-updated-next-content");
            }
        });
    }
    else {
        alert("0 Data affected");
    }
});