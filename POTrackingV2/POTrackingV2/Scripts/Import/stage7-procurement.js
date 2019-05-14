//Check / Unchecked All PO Items
$("input.st7-checkbox-all").on("change", function (obj) {
    var checkboxAll = $(this);
    $(this).closest(".po-item-section").find("input.checkbox-custom.st7-checkbox-item").each(function (index) {
        if (checkboxAll.prop("checked") === true && $(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", true).change();
        } else if ($(this).attr("disabled") !== "disabled") {
            $(this).prop("checked", false).change();
        }
    });
});

//Checked Button Confirm PO -- Undisabling Controller
$(".st7-checkbox-item").on("change", function (obj) {
    var row = $(this).closest(".po-form-item-st7");

    if ($(this).prop("checked") === true) {
        row.find(".st7-target-ata-on-airport").removeAttr("disabled");
        row.find(".st7-on-airport-confirm").removeAttr("disabled");
    } else {
        row.find(".st7-target-ata-on-airport").attr("disabled", "disabled");
        row.find(".st7-on-airport-confirm").attr("disabled", "disabled");
    }
});


$(".st7-on-airport-confirm").on("click", function (obj) {
    obj.preventDefault();

    var inputShipments = [];

    var checkboxItem = $(this).closest(".po-form-item-st7").find(".st7-checkbox-item");
    var inputETAOnAirport = $(this).closest(".po-form-item-st7").find(".st7-target-eta-on-airport");
    var inputATAOnAirport = $(this).closest(".po-form-item-st7").find(".st7-target-ata-on-airport");
    var buttonConfirmOnAirport = $(this);

    var itemID = $(this).closest(".po-form-item-st7").find(".st7-item-id").val();
    var etaOnAirport = reverseDayMonth(inputETAOnAirport.val());
    var ataOnAirport = reverseDayMonth(inputATAOnAirport.val());

    inputShipments.push({
        PurchasingDocumentItemID: itemID,
        ETADate: etaOnAirport,
        ATADate: ataOnAirport
    });

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 13;
    var donutProgress = 75.39822368615503 - 9 * donutProgressUnit;
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    if (inputETAOnAirport.val() !== '' && !isNaN(etaOnAirport.getTime())) {
        if (inputATAOnAirport.val() !== '' && !isNaN(ataOnAirport.getTime())) {
            $.ajax({
                type: "POST",
                url: "/Import/ProcurementConfirmOnAirport",
                data: JSON.stringify({ 'inputShipments': inputShipments }),
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    alert(response.responseText);

                    buttonConfirmOnAirport.attr("disabled", "disabled").addClass("selected");
                    inputATAOnAirport.attr("disabled", "disabled");
                    checkboxItem.attr("disabled", "disabled");

                    donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                    donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("8");
                },
                error: function (xhr, status, error) {
                    alert(xhr.status + " : " + error);
                }
            });
        }
        else {
            alert("Tanggal ATA tidak boleh kosong");
            inputATAOnAirport.focus();
        }
    }
    else {
        alert("Tanggal ETA tidak boleh kosong");
        inputETAOnAirport.focus();
    }

});


// Procurement Confirm All on Airport
$(".st7-confirm-all-on-airport").on("click", function (obj) {
    obj.preventDefault();

    var donutProgressUnit = 75.39822368615503 / 13;
    var donutProgress = 75.39822368615503 - 9 * donutProgressUnit;

    var inputShipments = [];

    $(this).closest(".po-item-section.stage-8").find(".po-form-item-st7").each(function (index) {

        var checkboxItem = $(this).find(".st7-checkbox-item");
        var inputETAOnAirport = $(this).find(".st7-target-eta-on-airport");
        var inputATAOnAirport = $(this).find(".st7-target-ata-on-airport");
        var buttonConfirmOnAirport = $(this).find(".st7-on-airport-confirm");

        var itemID = $(this).find(".st7-item-id").val();
        var etaOnAirport = reverseDayMonth(inputETAOnAirport.val());
        var ataOnAirport = reverseDayMonth(inputATAOnAirport.val());

        // Donut Progress
        var cssRow = $(this).closest(".po-item-data-content").prop("class");
        cssRow = cssRow.replace(" ", ".");
        cssRow = "." + cssRow;
        var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

        if (inputATAOnAirport.attr("disabled") !== "disabled" && checkboxItem.prop("checked") === true && checkboxItem.attr("disabled") !== "disabled") {
            if (inputETAOnAirport.val() !== '' && !isNaN(etaOnAirport.getTime())) {
                if (inputATAOnAirport.val() !== '' && !isNaN(ataOnAirport.getTime())) {

                    inputShipments.push({
                        PurchasingDocumentItemID: itemID,
                        ETADate: etaOnAirport,
                        ATADate: ataOnAirport
                    });

                    buttonConfirmOnAirport.addClass("row-updated-button");
                    inputATAOnAirport.addClass("row-updated");
                    checkboxItem.addClass("row-updated");

                    donutRow.find(".donut-chart").first().find("circle").next().addClass("row-updated-donut");
                    donutRow.find(".donut-chart").first().next().find("span.mark-donut").addClass("row-updated-donut-text");
                }
                else {
                    alert("Tanggal ATA tidak boleh kosong");
                    inputATAOnAirport.focus();
                }
            }
            else {
                alert("Tanggal ETA tidak boleh kosong");
                inputETAOnAirport.focus();
            }
        }
    });

    if (inputShipments.length > 0) {
        $.ajax({
            type: "POST",
            url: "/Import/ProcurementConfirmOnAirport",
            data: JSON.stringify({ 'inputShipments': inputShipments }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                $(".row-updated").attr("disabled", "disabled");
                $(".row-updated-button").attr("disabled", "disabled").addClass("selected");
                $(".row-updated").removeClass("row-updated");
                $(".row-updated-button").removeClass("row-updated-button");

                $(".row-updated-donut").attr("stroke-dashoffset", donutProgress);
                $(".row-updated-donut-text").text("8");
                $(".row-updated-donut").removeClass("row-updated-donut");
                $(".row-updated-donut-text").removeClass("row-updated-donut-text");
            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }

});