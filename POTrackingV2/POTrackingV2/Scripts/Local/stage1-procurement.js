// Validations
$(".st1-partial-confirm-qty-proc").on('input focus', function (e) {
    this.reportValidity();
});

//Check / Unchecked All PO Items
$("input.st1-checkbox-all-proc").on("change", function (obj) {
    var confirmAll = $(this);
    $(this).closest(".po-item-section").find("input.st1-checkbox-item-proc").each(function (index) {
        if (confirmAll.prop("checked") === true && $(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true).change();
        } else if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", false).change();
        }
    });
});

//Checked Button Confirm PO -- Undisabling Controller
$(".st1-checkbox-item-proc").on("change", function (obj) {
    var row = $(this).closest(".po-item-data-content");
    if ($(this).prop("checked") === true) {
        row.find(".st1-accept-item-proc").first().removeAttr("disabled");
        row.find(".st1-cancel-item-proc").first().removeAttr("disabled");
    } else {
        row.find(".st1-accept-item-proc").first().attr("disabled", "disabled");
        row.find(".st1-cancel-item-proc").first().attr("disabled", "disabled");
    }
});

//PROCUREMENT click Confirm button Item Quantity - PROCUREMENT
$(".st1-accept-item-proc").on("click", function (obj) {
    obj.preventDefault();

    var stage1ProcurementConfirmItem = $("#stage1ProcurementConfirmItem").val();

    var itemID;
    var inputPurchasingDocumentItems = [];

    var donutProgressUnit = 75.39822368615503 / 8;
    var donutProgress = 75.39822368615503 - 2 * donutProgressUnit;

    var checkboxItem = $(this).closest(".po-item-data-content").find(".st1-checkbox-item-proc").first();
    var buttonAcceptItem = $(this);
    var buttonCancelItem = $(this).closest(".form-inline").find(".st1-cancel-item-proc").first();
    var buttonEditItem = $(this).closest(".po-item-data-content").find(".st1-edit-row-proc").first();

    var partialQuantity = $(this).closest(".po-item-data-content").find(".st1-partial-confirm-qty-proc").first().val();
    var isChild = $(this).closest(".po-item-data-content").attr("child");

    if (isChild === undefined || isChild === null) {
        itemID = $(this).closest(".po-item-data-content").find("input.st1-pd-item-id-proc").first().val();
    }
    else {
        itemID = $(this).closest(".po-item-data-content").find("input.st1-pd-item-id-inner-proc").first().val();
    }

    inputPurchasingDocumentItems.push({
        ID: itemID
    });

    //progress donutpie
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    $.ajax({
        type: "POST",
        url: stage1ProcurementConfirmItem,
        data: JSON.stringify({ 'inputPurchasingDocumentItems': inputPurchasingDocumentItems }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            alert(response.responseText);

            if (response.responseText.includes("Item affected")) {
                buttonAcceptItem.attr("disabled", "disabled").addClass("selected");
                buttonCancelItem.attr("disabled", "disabled").removeClass("selected-negative");
                checkboxItem.attr("disabled", "disabled");
                buttonEditItem.attr("style", "visibility:display");

                donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("2");
            }
          
            
        },
        error: function (xhr, status, error) {
            alert(xhr.status + " : " + error);
        }
    });
});

//PROCUREMENT Confirm All PO -- Save All buat item yang aktif (gak disabled) - PROCUREMENT
$(".st1-accept-all-po-proc").on("click", function (obj) {
    obj.preventDefault();
    var stage1ProcurementConfirmItem = $("#stage1ProcurementConfirmItem").val();

    var inputPurchasingDocumentItems = [];

    var donutProgressUnit = 75.39822368615503 / 8;
    var donutProgress = 75.39822368615503 - 2 * donutProgressUnit;

    $(this).closest(".po-item-section.stage-1").find(".po-form-item-st1").each(function (index) {

        var checkboxItem = $(this).find(".po-item-data-content__outer").find(".st1-checkbox-item-proc");
        var buttonAcceptItem = $(this).find(".po-item-data-content__outer").find(".st1-accept-item-proc");
        var buttonCancelItem = $(this).find(".po-item-data-content__outer").find(".st1-cancel-item-proc");
        var buttonEditItem = $(this).find(".po-item-data-content__outer").find(".st1-edit-row-proc");

        var itemID = $(this).find(".po-item-data-content__outer").find("input.st1-pd-item-id-proc").val();
        var partialQuantity = $(this).find(".po-item-data-content__outer").find(".st1-partial-confirm-qty-proc").val();
        var childRow = $(this).find(".po-item-data-content").find(".po-item-data-content__child");

        //progress donutpie
        var cssRow = $(this).find(".po-item-data-content").prop("class");
        cssRow = cssRow.replace(" ", ".");
        cssRow = "." + cssRow;
        var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

        if (buttonAcceptItem.attr("disabled") !== "disabled" && checkboxItem.attr("disabled") !== "disabled" && checkboxItem.prop("checked") === true) {

            inputPurchasingDocumentItems.push({
                ID: itemID
            });

            checkboxItem.addClass("row-updated");
            buttonAcceptItem.addClass("row-updated-button");
            buttonCancelItem.addClass("row-updated");
            buttonEditItem.addClass("row-updated-link");

            donutRow.find(".donut-chart").first().find("circle").next().addClass("row-updated-donut");
            donutRow.find(".donut-chart").first().next().find("span.mark-donut").addClass("row-updated-donut-text");
        }

        childRow.find(".po-item-data-content").each(function (index) {
            checkboxItem = $(this).find(".st1-checkbox-item-proc");
            buttonAcceptItem = $(this).find(".st1-accept-item-proc");
            buttonCancelItem = $(this).find(".st1-cancel-item-proc");
            buttonEditItem = $(this).find(".st1-edit-row-proc");

            itemID = $(this).find("input.st1-pd-item-id-inner-proc").val();
            partialQuantity = $(this).find(".st1-partial-confirm-qty-proc").val();

            //progress donutpie
            cssRow = $(this).prop("class");
            cssRow = cssRow.replace(" ", ".");
            cssRow = "." + cssRow;
            donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

            if (buttonAcceptItem.attr("disabled") !== "disabled" && checkboxItem.prop("disabled") !== "disabled" && checkboxItem.prop("checked") === true) {

                inputPurchasingDocumentItems.push({
                    ID: itemID
                });

                checkboxItem.addClass("row-updated");
                buttonAcceptItem.addClass("row-updated-button");
                buttonCancelItem.addClass("row-updated");
                buttonEditItem.addClass("row-updated-link");

                donutRow.find(".donut-chart").first().find("circle").next().addClass("row-updated-donut");
                donutRow.find(".donut-chart").first().next().find("span.mark-donut").addClass("row-updated-donut-text");
            }
        });
    });

    $.ajax({
        type: "POST",
        url: stage1ProcurementConfirmItem,
        data: JSON.stringify({ 'inputPurchasingDocumentItems': inputPurchasingDocumentItems }),
        contentType: "application/json; charset=utf-8",
        dataType: "json",
        success: function (response) {
            alert(response.responseText);

            if (response.responseText.includes("Item affected")) {
                $(".row-updated").attr("disabled", "disabled");
                $(".row-updated").removeClass("selected-negative").removeClass("row-updated");

                $(".row-updated-button").attr("disabled", "disabled").addClass("selected");
                $(".row-updated-button").removeClass("row-updated-button");

                $(".row-updated-link").attr("style", "visibility:display");
                $(".row-updated-link").removeClass("row-updated-link");


                $(".row-updated-donut").attr("stroke-dashoffset", donutProgress);
                $(".row-updated-donut-text").text("2");
                $(".row-updated-donut").removeClass("row-updated-donut");
                $(".row-updated-donut-text").removeClass("row-updated-donut-text");
            }
            
        },
        error: function (xhr, status, error) {
            alert(xhr.status + " : " + error);
        }
    });
});

//PROCUREMENT Click cancel item - PROCUREMENT
$(".st1-cancel-item-proc").on("click", function (obj) {

    obj.preventDefault();

    var stage1CancelItem = $("#stage1CancelItem").val();

    var itemID;
    var checkboxItem = $(this).closest(".po-item-data-content").find(".st1-checkbox-item-proc").first();
    var buttonCancelItem = $(this);
    var buttonAcceptItem = $(this).closest(".form-inline").find(".st1-accept-item-proc").first();
    var buttonEditItem = $(this).closest(".po-item-data-content").find(".st1-edit-row-proc").first();

    var isChild = $(this).closest(".po-item-data-content").attr("child");

    if (isChild === undefined || isChild === null) {
        itemID = $(this).closest(".po-item-data-content").find("input.st1-pd-item-id-proc").first().val();
    }
    else {
        itemID = $(this).closest(".po-item-data-content").find("input.st1-pd-item-id-inner-proc").first().val();
    }

    var inputPurchasingDocumentItem = {
        ID: itemID
    };

    $.ajax({
        type: "POST",
        url: stage1CancelItem,
        data: JSON.stringify({ 'inputPurchasingDocumentItem': inputPurchasingDocumentItem }),
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            alert(response.responseText);

            if (response.responseText.includes("is canceled")) {
                buttonAcceptItem.attr("disabled", "disabled").removeClass("selected");
                buttonCancelItem.attr("disabled", "disabled").addClass("selected-negative");
                checkboxItem.attr("disabled", "disabled");
                buttonEditItem.attr("style", "visibility:display");
            }            
        },
        error: function (xhr, status, error) {
            alert(xhr.status + " : " + error);
        }
    });
});

//edit item Click - PROCUREMENT
$(".st1-edit-row-proc").on("click", function (obj) {
    var parentRow = $(this).closest(".po-item-data-content");

    parentRow.find(".st1-accept-item-proc").first().removeAttr("disabled").removeClass("selected");
    parentRow.find(".st1-cancel-item-proc").first().removeAttr("disabled").removeClass("selected-negative");
    parentRow.find(".st1-checkbox-item-proc").first().removeAttr("disabled");
});
