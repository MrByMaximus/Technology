import Split from 'split.js';

//#region переменные

var allowGlobalRefresh: boolean = true;
var manualSetLookup: boolean = false;

var checkedItems: any[] = [];
let jsonSource: any[];
var isShowTimer: boolean = false; 
//#endregion

//#region общие методы

Array.prototype.equals = function (array, strict) {
    if (!array)
        return false;

    if (arguments.length == 1)
        strict = true;

    if (this.length != array.length)
        return false;

    for (var i = 0; i < this.length; i++) {
        if (this[i] instanceof Array && array[i] instanceof Array) {
            if (!this[i].equals(array[i], strict))
                return false;
        } else if (strict && this[i] != array[i]) {
            return false;
        } else if (!strict) {
            return this.sort().equals(array.sort(), true);
        }
    }
    return true;
};


$(document).ready(function () {
    let dragTimeout: number;
    let instanceSplit = Split(['#one', '#two'],
        {
            sizes: [25, 75],
            minSize: 20,
            gutterSize: 41,
            onDragEnd: function () {
                clearTimeout(dragTimeout);
                dragTimeout = window.setTimeout(function () {
                    $(window).resize();
                },
                    200);
            }
        });

    $(".gutter").dblclick(function () {
        if (instanceSplit.getSizes()[0] > 2) {
            instanceSplit.collapse(0);
        } else {
            instanceSplit.setSizes([25, 75]);
        }
        //$(window).resize();
    });
});

//#endregion
    
//#region поиск по дереву
$("#search-treeview").dxTextBox({
    placeholder: "Поиск",
    width: 260,
    mode: "search",
    valueChangeEvent: "keyup",
    onValueChanged: function (e) {
        var searchValue = e.value;
        setTimeout(function () {
            if (searchValue == $('#search-treeview').find("input").val() && searchValue != null && searchValue != "") {
                treeView2.option("searchValue", searchValue);
            }
            else if (searchValue == '') {
                treeView2.option("searchValue", searchValue);
            }
        }, 500);
    }
});
//#endregion

function loadRealTimeData(checkedItems: any[]) {
    return $.when(
        loadDataRealTime(checkedItems)
    ).then((result: any) => {
        generateSource(checkedItems, result);
    }).done(() => {
        loadTechnologyGrid2(jsonSource);
    });
}

function loadFavoriteData() {
    return $.ajax({
        url: window.moduleContext.GetFavorites,
        success: function (json) {
            favoriteLookup.option("items", json);
        }
    });
}

//#region generateSource
function generateSource(checkedItems: any[], result: any) {
    jsonSource = [];

    if (result) {
        var data = result;

        $.each(data, function (i, item) {

            var model: any = {};

            model["iD_PARAM"] = item.iD_PARAM;
            model["vaL_AVG"] = item.vaL_AVG;
            // model["val_instal"] = item.val_instal;
            var find = checkedItems.filter(function (row) {

                if (row.id == item.iD_PARAM) {
                    return row;
                } else {
                    return null;
                }

            });
            if (find && find.length > 0) {
                var t = find[0].text.replace(/[\\n\r]/g, " ");
                model["param_name"] = t;
            }
            jsonSource.push(model);
        });
    }
}
//#endregion

//#region loadTreeData2
function loadTreeData2() {
    return $.ajax({
        url: window.moduleContext.GetTechnologyTree,
        success: function (json) {
            treeView2.option("items", json);
        }
    });
}
//#endregion

//#region clearRealTimeGrid
function clearRealTimeGrid() {
    $("#gridRealTime").hide();
    if (dataGridRealTime) {
        dataGridRealTime.option('dataSource', []);
    }
}
//#endregion

//#region loadDataRealTime
function loadDataRealTime(checkedItems: any[]) {

    if (checkedItems.length > 0) {
        var checkedId: any[] = [];
        $.each(checkedItems, function (i, item) {
            checkedId.push(item.id);
        });

        return $.ajax({
            url: window.moduleContext.GetTechnologyRealTimeData,
            type: "GET",
            dataType: "json",
            data: {
                array: checkedId.toString()
            }
        });

    } else {
        var obj = clearTechnologyGrid2;
        if (typeof obj === "function") {
            clearTechnologyGrid2();
        }

        return null;
    }
}
//#endregion

window.refreshModule = function () {
    if (treeView2) {
        treeView2.option("disabled", true);
    }

    return $.when(
        loadRealTimeData(checkedItems)
    ).done(function (result: any) {
        $("#refreshButton").focus();
    }).fail(function (request: any, status: any, error: any) {
        if (treeView2) {
            treeView2.unselectAll();
        }
    }).always(function () {
        if (treeView2) {
            treeView2.option("disabled", false);
        }
    });

}

//#region favoriteLookup
var favoriteLookup = new DevExpress.ui.dxLookup(document.getElementById("favoriteLookup"), {
    width: 214,
    dropDownOptions: {
        width: 314,
        showTitle: false,
        closeOnOutsideClick: true
    },
    //popupWidth: 314,
    displayExpr: "favoriteName",
    placeholder: "Избранные параметры",
    noDataText: "Нет сохраненных параметров",
    showClearButton: true,
    itemTemplate: function (data: any) {
        return "<div>" +
            data.favoriteName +
            "<div class='favoriteTemplate' onclick='Technology.ReportBuilder.deleteFromFavorites2(" + data.id + ", event)'>" +
            "<i class='material-icons md-24'>delete_forever</i>" +
            "</div>" +
            "</div>";
    },
    onValueChanged: function (e) {
        if (manualSetLookup === false) {
            allowGlobalRefresh = false;

            checkedItems = [];

            treeView2.unselectAll();

            if (e.value && e.value.favorite) {
                var data = JSON.parse(e.value.favorite);
                $.each(data, function (index, item) {
                    treeView2.selectItem(item.key);
                });
                $("#starIcon").text("star");
            } else {
                $("#starIcon").text("star_border");
            }

            allowGlobalRefresh = true;
            window.globalRefresh();
        }

        manualSetLookup = false;
    }
});
//#endregion

//#region treeView2
var treeView2 = $("#treeView2").dxTreeView({
    showCheckBoxesMode: "normal",
    searchValue: "",
    onItemSelectionChanged: function (e: any) {
        fill2(e.node);
        if (allowGlobalRefresh) {
            window.globalRefresh();
            checkFavoritesIdTree();
        }
    },
    itemTemplate: function (itemData: any, itemIndex: number, itemElement: DevExpress.core.dxElement) {
        itemElement.text(itemData.text.replace(/[_]/g, '.'));

    },
}).dxTreeView("instance");
//#endregion

//#region fill2
function fill2(item: any) {
    if (!item.items.length && item.parent) {
        var product: any = $.extend({ category: item.parent.text }, item);

        var itemIndex = -1;
        $.each(checkedItems,
            function (index, item) {
                if (item.key === product.key) {
                    itemIndex = index;
                    return false;
                }
            });

        if (product.selected && itemIndex === -1 && product.itemData.data) {

            var cItem: any = {};
            cItem["id"] = product.itemData.data.tag;
            cItem["key"] = product.key;
            cItem["text"] = product.itemData.data.text;

            checkedItems.push(cItem);

        } else if (product.selected && itemIndex === -1 && product.itemData.items) {
            $.each(product.itemData.items,
                function (index, product1) {
                    var cItem1: any = {};
                    cItem1["id"] = product1.data.tag;
                    cItem1["key"] = product1.id;
                    cItem1["text"] = product1.data.text;
                    checkedItems.push(cItem1);

                });
        } else if (!product.selected && product.itemData.data) {
            checkedItems.splice(itemIndex, 1);
        } else if (!product.selected && product.itemData.items) {
            $.each(product.itemData.items, function (index: any, product1) {
                checkedItems.splice(index, 1);
            });
        }

    } else {
        $.each(item.items,
            function (index, product) {
                //processProduct($.extend({ category: item.text }, product));
                fill2(product);
            });
    }
}
//#endregion

//#region dataGridRealTime
var dataGridRealTime = $("#gridRealTime").dxDataGrid({
    allowColumnReordering: true,
    allowColumnResizing: true,
    columnAutoWidth: true,
    rowAlternationEnabled: false,
    columnResizingMode: "widget",
    wordWrapEnabled: true,
    height: "100%",
    columns: [
        {
            dataField: "iD_PARAM",
            visible: false
        }, {
            dataField: "param_name",
            caption: "Параметр"
        },
        {
            dataField: "vaL_AVG",
            caption: "Значение"
        },
        {
            dataField: "val_instal",
            caption: "Установка"
        }
    ],
    columnFixing: {
        enabled: true
    },
    sorting: {
        mode: "none"
    },
    paging: {
        enabled: false
    },
    scrolling: {
        showScrollbar: "always",
        mode: "infinite"
    },
    onContentReady: function (e) {
        e.component.option("loadPanel.enabled", false);
    }

}).dxDataGrid('instance');
//#endregion

//#region clearTechnologyGrid2
function clearTechnologyGrid2() {
    $("#gridRealTime").hide();
    if (dataGridRealTime) {
        dataGridRealTime.option('dataSource', []);
    }
}
//#endregion

//#region loadTechnologyGrid2
function loadTechnologyGrid2(jsonSource: any[]) {
    $("#gridRealTime").show();

    var dataSource = new DevExpress.data.DataSource({
        store: jsonSource
    });

    dataGridRealTime.option('dataSource', dataSource);
  
    isShowTimer = true;
    $("#warning_request").hide();

    var strTime = new Date();
    var hours = strTime.getHours().toLocaleString() ;
    var minutes = strTime.getMinutes().toLocaleString();
    if (hours.length == 1) { hours = "0" + hours; }
    if (minutes.length == 1) { minutes = "0" + minutes; }
    $("#time_request").html("Данные на " + hours + ':' + minutes);

    setTimeout(showWarningTime, 300000);
}
//#endregion

//#region showWarningTime
function showWarningTime() {
    if (isShowTimer) {
        $("#warning_request").show();
        isShowTimer = false;
    }
}
//#endregion

//#region Favorites

export function ShowFavorites2(isShow: boolean) {
    if (isShow) {
        $("#showFavorites").hide();
        $("#showSearch").show();

        $("#favoritesContainer").show();
        $("#searchContainer").hide();
    } else {
        $("#showFavorites").show();
        $("#showSearch").hide();

        setTimeout(function () {
            $("#search-treeview input").focus();
        }, 100);

        $("#favoritesContainer").hide();
        $("#searchContainer").show();
    }
}


function checkFavoritesIdTree() {
    var items = favoriteLookup.option("items");

    var matchedItem: any[] = null;
    if (checkedItems.length > 0) {
        var tmpCheckedItems = $.map(checkedItems, function (item) {
            return JSON.stringify({ "id": item.id, "key": item.key });
        });

        $.each(items, function (index, item) {
            var favorites = JSON.parse(item.favorite);

            var tmpFavorite: any = $.map(favorites, function (item) {
                return JSON.stringify({ "id": item.id, "key": item.key });
            });

            if (tmpCheckedItems.equals(tmpFavorite, false)) {
                matchedItem = item;
            }
        });
    }



    manualSetLookup = true;
    if (matchedItem != null) {
        $("#starIcon").text("star");
    } else {
        $("#starIcon").text("star_border");
    }

    var isSetLookup = false;
    if (favoriteLookup.option("value") === matchedItem) {
        isSetLookup = true;
    }
    favoriteLookup.option("value", matchedItem);

    if (isSetLookup) {
        manualSetLookup = false;
    }
}

var popup: DevExpress.ui.dxPopup;

export function AddToFavorites2() {
    var notification: any;
    if (checkedItems && checkedItems.length > 0) {
        if ($("#starIcon").text() == "star_border") {
            if (!popup) {
                var popupOptions = {
                    width: 500,
                    height: 200,
                    contentTemplate: function (container: any) {
                        var name = $('<div id="favoriteAddName"/>').dxTextBox({
                            placeholder: "Введите название избранного",
                            valueChangeEvent: "keyup",
                            onValueChanged: function (e) {
                                setTimeout(function () {
                                    if (e.value && e.value.length > 0) {
                                        $("#favoriteAddButton").prop('disabled', false);
                                    } else {
                                        $("#favoriteAddButton").prop('disabled', true);
                                    }
                                }, 100);
                            }
                        });
                        name.css("margin-top", "10px");
                        name.appendTo(container);

                        var btn = document.createElement("BUTTON");
                        btn.setAttribute("id", "favoriteAddButton");
                        btn.setAttribute("class", "mdl-button mdl-js-button mdl-button--raised mdl-button--accent");
                        btn.setAttribute('disabled', 'disabled');
                        btn.onclick = function () {
                            SaveFavorites2();
                        }
                        $(btn).text("Добавить в избранное");
                        $(btn).css("width", "100%");
                        $(btn).css("margin-top", "15px");
                        $(btn).css("text-transform", "none");
                        $(btn).appendTo(container);

                        componentHandler.upgradeElement(btn);
                    },
                    showTitle: true,
                    title: "Добавление параметров в избранное",
                    visible: false,
                    dragEnabled: false,
                    closeOnOutsideClick: true
                };

                popup = $("#favoritePopup").dxPopup(popupOptions).dxPopup("instance");
                popup.show();

            } else {
                var favoriteText = $("#favoriteAddName").dxTextBox("instance");
                favoriteText.reset();
                popup.show();
            }
        } else {
            window.showSnackbar('Набор уже в избранном');
        }
    } else {
        window.showSnackbar('Нет выбранных элементов для сохранения');
    }

}

export function SaveFavorites2() {
    var favoriteText = $("#favoriteAddName").dxTextBox("instance");

    return $.ajax({
        url: window.moduleContext.SaveFavorites,
        type: "POST",
        data: {
            array: JSON.stringify(checkedItems),
            name: favoriteText.option('value')
        }
    }).done(function (result: any) {
        popup.hide();
        window.showSnackbar('Сохранено');
        loadFavoriteData();

        var search = $("#search-treeview").dxTextBox("instance");
        search.reset();

        checkFavoritesIdTree();
    });
}

export function deleteFromFavorites2(idFavorites: number) {
    event.stopPropagation();
    return $.ajax({
        url: window.moduleContext.DeleteFavorites,
        type: "POST",
        data: {
            id: idFavorites
        }
    }).done(function (result: any) {
        window.showSnackbar('Удалено', MessageType.default);

        var items = favoriteLookup.option("items");
        items = $.grep(items, function (item: any, index) {
            if (item.id === idFavorites) {
                return false;
            }
            return true;
        });
        favoriteLookup.option("items", items);

        $("#starIcon").text("star_border");

    });
}

//#endregion

$(function () {
    loadFavoriteData();
    loadTreeData2();
    ShowFavorites2(true);
});
