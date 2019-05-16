// Validations
$(".st5-ship-booking-date").on('input focus', function (e) {
    this.setCustomValidity('');
    var thisDate = $(this).val();
    var maxDate = $(this).attr("maxdate");
    if ((!isNaN(thisDate) || thisDate !== '') && (!isNaN(maxDate) || maxDate !== '')) {
        if (reverseDayMonth(thisDate) > reverseDayMonth(maxDate)) {
            this.setCustomValidity("Value must be less than or equal " + $(this).attr("maxdate"));
        }
    }
    this.reportValidity();
});

$(".st5-ATD").on('input focus', function (e) {
    //this.setCustomValidity('');
    //var thisDate = $(this).val();
    //var maxDate = $(this).attr("maxdate");
    //if ((!isNaN(thisDate) || thisDate !== '') && (!isNaN(maxDate) || maxDate !== '')) {
    //    if (reverseDayMonth(thisDate) < reverseDayMonth(maxDate)) {
    //        this.setCustomValidity("Value must be more than or equal " + $(this).attr("maxdate"));
    //    }
    //}
    this.reportValidity();
});

// Check / Unchecked All PO Items
$("input.st5-checkbox-all").on("change", function (obj) {
    var checkboxAll = $(this);
    $(this).closest(".po-item-section").find("input.st5-checkbox-item").each(function (index) {
        if (checkboxAll.prop("checked") === true && $(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true).change();
        } else if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", false).change();
        }
    });
});

//Checked Button Confirm PO -- Undisabling Controller
$(".st5-checkbox-item").on("change", function (obj) {
    var row = $(this).closest(".po-item-data-header__column");
    var isShipBooked = row.find(".st5-is-ship-booked").val();

    if ($(this).prop("checked") === true) {
        if (isShipBooked === "false") {
            row.find(".st5-ship-booking-date").removeAttr("disabled");
            row.find(".st5-ship-booking-date-confirm").removeAttr("disabled");
        }
        else {
            row.next().find(".st5-ATD").removeAttr("disabled");
            row.next().find(".st5-ATD-confirm").removeAttr("disabled");
        }
    } else {
        if (isShipBooked === "false") {
            row.find(".st5-ship-booking-date").attr("disabled", "disabled");
            row.find(".st5-ship-booking-date-confirm").attr("disabled", "disabled");
        }
        else {
            row.next().find(".st5-ATD").attr("disabled", "disabled");
            row.next().find(".st5-ATD-confirm").attr("disabled", "disabled");
        }
    }
});

//Vendor click Shipment Book Date
$(".st5-ship-booking-date-confirm").on("click", function (obj) {
    obj.preventDefault();

    var buttonShipBookingDateConfirm = $(this);
    var inputShipBookingDate = $(this).closest(".po-item-data-header__column").find(".st5-ship-booking-date");
    var buttonATDConfirm = $(this).closest(".po-item-data-header__column").next().find(".st5-ATD-confirm");
    var inputATD = $(this).closest(".po-item-data-header__column").next().find(".st5-ATD");
    var inputIsShipBooked = $(this).closest(".po-item-data-header__column").find(".st5-is-ship-booked");

    var itemID = $(this).closest(".po-item-data-header__column").find(".st5-item-id").val();
    var shipBookingDate = reverseDayMonth(inputShipBookingDate.val());
    var maxDate = reverseDayMonth(inputShipBookingDate.attr("maxdate"));

    var inputShipmentBookDates = [];

    inputShipmentBookDates.push({
        PurchasingDocumentItemID: itemID,
        BookingDate: shipBookingDate
    });

    if (!isNaN(shipBookingDate.getTime())) {
        if (shipBookingDate <= maxDate) {
            $.ajax({
                type: "POST",
                url: "VendorConfirmShipmentBookingDate",
                data: JSON.stringify({ 'inputShipmentBookDates': inputShipmentBookDates }),
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    alert(response.responseText);

                    buttonShipBookingDateConfirm.attr("disabled", "disabled").addClass("selected");
                    inputShipBookingDate.attr("disabled", "disabled");
                    inputATD.removeAttr("disabled");
                    inputATD.val(inputShipBookingDate.val());
                    inputATD.attr("maxdate", inputShipBookingDate.val());
                    buttonATDConfirm.removeAttr("disabled");
                    inputIsShipBooked.attr("value", "true");
                },
                error: function (xhr, status, error) {
                    alert(xhr.status + " : " + error);
                }
            });
        }
        else {
            alert("Tanggal tidak bisa lebih kecil dari kesepakatan di stage 4");
            inputShipBookingDate.focus();
        }
    }
    else {
        alert("Tanggal tidak Valid");
        inputShipBookingDate.focus();
    }
});

//Vendor click Shipment ATD
$(".st5-ATD-confirm").on("click", function (obj) {
    obj.preventDefault();

    var inputShipmentATDs = [];
    var buttonATDConfirm = $(this);
    var inputATD = $(this).closest(".po-item-data-header__column").find(".st5-ATD");
    var checkboxItem = $(this).closest(".po-item-data-header__column").prev().find(".st5-checkbox-item");

    var itemID = $(this).closest(".po-item-data-header__column").prev().find(".st5-item-id").val();
    var atdDate = reverseDayMonth(inputATD.val());

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 13;
    var donutProgress = 75.39822368615503 - 7 * donutProgressUnit;
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    // Next stage Controller
    cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

    inputShipmentATDs.push({
        PurchasingDocumentItemID: itemID,
        ATDDate: atdDate
    });

    if (!isNaN(atdDate.getTime())) {

        $.ajax({
            type: "POST",
            url: "VendorConfirmATD",
            data: JSON.stringify({ 'inputShipmentATDs': inputShipmentATDs }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                buttonATDConfirm.attr("disabled", "disabled").addClass("selected");
                inputATD.attr("disabled", "disabled");
                checkboxItem.attr("disabled", "disabled");

                donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("6");

                nextDataContent.find(".st6-fill-in-the-form").first().removeAttr("disabled");
            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
    else {
        alert("Tanggal tidak Valid");
        inputATD.focus();
    }
});

//Vendor Confirm All 
$(".st5-confirm-all").on("click", function (obj) {
    obj.preventDefault();

    var inputShipmentBookDates = [];
    var inputShipmentATDs = [];
    var count = 0;

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 13;
    var donutProgress = 75.39822368615503 - 7 * donutProgressUnit;

    $(this).closest(".po-item-section.stage-6").find(".po-form-item-st5").each(function (index) {

        var buttonShipBookingDateConfirm = $(this).find(".st5-ship-booking-date-confirm");
        var inputShipBookingDate = $(this).find(".st5-ship-booking-date");
        var buttonATDConfirm = $(this).find(".st5-ATD-confirm");
        var inputATD = $(this).find(".st5-ATD");
        var inputIsShipBooked = $(this).find(".st5-is-ship-booked");
        var itemID = $(this).find(".st5-item-id").val();
        var inputCheckboxItem = $(this).find(".st5-checkbox-item");

        // Donut Progress
        var cssRow = $(this).closest(".po-item-data-content").prop("class");
        cssRow = cssRow.replace(" ", ".");
        cssRow = "." + cssRow;
        var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

        // Next stage Controller
        cssRow = $(this).closest(".po-item-data-content").prop("class");
        cssRow = cssRow.replace(" ", ".");
        cssRow = "." + cssRow;
        var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

        if (inputIsShipBooked.val() === 'false') {

            var shipBookingDate = reverseDayMonth(inputShipBookingDate.val());
            var maxDate = reverseDayMonth(inputShipBookingDate.attr("maxdate"));

            if (inputCheckboxItem.prop("checked") === true && inputCheckboxItem.attr("disabled") !== "disabled") {
                if (shipBookingDate <= maxDate) {

                    inputShipmentBookDates.push({
                        PurchasingDocumentItemID: itemID,
                        BookingDate: shipBookingDate
                    });

                    buttonShipBookingDateConfirm.addClass("row-updated-button");
                    inputShipBookingDate.addClass("row-updated");
                    inputATD.addClass("row-updated-next");
                    inputATD.val(inputShipBookingDate.val());
                    buttonATDConfirm.addClass("row-updated-button-next");
                    inputIsShipBooked.addClass("row-updated-status");
                }
                else {
                    alert("Tanggal tidak bisa lebih kecil dari kesepakatan di stage 4");
                    inputShipBookingDate.focus();
                }
            }
        }
        else {

            var atdDate = reverseDayMonth(inputATD.val());

            if (inputCheckboxItem.prop("checked") === true && inputCheckboxItem.attr("disabled") !== "disabled") {

                inputShipmentATDs.push({
                    PurchasingDocumentItemID: itemID,
                    ATDDate: atdDate
                });

                buttonATDConfirm.addClass("row-updated-button-ATD");
                inputATD.addClass("row-updated-ATD");
                inputCheckboxItem.addClass("row-updated-ATD");

                donutRow.find(".donut-chart").first().find("circle").next().addClass("row-updated-donut");
                donutRow.find(".donut-chart").first().next().find("span.mark-donut").addClass("row-updated-donut-text");

                nextDataContent.addClass("row-updated-next-content");
            }
        }
    });

    if (inputShipmentBookDates.length > 0) {
        $.ajax({
            type: "POST",
            url: "VendorConfirmShipmentBookingDate",
            data: JSON.stringify({ 'inputShipmentBookDates': inputShipmentBookDates }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                count = count + (response.count);

                $(".row-updated").attr("disabled", "disabled");
                $(".row-updated").removeClass("row-updated");
                $(".row-updated-button").attr("disabled", "disabled").addClass("selected");
                $(".row-updated-button").removeClass("row-updated-button");
                $(".row-updated-next").removeAttr("disabled");
                $(".row-updated-next").removeClass("row-updated-next");
                $(".row-updated-button-next").removeAttr("disabled");
                $(".row-updated-button-next").removeClass("row-updated-button-next");
                $(".row-updated-status").attr("value", "true");
                $(".row-updated-status").removeClass("row-updated-status");
            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }

    if (inputShipmentATDs.length > 0) {
        $.ajax({
            type: "POST",
            url: "VendorConfirmATD",
            data: JSON.stringify({ 'inputShipmentATDs': inputShipmentATDs }),
            contentType: "application/json; charset=utf-8",
            dataType: "json",
            success: function (response) {
                count = count + (response.count);
                alert(count + " data affected");

                $(".row-updated-ATD").attr("disabled", "disabled");
                $(".row-updated-ATD").removeClass("row-updated-ATD");
                $(".row-updated-button-ATD").attr("disabled", "disabled").addClass("selected");
                $(".row-updated-button-ATD").removeClass("row-updated-button-ATD");

                $(".row-updated-donut").attr("stroke-dashoffset", donutProgress);
                $(".row-updated-donut-text").text("6");
                $(".row-updated-donut").removeClass("row-updated-donut");
                $(".row-updated-donut-text").removeClass("row-updated-donut-text");

                $(".row-updated-next-content").find(".st6-fill-in-the-form").first().removeAttr("disabled");
            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }

});