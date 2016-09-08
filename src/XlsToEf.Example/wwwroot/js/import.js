App.Import.ImportReady = function (uploadUrl, pageTitle, modalName) {

  var uploadedFile = '',
      matcherPickerInfo = {},
      destinationInfo = {},
      requiredTogether = {};

  $('#fileupload').fileupload({
    url: uploadUrl,
    dataType: 'json',
    autoUpload: true,
    acceptFileTypes: /(\.|\/)(xlsx)$/i,
    maxFileSize: 5000000, // 5 MB
  }).on('fileuploadadd', function (e, data) {
    $('#progress').toggle(true);
    data.context = $('<div/>').appendTo('#files');
    $.each(data.files, function (index, file) {
      var node = $('<p/>')
          .append($('<span/>').append($('<strong/>').text('File: ')))
          .append($('<span/>').text(file.name))
          .append($('<br />'));
      if (!index) {
        $(".fileinput-button").hide();
      }
      node.appendTo(data.context);
    });
  }).on('fileuploadprogressall', function (e, data) {
    var progress = parseInt(data.loaded / data.total * 100, 10);
    $('#progress .progress-bar').css(
        'width',
        progress + '%'
    );
  }).on('fileuploaddone', function (e, data) {
    uploadedFile = data.result.file;
    if (data.url) {
      var selectSheetNode = $('<select class="form-control" id="sheetPicker" />');
      selectSheetNode.append($('<option />'));
      $.each(data.result.sheets, function (sheet) {
        var sheetText = data.result.sheets[sheet];
        selectSheetNode.append($('<option />', { value: sheetText }).text(sheetText));
      });
      $('#sheets').append($("<span />").append($('<strong/>').text("Sheet"))).append(selectSheetNode);

      var destinationNode = $('<select class="form-control" id="destinationPicker" />');
      destinationNode.append($('<option />'));
      destinationInfo = data.result.destinations;
      $.each(destinationInfo, function (destIndex) {
        destinationNode.append($('<option />', { value: destinationInfo[destIndex].name }).text(destinationInfo[destIndex].name));
      });
      $('#destinations').append($("<span />").append($('<strong/>').text("Destination"))).append(destinationNode);

    } else if (data.result.error) {
      var error = $('<span class="text-danger"/>').text(data.result.error);
      $(data.context.children()[0])
          .append('<br>')
          .append(error);
    }
  }).on('fileuploadfail', function (e, data) {
    $.each(data.files, function (index) {
      var error = $('<span class="text-danger"/>').text('File upload failed.');
      $(data.context.children()[index])
          .append('<br>')
          .append(error);
    });
  }).prop('disabled', !$.support.fileInput)
      .parent().addClass($.support.fileInput ? undefined : 'disabled');

  $(".modal-body").on('change', '#sheetPicker, #destinationPicker', function () {
    var selectedDestination = destinationInfo.filter(function (t) { return t.name == $('#destinationPicker').val(); })[0];

    if ((selectedDestination == undefined) || (!!$('#sheetPicker').val() == false)) {
      $('#matcherDisplay').toggle(false);
      $("#ap-run").toggle(false);
      $('#matchRows').empty();
      return;
    }
    $.ajax(selectedDestination.selectSheetUrl, {
      type: "POST",
      data: JSON.stringify({ Sheet: $('#sheetPicker').val(), FileName: uploadedFile, Destination: selectedDestination }),
      contentType: "application/json; charset=utf-8",
      dataType: "json",
    })
    .done(function (data) {
      $('#matcherDisplay').toggle(true);
      $("#ap-run").toggle(true);
      var row = 1;
      $('#matchRows').empty();
      matcherPickerInfo = {};
      requiredTogether = data.requiredTogether;
      for (var key in data.tableColumns) {
        var colData = data.tableColumns[key];
        var displayText = colData.name;
        var required = colData.required;
        var selectXlsNode = $('<select class="form-control" />');
        if (required) {
          selectXlsNode.addClass("colRequired");
        }
        matcherPickerInfo[key] = selectXlsNode;
        selectXlsNode.append($('<option />'));
        $('#matchRows').append($('<tr />').append($('<td />').append($('<span class="center-vertical" />').text(row)))
            .append($('<td class="form-group" />').append(selectXlsNode))
            .append($('<td />').text(displayText)));
        $.each(data.xlsxColumns, function (xlsxItem) {
          var xlsColText = data.xlsxColumns[xlsxItem];
          selectXlsNode.append(($('<option>', { value: xlsColText }).text(xlsColText)));
        });
        row++;
      }
    }).fail(function (err) {
      alert("error");
    });
  });

  $('#ap-run').on('click', function () {
    var aprunBtn = $(this);
    var isValid = true;
    $("#matchRows select.colRequired").each(function () {
      var valid = !!this.value; //Assign the flag a bolean is valid or not
      if (!valid) {
        isValid = false;
        $(this).closest('.form-group').addClass('has-error');
      }
    });
    if (requiredTogether.length > 0) {
      requiredTogether.forEach(function (requiredSet) {
        var filledCount = 0;
        for (var key in matcherPickerInfo) {
          if ($.inArray(key, requiredSet) > -1) {
            if (!!matcherPickerInfo[key].val()) {
              filledCount++;
            }
          }
        }
        if (filledCount > 0 && filledCount < requiredSet.length) {
          isValid = false;
          requiredSet.forEach(function (requiredItem) {
            if (!matcherPickerInfo[requiredItem].val()) {
              matcherPickerInfo[requiredItem].parent().addClass('has-error');
            }
          });
        } else {
          requiredSet.forEach(function (requiredItem) {
            matcherPickerInfo[requiredItem].parent().removeClass('has-error');
          });
        }
      });
    }
    if (!isValid) {
      return;
    } 

    var matcherSubmissionValues = { FileName: uploadedFile, Selected: {}, Sheet: $('#sheetPicker').val() };
    for (var key in matcherPickerInfo) {
      if (!!matcherPickerInfo[key].val()) { // Do not send unselected cols
        matcherSubmissionValues.Selected[key] = matcherPickerInfo[key].val();
      }
    }

    var selectedDestination = destinationInfo.filter(function (t) { return t.Name == $('#destinationPicker').val(); })[0];
    $.ajax(selectedDestination.MatchSubmitUrl, {
      type: "POST",
      data: JSON.stringify(matcherSubmissionValues),
      contentType: "application/json; charset=utf-8",
      dataType: "json",
    })
        .done(function (data) {
          if ($.isEmptyObject(data.RowErrorDetails)) {
            toastr.success(pageTitle + " was successfully updated");
            $('#' + modalName).modal('hide');
          } else {
            var modalBody = $(".modal-body");
            modalBody.empty();
            var rowText = data.SuccessCount == 1 ? 'row.' : 'rows.';
            modalBody.append($('<p />').text('Successfully imported ' + data.SuccessCount + ' ' + rowText));

            modalBody.append($("<span />").text("The following rows could not be imported:"));
            var errorTable = $('<table width="100%" border="0" cellspacing="0" cellpadding="0" class="table" />');
            errorTable.empty();
            for (var errorKey in data.RowErrorDetails) {
              errorTable.append($('<tr class="importErrorRow" />')
                  .append($('<td />').text(errorKey))
                  .append($('<td />').text(data.RowErrorDetails[errorKey])));
            }
            modalBody.append(errorTable);
            modalBody.append($('<span />').text("Please correct the Excel file and try to import it again."));
            $('#ap-close').addClass('btn-primary').removeClass('btn-default').text("Close");
            $('#ap-run').hide();
          }
        }).fail(function (err) {
          alert("Error - failed to import");
        }).always(function () {
          //aprunBtn.hideLoadingAfterShortDelay();
        });
  });


  $("#ap-run").toggle(false);
  $(".modal-body").on('change', "#matchRows select", function () {
    $("#matchRows select").each(function () {
      var valid = !!this.value; //Assign the flag a bolean is valid or not
      if (valid) {
        $(this).closest('.form-group').removeClass('has-error');
      }
    });
  });

  $('#matcherDisplay').toggle(false);
  $('#progress').toggle(false);
};
