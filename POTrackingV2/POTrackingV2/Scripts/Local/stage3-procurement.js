// Validations
$(".st3-item-confirm-payment-date").on('input focus', function (e) {
    this.reportValidity();
});

// Check / Unchecked All PO Items
$("input.st3-checkbox-all").on("change", function (obj) {
    var checkboxAll = $(this);
    $(this).closest(".po-item-section").find("input.st3-checkbox-item").each(function (index) {
        if (checkboxAll.prop("checked") === true && $(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true).change();
        } else if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", false).change();
        }
    });
});

//Checked Button Confirm PO -- Undisabling Controller
$(".st3-checkbox-item").on("change", function (obj) {
    var row = $(this).closest(".form-inline");
    if ($(this).prop("checked") === true) {
        row.find(".st3-item-confirm-payment-date").removeAttr("disabled");
        row.find(".st3-confirm-payment-submit").removeAttr("disabled");
        row.find(".st3-confirm-payment-skip").removeAttr("disabled");
    } else {
        row.find(".st3-item-confirm-payment-date").attr("disabled", "disabled");
        row.find(".st3-confirm-payment-submit").attr("disabled", "disabled");
        row.find(".st3-confirm-payment-skip").attr("disabled", "disabled");
    }
});

//Vendor click Confirm button Item Quantity
$(".st3-confirm-payment-submit").on("click", function (obj) {

    var stage3ProcurementConfirmPaymentReceived = $("#stage3ProcurementConfirmPaymentReceived").val();
    obj.preventDefault();

    var inputPurchasingDocumentItems = [];
    var buttonConfirmPayment = $(this);
    var buttonConfirmPaymentSkip = $(this).closest(".form-inline").find(".st3-confirm-payment-skip");
    var inputCheckboxItem = $(this).closest(".form-inline").find(".st3-checkbox-item");
    var inputConfirmReceivedPaymentDate = $(this).closest(".form-inline").find("input.st3-item-confirm-payment-date");
    var editRow = $(this).closest(".form-inline").find(".edit-row-st3");

    var itemID = $(this).closest(".form-inline").find("input.st3-item-id").val();
    //var minDateConfirmReceivedPaymentDate = reverseDayMonth($(this).closest(".form-inline").find("input.st3-item-confirm-payment-date").attr("mindate"));
    var confirmedReceivedPaymentDate = reverseDayMonth(inputConfirmReceivedPaymentDate.val());

    inputPurchasingDocumentItems.push({
        ID: itemID,
        ConfirmReceivedPaymentDate: confirmedReceivedPaymentDate
    });

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 8;
    var donutProgress = 75.39822368615503 - 5 * donutProgressUnit;
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    // Next stage Controller
    cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

    if (confirmedReceivedPaymentDate !== '' && !isNaN(confirmedReceivedPaymentDate.getTime())) {
        $.ajax({
            type: "POST",
            url: stage3ProcurementConfirmPaymentReceived,
            data: JSON.stringify({ 'inputPurchasingDocumentItems': inputPurchasingDocumentItems }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                alert(response.responseText);

                if (response.responseText.includes("item affected")) {
                    buttonConfirmPayment.attr("disabled", "disabled").addClass("selected");
                    buttonConfirmPaymentSkip.attr("disabled", "disabled");
                    inputCheckboxItem.attr("disabled", "disabled");
                    inputConfirmReceivedPaymentDate.attr("disabled", "disabled");
                    editRow.attr("style", "visibility:display");

                    donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                    donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("4");

                    //nextDataContent.find(".st4-update-eta-date-on-time-confirm").first().removeAttr("disabled");
                    //nextDataContent.find(".st4-update-eta-date-delay").first().removeAttr("disabled");
                    //nextDataContent.find(".st4-update-eta-date-delay-confirm").first().removeAttr("disabled");
                    //nextDataContent.find(".st4-delay-reason").first().removeAttr("disabled");
                }

                
            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
    else {
        alert("Date cannot be empty");
        inputConfirmReceivedPaymentDate.focus();
    }
});

//Vendor Confirm All PO -- Save All buat item yang aktif (gak disabled)
$(".st3-confirm-payment-all").on("click", function (obj) {

    var stage3ProcurementConfirmPaymentReceived = $("#stage3ProcurementConfirmPaymentReceived").val();
    var inputPurchasingDocumentItems = [];
    var donutProgress;

    $(this).closest(".po-item-section.stage-4").find(".po-form-item-st3").each(function (index) {

        var buttonConfirmPayment = $(this).find(".st3-confirm-payment-submit");
        var buttonConfirmPaymentSkip = $(this).find(".st3-confirm-payment-skip");
        var inputCheckboxItem = $(this).find(".st3-checkbox-item");
        var inputConfirmReceivedPaymentDate = $(this).find(".st3-item-confirm-payment-date");
        var editRow = $(this).find(".edit-row-st3");

        var itemID = $(this).find(".st3-item-id").val();
        //var minDateConfirmReceivedPaymentDate = reverseDayMonth(inputConfirmReceivedPaymentDate.attr("mindate"));
        var confirmedReceivedPaymentDate = reverseDayMonth(inputConfirmReceivedPaymentDate.val());

        // Donut Progress
        var donutProgressUnit = 75.39822368615503 / 8;
        donutProgress = 75.39822368615503 - 5 * donutProgressUnit;
        var cssRow = $(this).closest(".po-item-data-content").prop("class");
        cssRow = cssRow.replace(" ", ".");
        cssRow = "." + cssRow;
        var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

        // Next stage Controller
        cssRow = $(this).closest(".po-item-data-content").prop("class");
        cssRow = cssRow.replace(" ", ".");
        cssRow = "." + cssRow;
        var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

        if (inputConfirmReceivedPaymentDate.attr("disabled") !== "disabled" && inputCheckboxItem.prop("checked") === true && inputCheckboxItem.attr("disabled") !== "disabled") {
            if (inputConfirmReceivedPaymentDate.val() !== '' && !isNaN(confirmedReceivedPaymentDate.getTime())) {
                inputPurchasingDocumentItems.push({
                    ID: itemID,
                    ConfirmReceivedPaymentDate: confirmedReceivedPaymentDate
                });

                inputConfirmReceivedPaymentDate.addClass("row-updated");
                inputCheckboxItem.addClass("row-updated");
                buttonConfirmPaymentSkip.addClass("row-updated");
                buttonConfirmPayment.addClass("row-updated-button");
                editRow.addClass("row-updated-link");

                donutRow.find(".donut-chart").first().find("circle").next().addClass("row-updated-donut");
                donutRow.find(".donut-chart").first().next().find("span.mark-donut").addClass("row-updated-donut-text");

                //nextDataContent.first().addClass("row-updated-next-content");
            }
            else {
                alert("Date cannot be empty");
                inputConfirmReceivedPaymentDate.focus();
            }
        }
    });

    $.ajax({
        type: "POST",
        url: stage3ProcurementConfirmPaymentReceived,
        data: JSON.stringify({ 'inputPurchasingDocumentItems': inputPurchasingDocumentItems }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            alert(response.responseText);

            if (response.responseText.includes("item affected")) {
                $(".row-updated").attr("disabled", "disabled");
                $(".row-updated").removeClass("row-updated");
                $(".row-updated-button").attr("disabled", "disabled").addClass("selected");
                $(".row-updated-button").removeClass("row-updated-button");
                $(".row-updated-link").attr("style", "visibility:display");
                $(".row-updated-link").removeClass("row-updated-link");

                $(".row-updated-donut").attr("stroke-dashoffset", donutProgress);
                $(".row-updated-donut-text").text("4");
                $(".row-updated-donut").removeClass("row-updated-donut");
                $(".row-updated-donut-text").removeClass("row-updated-donut-text");

                //$(".row-updated-next-content").find(".st4-update-eta-date-on-time-confirm").removeAttr("disabled");
                //$(".row-updated-next-content").find(".st4-update-eta-date-delay").removeAttr("disabled");
                //$(".row-updated-next-content").find(".st4-update-eta-date-delay-confirm").removeAttr("disabled");
                //$(".row-updated-next-content").find(".st4-delay-reason").removeAttr("disabled");
                //$(".row-updated-next-content").removeClass("row-updated-next-content");
            }

            
        }
    });
});

// Vendor skip Stage 3
$(".st3-confirm-payment-skip").on("click", function (obj) {
    var stage3ProcurementSkipConfirmPayment = $("#stage3ProcurementSkipConfirmPayment").val();
    obj.preventDefault();
    var inputPurchasingDocumentItemIDs = [];

    var buttonConfirmPaymentSkip = $(this);
    var buttonConfirmPayment = $(this).closest(".form-inline").find(".st3-confirm-payment-submit");
    var inputCheckboxItem = $(this).closest(".form-inline").find(".st3-checkbox-item");
    var inputConfirmReceivedPaymentDate = $(this).closest(".form-inline").find("input.st3-item-confirm-payment-date");
    var editRow = $(this).closest(".form-inline").find(".edit-row-st3");

    var inputPurchasingDocumentItemID = $(this).closest(".form-inline").find("input.st3-item-id").val();

    inputPurchasingDocumentItemIDs.push(inputPurchasingDocumentItemID);

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 8;
    var donutProgress = 75.39822368615503 - 5 * donutProgressUnit;
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
            url: stage3ProcurementSkipConfirmPayment,
            data: JSON.stringify({ 'inputPurchasingDocumentItemIDs': inputPurchasingDocumentItemIDs }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                if (response.responseText.includes("Skipped")) {
                    buttonConfirmPayment.attr("disabled", "disabled");
                    buttonConfirmPaymentSkip.attr("disabled", "disabled").addClass("selected-negative");
                    inputCheckboxItem.attr("disabled", "disabled");
                    inputConfirmReceivedPaymentDate.attr("disabled", "disabled");
                    editRow.attr("style", "visibility:display");

                    donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                    donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("4");

                    //nextDataContent.find(".st4-update-eta-date-on-time-confirm").first().removeAttr("disabled");
                    //nextDataContent.find(".st4-update-eta-date-delay").first().removeAttr("disabled");
                    //nextDataContent.find(".st4-update-eta-date-delay-confirm").first().removeAttr("disabled");
                    //nextDataContent.find(".st4-delay-reason").first().removeAttr("disabled");
                }                

            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
});

$(".st3-skip-all-comfirm-payment").on("click", function (obj) {

    var stage3ProcurementSkipConfirmPayment = $("#stage3ProcurementSkipConfirmPayment").val();
    var inputPurchasingDocumentItemIDs = [];
    var donutProgress;

    $(this).closest(".po-item-section.stage-4").find(".po-form-item-st3").each(function (index) {

        var buttonConfirmPayment = $(this).find(".st3-confirm-payment-submit");
        var buttonConfirmPaymentSkip = $(this).find(".st3-confirm-payment-skip");
        var inputCheckboxItem = $(this).find(".st3-checkbox-item");
        var inputConfirmReceivedPaymentDate = $(this).find(".st3-item-confirm-payment-date");
        var editRow = $(this).find(".edit-row-st3");

        var itemID = $(this).find(".st3-item-id").val();

        // Donut Progress
        var donutProgressUnit = 75.39822368615503 / 8;
        donutProgress = 75.39822368615503 - 5 * donutProgressUnit;
        var cssRow = $(this).closest(".po-item-data-content").prop("class");
        cssRow = cssRow.replace(" ", ".");
        cssRow = "." + cssRow;
        var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

        // Next stage Controller
        cssRow = $(this).closest(".po-item-data-content").prop("class");
        cssRow = cssRow.replace(" ", ".");
        cssRow = "." + cssRow;
        var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

        if (inputConfirmReceivedPaymentDate.attr("disabled") !== "disabled" && inputCheckboxItem.prop("checked") === true && inputCheckboxItem.attr("disabled") !== "disabled") {
            inputPurchasingDocumentItemIDs.push(itemID);

            inputConfirmReceivedPaymentDate.addClass("row-updated");
            inputCheckboxItem.addClass("row-updated");
            buttonConfirmPaymentSkip.addClass("row-updated-button");
            buttonConfirmPayment.addClass("row-updated");
            editRow.addClass("row-updated-link");

            donutRow.find(".donut-chart").first().find("circle").next().addClass("row-updated-donut");
            donutRow.find(".donut-chart").first().next().find("span.mark-donut").addClass("row-updated-donut-text");

            //nextDataContent.first().addClass("row-updated-next-content");
        }
    });

    if (inputPurchasingDocumentItemIDs.length > 0) {
        $.ajax({
            type: "POST",
            url: stage3ProcurementSkipConfirmPayment,
            data: JSON.stringify({ 'inputPurchasingDocumentItemIDs': inputPurchasingDocumentItemIDs }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                alert(response.responseText);

                if (response.responseText.includes("Skipped")) {
                    $(".row-updated").attr("disabled", "disabled");
                    $(".row-updated").removeClass("row-updated");
                    $(".row-updated-button").attr("disabled", "disabled").addClass("selected-negative");
                    $(".row-updated-button").removeClass("row-updated-button");
                    $(".row-updated-link").attr("style", "visibility:display");
                    $(".row-updated-link").removeClass("row-updated-link");

                    $(".row-updated-donut").attr("stroke-dashoffset", donutProgress);
                    $(".row-updated-donut-text").text("4");
                    $(".row-updated-donut").removeClass("row-updated-donut");
                    $(".row-updated-donut-text").removeClass("row-updated-donut-text");

                    //$(".row-updated-next-content").find(".st4-update-eta-date-on-time-confirm").removeAttr("disabled");
                    //$(".row-updated-next-content").find(".st4-update-eta-date-delay").removeAttr("disabled");
                    //$(".row-updated-next-content").find(".st4-update-eta-date-delay-confirm").removeAttr("disabled");
                    //$(".row-updated-next-content").find(".st4-delay-reason").removeAttr("disabled");
                    //$(".row-updated-next-content").removeClass("row-updated-next-content");
                }
                
            }
        });
    }
    else {
        alert("0 Data affected");
    }
});

//edit item Click - PROCUREMENT
$(".edit-row-st3").on("click", function (obj) {
    $(this).closest(".form-inline").find(".st3-item-confirm-payment-date").removeAttr("disabled");
    $(this).closest(".form-inline").find(".st3-confirm-payment-submit").removeAttr("disabled").removeClass("selected");
    $(this).closest(".form-inline").find(".st3-confirm-payment-skip").removeAttr("disabled").removeClass("selected-negative");
    $(this).closest(".form-inline").find(".st3-checkbox-item").removeAttr("disabled");
});