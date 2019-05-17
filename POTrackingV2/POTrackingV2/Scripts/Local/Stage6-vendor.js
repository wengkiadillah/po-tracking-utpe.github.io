//Vendor upload one Proforma Invoice
$(".st6-file-invoice-upload").on("click", function (obj) {
    obj.preventDefault();

    var buttonUploadInvoice = $(this);
    var inputFileInvoice = $(this).closest(".form-inline").find(".st6-file-invoice");
    var inputFileInvoiceDOM = inputFileInvoice.get(0);
    var formUploading = $(this).closest(".st6-form-upload");
    var formUploaded = $(this).closest(".po-item-data-header__column").find(".st6-form-uploaded");

    var itemID = $(this).closest(".po-item-data-header__column").find(".st6-item-id").val();

    var formData = new FormData();

    for (var i = 0; i < inputFileInvoiceDOM.files.length; i++) {
        var file = inputFileInvoiceDOM.files[i];

        formData.append("fileInvoice", file);
        formData.append("inputPurchasingDocumentItemID", itemID);
    }


    if (inputFileInvoiceDOM.files.length > 0) {
        $.ajax({
            type: "POST",
            url: "VendorUploadInvoice_2",
            data: formData,
            processData: false,
            contentType: false,
            success: function (response) {
                alert(response.responseText);

                buttonUploadInvoice.attr("disabled", "disabled");
                inputFileInvoice.attr("disabled", "disabled");
                formUploading.attr("hidden", true);
                formUploaded.removeAttr("hidden");

                formUploaded.find(".st6-file-invoice-download").attr("href", response.invoiceUrl);

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

//Vendor upload one Proforma Invoice
$(".st6-file-invoice-remove").on("click", function (obj) {
    obj.preventDefault();

    var buttonUploadInvoice = $(this).closest(".po-item-data-header__column").find(".st6-file-invoice-upload");
    var inputFileInvoice = $(this).closest(".po-item-data-header__column").find(".st6-file-invoice");
    var formUploaded = $(this).closest(".st6-form-uploaded");
    var formUploading = $(this).closest(".po-item-data-header__column").find(".st6-form-upload");

    var itemID = $(this).closest(".po-item-data-header__column").find(".st6-item-id").val();

    var formData = new FormData();

    formData.append("inputPurchasingDocumentItemID", itemID);

    $.ajax({
        type: "POST",
        url: "VendorRemoveUploadInvoice",
        data: formData,
        processData: false,
        contentType: false,
        success: function (response) {
            alert(response.responseText);

            buttonUploadInvoice.removeAttr("disabled");
            inputFileInvoice.removeAttr("disabled");
            formUploaded.attr("hidden", true);
            formUploading.removeAttr("hidden");
        },
        error: function (xhr, status, error) {
            alert(xhr.status + " : " + error);
        }
    });
});