// Validations
$(".st2-first-eta-date").on('input focus', function (e) {
    this.setCustomValidity('');
    var thisDate = $(this).val();
    var minDate = $(this).attr("mindate");
    if ((!isNaN(thisDate) || thisDate !== '') && (!isNaN(minDate) || minDate !== '')) {
        if (reverseDayMonth(thisDate) < reverseDayMonth(minDate)) {
            this.setCustomValidity("Value must be more than or equal " + $(this).attr("mindate"));
        }
    }
    this.reportValidity();
});

//Check / Unchecked All PO Items
$("input.st2-checkbox-all").on("change", function (obj) {
    var checkboxAll = $(this);
    $(this).closest(".po-item-section").find("input.checkbox-custom.st2-checkbox-item").each(function (index) {
        if (checkboxAll.prop("checked") === true && $(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true).change();
        } else if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", false).change();
        }
    });
});

//Checked Button Confirm PO -- Undisabling Controller
$(".st2-checkbox-item").on("change", function (obj) {
    var row = $(this).closest(".po-item-data-header__column.stage2-confirm-eta");

    if ($(this).prop("checked") === true) {
        row.find("input.st2-first-eta-date").removeAttr("disabled");
        row.find("button.st2-confirm-first-eta").removeAttr("disabled");
    } else {
        row.find("input.st2-first-eta-date").attr("disabled", "disabled");
        row.find("button.st2-confirm-first-eta").attr("disabled", "disabled");
    }
});

// Vendor one input ETA DATE
$(".st2-confirm-first-eta").on("click", function (obj) {

    obj.preventDefault();

    var inputETAHistories = [];

    var checkboxItem = $(this).closest(".form-inline").find(".st2-checkbox-item");
    var inputFirstEtaDate = $(this).closest(".form-inline").find(".st2-first-eta-date");
    var buttonConfirmFirstEta = $(this);
    var buttonEdit = $(this).closest(".po-form-item-st2").find(".edit-row-st2");

    var itemID = $(this).closest(".form-inline").find(".st2-pd-item-id").val();
    var minimumDate = $(this).closest(".form-inline").find(".st2-first-eta-date").attr("mindate");
    var etaDate = inputFirstEtaDate.val();
    var etaDateObject = reverseDayMonth(etaDate);

    inputETAHistories.push({
        PurchasingDocumentItemID: itemID,
        ETADate: etaDateObject
    });

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 13;
    var donutProgress = 75.39822368615503 - 3 * donutProgressUnit;
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    // Next stage Controller
    cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

    if (etaDate !== '' && !isNaN(etaDateObject.getTime())) {
        if (reverseDayMonth(etaDate) >= reverseDayMonth(minimumDate)) {
            $.ajax({
                type: "POST",
                url: "VendorConfirmFirstETA",
                data: JSON.stringify({ 'inputETAHistories': inputETAHistories }),
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    alert(response.responseText);

                    buttonConfirmFirstEta.attr("disabled", "disabled").addClass("selected");
                    checkboxItem.attr("disabled", "disabled");
                    inputFirstEtaDate.attr("disabled", "disabled");
                    buttonEdit.attr("style", "visibility:display");

                    //donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                    //donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("2a");

                    //nextDataContent.find(".st2a-file-proforma-invoice").first().removeAttr("disabled");
                    //nextDataContent.find(".st2a-upload-proforma-invoice").first().removeAttr("disabled");

                },
                error: function (xhr, status, error) {
                    alert(xhr.status + " : " + error);
                }
            });
        }
        else {
            alert("Tanggal tidak bisa lebih kecil dari kesepakatan di stage 1");
            inputFirstEtaDate.focus();
        }
    }
    else {
        alert("Tanggal tidak boleh kosong");
        inputFirstEtaDate.focus();
    }
});

//Vendor input All ETA DATE
$(".st2-confirm-first-eta-all").on("click", function (obj) {

    obj.preventDefault();

    var donutProgressUnit = 75.39822368615503 / 13;
    var donutProgress = 75.39822368615503 - 3 * donutProgressUnit;

    var inputETAHistories = [];

    $(this).closest(".po-item-section.stage-2").find(".po-form-item-st2").each(function (index) {

        var buttonConfirmFirstEta = $(this).find(".st2-confirm-first-eta");
        var checkboxItem = buttonConfirmFirstEta.closest(".form-inline").find(".st2-checkbox-item");
        var inputFirstEtaDate = buttonConfirmFirstEta.closest(".form-inline").find(".st2-first-eta-date");
        var buttonEdit = buttonConfirmFirstEta.closest(".po-form-item-st2").find(".edit-row-st2");

        var itemID = buttonConfirmFirstEta.closest(".form-inline").find(".st2-pd-item-id").val();
        var minimumDate = buttonConfirmFirstEta.closest(".form-inline").find(".st2-first-eta-date").attr("mindate");
        var etaDate = inputFirstEtaDate.val();
        var etaDateObject = reverseDayMonth(etaDate);

        ////progress donutpie
        //var cssRow = $(this).closest(".po-item-data-content").prop("class");
        //cssRow = cssRow.replace(" ", ".");
        //cssRow = "." + cssRow;
        //var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

        ////next stage Controller
        //cssRow = $(this).closest(".po-item-data-content").prop("class");
        //cssRow = cssRow.replace(" ", ".");
        //cssRow = "." + cssRow;
        //var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

        if (inputFirstEtaDate.attr("disabled") !== "disabled" && checkboxItem.prop("checked") === true && checkboxItem.attr("disabled") !== "disabled") {
            if (etaDate !== '' && !isNaN(etaDateObject.getTime())) {
                if (reverseDayMonth(etaDate) >= reverseDayMonth(minimumDate)) {
                    inputETAHistories.push({
                        PurchasingDocumentItemID: itemID,
                        ETADate: etaDateObject
                    });

                    buttonConfirmFirstEta.addClass("row-updated-button");
                    checkboxItem.addClass("row-updated");
                    inputFirstEtaDate.addClass("row-updated");
                    buttonEdit.addClass("row-updated-link");

                    //nextDataContent.find(".st2a-file-proforma-invoice").first().addClass("next-row-updated");
                    //nextDataContent.find(".st2a-upload-proforma-invoice").first().addClass("next-row-updated");

                    //donutRow.find(".donut-chart").first().find("circle").next().addClass("row-updated-donut");
                    //donutRow.find(".donut-chart").first().next().find("span.mark-donut").addClass("row-updated-donut-text");
                }
                else {
                    alert("Tanggal tidak bisa lebih kecil dari kesepakatan di stage 1");
                    inputFirstEtaDate.focus();
                }
            }
            else {
                alert("Tanggal tidak boleh kosong");
                inputFirstEtaDate.focus();
            }
        }
    });

    if (inputETAHistories.length > 0) {
        $.ajax({
            type: "POST",
            url: "VendorConfirmFirstETA",
            data: JSON.stringify({ 'inputETAHistories': inputETAHistories }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                $(".row-updated").attr("disabled", "disabled");
                $(".row-updated-button").attr("disabled", "disabled").addClass("selected");
                $(".row-updated-link").attr("style", "visibility:display");
                $(".row-updated").removeClass("row-updated");
                $(".row-updated-button").removeClass("row-updated-button");
                $(".row-updated-link").removeClass("row-updated-link");

                //$(".next-row-updated").removeAttr("disabled");
                //$(".next-row-updated").removeClass("next-row-updated");

                //$(".row-updated-donut").attr("stroke-dashoffset", donutProgress);
                //$(".row-updated-donut-text").text("2a");
                //$(".row-updated-donut").removeClass("row-updated-donut");
                //$(".row-updated-donut-text").removeClass("row-updated-donut-text");

            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
});

// edit item Click - PROCUREMENT
$(".edit-row-st2").on("click", function (obj) {
    formRow = $(this).closest(".po-form-item-st2");

    formRow.find(".st2-first-eta-date").removeAttr("disabled");
    formRow.find(".st2-confirm-first-eta").removeAttr("disabled").removeClass("selected");
});