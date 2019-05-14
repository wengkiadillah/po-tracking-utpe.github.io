// Validations
$(".st5-ship-booking-date").on('input focus', function (e) {
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

$(".st5-ATD").on('input focus', function (e) {
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
    var buttonShipBookingDateConfirm = $(this);
    var inputShipBookingDate = $(this).closest(".po-item-data-header__column").find(".st5-ship-booking-date");
    var buttonATDConfirm = $(this).closest(".po-item-data-header__column").next().find(".st5-ATD-confirm");
    var inputATD = $(this).closest(".po-item-data-header__column").next().find(".st5-ATD");
    var inputIsShipBooked = $(this).closest(".po-item-data-header__column").find(".st5-is-ship-booked");

    var itemID = $(this).closest(".po-item-data-header__column").find(".st5-item-id").val();
    var shipBookingDate = reverseDayMonth(inputShipBookingDate.val());
    var minDate = reverseDayMonth(inputShipBookingDate.attr("mindate"));

    var inputShipment = {
        PurchasingDocumentItemID: itemID,
        BookingDate: shipBookingDate
    };

    console.log(inputShipment);

    if (shipBookingDate >= minDate) {
        $.ajax({
            type: "POST",
            url: "/Import/VendorConfirmShipmentBookingDate",
            data: JSON.stringify({ 'inputShipment': inputShipment }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                buttonShipBookingDateConfirm.attr("disabled", "disabled").addClass("selected");
                inputShipBookingDate.attr("disabled", "disabled");
                inputATD.removeAttr("disabled");
                buttonATDConfirm.removeAttr("disabled");
                inputIsShipBooked.attr("value", "true");
                inputATD.attr("mindate", inputShipBookingDate.val());

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
});

//Vendor click Shipment Book Date
$(".st5-ATD-confirm").on("click", function (obj) {
    var buttonATDConfirm = $(this);
    var inputATD = $(this).closest(".po-item-data-header__column").find(".st5-ATD");
    var checkboxItem = $(this).closest(".po-item-data-header__column").prev().find(".st5-checkbox-item");

    var itemID = $(this).closest(".po-item-data-header__column").prev().find(".st5-item-id").val();
    var atdDate = reverseDayMonth(inputATD.val());
    var minDate = reverseDayMonth(inputATD.attr("mindate"));

    var inputShipment = {
        PurchasingDocumentItemID: itemID,
        ATDDate: atdDate
    };

    console.log(inputShipment);

    if (atdDate >= minDate) {
        //$.ajax({
        //    type: "POST",
        //    url: "/Import/",
        //    data: JSON.stringify({ 'inputShipment': inputShipment }),
        //    contentType: "application/json; charset=utf-8",
        //    success: function (response) {
        //        alert(response.responseText);

        alert("inside");

        buttonATDConfirm.attr("disabled", "disabled").addClass("selected");
        inputATD.attr("disabled", "disabled");
        checkboxItem.attr("disabled", "disabled");

        //    },
        //    error: function (xhr, status, error) {
        //        alert(xhr.status + " : " + error);
        //    }
        //});
    }
    else {
        alert("Tanggal tidak bisa lebih kecil dari Ship Booking Date");
        inputATD.focus();
    }
});