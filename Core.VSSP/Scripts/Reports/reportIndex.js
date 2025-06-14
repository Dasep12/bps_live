
$(document).ready(function () {
    createGrid();
});

function createGrid() {
    var filterSupplier = '';
    $gridFilter = $("#jqGridReport").jqGrid({
        url: "../System/MenuReportFilterListJson",
        mtype: "GET",
        datatype: "json",
        postData: {
            menuId: menuId,
            isActive: true,
            supplierid: filterSupplier
        },
        colModel: [
            { label: 'Schema', name: 'SchemaName', align: 'left', hidden: true, width: 80 },
            { label: 'Field', name: 'Field', align: 'left', hidden: true, width: 100 },
            { label: 'Caption', name: 'Caption', align: 'left', fixed: true, width: 150, editable: false, sortable: false },
            { label: 'FilterName', name: 'FilterName', align: 'left', hidden: true, width: 90, editable: false, sortable: false },
            { label: 'Type', name: 'FilterType', align: 'center', hidden: true, width: 60, editable: false, sortable: false },
            { label: 'Filter Values', name: 'FilterValues', align: 'center', autoResizing: { minColWidth: 250 }, editable: true, sortable: false },
            { label: '...', name: 'Browse', align: 'center', fixed: true, width: 70, editable: false, sortable: false, formatter: actionFormatter },
        ],
        gridview: true,
        loadonce: true,
        height: 168,
        pgbuttons: false,
        pgtext: null,
        viewrecords: true,
        rownumbers: true,
        rownumWidth: 40,
        rowNum: 9999,
        autoresizeOnLoad: true,
        autowidth: true,
        shrinkToFit: true,
        fromServer: true,
        caption: "Filter",
    });

};

function reloadGridReport() {
    $("#jqGridReport").jqGrid('setGridParam', {
        datatype: 'json',
        mtype: 'POST',
        postData: {
            menuId: menuId,
            isActive: true
        }
    }).trigger('reloadGrid');
};

function actionFormatter(cellvalue, options, rowObject) {

    if (rowObject.FilterValues != '') {
        return "";
    } else {

        var rowid = options.rowId;
        var btn = "<div class='table-link'>";
        if (rowObject.FilterName != "N/A") {
            btn += "<a href='#' class='btn btn-outline-secondary text-primary' onclick=\"browseFilter('" + rowid + "','" + rowObject.FilterType + "','" + rowObject.FilterName + "','jqGridReport')\" title='Browse " + formTitle + " [ " + rowObject.Caption + " ]'>";
            btn += "<span class='icon-magnifier'></span>";
            btn += "</a> ";
        } else {
            btn += "<a href='#' class='btn btn-outline-secondary text-primary' onclick=\"browseFilter('" + rowid + "','" + rowObject.FilterType + "','" + rowObject.FilterName + "','jqGridReport')\" title='Type filter values " + formTitle + " [ " + rowObject.Caption + " ]'>";
            btn += "<span class='icon-note'></span>";
            btn += "</a> ";
        }
        btn += "<a href='#' class='btn btn-outline-secondary text-danger' onclick=\"browseFilter('" + rowid + "','','reset','jqGridReport')\" title='Clear filter values " + formTitle + " [ " + rowObject.Caption + " ]'>";
        btn += "<span class='icon-loop'></span>";
        btn += "</a> ";

        btn += "</div>";
        return btn;
    }
}

$('.modal').on('hidden.bs.modal', function () {

    if (filterResult != "") {
        parameters =
        {
            FilterValues: filterResult,
        }

        $("#jqGridReport").jqGrid('setRowData', filterRowId, parameters);
    }

    unloadblockspinner();

});

$(document).ready(function () {
    setTimeout(function () {
        reloadGridReport();
    }, 1000);
});



