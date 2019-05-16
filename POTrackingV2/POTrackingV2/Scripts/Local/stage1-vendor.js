﻿// Validations
$(".st1-partial-confirm-qty").on('input focus', function (e) {
    this.reportValidity();
});

$(".form-date.st1-confirmed-date").on('input focus', function (e) {
    this.reportValidity();
});

$(".form-date.st1-partial-date").on('input focus', function (e) {
    this.reportValidity();
});

//Check / Unchecked All PO Items
$("input.st1-checkbox-all").on("change", function (obj) {
    var confirmAll = $(this);
    $(this).closest(".po-item-section").find("input.st1-checkbox-item").each(function (index) {
        if (confirmAll.prop("checked") === true && $(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true).change();
        } else if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", false).change();
        }
    });
});

/*Checked Button Confirm PO -- Undisabling Controller lama
$(".st1-checkbox-item").on("change", function (obj) {
    var row = $(this).closest(".input-group.mr-2");
    if ($(this).prop("checked") == true) {
        row.find(".st1-pd-item-confirm-qty").removeAttr("disabled");
        row.find(".btn.btn-positive").removeAttr("disabled");
        row.find(".btn.btn-negative").removeAttr("disabled");
        row.find(".st1-pd-item-confirm-qty").val(row.find(".st1-pd-item-confirm-qty").attr("placeholder").replace('Qty available ', ''));

    } else {
        row.find(".st1-pd-item-confirm-qty").attr("disabled", "disabled");
        row.find(".btn.btn-positive").attr("disabled", "disabled");
        row.find(".btn.btn-negative").attr("disabled", "disabled");
        row.find(".st1-pd-item-confirm-qty").val("");
    }
});*/

//Checked Button Confirm PO -- Undisabling Controller
$(".st1-checkbox-item").on("change", function (obj) {
    var row = $(this).closest(".po-item-data-content");
    if ($(this).prop("checked") === true) {
        var latestDeliveryDate = row.find(".st1-pd-item-delivery-date").val();

        row.find(".st1-confirmed-date").removeAttr("disabled");
        row.find(".st1-delivery-method").removeAttr("disabled");
        row.find(".st1-partial-confirm-qty").removeAttr("disabled");
        row.find(".st1-accept-item").removeAttr("disabled");
        row.find(".st1-cancel-item").removeAttr("disabled");
        row.find(".st1-confirmed-date").val(latestDeliveryDate);
        row.find(".st1-partial-date").first().val(latestDeliveryDate);

        if (row.find(".st1-delivery-method").val() === "partial") {

            row.find(".st1-partial-date").removeAttr("disabled");
            row.find(".st1-partial-date").first().attr("disabled", "disabled");

            row.find(".st1-add-row").attr("style", "visibility:display");
            row.find(".st1-del-row").attr("style", "visibility:display");
        }

    } else {
        row.find(".st1-confirmed-date").attr("disabled", "disabled");
        row.find(".st1-delivery-method").attr("disabled", "disabled");
        row.find(".st1-partial-confirm-qty").attr("disabled", "disabled");
        row.find(".st1-partial-date").attr("disabled", "disabled");
        row.find(".st1-accept-item").attr("disabled", "disabled");
        row.find(".st1-cancel-item").attr("disabled", "disabled");
        row.find(".st1-confirmed-date").val("");
        row.find(".st1-partial-date").val("");

        row.find(".st1-add-row").attr("style", "visibility:hidden");
        row.find(".st1-del-row").attr("style", "visibility:hidden");
    }
});


//Vendor Confirm All PO -- Save All buat item yang aktif (gak disabled)
$(".st1-accept-all-po").on("click", function (obj) {
    obj.preventDefault();
    var inputPurchasingDocumentItems = [];

    var donutProgressUnit = 75.39822368615503 / 12;
    var donutProgress = 75.39822368615503 - 1 * donutProgressUnit;

    $(this).closest(".po-item-section.stage-1").find(".po-form-item-st1").each(function (index) {

        var allQuantity = parseInt(0);
        var validateMaxQuantity = true;
        var validateMinQuantity = true;
        var validateDate = true;
        var dateValidationArray = [];
        var dateParts = [];
        var partialDateObject;
        var itemPartialPurchasingDocumentItems = [];

        var checkboxItem = $(this).find(".po-item-data-content__outer").find(".st1-checkbox-item");
        var inputConfirmedDate = $(this).find(".po-item-data-content__outer").find(".st1-confirmed-date");
        var inputDeliveryMethod = $(this).find(".po-item-data-content__outer").find(".st1-delivery-method");
        var inputPartialQuantity = $(this).find(".po-item-data-content__outer").find(".st1-partial-confirm-qty");
        var inputPartialDate = $(this).find(".po-item-data-content__outer").find(".st1-partial-date");
        var buttonAddRow = $(this).find(".po-item-data-content__outer").find(".st1-add-row");
        var buttonDelRow = $(this).find(".po-item-data-content__outer").find(".st1-del-row");
        var buttonAcceptItem = $(this).find(".po-item-data-content__outer").find(".st1-accept-item");
        var buttonCancelItem = $(this).find(".po-item-data-content__outer").find(".st1-cancel-item");
        var editButton = $(this).find(".po-item-data-content__outer").find(".st1-edit-row");
        var childRow = $(this).find(".po-item-data-content__child");

        var itemID = $(this).find(".po-item-data-content__outer").find("input.st1-pd-item-id").val();
        var maxQuantity = $(this).find(".po-item-data-content__outer").find("input.st1-pd-item-max-qty").val();
        var maxDeliveryDate = $(this).find(".po-item-data-content__outer").find(".st1-pd-item-delivery-date").val();
        var confirmedDate = inputConfirmedDate.val();
        var deliveryMethod = inputDeliveryMethod.val();
        var partialQuantity = inputPartialQuantity.val();
        var partialDate = inputPartialDate.val();

        confirmedDate = reverseDayMonth(confirmedDate);

        allQuantity = allQuantity + parseInt(partialQuantity);
        partialDateObject = reverseDayMonth(partialDate);

        if (partialQuantity === '') {
            validateMinQuantity = false;
            inputPartialQuantity.focus();
        }

        if (confirmedDate === '' || partialDate === '') {
            validateDate = false;
            inputPartialDate.focus();
        }

        itemPartialPurchasingDocumentItems.push({
            ID: itemID,
            ConfirmedQuantity: partialQuantity,
            ConfirmedDate: partialDate
        });

        dateValidationArray.push({
            date: partialDate
        });

        //progress donutpie
        cssRow = $(this).find(".po-item-data-content").prop("class");
        cssRow = cssRow.replace(" ", ".");
        cssRow = "." + cssRow;
        var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

        if (deliveryMethod === "partial") {
            childRow.find(".po-item-data-content").each(function (index) {
                var inputChildPartialQuantity = $(this).find(".st1-partial-confirm-qty");
                var inputChildPartialDate = $(this).find(".st1-partial-date");
                partialQuantity = inputChildPartialQuantity.val();
                partialDate = inputChildPartialDate.val();

                allQuantity = allQuantity + parseInt(partialQuantity);
                partialDateObject = reverseDayMonth(partialDate);

                if (partialQuantity === '') {
                    validateMinQuantity = false;
                    inputChildPartialQuantity.focus();
                }
                if (partialDate === '') {
                    validateDate = false;
                    inputChildPartialDate.focus();
                }
                dateValidationArray.forEach(function (item, index) {
                    console.log(validateDate);
                    console.log("===");
                    console.log(partialDate);
                    console.log("VS");
                    console.log(item.date);
                    console.log("===");
                    if (partialDate === item.date) {
                        validateDate = false;
                        inputChildPartialDate.focus();
                    }
                });

                itemPartialPurchasingDocumentItems.push({
                    ParentID: itemID,
                    ConfirmedQuantity: partialQuantity,
                    ConfirmedDate: partialDate
                });

                dateValidationArray.push({
                    date: partialDate
                });
            });
        }

        if (allQuantity > maxQuantity) {
            validateMaxQuantity = false;
        }
        if (allQuantity < 0) {
            validateMinQuantity = false;
        }

        if (inputPartialQuantity.attr("disabled") !== "disabled" && checkboxItem.prop("checked") === true && checkboxItem.attr("disabled") !== "disabled") {
            if (validateMaxQuantity === true) {
                if (validateMinQuantity === true) {
                    if (validateDate === true) {
                        itemPartialPurchasingDocumentItems.forEach(function (item, index) {
                            inputPurchasingDocumentItems.push(item);
                        });

                        checkboxItem.addClass("row-updated");
                        inputConfirmedDate.addClass("row-updated");
                        inputDeliveryMethod.addClass("row-updated");
                        inputPartialQuantity.addClass("row-updated");
                        inputPartialDate.addClass("row-updated");
                        buttonAddRow.addClass("row-updated-link-negative");
                        buttonDelRow.addClass("row-updated-link-negative");
                        buttonAcceptItem.addClass("row-updated-button");
                        buttonCancelItem.addClass("row-updated");
                        editButton.addClass("row-updated-link");
                        donutRow.addClass("row-updated-donut");

                        if (deliveryMethod === "partial") {
                            childRow.find(".po-item-data-content").each(function (index) {
                                $(this).find(".st1-partial-confirm-qty").addClass("row-updated");
                                $(this).find(".st1-partial-date").addClass("row-updated");

                                cssRow = $(this).prop("class");
                                cssRow = cssRow.replace(" ", ".");
                                cssRow = "." + cssRow;
                                $(this).closest(".custom-scrollbar").prev().find(cssRow).addClass("row-updated-donut");
                            });
                        }
                    }
                    else {
                        alert("Tanggal tidak boleh sama atau kosong");
                    }
                }
                else {
                    alert("Kuantitas tidak boleh kurang dari 0");
                }
            }
            else {
                alert("Kuantitas tidak boleh lebih dari permintaan");
            }
        }
    });

    if (inputPurchasingDocumentItems.length > 0) {
        $.ajax({
            type: "POST",
            url: "VendorConfirmQuantity",
            data: JSON.stringify({ 'inputPurchasingDocumentItems': inputPurchasingDocumentItems }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                alert(response.responseText);

                $(".row-updated").attr("disabled", "disabled");
                $(".row-updated-button").attr("disabled", "disabled").addClass("selected");
                $(".row-updated-link").attr("style", "visibility:display");
                $(".row-updated-link-negative").attr("style", "visibility:hidden");
                $(".row-updated-donut").find(".donut-chart").find("circle").next().attr("stroke-dashoffset", donutProgress);
                $(".row-updated-donut").find(".donut-chart").next().find("span.mark-donut").text("1");

                $(".row-updated").removeClass("selected-negative").removeClass("row-updated");
                $(".row-updated-button").removeClass("row-updated-button");
                $(".row-updated-link").removeClass("row-updated-link");
                $(".row-updated-link-negative").removeClass("row-updated-link-negative");
                $(".row-updated-donut").removeClass("row-updated-donut");
            }
        });
    }
    else {
        alert("No Item affected");
    }

});

//Vendor click Confirm button Item Quantity
$(".st1-accept-item").on("click", function (obj) {
    obj.preventDefault();
    var inputPurchasingDocumentItems = [];
    var dateValidationArray = [];
    var allQuantity = parseInt(0);
    var validateMaxQuantity = true;
    var validateMinQuantity = true;
    var validateDate = true;
    var cssRow;
    var partialDateObject;

    var donutProgressUnit = 75.39822368615503 / 12;
    var donutProgress = 75.39822368615503 - 1 * donutProgressUnit;

    var checkboxItem = $(this).closest(".po-item-data-content__outer").find(".st1-checkbox-item");
    var inputConfirmedDate = $(this).closest(".po-item-data-content__outer").find(".st1-confirmed-date");
    var inputDeliveryMethod = $(this).closest(".po-item-data-content__outer").find(".st1-delivery-method");
    var inputPartialQuantity = $(this).closest(".po-item-data-content__outer").find(".st1-partial-confirm-qty");
    var inputPartialDate = $(this).closest(".po-item-data-content__outer").find(".st1-partial-date");
    var inputIsEdit = $(this).closest(".po-item-data-content__outer").find(".st1-pd-item-is-edit");
    var buttonAddRow = $(this).closest(".po-item-data-content__outer").find(".st1-add-row");
    var buttonDelRow = $(this).closest(".po-item-data-content__outer").find(".st1-del-row");
    var buttonAcceptItem = $(this);
    var buttonCancelItem = $(this).closest(".form-inline").find(".st1-cancel-item");
    var editButton = $(this).closest(".po-item-data-content__outer").find(".st1-edit-row");
    var childRow = $(this).closest(".po-item-data-content").find(".po-item-data-content__child");

    var itemID = $(this).closest(".po-item-data-content__outer").find("input.st1-pd-item-id").val();
    var maxQuantity = $(this).closest(".po-item-data-content__outer").find("input.st1-pd-item-max-qty").val();
    var maxDeliveryDate = $(this).closest(".po-item-data-content__outer").find(".st1-pd-item-delivery-date").val();
    var isEdit = $(this).closest(".po-item-data-content__outer").find(".st1-pd-item-is-edit").val();
    var confirmedDate = inputConfirmedDate.val();
    var deliveryMethod = inputDeliveryMethod.val();
    var partialQuantity = inputPartialQuantity.val();
    var partialDate = inputPartialDate.val();

    confirmedDate = reverseDayMonth(confirmedDate);

    allQuantity = allQuantity + parseInt(partialQuantity);
    partialDateObject = reverseDayMonth(partialDate);

    if (partialQuantity === '') {
        validateMinQuantity = false;
        inputPartialQuantity.focus();
    }

    if (confirmedDate === '' || partialDate === '') {
        validateDate = false;
        inputPartialDate.focus();
    }

    inputPurchasingDocumentItems.push({
        ID: itemID,
        ConfirmedQuantity: partialQuantity,
        ConfirmedDate: partialDate
    });

    dateValidationArray.push({
        date: partialDate
    });

    //progress donutpie
    cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    $(this).closest(".custom-scrollbar").prev().find(cssRow).addClass("row-updated-donut");

    if (deliveryMethod === "partial") {
        childRow.find(".po-item-data-content").each(function (index) {
            var inputChildPartialQuantity = $(this).find(".st1-partial-confirm-qty");
            var inputChildPartialDate = $(this).find(".st1-partial-date");
            partialQuantity = inputChildPartialQuantity.val();
            partialDate = inputChildPartialDate.val();

            allQuantity = allQuantity + parseInt(partialQuantity);
            partialDateObject = reverseDayMonth(partialDate);

            if (partialQuantity === '') {
                validateMinQuantity = false;
                inputChildPartialQuantity.focus();
            }
            if (partialDate === '') {
                validateDate = false;
                inputChildPartialDate.focus();
            }
            dateValidationArray.forEach(function (item, index) {
                if (partialDate === item.date) {
                    validateDate = false;
                    inputChildPartialDate.focus();
                }
            });

            inputPurchasingDocumentItems.push({
                ParentID: itemID,
                ConfirmedQuantity: partialQuantity,
                ConfirmedDate: partialDate
            });

            dateValidationArray.push({
                date: partialDate
            });

            cssRow = $(this).closest(".po-item-data-content").prop("class");
            cssRow = cssRow.replace(" ", ".");
            cssRow = "." + cssRow;
            $(this).closest(".custom-scrollbar").prev().find(cssRow).addClass("row-updated-donut");
        });
    }

    console.log(inputPurchasingDocumentItems);

    if (allQuantity > maxQuantity) {
        validateMaxQuantity = false;
    }
    if (allQuantity < 0) {
        validateMinQuantity = false;
    }

    //progress donutpie
    //var cssRow = $(this).closest(".po-item-data-content").prop("class");
    //cssRow = cssRow.replace(" ", ".");
    //cssRow = "." + cssRow;
    //var informationDataContent = $(this).closest(".custom-scrollbar").prev().find(cssRow);
    //var donutProgressUnit = 75.39822368615503 / 12;
    //var donutProgress = 75.39822368615503 - 1 * donutProgressUnit;

    if (validateMaxQuantity === true) {
        if (validateMinQuantity === true) {
            if (validateDate === true) {
                if (isEdit === "false") {
                    $.ajax({
                        type: "POST",
                        url: "VendorConfirmQuantity",
                        data: JSON.stringify({ 'inputPurchasingDocumentItems': inputPurchasingDocumentItems }),
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: function (response) {
                            alert(response.responseText);

                            checkboxItem.attr("disabled", "disabled");
                            inputConfirmedDate.attr("disabled", "disabled");
                            inputDeliveryMethod.attr("disabled", "disabled");
                            inputPartialQuantity.attr("disabled", "disabled");
                            inputPartialDate.attr("disabled", "disabled");
                            buttonAddRow.attr("style", "visibility:hidden");
                            buttonDelRow.attr("style", "visibility:hidden");
                            buttonAcceptItem.attr("disabled", "disabled").addClass("selected");
                            buttonCancelItem.attr("disabled", "disabled").removeClass("selected-negative");
                            editButton.attr("style", "visibility:display");

                            if (deliveryMethod === "partial") {
                                childRow.find(".po-item-data-content").each(function (index) {
                                    $(this).find(".st1-partial-confirm-qty").attr("disabled", "disabled");
                                    $(this).find(".st1-partial-date").attr("disabled", "disabled");
                                });
                            }

                            $(".row-updated-donut").find(".donut-chart").find("circle").next().attr("stroke-dashoffset", donutProgress);
                            $(".row-updated-donut").find(".donut-chart").next().find("span.mark-donut").text("1");
                            $(".row-updated-donut").removeClass("row-updated-donut");
                        },
                        error: function (xhr, status, error) {
                            alert(xhr.status + " : " + error);
                        }
                    });
                }
                else {
                    $.ajax({
                        type: "POST",
                        url: "VendorEditItem",
                        data: JSON.stringify({ 'inputPurchasingDocumentItems': inputPurchasingDocumentItems }),
                        contentType: "application/json; charset=utf-8",
                        dataType: "json",
                        success: function (response) {
                            alert(response.responseText);

                            inputIsEdit.val("false");
                            checkboxItem.attr("disabled", "disabled");
                            inputConfirmedDate.attr("disabled", "disabled");
                            inputDeliveryMethod.attr("disabled", "disabled");
                            inputPartialQuantity.attr("disabled", "disabled");
                            inputPartialDate.attr("disabled", "disabled");
                            buttonAddRow.attr("style", "visibility:hidden");
                            buttonDelRow.attr("style", "visibility:hidden");
                            buttonAcceptItem.attr("disabled", "disabled").addClass("selected");
                            buttonCancelItem.attr("disabled", "disabled").removeClass("selected-negative");
                            editButton.attr("style", "visibility:display");

                            if (deliveryMethod === "partial") {
                                childRow.find(".po-item-data-content").each(function (index) {
                                    $(this).find(".st1-partial-confirm-qty").attr("disabled", "disabled");
                                    $(this).find(".st1-partial-date").attr("disabled", "disabled");
                                });
                            }

                            $(".row-updated-donut").find(".donut-chart").find("circle").next().attr("stroke-dashoffset", donutProgress);
                            $(".row-updated-donut").find(".donut-chart").next().find("span.mark-donut").text("1");
                            $(".row-updated-donut").removeClass("row-updated-donut");
                        },
                        error: function (xhr, status, error) {
                            alert(xhr.status + " : " + error);
                        }
                    });
                }
            }
            else {
                alert("Tanggal tidak boleh sama atau kosong");
            }
        }
        else {
            alert("Kuantitas tidak boleh kurang dari 0");
        }
    }
    else {
        alert("Kuantitas tidak boleh lebih dari permintaan");
    }
});

//Vendor Click cancel item
$(".st1-cancel-item").on("click", function (obj) {

    var checkboxItem = $(this).closest(".po-item-data-content__outer").find(".st1-checkbox-item");
    var inputConfirmedDate = $(this).closest(".po-item-data-content__outer").find(".st1-confirmed-date");
    var inputDeliveryMethod = $(this).closest(".po-item-data-content__outer").find(".st1-delivery-method");
    var inputPartialQuantity = $(this).closest(".po-item-data-content__outer").find(".st1-partial-confirm-qty");
    var inputPartialDate = $(this).closest(".po-item-data-content__outer").find(".st1-partial-date");
    var buttonCancelItem = $(this);
    var buttonAcceptItem = $(this).closest(".form-inline").find(".st1-accept-item");
    var childRow = $(this).closest(".po-item-data-content").find(".po-item-data-content__child");

    var itemID = $(this).closest(".po-item-data-content__outer").find("input.st1-pd-item-id").val();
    var deliveryMethod = inputDeliveryMethod.val();

    var inputPurchasingDocumentItem = {
        ID: itemID
    };

    console.log(inputPurchasingDocumentItem);

    $.ajax({
        type: "POST",
        url: "CancelItem",
        data: JSON.stringify({ 'inputPurchasingDocumentItem': inputPurchasingDocumentItem }),
        contentType: "application/json; charset=utf-8",
        success: function (response) {
            alert(response.responseText);

            checkboxItem.attr("disabled", "disabled");
            inputConfirmedDate.attr("disabled", "disabled");
            inputDeliveryMethod.attr("disabled", "disabled");
            inputPartialQuantity.attr("disabled", "disabled");
            inputPartialDate.attr("disabled", "disabled");
            buttonAcceptItem.attr("disabled", "disabled").removeClass("selected");
            buttonCancelItem.attr("disabled", "disabled").addClass("selected-negative");


            if (deliveryMethod === "partial") {
                childRow.find(".po-item-data-content").each(function (index) {
                    $(this).find(".st1-partial-confirm-qty").attr("disabled", "disabled");
                    $(this).find(".st1-partial-date").attr("disabled", "disabled");
                });
            }
        },
        error: function (xhr, status, error) {
            alert(xhr.status + " : " + error);
        }
    });
});

/*edit item Click
$(".edit-row-st1").on("click", function (obj) {
    $(this).parent().find("button.btn-negative.st1-cancel-item").removeAttr("disabled").removeClass("selected-negative");
    $(this).closest(".input-group").find("input.st1-pd-item-confirm-qty").removeAttr("disabled");
    $(this).closest(".input-group").find("button.btn-positive.st1-confirm-quantity").removeAttr("disabled").removeClass("selected");
});*/

//edit item Click
$(".st1-edit-row").on("click", function (obj) {
    var parentRow = $(this).closest(".po-item-data-content__outer");
    var childRow = $(this).closest(".po-item-data-content").find(".po-item-data-content__child");
    var deliveryMethod = parentRow.find(".st1-delivery-method").val();

    parentRow.find(".st1-pd-item-is-edit").val("true");
    parentRow.find(".st1-confirmed-date").removeAttr("disabled");
    parentRow.find(".st1-delivery-method").removeAttr("disabled");
    parentRow.find(".st1-partial-confirm-qty").removeAttr("disabled");

    if (deliveryMethod === "partial") {
        parentRow.find(".st1-add-row").attr("style", "visibility:display");
        parentRow.find(".st1-del-row").attr("style", "visibility:display");
    }

    childRow.find(".po-item-data-content").each(function () {
        $(this).find(".st1-partial-confirm-qty").removeAttr("disabled");
        $(this).find(".st1-partial-date").removeAttr("disabled");
    });

    parentRow.find(".st1-accept-item").removeAttr("disabled").removeClass("selected");
    parentRow.find(".st1-cancel-item").removeAttr("disabled").removeClass("selected-negative");
});


$(".st1-confirmed-date").on("change", function (obj) {
    var confirmedDate = $(this).val();
    $(this).closest(".po-item-data-content").find(".st1-partial-date").first().val(confirmedDate);
    /*$(this).closest(".po-item-data-content").find(".st1-partial-date").each(function () {
        if ($(this).attr("disabled") === "disabled") {
            $(this).attr("value", confirmedDate);
        }
    */
});


// Add Delete Child Row
var rowHeights = [];
var totalRowHeight = 0;

function increaseRowHeight(rowPOItemDataContent) {
    var tempNewArray = [];
    var tempNewMaxHeight = 0;
    var newRowExist = 1;

    while (newRowExist > 0) {
        if (rowPOItemDataContent.find(".row-" + newRowExist).length > 0) {
            tempNewArray = [];
            tempNewMaxHeight = 0;
            rowPOItemDataContent.find(".row-" + newRowExist).each(function () {
                tempNewArray.push($(this).height());
            });
            tempNewMaxHeight = Math.max(...tempNewArray);
            rowPOItemDataContent.find(".row-" + newRowExist).height(tempNewMaxHeight);
            newRowExist++;
        } else {
            newRowExist = -1;
        }
    }
    tempNewArray = [];
    totalRowHeight = 0;
}

function decreaseRowHeight(rowPOItemDataContent) {
    var tempNewArray = [];
    var tempNewMinHeight = 0;
    var newRowExist = 1;

    while (newRowExist > 0) {
        if (rowPOItemDataContent.find(".row-" + newRowExist).length > 0) {
            tempNewArray = [];
            rowPOItemDataContent.find(".row-" + newRowExist).each(function () {
                tempNewArray.push($(this).height());
            });
            tempNewMinHeight = Math.min(...tempNewArray);
            rowPOItemDataContent.find(".row-" + newRowExist).height(tempNewMinHeight);
            newRowExist++;
        } else {
            newRowExist = -1;
        }
    }
    tempNewArray = [];
    totalRowHeight = 0;
}

//delivery method onchange
$(".st1-delivery-method").on("change", function (obj) {
    if ($(this).val() === "partial") {
        var currentRowHeight;
        var newChildHeight;
        var initialHeight;
        var row = $(this).closest(".po-item-data-content").attr("row");
        var stage1 = $(this).closest(".stage-1").find(".po-item-data-content.row-" + row + " .po-item-data-content__child");
        var itemID = $(this).closest(".stage-1").find(".po-item-data-content.row-" + row + " .po-item-data-content__outer .pd-item-id").val();
        var maxDate = $(this).closest(".stage-1").find(".po-item-data-content.row-" + row + " .po-item-data-content__outer .st1-confirmed-date").val();
        var currentRow = 1;

        $(this).closest(".po-item-data-content__outer").find(".st1-add-row").attr("style", "visibility:display");
        $(this).closest(".po-item-data-content__outer").find(".st1-del-row").attr("style", "visibility:display");

        $(stage1).append("<div class='po-item-data-content row-" + row + "-" + currentRow + "' row='" + row + "' child='" + currentRow + "'>" +
            "<div class='po-item-data-header__column' style='visibility:hidden'>" +
            "<div class='form-inline'>" +
            "<div class='input-group'>" +
            "<label class='checkbox-custom__label'>" +
            "<input class='checkbox-custom' checked='' type='checkbox' disabled>" +
            "<span class='checkmark'></span>" +
            "</label>" +
            "<div class='form-group'>" +
            "<input type='text' class='form-control form-date' required autofocus pattern='^([0-2][0-9]|(3)[0-1])(\\/)(((0)[0-9])|((1)[0-2]))(\\/)\\d{4}$' maxlength='10' placeholder='dd/mm/yyyy' disabled>" +
            "<i class='fas fa-calendar-alt form-type-icon'></i>" +
            "</div>" +
            "</div>" +
            "</div>" +
            "</div>" +
            "<div class='po-item-data-header__column'  style='visibility:hidden'>" +
            "<div class='form-inline'>" +
            "<div class='form-group'>" +
            "<select class='form-control'>" +
            "<option>Full</option>" +
            "<option>Partial</option>" +
            "</select>" +
            "</div>" +
            "</div>" +
            "</div>" +
            "<div class='po-item-data-header__column'>" +
            "<div class='form-inline'>" +
            "<input type='number' class='form-control st1-partial-confirm-qty' autofocus required pattern='[0-9]' min='1' placeholder='Quantity'>" +
            "</div>" +
            "</div>" +
            "<div class='po-item-data-header__column'>" +
            "<div class='form-inline'>" +
            "<div class='form-group'>" +
            "<input type='text' class='form-control form-date with-icon st1-partial-date' required autofocus pattern='^([0-2][0-9]|(3)[0-1])(\\/)(((0)[0-9])|((1)[0-2]))(\\/)\\d{4}$' maxlength='10' placeholder='dd/mm/yyyy'>" +
            "<i class='fas fa-calendar-alt form-type-icon'></i>" +
            "</div>" +
            "</div>" +
            "</div>" +
            "</div>");

        $('.form-date').datepicker({ autoHide: true, format: 'dd/mm/yyyy' });

        $(".st1-partial-confirm-qty").on('input focus', function (e) {
            this.reportValidity();
        });

        $(".form-date.st1-confirmed-date").on('input focus', function (e) {
            this.reportValidity();
        });

        $(".form-date.st1-partial-date").on('input focus', function (e) {
            this.reportValidity();
        });

        //currentRowHeight = $(this).closest(".po-item-data-content").height();
        //newChildHeight = $(this).closest(".po-item-data-content").find(".po-item-data-content__child").height();
        //rowHeights.push(currentRowHeight);
        //initialHeight = rowHeights[0];
        //totalRowHeight = initialHeight + newChildHeight;
        //$(this).closest(".po-item-data-content").height(totalRowHeight);
        //increaseRowHeight($(this).closest(".po-item-container"));

        currentRowHeight = $(this).closest(".po-item-data-content").height();
        newChildHeight = $(this).closest(".po-item-data-content").find(".po-item-data-content__child").outerHeight(true);
        rowHeights.push(currentRowHeight);
        initialHeight = rowHeights[0];
        totalRowHeight = initialHeight + newChildHeight;
        $(this).closest(".po-item-data-content").height(totalRowHeight);
        increaseRowHeight($(this).closest(".po-item-container"));
    } else {

        currentRowHeight = $(this).closest(".po-item-data-content").outerHeight(true);
        newChildHeight = $(this).closest(".po-item-data-content").find(".po-item-data-content__child").outerHeight(true);
        totalRowHeight = currentRowHeight - newChildHeight;
        $(this).closest(".po-item-data-content").height(rowHeights[0]);
        decreaseRowHeight($(this).closest(".po-item-container"));

        row = $(this).closest(".po-item-data-content").attr("row");
        maxDate = $(this).closest(".stage-1").find(".po-item-data-content.row-" + row + " .po-item-data-content__outer .st1-confirmed-date").val();

        $(this).closest(".po-item-data-content__outer").find(".st1-add-row").attr("style", "visibility:hidden");
        $(this).closest(".po-item-data-content__outer").find(".st1-del-row").attr("style", "visibility:hidden");
        $(this).closest(".po-item-data-content__outer").find(".st1-partial-date").attr("disabled", "disabled");
        $(this).closest(".po-item-data-content__outer").find(".st1-partial-date").attr("value", maxDate);
        $(this).closest(".po-item-data-content").find(".po-item-data-content__child").empty();
        $(this).closest(".custom-scrollbar").find(".po-item-section.stage-2 [child]").empty();
        $(this).closest(".po-item-container").find('.po-item-section-body').removeAttr("style");
    }
});

//add rows
$(".st1-add-row").on("click", function (obj) {
    var currentRowHeight;
    var newChildHeight;
    var initialHeight;
    var row = $(this).closest(".po-item-data-content").attr("row");
    var stage1 = $(this).closest(".stage-1").find(".po-item-data-content.row-" + row + " .po-item-data-content__child");
    var currentRow = stage1.find(".po-item-data-content").last().attr("child");
    var itemID = $(this).closest(".stage-1").find(".po-item-data-content.row-" + row + " .po-item-data-content__outer .pd-item-id").val();
    var maxDate = $(this).closest(".stage-1").find(".po-item-data-content.row-" + row + " .po-item-data-content__outer .st1-confirmed-date").val();

    if (typeof (currentRow) === "undefined") {
        currentRow = 1;
    } else {
        currentRow++;
    }

    $(stage1).append("<div class='po-item-data-content row-" + row + "-" + currentRow + "' row='" + row + "' child='" + currentRow + "'>" +
        "<div class='po-item-data-header__column' style='visibility:hidden'>" +
        "<div class='form-inline'>" +
        "<div class='input-group'>" +
        "<label class='checkbox-custom__label'>" +
        "<input class='checkbox-custom' checked='' type='checkbox' disabled>" +
        "<span class='checkmark'></span>" +
        "</label>" +
        "<div class='form-group'>" +
        "<input type='text' class='form-control form-date' required autofocus pattern='^([0-2][0-9]|(3)[0-1])(\\/)(((0)[0-9])|((1)[0-2]))(\\/)\\d{4}$' maxlength='10' placeholder='dd/mm/yyyy' disabled>" +
        "<i class='fas fa-calendar-alt form-type-icon'></i>" +
        "</div>" +
        "</div>" +
        "</div>" +
        "</div>" +
        "<div class='po-item-data-header__column'  style='visibility:hidden'>" +
        "<div class='form-inline'>" +
        "<div class='form-group'>" +
        "<select class='form-control'>" +
        "<option>Full</option>" +
        "<option>Partial</option>" +
        "</select>" +
        "</div>" +
        "</div>" +
        "</div>" +
        "<div class='po-item-data-header__column'>" +
        "<div class='form-inline'>" +
        "<input type='number' class='form-control st1-partial-confirm-qty' autofocus required pattern='[0-9]' min='1' placeholder='Quantity'>" +
        "</div>" +
        "</div>" +
        "<div class='po-item-data-header__column'>" +
        "<div class='form-inline'>" +
        "<div class='form-group'>" +
        "<input type='text' class='form-control form-date with-icon st1-partial-date' required autofocus pattern='^([0-2][0-9]|(3)[0-1])(\\/)(((0)[0-9])|((1)[0-2]))(\\/)\\d{4}$' maxlength='10' placeholder='dd/mm/yyyy'>" +
        "<i class='fas fa-calendar-alt form-type-icon'></i>" +
        "</div>" +
        "</div>" +
        "</div>" +
        "</div>");

    $('.form-date').datepicker({ autoHide: true, format: 'dd/mm/yyyy' });

    $(".st1-partial-confirm-qty").on('input focus', function (e) {
        this.reportValidity();
    });

    $(".form-date.st1-confirmed-date").on('input focus', function (e) {
        this.reportValidity();
    });

    $(".form-date.st1-partial-date").on('input focus', function (e) {
        this.reportValidity();
    });

    currentRowHeight = $(this).closest(".po-item-data-content").outerHeight(true);
    newChildHeight = $(this).closest(".po-item-data-content").find(".po-item-data-content__child").outerHeight(true);
    rowHeights.push(currentRowHeight);
    initialHeight = rowHeights[0];
    totalRowHeight = initialHeight + newChildHeight;
    $(this).closest(".po-item-data-content").height(totalRowHeight);
    increaseRowHeight($(this).closest(".po-item-container"));
});

//delete rows
$(".st1-del-row").on("click", function (obj) {
    var currentRowHeight;
    var newChildHeight;
    var initialHeight;
    var row = $(this).closest(".po-item-data-content").attr("row");
    var stage1 = $(this).closest(".stage-1").find(".po-item-data-content.row-" + row + " .po-item-data-content__child");
    var lastRow = stage1.find(".po-item-data-content").last();
    var lastChild = lastRow.attr("child");

    if (lastChild > 1) {

        lastRow.remove();

        currentRowHeight = $(this).closest(".po-item-data-content").outerHeight(true);
        newChildHeight = $(this).closest(".po-item-data-content").find(".po-item-data-content__child").outerHeight(true);
        rowHeights.push(currentRowHeight);
        initialHeight = rowHeights[0];
        rowHeightDifference = currentRowHeight - (initialHeight + newChildHeight);
        decreasedHeight = currentRowHeight - rowHeightDifference;
        $(this).closest(".po-item-data-content").height(decreasedHeight);
        decreaseRowHeight($(this).closest(".po-item-container"));
    }
});

