// Donut Progress
var donutProgressUnit = 75.39822368615503 / 12;
var donutProgress = 75.39822368615503 - 4 * donutProgressUnit;

//Procurement Approve Proforma Invoice uploaded by Vendor
$(".st2a-approve-proforma-proc").on("click", function (obj) {
    obj.preventDefault();

    var buttonApprovePI = $(this);
    var buttonDisapprovePI = $(this).closest(".form-inline").find(".st2a-disapprove-proforma-proc");

    var itemID = $(this).closest(".form-inline").find(".st2a-item-id").val();

    var inputPurchasingDocumentItem = {
        ID: itemID
    };

    //Donut
    var cssRow = $(this).closest(".po-item-data-content").prop("class");
    cssRow = cssRow.replace(" ", ".");
    cssRow = "." + cssRow;
    donutRow = $(this).closest(".custom-scrollbar").prev().find(cssRow);

    if (itemID !== null) {
        $.ajax({
            type: "POST",
            url: "/Import/ProcurementApprovePI",
            data: JSON.stringify({ 'inputPurchasingDocumentItem': inputPurchasingDocumentItem }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                buttonApprovePI.addClass("selected").attr("disabled", "disabled");
                buttonDisapprovePI.attr("disabled", "disabled");

                donutRow.find(".donut-chart").first().find("circle").next().attr("stroke-dashoffset", donutProgress);
                donutRow.find(".donut-chart").first().next().find("span.mark-donut").text("3");
            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
});


//Procurement Disapprove Proforma Invoice uploaded by Vendor
$(".st2a-disapprove-proforma-proc").on("click", function (obj) {
    obj.preventDefault();

    var buttonDisapprovePI = $(this);
    var buttonApprovePI = $(this).closest(".form-inline").find(".st2a-approve-proforma-proc");
    var proformaInvoiceItemIDBox = $(this).closest(".form-inline").find(".st2a-item-id");

    var proformaInvoiceItemID = proformaInvoiceItemIDBox.val();

    var inputPurchasingDocumentItem = {
        ID: proformaInvoiceItemID
    };

    if (proformaInvoiceItemID !== null) {
        $.ajax({
            type: "POST",
            url: "/Import/ProcurementDisapprovePI",
            data: JSON.stringify({ 'inputPurchasingDocumentItem': inputPurchasingDocumentItem }),
            contentType: "application/json; charset=utf-8",
            success: function (response) {
                alert(response.responseText);

                buttonApprovePI.attr("disabled", "disabled");
                buttonDisapprovePI.addClass("selected-negative").attr("disabled", "disabled");
            },
            error: function (xhr, status, error) {
                alert(xhr.status + " : " + error);
            }
        });
    }
});