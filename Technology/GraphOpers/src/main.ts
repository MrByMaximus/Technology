import Split from 'split.js';

var currentCount = 0;

var timerId = setInterval(function () {
    onValueCh();
}, 60000);

window.hasChangesModule = () => {
    clearInterval(timerId);
    document.removeEventListener("visibilitychange", checkTab);
    return false;
}

function checkTab() {
    if (document.hidden) {
        //console.log('Вкладка не активна');
        clearInterval(timerId);
    } else {
        //console.log('Вкладка активна');
        timerId = setInterval(function () {
            onValueCh();
        }, 60000);
    }
}

document.addEventListener("visibilitychange", checkTab);

function onValueCh() {
    if (currentCount >= 0 && checkedItems.length != 0) {
        resButton.show();
        GetDataCharts().done(function (response) {
            GetTechLim().done(function (res) {
                var json = res;

                if (currentCount >= 1 && currentCount <= 9)
                    genGraphData(currentCount, json, response);
            })
        });
    }
    else {
        resButton.hide();
        for (var i = 1; i <= 9; i++) $('#' + i).hide();
    }
}
//#region Split

let dragTimeout: any;
var lastSizes = [25, 75]; // last sizes
var sizes = [25, 75]; // default sizes

var instanceSplit = Split(['.tree-container', '.data-container'], {
    sizes: sizes,
    minSize: 0,
    gutterSize: 41,
    direction: "horizontal",
    onDragEnd: function () {
        clearTimeout(dragTimeout);
        dragTimeout = setTimeout(function () {
            setGutterCollapsable();
            $(window).resize();
        }, 100);
    },
    elementStyle(dimension, size, gutterSize) { return { "flex-basis": "calc(" + size + "% - " + gutterSize + "px)" } },
    gutterStyle(dimension, gutterSize) { return { "flex-basis": gutterSize + "px", } }
});

function setGutterCollapsable() {
    var gutter = document.querySelector(".gutter");

    gutter.classList.toggle("gutter-collapsable--expand", !(instanceSplit.getSizes()[0] > 2));
    gutter.classList.toggle("gutter-collapsable--collapse", (instanceSplit.getSizes()[0] > 2));
}

function initGutter() {
    var gutter = document.querySelector(".gutter");

    const gutterContent = document.createElement("div");
    gutterContent.textContent = "chevron_left";

    gutter.appendChild(gutterContent);
    gutter.classList.add("gutter-collapsable");

    setGutterCollapsable();

    gutterContent.addEventListener("click", () => {
        if (gutter.classList.contains("gutter-collapsable--collapse")) {
            lastSizes = instanceSplit.getSizes();
            instanceSplit.collapse(0);
        } else {
            instanceSplit.setSizes(lastSizes);
        }
    });


    document.querySelector(".gutter").addEventListener("dblclick", () => {
        if (instanceSplit.getSizes()[0] > 2) {
            lastSizes = instanceSplit.getSizes();
            instanceSplit.collapse(0);
        } else {
            instanceSplit.setSizes(lastSizes);
        }
    });
}

initGutter();

//#endregion

new DevExpress.ui.dxButton(document.getElementById("dx-icon-preferences"), {
    icon: "preferences",
    onClick: function (e) {
        if (instanceSplit.getSizes()[0] > 2) {
            lastSizes = instanceSplit.getSizes();
            instanceSplit.collapse(0);
        } else {
            instanceSplit.setSizes(lastSizes);
        }
    }
});

var hideShowResButton: boolean = false;
var resButton = $("#reset-button").dxButton({
    text: "Сжать границы",
    onClick: function (e) {
        hideShowResButton = !hideShowResButton;
        if (hideShowResButton) {
            e.component.option("text", "Вернуть график в первоначальное положение");
        } else {
            e.component.option("text", "Сжать границы");
        }
        onValueCh();
    }
});


let searchTimer: any;
new DevExpress.ui.dxTextBox(document.getElementById("search-tree"), {
    placeholder: "Поиск",
    //width: 200,
    mode: "search",
    valueChangeEvent: "keyup",
    onValueChanged(e) {
        var searchValue = e.value;
        clearTimeout(searchTimer);

        searchTimer = setTimeout(() => {
            if (searchValue !== null && searchValue !== "") {
                treeList.searchByText(searchValue);
            } else if (searchValue === "") {
                treeList.searchByText(searchValue);
            }
        }, 500);
    }
});

//#region TreeList

let checkedItems: any[] = [];

function loadTreeListData() {
    $.ajax({
        url: `${window.location.pathname}/GetTechnologyTree`,
        success: function (json) {
            treeList.option({
                rootValue: json.root_value,
                dataSource: new DevExpress.data.DataSource({
                    store: json.data,
                    sort: ["num_order", "seq", "name"]
                })
            });
        },
        error: function (problem) {
            console.log("problem", problem);
        }
    });
}
const treeList = new DevExpress.ui.dxTreeList(document.querySelector(".tree-view"), {
    showColumnHeaders: false,
    columns: [
        {
            dataField: "name",
            dataType: "string"
        },
        {
            dataField: "tag_id",
            visible: false
        },
        {
            dataField: "num_order",
            visible: false
        },
        {
            dataField: "seq",
            visible: false
        },
    ],
    sorting: {
        mode: 'multiple'
    },
    keyExpr: "id",
    parentIdExpr: "parent_id",
    rootValue: null,
    autoExpandAll: false,
    height: "100%",
    headerFilter: {
        visible: false
    },
    hoverStateEnabled: true,
    allowColumnResizing: true,
    columnResizingMode: "widget",

    showColumnLines: false,
    showRowLines: false,
    showBorders: false,
    searchPanel: {
        visible: false
    },
    selection: {
        mode: "multiple",
        recursive: true,
        allowSelectAll: true
    },
    scrolling: {
        mode: "standard", //"standard" or "virtual"
        rowRenderingMode: "standard", //"standard" or "virtual"
        showScrollbar: "always"
    },
    onSelectionChanged(e) {

        var selected = e.component.getSelectedRowsData("leavesOnly");
        checkedItems = selected?.filter((r) => r.data).map((r) => {
            return {
                id: r.data.tag,
                key: r.id,
                text: r.data.text
            }
        });

        //console.log("selected", selected);
        //console.log("checkedItems", checkedItems);

        if (checkedItems.length <= 9 && checkedItems.length != 0) { //todo неправильно работает диалоговое окно при отмене чекбоксов
            currentCount = checkedItems.length;
            onValueCh();
        } else {
            for (var i = 1; i <= 9; i++) $('#' + i).hide();
            e.component.deselectRows(e.selectedRowKeys);
            resButton.hide();
        }

        if (checkedItems.length > 9) {
            DevExpress.ui.notify("Графиков должно быть не больше 9-ти/данных по данному параметру нет", "warning");
            for (var i = 1; i <= 9; i++) $('#' + i).hide();
            e.component.deselectRows(e.selectedRowKeys);
        }
    },
    onRowClick(e) {
        if (e.event.target.classList.contains("dx-treelist-text-content")) {
            if (e.isSelected) {
                e.component.deselectRows([e.key]);
            } else {
                e.component.selectRows([e.key], true);
            }
        }
    }
});

//#endregion

function GetDataCharts() {
    if (checkedItems.length > 0) {
        let checkedId: any[] = [];
        $.each(checkedItems, function (i, item) {
            checkedId.push(item.id);
        });
        var deferred = $.Deferred();
        $.ajax({
            url: `${window.location.pathname}/GetTechnology`,
            data: {
                array: checkedId.toString()
            },
            success: function (json) {
                deferred.resolve(json);
            },
            error: function (problem) {
                deferred.reject(problem);
            }
        });

        return deferred.promise();
    }
}

function GetTechLim() {
    if (checkedItems.length > 0) {
        let checkedId: any[] = [];
        $.each(checkedItems, function (i, item) {
            checkedId.push(item.id);
        });
        var deferred = $.Deferred();
        $.ajax({
            url: `${window.location.pathname}/GetTechnologyLim`,
            data: {
                array: checkedId.toString()
            },
            success: function (json) {
                deferred.resolve(json);
            },
            error: function (problem) {
                deferred.reject(problem);
            }
        });

        return deferred.promise();
    }
}

function genGraphData(currentCount: any, json: any, response: any) {
    var arrayLine = [];

    var option: any = {
        zoomAndPan: {
            valueAxis: "both",
            argumentAxis: "both",
            dragToZoom: true,
            allowMouseWheel: true,
            panKey: "shift"
        },
        commonSeriesSettings: {
            type: "line",
            argumentField: "datE_TIME",
            point: {
                visible: true,
                size: 9
            },
            width: 6,
            ignoreEmptyPoints: true
        },
        argumentAxis: {
            label: {
                format: {
                    type: "decimal"
                }
            }
        },
        legend: {
            verticalAlignment: "top",
            horizontalAlignment: "center"
        },
        tooltip: {
            enabled: true
        }
    };
    option.dataSource = response;
    //console.log(response)

    $('#' + currentCount).show();
    if (currentCount != 9)
        $('#' + (currentCount + 1)).hide();

    if (currentCount != 1)
        $('#' + (currentCount - 1)).hide();

    arrayLine.length = 0;

    for (var i = 0; i < currentCount; i++) {
        for (var j = 0; j < json.length; j++) {
            if (checkedItems[i].id === json[j].ParamId) {
                arrayLine.push(json[j]);
            }
        }
        if (!arrayLine[i]) {
            arrayLine.push(null);
        }

        option.valueAxis = null;

        let values: any[] = [];
        $.each(response, function (index, data) {
            $.each(data, function (key, value) {
                if (key !== "datE_STAMP" && key !== "datE_TIME" && key == checkedItems[i].id) {
                    values.push(value);
                }
            });
        });
        var valuesForMinMax = values.filter(function (val) {
            return val !== null;
        });

        var maxSource = null;
        var minSource = null;
        if (valuesForMinMax.length > 0) {
            maxSource = Math.max.apply(null, valuesForMinMax);
            minSource = Math.min.apply(null, valuesForMinMax);
        }
        //console.log("arrayLine", arrayLine);

        try {
            if (arrayLine.length != 0 /*&& minSource > 0*/) {
                var arrayLineOfNull = arrayLine[i] == null || arrayLine === undefined;
                var _Max_Err = arrayLineOfNull ? {} : arrayLine[i].max_err;
                var _Max_War = arrayLineOfNull ? {} : arrayLine[i].max_war;
                var _Min_Err = arrayLineOfNull ? {} : arrayLine[i].min_err;
                var _Min_War = arrayLineOfNull ? {} : arrayLine[i].min_war;

                //if (arrayLine.length != 0)
                var maxErr = arrayLineOfNull ? null : _Max_Err;
                var maxWar = arrayLineOfNull ? null : _Max_War;

                var minWar = arrayLineOfNull ? null : _Min_War;
                var minErr = arrayLineOfNull ? null : _Min_Err;

                var max = maxErr > maxWar ? maxErr || maxWar : maxWar || maxErr;
                var min = minWar > minErr ? minErr || minWar : minWar || minErr;
                //option.valueAxis = null;

                //console.log("maxSource", maxSource);
                //console.log("max", max)

                //console.log("minSource", minSource);
                //console.log("min", min)


                var startValue = undefined;
                var endValue = undefined;

                if (hideShowResButton) {
                    startValue = max || maxSource;
                    endValue = min || minSource;
                }
                else {
                    if (maxSource > max) {
                        startValue = maxSource || max;
                    } else {
                        startValue = max || maxSource;
                    }
                    if (minSource < min) {
                        endValue = minSource || min;
                    }
                    else {
                        endValue = min || minSource;
                    }
                }

                var val = {
                    valueType: "numeric",
                    valueMarginsEnabled: true,
                    maxValueMargin: 0.1,
                    visualRange: {
                        startValue: startValue,
                        endValue: endValue
                    },
                    constantLines: [{
                        label: { text: "Max пред. " + _Max_War },
                        value: arrayLineOfNull ? null : _Max_War,
                        color: "#e59e1f",
                        dashStyle: "dash",
                        width: 3
                    }, {
                        label: { text: "Min пред. " + _Min_War },
                        value: arrayLineOfNull ? null : _Min_War,
                        color: "#e59e1f",
                        dashStyle: "dash",
                        width: 3
                    }, {
                        label: { text: "Max авар. " + _Max_Err },
                        value: arrayLineOfNull ? null : _Max_Err,
                        color: "#F44336",
                        dashStyle: "dash",
                        width: 3
                    }, {
                        label: { text: "Min авар. " + _Min_Err },
                        value: arrayLineOfNull ? null : _Min_Err,
                        color: '#F44336',
                        dashStyle: "dash",
                        width: 3
                    }],
                    grid: {
                        opacity: 20
                    }
                };
                option.valueAxis = val;
            }

            //var pointValue = arrayLine[i].min_err >= arrayLine[i].max_err ? arrayLine[i].max_err : arrayLine[i].min_err;
            var pointValueMax = arrayLineOfNull ? null : _Max_Err;
            var pointValueMin = arrayLineOfNull ? null : _Min_Err;
            //var titleValue = valuesForMinMax[valuesForMinMax.length - 1] != null ? valuesForMinMax[valuesForMinMax.length - 1].toString() : ""
            //console.log("titleValue", titleValue);
        }
        catch (e) {
            console.log(e);
        }

        let customPoint = function (pointInfo: any) {
            if (pointValueMin != null && pointValueMax != null) {
                return pointInfo.value > pointValueMax ? { color: 'red' } : pointInfo.value < pointValueMin ? { color: 'red' } : {}
            }
            else {
                return {}
            }
        };

        var titleValue = valuesForMinMax[valuesForMinMax.length - 1] != null ? valuesForMinMax[valuesForMinMax.length - 1].toString() : "";

        let title = {
            font: {
                color: "black"
            },
            text: titleValue,
            margin: 0,
            horizontalAlignment: "right"
        };

        option.title = title;
        option.customizePoint = customPoint;
        option.series = { valueField: checkedItems[i].id, name: JSON.stringify(checkedItems[i].text).replace(/\\\\n/g, ' - '), color: "#cd7f32" };
        $('#' + currentCount + i).dxChart(option);
    }
}

$(document).ready(function () {
    resButton.hide();

    loadTreeListData();
});