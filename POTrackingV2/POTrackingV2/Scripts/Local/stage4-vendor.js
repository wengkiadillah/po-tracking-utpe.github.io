// Validations
$(".st4-update-eta-date-on-time").on('input focus', function (e) {
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

$(".st4-update-eta-date-delay").on('input focus', function (e) {
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

//Vendor click one Update Eta on time
$(".st4-update-eta-date-on-time-confirm").on("click", function (obj) {
    var buttonEtaDateOnTimeConfirm = $(this);
    var inputUpdateEtaDateOntime = $(this).closest(".form-inline").find(".st4-update-eta-date-on-time");
    var inputUpdateEtaDateDelay = $(this).closest(".form-inline").find(".st4-update-eta-date-delay");
    var inputDelayReason = $(this).closest(".form-inline").find(".st4-delay-reason");
    var buttonEtaDateDelayConfirm = $(this).closest(".form-inline").find(".st4-update-eta-date-delay-confirm");

    var uploadColoumn = $(this).closest(".po-item-data-header__column").next();
    var inputUploadProgressPhotoes = uploadColoumn.find(".st4-upload-progress-photoes");
    var buttonUploadProgressPhotoes = uploadColoumn.find(".st4-upload-progress-photoes-confirm");

    //var editRow = $(this).closest(".form-inline").find(".edit-row-st3");

    var itemID = $(this).closest(".form-inline").find(".st4-item-id").val();
    var etaOnTime = reverseDayMonth(inputUpdateEtaDateOntime.val());
    var minDate = reverseDayMonth($(this).closest(".form-inline").find(".st4-update-eta-date-on-time").attr("mindate"));

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 12;
    var donutProgress = 75.39822368615503 - 6 * donutProgressUnit;
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    // Next stage Controller
    cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

    var inputETAHistory = {
        PurchasingDocumentItemID: itemID,
        ETADate: etaOnTime
    };

    if (!isNaN(etaOnTime.getTime())) {
        if (etaOnTime >= minDate) {
            $.ajax({
                type: "POST",
                url: "VendorUpdateETA",
                data: JSON.stringify({ 'inputETAHistory': inputETAHistory }),
                contentType: "application/json; charset=utf-8",
                success: function (response) {
                    alert(response.responseText);

                    buttonEtaDateOnTimeConfirm.attr("disabled", "disabled").addClass("selected");
                    inputUpdateEtaDateOntime.attr("disabled", "disabled");
                    inputUpdateEtaDateDelay.attr("disabled", "disabled");
                    inputDelayReason.attr("disabled", "disabled");
                    buttonEtaDateDelayConfirm.attr("disabled", "disabled");
                    inputUploadProgressPhotoes.removeAttr("disabled");
                    buttonUploadProgressPhotoes.removeAttr("disabled");

                    donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                    donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("5");

                    nextDataContent.find(".st5-checkbox-item").first().removeAttr("disabled");
                },
                error: function (xhr, status, error) {
                    alert(xhr.status + " : " + error);
                }
            });
        }
        else {
            alert("The Date cannot be less than the Date agreed on stage 2");
            inputUpdateEtaDateOntime.focus();
        }
    }
    else {
        alert("Date is not valid");
        inputUpdateEtaDateOntime.focus();
    }
});

//Vendor click one Update Eta Delay
$(".st4-update-eta-date-delay-confirm").on("click", function (obj) {
    var buttonEtaDateDelayConfirm = $(this);
    var inputUpdateEtaDateDelay = $(this).closest(".form-inline").find(".st4-update-eta-date-delay");
    var inputDelayReason = $(this).closest(".form-inline").find(".st4-delay-reason");
    var buttonEtaDateOnTimeConfirm = $(this).closest(".form-inline").find(".st4-update-eta-date-on-time-confirm");
    var inputUpdateEtaDateOntime = $(this).closest(".form-inline").find(".st4-update-eta-date-on-time");

    var uploadColoumn = $(this).closest(".po-item-data-header__column").next();
    var inputUploadProgressPhotoes = uploadColoumn.find(".st4-upload-progress-photoes");
    var buttonUploadProgressPhotoes = uploadColoumn.find(".st4-upload-progress-photoes-confirm");

    //var editRow = $(this).closest(".form-inline").find(".edit-row-st3");

    var itemID = $(this).closest(".form-inline").find(".st4-item-id").val();
    var delayReasonID = inputDelayReason.val();
    var etaDelay = reverseDayMonth(inputUpdateEtaDateDelay.val());
    var minDate = reverseDayMonth($(this).closest(".form-inline").find(".st4-update-eta-date-delay").attr("mindate"));

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 12;
    var donutProgress = 75.39822368615503 - 6 * donutProgressUnit;
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    // Next stage Controller
    cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

    var inputETAHistory = {
        PurchasingDocumentItemID: itemID,
        ETADate: etaDelay,
        DelayReasonID: delayReasonID
    };

    if (!isNaN(etaDelay.getTime())) {
        if (etaDelay > minDate) {
            if (delayReasonID !== '' && delayReasonID !== '0') {
                $.ajax({
                    type: "POST",
                    url: "VendorUpdateETA",
                    data: JSON.stringify({ 'inputETAHistory': inputETAHistory }),
                    contentType: "application/json; charset=utf-8",
                    success: function (response) {
                        alert(response.responseText);

                        buttonEtaDateDelayConfirm.attr("disabled", "disabled").addClass("selected-negative");
                        inputUpdateEtaDateDelay.attr("disabled", "disabled");
                        buttonEtaDateOnTimeConfirm.attr("disabled", "disabled");
                        inputUpdateEtaDateOntime.attr("disabled", "disabled");
                        inputDelayReason.attr("disabled", "disabled");
                        inputUploadProgressPhotoes.removeAttr("disabled");
                        buttonUploadProgressPhotoes.removeAttr("disabled");

                        donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                        donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("5");

                        nextDataContent.find(".st5-checkbox-item").first().removeAttr("disabled");

                    },
                    error: function (xhr, status, error) {
                        alert(xhr.status + " : " + error);
                    }
                });
            }
            else {
                alert("Choose a reason for your delay");
                inputDelayReason.focus();
            }
        }
        else {
            alert("The Date cannot be less than the Date agreed on stage 2");
            inputUpdateEtaDateDelay.focus();
        }
    }
    else {
        alert("Date is not valid");
        inputUpdateEtaDateOntime.focus();
    }
});

//Vendor Upload Progress Photoes
$(".st4-upload-progress-photoes-confirm").on("click", function (obj) {
    var buttonUploadProgressPhotoesConfirm = $(this);
    var inputUploadProgressPhotoes = $(this).closest(".form-inline").find(".st4-upload-progress-photoes");
    var inputUploadProgressPhotoesDOM = inputUploadProgressPhotoes.get(0);

    var itemID = $(this).closest(".form-inline").find(".st4-item-id-inner").val();

    // Donut Progress
    var donutProgressUnit = 75.39822368615503 / 13;
    var donutProgress = 75.39822368615503 - 6 * donutProgressUnit;
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    // Next stage Controller
    cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    var nextDataContent = $(this).closest(".po-item-section").next().find(cssRow);

    var formData = new FormData();

    for (var i = 0; i < inputUploadProgressPhotoesDOM.files.length; i++) {
        var file = inputUploadProgressPhotoesDOM.files[i];

        formData.append("fileProgressPhotoes", file);
        formData.append("inputPurchasingDocumentItemID", itemID);
    }

    if (inputUploadProgressPhotoesDOM.files.length > 0) {
        $.ajax({
            type: "POST",
            url: "VendorUploadProgressPhotoes",
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                alert(response.responseText);

                buttonUploadProgressPhotoesConfirm.attr("disabled", "disabled");
                inputUploadProgressPhotoes.attr("disabled", "disabled");

                /*donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("5");

                nextDataContent.find(".st4-update-eta-date-on-time-confirm").first().removeAttr("disabled");
                nextDataContent.find(".st4-update-eta-date-delay").first().removeAttr("disabled");
                nextDataContent.find(".st4-update-eta-date-delay-confirm").first().removeAttr("disabled");*/
            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
    else {
        alert("File failed to upload");
    }
});