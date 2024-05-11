import axios from '_axios';
import Split from 'split.js';
import jsrender from "jsrender";

import * as datePicker from "components/date-picker";

import * as Main from "./main";
import * as Grid from "./grid";
import { MDCRipple } from '@material/ripple';
import { MDCDialog } from '@material/dialog';


Main.init(document.getElementById("grid"), document.getElementById("chart"));


//#region переменные

let dateBegin: Date;
let dateEnd: Date;

let allowRefresh = true;

let showChartSeparate = false;
let chartShowAllValues = true;

let allowGlobalRefresh = true;
let manualSetLookup = false;

/*----------шаг----------*/
const step = [
    { "Id": 0, "Name": "минута" },
    { "Id": 1, "Name": "час" }
];

let checkedItems: any[] = [];

let chartUpdateWidth = parseFloat(getComputedStyle(Main.chartElement.parentElement, null).width.replace("px", ""));
let chartUpdateHeight = parseFloat(getComputedStyle(Main.chartElement.parentElement, null).height.replace("px", ""));

//#endregion

//#region Split

let dragTimeout: number;
const splitSizes = localStorage.getItem(`${globals.areaController}/splitSizes`);
let lastSizes = [25, 75] // last sizes
let sizes = [25, 75] // default sizes
if (splitSizes) {
    sizes = JSON.parse(splitSizes)
}

const instanceSplit = Split([document.querySelector(".tree-container") as HTMLElement, document.querySelector(".data-container") as HTMLElement], {
    sizes: sizes,
    minSize: 0,
    gutterSize: 41,
    direction: "horizontal",
    onDragEnd(sizes) {
        clearTimeout(dragTimeout);
        dragTimeout = window.setTimeout(() => {
            localStorage.setItem(`${globals.areaController}/splitSizes`, JSON.stringify(sizes));

            setGutterCollapsable();
            componentResize();
        }, 100);
    },
    elementStyle(dimension, size, gutterSize) { return { "flex-basis": "calc(" + size + "% - " + gutterSize + "px)" } },
    gutterStyle(dimension, gutterSize) { return { "flex-basis": gutterSize + "px", } }
});

function setGutterCollapsable() {
    const gutter = document.querySelector(".gutter") as HTMLElement;

    gutter.classList.toggle("gutter-collapsable--expand", !(instanceSplit.getSizes()[0] > 2));
    gutter.classList.toggle("gutter-collapsable--collapse", (instanceSplit.getSizes()[0] > 2));
}

function initGutter() {
    const gutter = document.querySelector(".gutter") as HTMLElement;

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

//#region общие методы

Array.prototype.equals = function (array, strict) {
    if (!array)
        return false;

    if (arguments.length === 1)
        strict = true;

    if (this.length !== array.length)
        return false;

    for (let i = 0; i < this.length; i++) {
        if (this[i] instanceof Array && array[i] instanceof Array) {
            if (!this[i].equals(array[i], strict))
                return false;
        } else if (strict && this[i] !== array[i]) {
            return false;
        } else if (!strict) {
            return this.sort().equals(array.sort(), true);
        }
    }
    return true;
};

function loadMessages() {
    DevExpress.localization.loadMessages({
        "en": {
            "rbs_refresh": "Refresh",
            "rbs_search_show": "Show Search",
            "rbs_favorites": "Favorites",
            "rbs_favorites_show": "Show favorites",
            "rbs_favorites_enter_name": "Favorite name",
            "rbs_favorites_add": "Add to favorites",
            "rbs_favorites_add_options": "Add options to favorites",
            "rbs_favorites_already_in": "The set is already in your favorites",
            "rbs_favorites_no_selected": "No items selected to save",
            "rbs_removed": "Removed",
            "rbs_saved": "Saved",
        },
        "ru": {
            "rbs_refresh": "Обновить",
            "rbs_search_show": "Показать Поиск",
            "rbs_favorites": "Избранное",
            "rbs_favorites_show": "Показать избранное",
            "rbs_favorites_enter_name": "Имя избранного",
            "rbs_favorites_add": "Добавить в избранное",
            "rbs_favorites_add_options": "Добавление параметров в избранное",
            "rbs_favorites_already_in": "Набор уже в избранном",
            "rbs_favorites_no_selected": "Нет выбранных элементов для сохранения",
            "rbs_removed": "Удалено",
            "rbs_saved": "Сохранено",
        }
    });
}

loadMessages();

function componentResize() {
    
    /*таблица*/
    if (Main.gridElement) {
        const component = DevExpress.ui.dxDataGrid.getInstance(Main.gridElement) as DevExpress.ui.dxDataGrid;
        if (component) {
            component.updateDimensions();
        }
    }

    /*графики*/
    if (!document.getElementById("chartContainer")?.classList.contains("hide")) 
    {
        if (Main.chartElement) {
            const widthChart = parseFloat(getComputedStyle(Main.chartElement.parentElement, null).width.replace("px", ""));
            const heightChart = parseFloat(getComputedStyle(Main.chartElement.parentElement, null).height.replace("px", ""));

            if (chartUpdateWidth !== widthChart || chartUpdateHeight !== heightChart) {

                const charts = Main.chartElement.querySelectorAll(".chart");
                charts.forEach((item: HTMLElement) => {
                    
                    item.style.width = `${widthChart}px`;
                    item.style.height = `${heightChart}px`;

                    const component = DevExpress.viz.dxChart.getInstance(item) as DevExpress.viz.dxChart;
                    if (component) {
                        component.option({ size: { width: widthChart, height: heightChart } });

                        (component as DevExpress.viz.dxChart).render({
                            //force: true, // forces redrawing
                            animate: true // redraws the widget with animation
                        });

                        //component.resetVisualRange();
                    }
                });

                chartUpdateWidth = widthChart;
                chartUpdateHeight = heightChart;
            }
        }
        
    }
}
    
let windowResizeTimer: any;
window.addEventListener("resize", () => {
    clearTimeout(windowResizeTimer);
    windowResizeTimer = setTimeout(() => {
        componentResize();
    }, 100);
});

function eventHandler() {


    const isShowFavorites = localStorage.getItem(`${globals.areaController}/showFavorites`);
    if (isShowFavorites) {
        showFavorites((isShowFavorites.toLowerCase() === "true"));
    } else {
        showFavorites(false);
    }

    const valuesMode = Number(window.getLocalStorage("valuesMode"));
    if (!isNaN(valuesMode)) {
        sbValuesMode.option("value", valuesMode);
    }

    const elementRefresh = document.getElementById("refreshButton");
    new MDCRipple(elementRefresh);
        
    (elementRefresh.querySelector(".mdc-button__label") as HTMLElement).innerHTML = DevExpress.localization.formatMessage("rbs_refresh", "");

    const buttonRefresh = elementRefresh as HTMLButtonElement;
    buttonRefresh.onclick = () => {
        window.globalRefresh();
    };    
    
    //добавляем event и ставим локализованные title
    
    const favoritesContainerAnchor = document.querySelector("#starIcon") as HTMLElement;
    if (favoritesContainerAnchor) {
        favoritesContainerAnchor.onclick = () => { addToFavorites(); }
        favoritesContainerAnchor.title = DevExpress.localization.formatMessage("rbs_favorites_add", "");
    }

    const showFavoritesAnchor = document.querySelector("#showFavorites") as HTMLElement;
    if (showFavoritesAnchor) {
        showFavoritesAnchor.onclick = () => { showFavorites(false); }
        showFavoritesAnchor.title = DevExpress.localization.formatMessage("rbs_favorites_show", "");
    }
    const showSearchAnchor = document.querySelector("#showSearch") as HTMLElement;
    if (showSearchAnchor) {
        showSearchAnchor.onclick = () => { showFavorites(true); }
        showSearchAnchor.title = DevExpress.localization.formatMessage("rbs_search_show", "");
    }

    const showTableInput = document.querySelector("#switch_left") as HTMLInputElement;
    if (showTableInput) {
        showTableInput.onclick = () => { showTable(); }
    }

    const showChartInput = document.querySelector("#switch_right") as HTMLInputElement;
    if (showChartInput) {
        showChartInput.onclick = () => { showChart(); }
    }


    const now = new Date();

    dateBegin = new Date();
    dateEnd = new Date();

    if (now.getHours() >= 0 && now.getHours() < 8) {
        dateBegin = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 0, 0);
        dateBegin.setDate(dateBegin.getDate() - 1);

        dateEnd = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 7, 0, 0);
    }
    else if (now.getHours() >= 8 && now.getHours() < 16) {
        dateBegin = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 7, 0, 0);
        dateEnd = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 15, 0, 0);
    }
    else if (now.getHours() >= 16) {
        dateBegin = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 15, 0, 0);
        dateEnd = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 0, 0);
    }

    const dp = new datePicker.DatePicker({ type: datePicker.Type.period, formatType: datePicker.FormatType.datetime, value: { dateBegin: dateBegin, dateEnd: dateEnd }, refreshOnChange: false });

    datePicker.createComponent(
        dp,
        document.getElementById("dateComponent"),
        (begin: Date, end: Date) => {
            dateBegin = begin;
            dateEnd = end;
        },
        () => window.globalRefresh()
    ); 

    loadTreeListData();
}


//#endregion

//#region компоненты
        
const sbStep = new DevExpress.ui.dxSelectBox(document.getElementById("sbStep"), {
    dataSource: step,
    value: 1,
    valueExpr: "Id",
    displayExpr: "Name",
    width: 100,
    stylingMode: "underlined",
    onValueChanged(data) {
        if (allowRefresh === false) {
            allowRefresh = true;
        } else {
            if (data.value === 0) {
                const diff = (dateEnd.getTime() - dateBegin.getTime()) / 3600000;
                if (diff > 8) {
                    allowRefresh = false;

                    window.showSnackbar("Период больше 8-х часов, для минутных данных");

                    sbStep.option("value", data.previousValue);
                } else {
                    window.globalRefresh();
                }
            } else {
                window.globalRefresh();
            }
        }
    }
} as DevExpress.ui.dxSelectBoxOptions<DevExpress.ui.dxSelectBox>);

const sbValuesMode = new DevExpress.ui.dxSelectBox(document.getElementById("sbValuesMode"), {
    label: "Режим",
    width: 175,
    items: [
        { id: 0, name: "Усреднённый", },
        { id: 1, name: "Последнее значение", }
    ],
    stylingMode: "underlined",
    displayExpr: "name",
    valueExpr: "id",
    value: 0,
    onValueChanged(e) {
        window.saveLocalStorage("valuesMode", e.value);
    }
});


/*--------------------*/

/*----------экспорт в excel----------*/
new DevExpress.ui.dxButton(document.getElementById("exportBtn"), {
    icon: "export-excel-button",
    onClick() {
        Grid.exportTechnologyGrid();
    }
});
/*--------------------*/



/*----------кнопки переключения типов графика----------*/
new DevExpress.ui.dxCheckBox(document.getElementById("cbChartSeparate"), {
    value: showChartSeparate,
    text: "Графики раздельно",
    onValueChanged(data) {
        showChartSeparate = data.value;

        Main.loadChart(showChartSeparate, chartShowAllValues);

        setTimeout(() => { componentResize(); }, 10);
    }
});
    
new DevExpress.ui.dxCheckBox(document.getElementById("cbChartShowAllValues"), {
    value: chartShowAllValues,
    text: "Все точки",
    onValueChanged(data) {
        chartShowAllValues = data.value;

        Main.loadChart(showChartSeparate, chartShowAllValues);

        setTimeout(() => { componentResize(); }, 10);
    }
});
/*--------------------*/


/*----------дерево оборудования/параметров----------*/

function loadTreeListData() {
    axios.get(`${window.location.pathname}/GetTechnologyTree`).then((response) => {
        const result = response.data;
        treeList.option({
            rootValue: result.root_value,
            dataSource: new DevExpress.data.DataSource({
                store: result.data,
                sort: ["num_order", "seq", "name"]
            })
        } as any);
    }).catch((error) => {
        console.log("error", error);
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
        mode: "multiple"
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
        showScrollbar: "always",
        useNative: true
    },
    onSelectionChanged(e) {

        const selected = e.component.getSelectedRowsData("leavesOnly") as any[];
        checkedItems = selected?.filter((r) => r.data).map((r) => {
            return {
                id: r.data.tag,
                key: r.id,
                text: r.data.text,
                type: r.data.type
            }
        });

        if (allowGlobalRefresh) {
            window.globalRefresh();
            checkFavoritesIdTree();
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
    },
    onNodesInitialized(e) {
        if (e.root && e.root.children && e.root.children.length === 1) {
            e.root.children.forEach((r) => {
                e.component.expandRow(r.key);
            });
        }
    }
});

/*--------------------*/


//#endregion


//#region Избранное / Поиск

let searchTimer: any;
const searchTree = new DevExpress.ui.dxTextBox(document.getElementById("search-tree"), {
    placeholder: "Поиск",
    //width: 200,
    mode: "search",
    valueChangeEvent: "keyup",
    onValueChanged(e) {
        const searchValue = e.value;
        clearTimeout(searchTimer);

        searchTimer = setTimeout(() => {
            if (searchValue !== null && searchValue !== "") {
                treeList.searchByText(searchValue);
            } else if (searchValue === "") {
                treeList.searchByText(searchValue);
            }
        }, 500);
    }
} as DevExpress.ui.dxTextBoxOptions<DevExpress.ui.dxTextBox>);


class Favorite {
    id: number;
    module_id: number;
    user_id: number;
    favorite_name: string;
    favorite: string;
}

const favoritesLookupStore = new DevExpress.data.DataSource({
    key: "id",
    load() {
        return new Promise((resolve, reject) => {
            if (!manualSetLookup) {
                axios.get(`${window.location.pathname}/GetFavorites`).then((response) => {
                    const favoritesResult = response.data as Array<Favorite>;
                    if (addToFavoritesDialog) {
                        addToFavoritesDialog.close();
                    }

                    favoritesResult.forEach((r) => {
                        r.favorite = JSON.parse(r.favorite);
                    });

                    resolve(favoritesResult);
                }).catch((error) => {
                    reject(error);
                });
            } else {
                resolve([]);
            }
        });
    }
});

const favoritesLookup = new DevExpress.ui.dxLookup(document.getElementById("favoritesLookup"), {
    dataSource: favoritesLookupStore,
    displayExpr: "favorite_name",
    placeholder: DevExpress.localization.formatMessage("rbs_favorites", ""),
    dropDownOptions: {
        hideOnOutsideClick: true,
        showTitle: false,
        minWidth: 400,
        width: "90%",
        container: document.querySelector(".tree-container"),
        position: {
            of: document.querySelector(".tree-container__header"),
            my: "top", at: "bottom"

        }
    },
    showCancelButton: false,
    //applyValueMode: "instantly",
    //showClearButton: true,
    itemTemplate(data, index, element) {
        const tmpl = jsrender.templates(document.getElementById("favorite-tmpl").innerHTML);
        
        const res = tmpl.render(data);
        element.append(res);
        (element[0].querySelector(".favorite__icon") as HTMLElement).onclick = (event) => { deleteFavorites(data.id, event); }
    },
    onValueChanged(e) {
        if (manualSetLookup === false) {
            allowGlobalRefresh = false;

            checkedItems = [];

            treeList.clearSelection();
            if (e.value && e.value.favorite) {
                const data: any[] = e.value.favorite;

                treeList.selectRows(data.map((r) => r.key), false);
                        
                document.getElementById("starIcon").textContent = "star";
            } else {
                document.getElementById("starIcon").textContent = "star_border";
            }

            allowGlobalRefresh = true;
            window.globalRefresh();
        }

        manualSetLookup = false;
    }
});


let addToFavoritesDialog: MDCDialog;
let favoriteText: DevExpress.ui.dxTextBox;

function addToFavorites() {
    if (checkedItems && checkedItems.length > 0) {
        if (document.getElementById("starIcon").textContent === "star_border") {
            if (!addToFavoritesDialog) {
                const dialogElement = document.querySelector(".add-to-favorite-dialog");
                if (dialogElement) {
                    addToFavoritesDialog = new MDCDialog(dialogElement);
                    addToFavoritesDialog.listen("MDCDialog:closing", function (event: any) {
                        if (event.detail.action === "accept") {
                            saveFavorites();
                        }
                    });

                    favoriteText = new DevExpress.ui.dxTextBox(dialogElement.querySelector(".favorite_name__value"), {
                        valueChangeEvent: "keyup",
                        onValueChanged(e) {
                            const btnSave = dialogElement.querySelector(".add-to-favorite-dialog__save") as HTMLButtonElement;
                            btnSave.disabled = (!e.value || e.value.length === 0);
                        }
                    } as DevExpress.ui.dxTextBoxOptions<DevExpress.ui.dxTextBox>);

                    dialogElement.querySelector(".mdc-dialog__title").textContent = DevExpress.localization.formatMessage("rbs_favorites_add_options", "");
                    dialogElement.querySelector(".favorite_name__title").textContent = DevExpress.localization.formatMessage("rbs_favorites_enter_name", "");

                }
            }
            favoriteText.reset();
            addToFavoritesDialog.open();

        } else {
            window.showSnackbar(DevExpress.localization.formatMessage("rbs_favorites_already_in", ""));
        }
    } else {
        window.showSnackbar(DevExpress.localization.formatMessage("rbs_favorites_no_selected", ""));
    }
}
    
function saveFavorites() {
    return axios.post(`${window.location.pathname}/SaveFavorites`, {
        subModuleId: globals.subModuleId,
        userId: globals.userId,
        array: JSON.stringify(checkedItems),
        name: favoriteText.option("value")
    }).then((response) => {
        favoritesLookupStore.load().done(() => {
            checkFavoritesIdTree();
        });

        window.showSnackbar(DevExpress.localization.formatMessage("rbs_saved", ""));
    });
}

function deleteFavorites(idFavorites: number, event: Event) {
    event.stopPropagation();
    
    return axios.post(`${window.location.pathname}/DeleteFavorites`, { id: idFavorites }).then((response) => {
        window.showSnackbar(DevExpress.localization.formatMessage("rbs_removed", ""));
            
        favoritesLookupStore.load().done(() => {
            checkFavoritesIdTree();
        });

        document.getElementById("starIcon").textContent = "star_border";
    });
}
        
function showFavorites(isShow: boolean) {
    const showFavorites = document.getElementById("showFavorites");
    const showSearch = document.getElementById("showSearch");

    const favoritesContainer = document.getElementById("favoritesContainer");
    const searchContainer = document.getElementById("searchContainer");



    showFavorites.classList.toggle("hide", !isShow);
    showSearch.classList.toggle("hide", isShow);

    favoritesContainer.classList.toggle("hide", isShow);
    searchContainer.classList.toggle("hide", !isShow);

    if (isShow) {
    } else {
        setTimeout(() => {
            searchTree?.focus();
        }, 100);
    }

    localStorage.setItem(`${globals.areaController}/showFavorites`, isShow.toString());
}


function checkFavoritesIdTree() {
    const items: any[] = favoritesLookup.option("items");

    let matchedItem: any[] = null;    
    if (checkedItems.length > 0) {

        const tmpCheckedItems = checkedItems.map(item => JSON.stringify({ "id": item.id, "key": item.key }));

        items.forEach((item) => {
            const favorites = item.favorite;

            const tmpFavorite = favorites.map((item: any) => JSON.stringify({ "id": item.id.toString(), "key": item.key }));
            if (tmpCheckedItems.equals(tmpFavorite, false)) {
                matchedItem = item;
            }
        });
    }



    manualSetLookup = true;
    if (matchedItem !== null) {
        document.getElementById("starIcon").textContent = "star";
    } else {
        document.getElementById("starIcon").textContent = "star_border";
    }

    let isSetLookup = false;
    if (favoritesLookup.option("value") === matchedItem) {
        isSetLookup = true;
    }
    favoritesLookup.option("value", matchedItem);

    if (isSetLookup) {
        manualSetLookup = false;
    }
}

    
favoritesLookupStore.load();

//#endregion
    


window.refreshModule = () => {
    document.querySelector(".module-view__content").classList.add("disabled");
    return Main.loadTechnology(checkedItems, dateBegin, dateEnd, sbStep.option("value"), showChartSeparate, chartShowAllValues, sbValuesMode.option("value")).then(() => {
    }).catch(() => {
    }).then(() => {
        document.querySelector(".module-view__content").classList.remove("disabled");
    });
}
    
    

export function showTable() {
        
    document.getElementById("chartContainer")?.classList.add("hide");
    document.getElementById("gridContainer")?.classList.remove("hide");
   
    document.getElementById("exportBtn")?.classList.remove("hide");
    document.getElementById("cbChartSeparate")?.classList.add("hide");
    document.getElementById("cbChartShowAllValues")?.classList.add("hide");

    setTimeout(() => { componentResize(); }, 10);
}

export function showChart() {

    window.ShowLoad(true);
        
    document.getElementById("gridContainer")?.classList.add("hide");
    document.getElementById("chartContainer")?.classList.remove("hide");

    document.getElementById("exportBtn")?.classList.add("hide");
    document.getElementById("cbChartSeparate")?.classList.remove("hide");
    document.getElementById("cbChartShowAllValues")?.classList.remove("hide");

    setTimeout(() => { componentResize(); }, 10);

    window.ShowLoad(false);
}


if (document.readyState !== "loading") {
    eventHandler();
} else {
    document.addEventListener("DOMContentLoaded", eventHandler);
}