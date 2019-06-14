//Check / Unchecked All PO Items
$("input.st2-proc-checkbox-all").on("change", function (obj) {
    var checkboxAll = $(this);
    $(this).closest(".po-item-section").find("input.checkbox-custom.st2-proc-checkbox-item").each(function (index) {
        if (checkboxAll.prop("checked") === true && $(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true).change();
        } else if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", false).change();
        }
    });
});

//Checked Button Confirm PO -- Undisabling Controller
$(".st2-proc-checkbox-item").on("change", function (obj) {
    var row = $(this).closest(".po-item-data-header__column.stage2-confirm-eta");

    if ($(this).prop("checked") === true) {
        row.find("button.st2-proc-accept-first-eta").removeAttr("disabled");
        row.find("button.st2-proc-decline-first-eta").removeAttr("disabled");
    } else {
        row.find("button.st2-proc-accept-first-eta").attr("disabled", "disabled");
        row.find("button.st2-proc-decline-first-eta").attr("disabled", "disabled");
    }
});

// Procurement one Accept
$(".st2-proc-accept-first-eta").on("click", function (obj) {
    var stage2ProcurementAcceptFirstEta = $("#stage2ProcurementAcceptFirstEta").val();

    obj.preventDefault();

    var inputPurchasingDocumentItemIDs = [];

    var checkboxItem = $(this).closest(".form-inline").find(".st2-proc-checkbox-item");
    var buttonAcceptFirstEta = $(this);
    var buttonDeclineFirstEta = $(this).closest(".form-inline").find(".st2-proc-decline-first-eta");
    var buttonEdit = $(this).closest(".po-form-item-st2").find(".edit-row-st2-proc");
    var etaDate = $(this).closest(".po-form-item-st2").find(".st2-proc-first-eta-date").val();

    var etaDateObject = reverseDayMonth(etaDate);

    var itemID = $(this).closest(".form-inline").find(".st2-proc-pd-item-id").val();

    inputPurchasingDocumentItemIDs.push(itemID);

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 8;
    var donutProgress = 75.39822368615503 - 3 * donutProgressUnit;
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    if (etaDate !== '' && !isNaN(etaDateObject.getTime())) {
        $.ajax({
            type: "POST",
            url: stage2ProcurementAcceptFirstEta,
            data: JSON.stringify({ 'inputPurchasingDocumentItemIDs': inputPurchasingDocumentItemIDs }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                console.log(response.imageSources);

                buttonAcceptFirstEta.attr("disabled", "disabled").addClass("selected");
                buttonDeclineFirstEta.attr("disabled", "disabled");
                checkboxItem.attr("disabled", "disabled");
                buttonEdit.attr("style", "visibility:display");

                donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("2a");

            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
    else {
        alert("Something went wrong!");
    }
});

//Procurement accept All ETA DATE
$(".st2-proc-accept-first-eta-all").on("click", function (obj) {
    var stage2ProcurementAcceptFirstEta = $("#stage2ProcurementAcceptFirstEta").val();

    obj.preventDefault();

    var donutProgressUnit = 75.39822368615503 / 8;
    var donutProgress = 75.39822368615503 - 3 * donutProgressUnit;

    var inputPurchasingDocumentItemIDs = [];

    $(this).closest(".po-item-section.stage-2").find(".po-form-item-st2").each(function (index) {

        var buttonAcceptFirstEta = $(this).find(".st2-proc-accept-first-eta");
        var buttonDeclineFirstEta = buttonAcceptFirstEta.closest(".form-inline").find(".st2-proc-decline-first-eta");
        var checkboxItem = buttonAcceptFirstEta.closest(".form-inline").find(".st2-proc-checkbox-item");
        var buttonEdit = buttonAcceptFirstEta.closest(".po-form-item-st2").find(".edit-row-st2-proc");
        var etaDate = buttonAcceptFirstEta.closest(".form-inline").find(".st2-proc-first-eta-date").val();

        var etaDateObject = reverseDayMonth(etaDate);

        var itemID = buttonAcceptFirstEta.closest(".form-inline").find(".st2-proc-pd-item-id").val();

        //progress donutpie
        var cssRow = $(this).closest(".po-item-data-content").prop("class");
        cssRow = cssRow.replace(" ", ".");
        cssRow = "." + cssRow;
        var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

        if (checkboxItem.prop("checked") === true && checkboxItem.attr("disabled") !== "disabled") {
            if (etaDate !== '' && !isNaN(etaDateObject.getTime())) {

                inputPurchasingDocumentItemIDs.push(itemID);

                buttonAcceptFirstEta.addClass("row-updated-button");
                buttonDeclineFirstEta.addClass("row-updated");
                checkboxItem.addClass("row-updated");
                buttonEdit.addClass("row-updated-link");

                donutRow.find(".donut-chart").first().find("circle").next().addClass("row-updated-donut");
                donutRow.find(".donut-chart").first().next().find("span.mark-donut").addClass("row-updated-donut-text");
            }
            else {
                alert("Something went wrong!");
            }
        }
    });

    if (inputPurchasingDocumentItemIDs.length > 0) {
        $.ajax({
            type: "POST",
            url: stage2ProcurementAcceptFirstEta,
            data: JSON.stringify({ 'inputPurchasingDocumentItemIDs': inputPurchasingDocumentItemIDs }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                $(".row-updated").attr("disabled", "disabled");
                $(".row-updated-button").attr("disabled", "disabled").addClass("selected");
                $(".row-updated-link").attr("style", "visibility:display");
                $(".row-updated").removeClass("row-updated");
                $(".row-updated-button").removeClass("row-updated-button");
                $(".row-updated-link").removeClass("row-updated-link");

                $(".row-updated-donut").attr("stroke-dashoffset", donutProgress);
                $(".row-updated-donut-text").text("2a");
                $(".row-updated-donut").removeClass("row-updated-donut");
                $(".row-updated-donut-text").removeClass("row-updated-donut-text");

            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
});


// Procurement one Decline
$(".st2-proc-decline-first-eta").on("click", function (obj) {
    var stage2ProcurementDeclineFirstEta = $("#stage2ProcurementDeclineFirstEta").val();

    obj.preventDefault();

    var inputPurchasingDocumentItemIDs = [];

    var checkboxItem = $(this).closest(".form-inline").find(".st2-proc-checkbox-item");
    var buttonAcceptFirstEta = $(this).closest(".form-inline").find(".st2-proc-accept-first-eta");
    var buttonDeclineFirstEta = $(this);
    var buttonEdit = $(this).closest(".po-form-item-st2").find(".edit-row-st2-proc");
    var etaDate = $(this).closest(".po-form-item-st2").find(".st2-proc-first-eta-date").val();

    var itemID = $(this).closest(".form-inline").find(".st2-proc-pd-item-id").val();

    inputPurchasingDocumentItemIDs.push(itemID);

    if (etaDate !== '') {
        $.ajax({
            type: "POST",
            url: stage2ProcurementDeclineFirstEta,
            data: JSON.stringify({ 'inputPurchasingDocumentItemIDs': inputPurchasingDocumentItemIDs }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                console.log(inputPurchasingDocumentItemIDs);

                buttonAcceptFirstEta.attr("disabled", "disabled");
                buttonDeclineFirstEta.attr("disabled", "disabled").addClass("selected-negative");
                checkboxItem.attr("disabled", "disabled");
                buttonEdit.attr("style", "visibility:display");

            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
    else {
        alert("Something went wrong!");
    }
});

// edit item Click - PROCUREMENT
$(".edit-row-st2-proc").on("click", function (obj) {
    formRow = $(this).closest(".po-form-item-st2");

    formRow.find(".st2-proc-accept-first-eta").removeAttr("disabled").removeClass("selected");
    formRow.find(".st2-proc-decline-first-eta").removeAttr("disabled").removeClass("selected-negative");
    formRow.find(".st2-proc-checkbox-item").removeAttr("disabled");
});