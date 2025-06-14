// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.
//var datepicker = $.fn.datepicker.noConflict(); // return $.fn.datepicker to previously assigned value
//$.fn.bootstrapDP = datepicker;                 // give $().bootstrapDP the bootstrap-datepicker functionality

// Write your JavaScript code.
var filterRowId = "";
var filterName = "";
var filterResult = "";

$(document).on('shown.bs.modal', '.modal', function () {
    $('.modal-backdrop').before($(this));
});


// starter JavaScript for disabling form submissions if there are invalid fields
(function () {
    'use strict';

    // Fetch all the forms we want to apply custom Bootstrap validation styles to
    var forms = document.querySelectorAll('.needs-validation');

    // Loop over them and prevent submission
    Array.prototype.slice.call(forms)
        .forEach(function (form) {
    form.addEventListener('submit', function (event) {
        if (!form.checkValidity()) {
            event.preventDefault();
            event.stopPropagation();
        }

        form.classList.add('was-validated');
    }, false);
        });
})();

function getTempOrderNumber() {

    var result = moment(new Date).format("YMDhhmmss");

    $.ajax({
        url: "../System/GetGUIDJson",
        type: "GET",
        dataType: "JSON",
        async: false,
        data: {},
        success: function (response) {
            result = response;
        }
    });

    console.log(result);
    return result;
}

function checkConnection() {

    var result = "404";

    $.ajax({
        url: "../System/checkConnection",
        type: "GET",
        dataType: "JSON",
        async: false,
        data: {},
        success: function (response) {
            result = response;
        },
        error: function (xhr, desc, err) {
            var responsetext = "";
            try {
                responsetext = eval(xhr.responseText);
            } catch {
                responsetext = xhr.responseText;
            }
            result = responsetext;
        }

    });
    return result;
}
function getDiffDay(firstDate, secondDate) {
    var startDay = new Date(firstDate);
    var endDay = new Date(secondDate);

    // Determine the time difference between two dates     
    var millisBetween = startDay.getTime() - endDay.getTime();

    // Determine the number of days between two dates  
    var days = millisBetween / (1000 * 3600 * 24);

    // Show the final number of days between dates     
    return Math.round(Math.abs(days));
}

function getDiffTime(startDate, endDate) {
    const hours = parseInt(Math.abs(endDate - startDate) / (1000 * 60 * 60) % 24);
    const minutes = parseInt(Math.abs(endDate.getTime() - startDate.getTime()) / (1000 * 60) % 60);
    const seconds = parseInt(Math.abs(endDate.getTime() - startDate.getTime()) / (1000) % 60);
    return ((hours < 10 ? "0" : "") + hours + ':' + (minutes < 10 ? "0" : "") + minutes + ':' + (seconds < 10 ? "0" : "") + seconds);
}

async function exportGrid(grid, pagesize, filetitle, orientation, exporttype) {

    if (exporttype === undefined) {
        exporttype = $('#ExportOption').val();
    }

    if (exporttype === "xls") {
        exporttype = "exportToExcel";
        options = {
            includeLabels: true,
            includeGroupHeader: true,
            includeFooter: true,
            fileName: filetitle + ".xlsx",
            mimetype: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            maxlength: 40,
            onBeforeExport: null,
            replaceStr: null,
            loadIndicator: true
        };
    } else
        if (exporttype === "pdf") {
            exporttype = "exportToPdf";
            options = {
                title: filetitle,
                orientation: orientation,
                pageSize: pagesize,
                description: null,
                onBeforeExport: null,
                download: 'download',
                includeLabels: true,
                includeGroupHeader: true,
                includeFooter: true,
                fileName: filetitle + ".pdf",
                mimetype: "application/pdf",
                loadIndicator: true
            };
        }

    $(grid).hideCol("Action");
    $(grid).hideCol("Status");
    $(grid).jqGrid(exporttype, options)
    $(grid).showCol("Action");
    $(grid).showCol("Status");

};

$(document).ready(function () {
    gridResize();
    gridStockListResize();
    gridSearchResize();
    gridScheduleResize();
    gridWorkingHourResize();
    gridShiftPatternResize();
    gridInspectionResize();
})
$(window).on("resize", function () {
    window.setTimeout(
        gridResize(),
        100
    );
    window.setTimeout(
        gridStockListResize(),
        100
    );
    window.setTimeout(
        gridSearchResize(),
        100
    );
    window.setTimeout(
        gridScheduleResize(),
        100
    );
    window.setTimeout(
        gridWorkingHourResize(),
        100
    );
    window.setTimeout(
        gridShiftPatternResize(),
        100
    );
    window.setTimeout(
        gridInspectionResize(),
        100
    );
});

$('.sidebar-minimizer, .navbar-toggler').click(function () {

    setTimeout(function () {
        gridResize();
        gridStockListResize();
        gridScheduleResize();
        gridWorkingHourResize();
        gridShiftPatternResize();
        gridInspectionResize();
    }, 10);

});

$(window).on('shown.bs.modal', function () {
    setTimeout(function () {
        gridResize();
        gridSearchResize();
    }, 10);
});

$(document).on('shown.bs.tab', 'a[data-toggle="tab"]', function (e) {
    gridResize();
});

function gridResize() {

    $('.app-section table').each(function () {
        var id;
        if (this.id != "") {
            id = "#" + this.id;
            if (id) {
                var $grid = $(id),
                    newWidth = $grid.closest(".ui-jqgrid").parent().width(),
                    newHeight = ($(window).innerHeight() - 330);

                //grid size
                $grid.jqGrid("setGridWidth", newWidth, true);
                if (id === "#jqGridMain") {
                    $grid.jqGrid("setGridHeight", newHeight, true);
                }
            }
            else {
                console.log('nogrid');
                return false;
            }
            //card-body size
            //$('.app-section .card-body').height() = 100; 
        }

    });
};

function gridInspectionResize() {
    if ($("#jqGridInspection")) {
        var $grid = $("#jqGridInspection"),
            newWidth = $grid.closest(".ui-jqgrid").parent().width(),
            newHeight = ($(window).innerHeight() - 355);

        //grid size
        $grid.jqGrid("setGridWidth", newWidth, true);
        $grid.jqGrid("setGridHeight", newHeight, true);
    } else {
        return false;
    }
};

function gridStockListResize() {
    if ($("#jqGridStockList")) {
        var $grid = $("#jqGridStockList"),
            newWidth = $grid.closest(".ui-jqgrid").parent().width(),
            newHeight = ($(window).innerHeight() - 355);

        //grid size
        $grid.jqGrid("setGridWidth", newWidth, true);
        $grid.jqGrid("setGridHeight", newHeight, true);
    } else {
        return false;
    }
    
};
function gridSearchResize() {
    $('table').each(function () {
        var id;
        if (this.id != "" && this.id.includes("Search")) {
            id = "#" + this.id;
            if (id) {
                var $grid = $(id),
                    newWidth = $grid.closest(".ui-jqgrid").parent().width(),
                    newHeight = ($(window).innerHeight() * 40) / 100;

                //grid size
                $grid.jqGrid("setGridWidth", newWidth, true);
                $grid.jqGrid("setGridHeight", newHeight, true);

            }
            else {
                return false;
            }
        }

    });
};

function gridAccountMasterResize(grid) {
    window.setTimeout(
        "gridAccountMasterResize()",
        500
    );

    var $grid = $("#" + grid),
        newWidth = $grid.closest(".ui-jqgrid").parent().width(),
        newHeight = ($(window).innerHeight() - 435 );

    //grid size
    $grid.jqGrid("setGridWidth", newWidth, true);
    $grid.jqGrid("setGridHeight", newHeight, true);

};

function gridGeneralLedgerResize(grid) {
    window.setTimeout(
        "gridAccountMasterResize()",
        500
    );

    var $grid = $("#" + grid),
        newWidth = $grid.closest(".ui-jqgrid").parent().width(),
        newHeight = ($(window).innerHeight() - 400);

    //grid size
    $grid.jqGrid("setGridWidth", newWidth, true);
    $grid.jqGrid("setGridHeight", newHeight, true);

};

function gridScheduleResize() {

    if ($("#jqGridSchedule")) {
        var $grid = $("#jqGridSchedule"),
            newWidth = $grid.closest(".ui-jqgrid").parent().width(),
            newHeight = ($(window).innerHeight() - 360);

        //grid size
        $grid.jqGrid("setGridWidth", newWidth, true);
        $grid.jqGrid("setGridHeight", newHeight, true);
    }
    else {
        return false;
    }
};

function gridWorkingHourResize() {

    if ($("#jqGridHour")) {
        var $grid = $("#jqGridHour"),
            newWidth = $grid.closest(".ui-jqgrid").parent().width(),
            newHeight = ($(window).innerHeight() - 485);

        //grid size
        $grid.jqGrid("setGridWidth", newWidth, true);
        $grid.jqGrid("setGridHeight", newHeight, true);
    } else {
        return false;
    }
};
function gridShiftPatternResize() {

    if ($("#jqGridPattern")) {
        var $grid = $("#jqGridPattern"),
            newWidth = $grid.closest(".ui-jqgrid").parent().width(),
            newHeight = ($(window).innerHeight() - 350);

        //grid size
        $grid.jqGrid("setGridWidth", newWidth, true);
        $grid.jqGrid("setGridHeight", newHeight, true);
    } else {
        return false;
    }
};
function msgbox(messages,mtype="info",mbutton="ok") {
    var mstyle, mbtn;
    switch (mtype) {
        case "info":
            break;
        case ":primary:success:warning:danger":
            break
    }

}

function getMonthName(monthVal, shortname = false) {

    var monthNames = ["January", "February", "March", "April", "May", "June","July", "August", "September", "October", "November", "December"];

    var shortmonthNames = ["Jan", "Feb", "Mar", "Apr", "May", "Jun","Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];

    var d = parseInt(monthVal)-1;

    if (shortname === false) {
        return monthNames[d];
    } else {
        return shortmonthNames[d];
    }
}

$('body').on('hidden.bs.modal', function () {
    if ($('.modal.show').length > 0) {
        $('body').addClass('modal-open');
    }
});

var prevHtml = "";
var ftitle = "";

$(document).ready(function () {

    $('.btn-process').click(function () {

        //console.log(ftitle);

        var blable = "";
        if (ftitle = "Login") {
            blable = "Authentication";
        } else {
            blable = "Processing";
        }

        prevHtml = $(this).html();
        $(this).html("<span class='fa fa-spinner fa-pulse'></span> " + blable + "");
    });

});


function StopProcess() {
    if (prevHtml != "") {
        $('.btn-process').html(prevHtml);
        prevHtml = "";
    }
};

$(document).ready(function () {

    $(function () {
        $("#form-filter").submit(function (event) {
            event.preventDefault();
            event.stopPropagation();
        });
    });
});

function parseBool(val) {
    val = val.trim();

    if (val === "true" || val === true || val === 1 || val === "1") {
        return true;
    }
    else if (val = "null") {
        return null;
    }
    else {
        return false;
    }
}

function isNullOrEmpty(val) {
    val = val.trim();
    if (val === "" || val === null) {
        return false;
    } else {
        return true;
    }
}

$(".selectpicker").change(function () {
    $("label.error").remove();
});

function disableAllInput(formId) {
    $("#" + formId +" :input").each(function () {
        var typeOfObject = $(this).prop('tagName');
        var typeOfInput = $(this).prop('type');
        switch (typeOfObject) {
            case "SELECT":
                $(this).attr("disabled", true);
                break;
            case "INPUT":
                if (typeOfInput === 'radio') {
                    $(this).attr("disabled", true);
                } else {
                    $(this).attr("readonly", true);
                }
                break;
            case "TEXTAREA":
                $(this).attr("readonly", true);
                break;
        }
    });
}

function enableAllInput(formId) {
    $("#" + formId + " :input").each(function () {
        $(this).removeAttr("readonly").removeAttr("disabled").removeClass('error').removeClass('bs-invalid').removeClass('is-invalid').next('.nf-error-message').attr("style", "display:none;opacity:0").next('label.error').remove().val("");
    });
    $("#" + formId + " input,select").each(function () {
        $(this).removeAttr("readonly").removeAttr("disabled").removeClass('error').next('label.error').remove().val("");
    });

    $("#" + formId + " .nf-error-message").each(function () {
        $(this).attr("style", "display:none;opacity:0");
    })

    $("#" + formId + " .selectpicker").val("").selectpicker("refresh");
}

// modal draggable
$(".modal-draggable .modal-header").on("mousedown", function (mousedownEvt) {
    var $draggable = $(this);
    var x = mousedownEvt.pageX - $draggable.offset().left,
        y = mousedownEvt.pageY - ($draggable.offset().top /2);
    $("body").on("mousemove.draggable", function (mousemoveEvt) {
        $draggable.closest(".modal-dialog").offset({
            "left": mousemoveEvt.pageX - x,
            "top": mousemoveEvt.pageY - y
        });
    });
    $("body").one("mouseup", function () {
        $("body").off("mousemove.draggable");
    });
    $draggable.closest(".modal").one("bs.modal.hide", function () {
        $("body").off("mousemove.draggable");
    });
});

$(document).ready(function () {
    $.jgrid.jqModal = $.extend($.jgrid.jqModal || {}, {
        beforeOpen: centerInfoDialog
    });
});

function centerInfoDialog() {
    var $infoDlg = $("#info_dialog");
    var $parentDiv = $infoDlg.parent();
    var dlgWidth = $infoDlg.width();
    var parentWidth = $parentDiv.width();

    $infoDlg[0].style.left = Math.round((parentWidth - dlgWidth) / 2) + "px";
    $infoDlg[0].style.zIndex = 1500;

}

var xrows = 0;
var xrow = 0;
var xscr = null;

function autoscrollrows(con=false) {

    scrObj = $('.auto-scroll-row tr');
    xrows = scrObj.length - 1;

    if (con === false) {
        clearInterval(xscr);
        // Update the count down every 1 second
        xrow = 0;
        if (xrows < 12) return false;
    }

    if (scrObj[xrow]) {
        xscr = setInterval(function () {
            xrow += 1;
            var ids = scrObj[xrow].id;
            //console.log(xrow + ' : ' + ids);
            autoscrollToRow(ids);
            if (xrow === xrows) xrow = 0;
        }, 1000);
    }
}

function autoscrollToRow(id) {
    var container = $('.table-responsive'),
        scrollTo = $('#' + id);

    container.animate({
        scrollTop: scrollTo.offset().top - container.offset().top + container.scrollTop()
    });

}

$(".table-responsive").mouseover(function () {
    clearTimeout(xscr);
}).mouseout(function () {
    autoscrollrows(true);
});

$("html").on("mouseup", function (e) {
    var l = $(e.target);
    if (l[0].className.indexOf("popover") == -1) {
        $(".popover").each(function () {
            $(this).popover("hide");
        });
    }
});