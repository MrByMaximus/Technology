import axios from "_axios";
import jsrender from "jsrender";
import {MDCRipple} from '@material/ripple';
import {MDCDialog} from '@material/dialog';
import {MDCIconButtonToggle} from '@material/icon-button';
import WZoom from "vanilla-js-wheel-zoom";


import * as Main from "technology/main";
import * as Grid from "technology/grid";
import * as Chart from "technology/chart";


//#region Общие методы

class Gage {
    id: string;
    name: string;
}

let timerId: number;
let timerSecond: number = 30;
let mnemoArray: any[];
let mnemoDetailArray: any[] = [];
let gage: Gage;

let statePanelVisible = false;
let predictionPanelVisible: boolean = null;

let wzoom: any;

window.refreshModule = function() {
    if (document.querySelector(".technology-container").classList.contains("hide")) {
        stopTimer();
        return loadMnemoData().then(data => loadSchemas(data)).then(() => loadPredictionType()).then(() => predictionGrid.refresh()).then(() => {
            document.getElementById("lastDateRefresh").innerHTML = `${new Date(Date.now()).toLocaleString()}`;
            startTimer();
        });
    } else {
        return loadTechnology(window.dateBegin, window.dateEnd);
    }
}

window.hasChangesModule = function () {
    stopTimer();
    return false;
};

if (document.readyState !== 'loading') {
    onLoad();
} else {
    document.addEventListener('DOMContentLoaded', onLoad);
}

function customGlobalRefresh() {
    timerSecond = 30;
    window.globalRefresh(true, false);
}

function onLoad() {
    axios.get(window.moduleContext.GetMnemo).then(function (response) {
        mnemoArray = response.data;

        const placeData: any[] = mnemoArray.filter((item: any) => item.id_parent === null);
        placeSelectBox.option("dataSource", placeData);

        if (placeData && placeData.length > 0) {
            let placeId: any = window.urlParam()["place_id"];


            if(typeof placeData[0].id == "number"){
                placeId = Number(placeId);
            }


            if (placeId) {
                placeSelectBox.option("value", placeId);
            } else {
                let equipmentId: any = window.urlParam()["equipment_id"];

                if (typeof placeData[0].id == "number") {
                    equipmentId = Number(equipmentId);
                }

                const equipment = mnemoArray.find((v) => v.equipment_id == equipmentId);
                if (equipment) {
                    placeSelectBox.option("value", equipment.id_parent);
                } else {
                    if (placeData.length === 1) {
                        placeSelectBox.option("value", placeData[0].id);
                    }
                }
            }
        }
    });
    const refresh = document.getElementById("refresh-button");
    refresh.onclick = customGlobalRefresh;
    const back = document.getElementById("back-button");
    back.onclick = showSheme;
    const refreshTechnology = document.getElementById("refreshTechnology-button");
    refreshTechnology.onclick = () => { customGlobalRefresh(); };

    

    const buttonRipple = new MDCRipple(document.querySelector('.mdc-button__show-state'));
    const btnShowState = document.querySelector(".mdc-button__show-state") as HTMLButtonElement;
    btnShowState.onclick = () => {
        document.querySelector(".state-container").classList.toggle("hide", statePanelVisible);

        if (statePanelVisible) {
            btnShowState.classList.remove("mdc-button--unelevated");
            btnShowState.classList.add("mdc-button--outlined");

            statePanelVisible = false;
        } else {
            btnShowState.classList.add("mdc-button--unelevated");
            btnShowState.classList.remove("mdc-button--outlined");

            statePanelVisible = true;
        }


        stateGrid?.repaint();
        
        //window.saveLocalStorage("StatePanelVisible", statePanelVisible.toString());
    };

    const buttonRipplePrediction = new MDCRipple(document.querySelector('.mdc-button__show-prediction'));
    const btnShowPrediction = document.querySelector(".mdc-button__show-prediction") as HTMLButtonElement;
    btnShowPrediction.onclick = () => {
        document.querySelector(".prediction-container").classList.toggle("hide", predictionPanelVisible);

        if (predictionPanelVisible) {
            btnShowPrediction.classList.remove("mdc-button--unelevated");
            btnShowPrediction.classList.add("mdc-button--outlined");

            predictionPanelVisible = false;
        } else {
            btnShowPrediction.classList.add("mdc-button--unelevated");
            btnShowPrediction.classList.remove("mdc-button--outlined");

            predictionPanelVisible = true;
        }


        predictionGrid?.repaint();
        
        window.saveLocalStorage("PredictionPanelVisible", predictionPanelVisible.toString());
    };
    const prediction = window.getLocalStorage("PredictionPanelVisible");
    if (prediction) {
        predictionPanelVisible = JSON.parse(prediction.toLowerCase());
    }
    
    const btnChangeState = document.querySelector(".state-container__change-button") as HTMLButtonElement;
    if (window.moduleAllowEdit) {
        btnChangeState.onclick = () => {
            changeState();
        };
    } else {
        btnChangeState.classList.add("hide");
    }
    btnChangeState.disabled = true;

    Main.init(document.querySelector(".technology-grid"), document.querySelector(".technology-container--chart"));
    
     
            
    const container = document.querySelector(".scheme-container") as HTMLElement;    
                
    wzoom = WZoom.create(container, {
        type: 'html',
        dragScrollableOptions: {
            smoothExtinction: .05
        },
        maxScale: 5,
        speed: 3,
        zoomOnClick: false,
        //smoothExtinction: .2,
    });

    const schemeFullscreenButton = document.querySelector(".scheme__fullscreen-button") as HTMLButtonElement;
    const schemeFullscreenButtonToggle = new MDCIconButtonToggle(schemeFullscreenButton);
    
    schemeFullscreenButton.onmouseup = (e) => {
        schemeFullscreenButton.blur();

        const scheme = document.querySelector(".scheme") as HTMLElement;
        const header: HTMLElement = document.querySelector(".layout-header");
        
        wzoom.maxZoomDown();

        if (scheme.classList.contains("scheme--fullscreen")) {
            header.style.display = '';
            schemeFullscreenButton.querySelector(".material-icons").innerHTML = "fullscreen";
        } else {
            header.style.display = 'none';
            schemeFullscreenButton.querySelector(".material-icons").innerHTML = "fullscreen_exit";
        }

        scheme.classList.toggle("scheme--fullscreen");
    }
}
 

//#endregion    


//#region компоненты

const placeSelectBox = new DevExpress.ui.dxSelectBox(document.getElementById("sbPlace"), {
    //value: defaultValue,
    width: 150,
    placeholder: "Выберите ЛО",
    stylingMode: "underlined",
    valueExpr: "id",
    displayExpr: "name",
    onValueChanged(e: any) {
        //console.log('place onValueChanged', e.value);
        //console.log('mnemoArray', mnemoArray);
        const array = mnemoArray.filter((item: any) => item.id_parent === e.value)/*.sort((item: any) => item.name)*/;
        //console.log('array', array);
        /*if (array && array.length > 1) {
            array.push({ id: -1, id_parent: e.value, name: "Все" });
            array.sort((a, b) => a.id - b.id);
        }*/
        mixerSelectBox.option("dataSource", array);
        if (array && array.length > 0) {
            const mixerId: any = window.urlParam()["mixer_id"];
            if (mixerId) {
                const item = array.filter((item: any) => item.id == mixerId);
                if (item.length > 0) {
                    mixerSelectBox.option("value", item[0]);
                } else {
                    mixerSelectBox.option("value", array[0]);
                }
            } else {
                const equipmentId: any = window.urlParam()["equipment_id"];
                if (equipmentId) {
                    const item = array.filter((item: any) => item.equipment_id == equipmentId);
                    if (item.length > 0) {
                        mixerSelectBox.option("value", item[0]);
                    } else {
                        mixerSelectBox.option("value", array[0]);
                    }
                } else {
                    mixerSelectBox.option("value", array[0]);
                }
            }
        } else {
            mixerSelectBox.option("value", null);
        }

        window.setUrlParam("place_id", e.value);
    }
});

const mixerSelectBox = new DevExpress.ui.dxSelectBox(document.getElementById("sbMixer"), {
    //dataSource: placeStore,
    //value: defaultValue,
    placeholder: "Выберите миксер",
    width: 200,
    stylingMode: "underlined",
    //valueExpr: "id",
    displayExpr: "name",
    onValueChanged(e) {

        stateGrid.clearSelection();
        stateGrid.refresh();
        
        if (e.previousValue !== null) {
            predictionGrid.refresh();
        }

        loadBlobFile(e.value);
        if (e.value) {
            btnGroupSchemeFile.option("disabled", false);
        }

        window.setUrlParam("mixer_id", e.value.id);
    }
}as DevExpress.ui.dxSelectBoxOptions<DevExpress.ui.dxSelectBox>);

const btnGroupSchemeFile = new DevExpress.ui.dxButtonGroup(document.getElementById("btnGroupSchemeFile"), {
    disabled: true,
    items: [
        {
            icon: "download",
            text: DevExpress.localization.formatMessage('rbs_download', ''),
            hint: DevExpress.localization.formatMessage('rbs_scheme_file_download', '')
        }, {
            icon: "upload",
            text: DevExpress.localization.formatMessage('rbs_upload', ''),
            hint: DevExpress.localization.formatMessage('rbs_scheme_file_upload', '')
        }
    ],
    onItemClick: function (e) {
        if (mixerSelectBox.option("value")) {
            if (e.itemIndex === 0) {
                downloadFile(mixerSelectBox.option("value"));
            }
        }
    },
    visible: window.moduleAllowEdit
});

new DevExpress.ui.dxFileUploader(document.getElementById("fileUploader"), {
    uploadUrl: "",
    labelText: "",
    multiple: false,
    dialogTrigger: document.getElementById("btnGroupSchemeFile").querySelector(".dx-buttongroup-item:last-child"),
    showFileList: false,
    uploadMethod: "POST",
    accept: ".svg",
    uploadMode: "instantly",
    uploadHeaders: {
        "X-SignalR-ConnectionId": globals.signalRConnectionId,
        "X-RBS-SubModuleId": globals.subModuleId
    },
    onValueChanged(e: any) {
        const mixer = mixerSelectBox.option("value") as any;
        if (mixer) {
            window.ShowLoad(true);
            const url = window.moduleContext.UploadMnemoFile.replace("_mixerId", mixer.id);
            e.component.option("uploadUrl", url);
        }
    },
    onUploaded(e: any) {
        e.component.reset();
        window.showSnackbar("Загружено");
        loadBlobFile(mixerSelectBox.option("value"));
        window.ShowLoad(false);
    },
    visible: window.moduleAllowEdit
});

let stateDialog: MDCDialog;

let stateType: DevExpress.ui.dxSelectBox;
let stateDateBeginDay: DevExpress.ui.dxDateBox;
let stateDateBeginHour: DevExpress.ui.dxDateBox;
let stateDateEndDay: DevExpress.ui.dxDateBox;
let stateDateEndHour: DevExpress.ui.dxDateBox;
let stateRequest: DevExpress.ui.dxTextBox;
let stateNote: DevExpress.ui.dxTextArea;


export function changeState() {
    if (!stateDialog) {
        stateDialog = new MDCDialog(document.querySelector('.state-dialog'));
        stateDialog.listen('MDCDialog:closing', function(event:any) {
            //if (event.detail.action === "accept") {
            //    //console.log("save");
            //    saveChanges();
            //} else {
            //    //console.log("NOT save");
            //}
            startTimer();
        });
        stateDialog.listen('MDCDialog:opening', function(event:any) {
            (document.querySelector(".change-state-object__value") as HTMLElement).focus();
            stopTimer();
        });

        stateType = new DevExpress.ui.dxSelectBox(document.querySelector(".change-state-type__value"), {
            dataSource: [
                { "Id": 0, "Name": "Отключена" },
                { "Id": 1, "Name": "Включена" }
            ],
            value: 0,
            valueExpr: 'Id',
            displayExpr: 'Name',
            readOnly: !window.moduleAllowEdit,
            onValueChanged(e: any) {
                //console.log("onValueChanged", e);
                if (e && e.value) {                    
                    stateDialog.root.querySelector(".state-disable-container").classList.add("visibility--hidden");
                } else {
                    stateDialog.root.querySelector(".state-disable-container").classList.remove("visibility--hidden");
                    if (!stateDateBeginDay.option("value") && !stateDateBeginHour.option("value")) {
                        const now = new Date();
                        now.setHours(now.getHours() + 1);
                        now.setMinutes(0);
                        now.setSeconds(0);
                        stateDateBeginDay.option("value", now);
                        stateDateBeginHour.option("value", now);
                    }
                }
            }
        } as DevExpress.ui.dxSelectBoxOptions<DevExpress.ui.dxSelectBox>);
        new DevExpress.ui.dxValidator(document.querySelector(".change-state-type__value"),{
            validationGroup: "changeStateType",
            validationRules: [{
                type: "required"
            }]
        });


        stateDateBeginDay = new DevExpress.ui.dxDateBox(document.querySelector(".change-state-date-begin__value--day"), {
            displayFormat: "shortDate",
            type: "date",
            useMaskBehavior: true,
            readOnly: !window.moduleAllowEdit,
        });
        new DevExpress.ui.dxValidator(document.querySelector(".change-state-date-begin__value--day"),{
            validationGroup: "changeState",
            validationRules: [{
                type: "required"
            }]
        });

        stateDateBeginHour = new DevExpress.ui.dxDateBox(document.querySelector(".change-state-date-begin__value--hour"), {
            type: "time",
            interval: 60,
            useMaskBehavior: true,
            readOnly: !window.moduleAllowEdit,
            onContentReady(e) {
                (e.element[0].querySelector(".dx-texteditor-input") as HTMLInputElement).disabled = true;
            }
        });
        new DevExpress.ui.dxValidator(document.querySelector(".change-state-date-begin__value--hour"),{
            validationGroup: "changeState",
            validationRules: [{
                type: "required"
            }],
        });

        stateDateEndDay = new DevExpress.ui.dxDateBox(document.querySelector(".change-state-date-end__value--day"), {
            displayFormat: "shortDate",
            type: "date",
            useMaskBehavior: true,
            readOnly: !window.moduleAllowEdit,
        });
        new DevExpress.ui.dxValidator(document.querySelector(".change-state-date-end__value--day"),{
            validationGroup: "changeState",
            validationRules: [{
                type: "required"
            }]
        });

        stateDateEndHour = new DevExpress.ui.dxDateBox(document.querySelector(".change-state-date-end__value--hour"), {
            type: "time",
            interval: 60,
            useMaskBehavior: true,
            readOnly: !window.moduleAllowEdit,
            onContentReady(e) {
                (e.element[0].querySelector(".dx-texteditor-input") as HTMLInputElement).disabled = true;
            }
        });
        new DevExpress.ui.dxValidator(document.querySelector(".change-state-date-end__value--hour"),{
            validationGroup: "changeState",
            validationRules: [{
                type: "required"
            }]
        });

        stateRequest = new DevExpress.ui.dxTextBox(document.querySelector(".change-state-request__value"), {
            maxLength: 50,
            readOnly: !window.moduleAllowEdit,
        } as DevExpress.ui.dxTextBoxOptions<DevExpress.ui.dxTextBox>);
        new DevExpress.ui.dxValidator(document.querySelector(".change-state-request__value"),{
            validationGroup: "changeState",
            validationRules: [{
                type: "required"
            }]
        });

        stateNote = new DevExpress.ui.dxTextArea(document.querySelector(".change-state-note__value"), {
            maxLength: 2000,
            readOnly: !window.moduleAllowEdit,
            placeholder: "Укажите причину отключения"
        });

        const saveBtn = stateDialog.root.querySelector(".state-dialog__save") as HTMLButtonElement;
        if (saveBtn) {
            saveBtn.onclick = () => {
                let changeStateType = DevExpress.validationEngine.validateGroup("changeStateType");
                if (changeStateType.isValid) {
                    if (stateType.option("value") === 0) {
                        let changeState = DevExpress.validationEngine.validateGroup("changeState");
                        if (changeState.isValid) {
                            saveChanges();
                        }
                    } else {
                        saveChanges();
                    }
                }
            }
        }
    }




    const selectedGages = stateGrid.getSelectedRowsData().sort((a, b) => { return a.gage_id - b.gage_id});
    if (selectedGages.length > 0) {
        
        const array = [];
        if (selectedGages.length === stateGrid.getDataSource().items().length) {
            array.push(`<div class='gage-tag-content'>${placeSelectBox.option("text")} -> ${mixerSelectBox.option("text")}</div>`);
        } else {
            for (let i = 0; i < selectedGages.length; i++) {
                array.push(`<div class='gage-tag-content'>${selectedGages[i].gage_name}</div>`);
            }
        }

        stateDialog.root.querySelector(".change-state-object__value").innerHTML = array.join("");

        stateDialog.open();
    }

    if (selectedGages.length === 1) {
        let state = selectedGages[0].state;
        if (state == 1 && selectedGages[0].date_begin) {
            state = 0;
        }
        stateType.option("value", state);
        stateDateBeginDay.option("value", selectedGages[0].date_begin);
        stateDateBeginHour.option("value", selectedGages[0].date_begin);
        stateDateEndDay.option("value", selectedGages[0].date_end);
        stateDateEndHour.option("value", selectedGages[0].date_end);
        stateRequest.option("value", selectedGages[0].request);
        stateNote.option("value", selectedGages[0].description);
    } else {
        stateType.reset();
        stateDateBeginDay.reset();
        stateDateBeginHour.reset();
        stateDateEndDay.reset();
        stateDateEndHour.reset();
        stateRequest.reset();
        stateNote.reset();
    }
    
}

function saveChanges() {
    const state = stateType.option("value");
    const dateBeginDay = new Date(stateDateBeginDay.option("value"));
    const dateBeginHour = new Date(stateDateBeginHour.option("value"));
    const dateBegin = new Date(dateBeginDay.getFullYear(), dateBeginDay.getMonth(), dateBeginDay.getDate(), dateBeginHour.getHours());
    
    const dateEndDay = new Date(stateDateEndDay.option("value"));
    const dateEndHour = new Date(stateDateEndHour.option("value"));
    const dateEnd = new Date(dateEndDay.getFullYear(), dateEndDay.getMonth(), dateEndDay.getDate(), dateEndHour.getHours());

    const request = stateRequest.option("value");
    const note = stateNote.option("value");

    //console.log("saveChanges", state, dateBegin, dateEnd, request, note);

    const array = [];
    const selectedGages = stateGrid.getSelectedRowsData();
    if (selectedGages.length > 0) {        
        for (let i = 0; i < selectedGages.length; i++) {
            array.push(selectedGages[i].gage_id);
        }
    }

    return axios.post(window.moduleContext.ChangeGagesState, {
        state: state,
        dateBegin: dateBegin,
        dateEnd: dateEnd,
        request: request,
        note: note,
        array: array.join(",")
    }).then((response) => {
        stateDialog.close();

        stateGrid.clearSelection();
        stateGrid.refresh();

    }).catch((problem) => {
        console.log("problem", problem);
    });
}


const stateGrid = new DevExpress.ui.dxDataGrid(document.querySelector(".state-container__grid"), {
    dataSource: new DevExpress.data.DataSource({
        key: "gage_id",
        load() {
            return new Promise((resolve, reject) => {
                const mixerId = (mixerSelectBox.option("value") as any)?.id;
                if (mixerId) {
                    axios.get(window.moduleContext.GetMixerGagesState, {
                        params: {
                            mixerId: mixerId
                        }
                    }).then(function (response) {
                        //console.log(response.data);
                        (response.data as any[]).forEach((item) => {
                            //if (item.date_begin) {
                            //    item.date_begin = new Intl.DateTimeFormat('ru-RU', {
                            //        year: "numeric",
                            //        month: "2-digit",
                            //        day: "2-digit",
                            //        hour: "2-digit",
                            //        minute: "2-digit"
                            //    }).format(new Date(item.date_begin));
                            //}
                            //if (item.date_end) {
                            //    item.date_end = new Intl.DateTimeFormat('ru-RU', {
                            //        year: "numeric",
                            //        month: "2-digit",
                            //        day: "2-digit",
                            //        hour: "2-digit",
                            //        minute: "2-digit"
                            //    }).format(new Date(item.date_end));
                            //}
                        });
                        resolve(response.data);
                    }).catch((problem: any) => {
                        reject(problem);
                    });
                } else {
                    resolve([]);
                }
            });
        }
    }),
    columns: [
        {
            dataField: "gage_name",
            caption: "Наименование",
            //cellTemplate: document.getElementById("gage-state-tmpl")
            cellTemplate: function (container: any, options: any) {
                const html = document.getElementById("gage-state-tmpl")?.innerHTML;
                if (html) {
                    const tmpl = jsrender.templates(document.getElementById("gage-state-tmpl").innerHTML);
                    //console.log(options.data);
                    const res = tmpl.render(options.data);
                    container.append(res);
                    /*if (options.data.state === 0)*/ {
                        (container[0].querySelector("div") as HTMLDivElement).onclick = (event) => {
                            //console.log("добавить в избранное");
                            (options.component as DevExpress.ui.dxDataGrid).clearSelection();
                            (options.component as DevExpress.ui.dxDataGrid).selectRowsByIndexes([options.rowIndex]).then(() => changeState());
                        }
                    }
                }
                
            }
        },        
    ],
    paging: {
        enabled: false
    },
    sorting: {
        mode: "none"
    },
    scrolling: {
        showScrollbar: "always"
    },
    selection: {
        mode: window.moduleAllowEdit ? "multiple" : "none",
        allowSelectAll: true,
        showCheckBoxesMode: "always"
    },
    onSelectionChanged(e) {
        const btnChangeState = document.querySelector(".state-container__change-button") as HTMLButtonElement;
        if (e.selectedRowKeys.length > 0) {
            btnChangeState.disabled = false;
        } else {
            btnChangeState.disabled = true;
        }
    }
});
    
const sbStep = new DevExpress.ui.dxSelectBox(document.getElementById("sbStep"), {
    dataSource: [
        { "Id": 0, "Name": "минута" },
        { "Id": 1, "Name": "час" }
    ],
    value: 1,
    valueExpr: 'Id',
    displayExpr: 'Name',
    width: 100,
    onValueChanged() {
        customGlobalRefresh();
    }
});


const predictionGridOptions: DevExpress.ui.dxDataGridOptions = {
    dataSource: new DevExpress.data.DataSource({
        load() {
            return new Promise((resolve, reject) => {
                const equipmentId = (mixerSelectBox.option("value") as any)?.equipment_id;
                if (equipmentId) {
                    axios.get(window.moduleContext.GetTagPrediction, {
                        params: {
                            equipmentId: equipmentId,
                            showAll: predictionShowAll.option("value") ? 1 : 0,
                            type: predictionType.option("value"),
                        }
                    }).then(function (response) {
                        if (response.data) {
                            const btnShowPrediction = (document.querySelector(".mdc-button__show-prediction") as HTMLButtonElement);
                            btnShowPrediction.classList.remove("hide");                            
                            if (predictionPanelVisible !== false) {
                                predictionPanelVisible = false;
                                btnShowPrediction.click();
                            }

                            const filterTags = (response.data as any[]).filter((item) => item.date_value_prediction !== null);
                            if (filterTags && filterTags.length > 0) {
                                const lastDateTag = filterTags.reduce((a, b) => {
                                    return  new Date(a.date_value_prediction) < new Date(b.date_value_prediction) ? a : b;
                                });
                                lastDateTag.first_date = 1;
                            }

                            resolve(response.data);
                        } else {
                            window.deleteLocalStorage("PredictionPanelVisible");
                            document.querySelector(".prediction-container").classList.toggle("hide", true);
                            resolve([]);
                        }
                    }).catch((problem: any) => {
                        reject(problem);
                    });
                } else {
                    resolve([]);
                }
            });
        }
    }),
    columns: [
        {
            dataField: "tag_num",
            caption: "Датчик температуры",
            width: 90,
            alignment: "center",
        },
        ({
            dataField: "date_prediction",
            caption: "Дата формирования прогноза",
            dataType: "date",
            width: 100,
            alignment: "center",
            merge: {mode: "nullDependOn", dependOn: "date_value_prediction"},
        } as any),
        {
            dataField: "value_limit",
            caption: "Пороговая температура",
            width: 90,
            alignment: "center",
            merge: {mode: "nullDependOn", dependOn: "date_value_prediction"},
        },
        {
            dataField: "merge_column",
            width: 0,
            minWidth:0,
        },
        {
            dataField: "date_value_prediction",
            caption: "Дата превышения температуры",
            dataType: "datetime",
            width: 120,
            alignment: "center",
        },
        {
            dataField: "days_before_fail",
            caption: "Дней до отказа",
            width: 70,
            alignment: "center",
        },
    ],
    onCellPrepared(e) {
        if (e.rowType === "data") {
           if (e.column.dataField === "tag_num") {
               const cellElement = (e.cellElement[0] as HTMLElement);
               cellElement.setAttribute("data-state", e.row.data.state);
           }
           if (e.column.dataField === "date_value_prediction" && e.row.data.first_date === 1) {
               const cellElement = (e.cellElement[0] as HTMLElement);
               cellElement.setAttribute("data-first_date", e.row.data.first_date);
               cellElement.title = "Ближайшая дата превышения температуры";
           }
     }
    },
    width: "100%",
    paging: {
        enabled: false
    },
    sorting: {
        mode: "none"
    },
    scrolling: {
        showScrollbar: "always"
    },
    onContentReady(e) {
        (e.component as any).startMerge();
    }
}

const predictionGrid = new DevExpress.ui.dxDataGrid(document.querySelector(".prediction-container__grid"), predictionGridOptions as DevExpress.ui.dxDataGridOptions);

const predictionShowAll = new DevExpress.ui.dxCheckBox(document.querySelector(".prediction__show-all"), {
    value: (window.getLocalStorage("PredictionShowAll") === "true" ? true : false),
    text: "Все точки",
    onValueChanged(e) {
        predictionGrid.refresh();
        window.saveLocalStorage("PredictionShowAll", e.value.toString().toLowerCase());
    }
});

const predictionType = new DevExpress.ui.dxSelectBox(document.querySelector(".prediction__type"), {
    stylingMode: "underlined",
    //placeholder: "Тип прогноза",
    width: 200,
    valueExpr: "id_predictiontype",
    displayExpr: "name",
    showClearButton: false,
    onValueChanged(e) {
        if (e.previousValue !== null) {
            predictionGrid.refresh();
        }
    }
} as DevExpress.ui.dxSelectBoxOptions<DevExpress.ui.dxSelectBox>);

function loadPredictionType() {
    return new Promise((resolve, reject) => {
        axios.get(window.moduleContext.GetTagPredictionType).then(function (response) {
            const data = response.data as any[];
            if (data && data.length > 0) {
                predictionType.option("items", data);
                predictionType.option("value", data[0].id_predictiontype);
            }
            resolve(null);
        }).catch((problem: any) => {
            reject(problem);
        });
    });
}

//#endregion

function startTimer() {
    stopTimer();

    const timeToRefresh = document.getElementById("timeToRefresh");

    timerId = window.setInterval(function () {
        if (timeToRefresh) {
            timeToRefresh.innerHTML = timerSecond.toString();
            if (timerSecond === 0) {
                window.globalRefresh(true, false);
                timerSecond = 30;
            } else {
                timerSecond--;
            }
        } else {
            stopTimer();
        }
    },
    1000);
}

function stopTimer() {
    timerSecond = 30;
    clearTimeout(timerId);
}

function loadBlobFile(mixer: any) {
        
    const container = document.querySelector(".scheme-container") as HTMLElement;
    while (container.firstChild) {
        container.removeChild(container.firstChild);
    }

    Grid.disposeTechnologyGrid();
    Chart.disposeTechnologyChart();

    if (mixer) {
        wzoom.maxZoomDown();
        axios.get(window.moduleContext.GetMnemoFile, { params: { blobId: mixer.id_blob } }).then(function (response) {
            const result = response.data;// as XMLDocument
            if (result && result.length > 0) {
                container.innerHTML = result;
                    
                loadMnemoDetail().then(() => {
                    window.globalRefresh(true, false);
                });

                startTimer();
            } else {
                window.showNotification("Схема не найдена", "default");
                resetTimer();
            }
        });
    } else {
        resetTimer();
    }

}

function resetTimer() {
    stopTimer();

            
    const timeToRefresh = document.getElementById("timeToRefresh");

    while (timeToRefresh.firstChild) {
        timeToRefresh.removeChild(timeToRefresh.firstChild);
    }
            
    const lastDateRefresh = document.getElementById("lastDateRefresh");
    while (lastDateRefresh.firstChild) {
        lastDateRefresh.removeChild(lastDateRefresh.firstChild);
    }
}

function downloadFile(mixer: any) {
        
    window.ShowLoad(true);

    const win = window.open("");
    const oldOpen = window.open;

    /*window.open = function(url: string) {
        win.location.href = url;
        window.open = oldOpen;
        win.focus();
    };*/
    
    axios.get(window.moduleContext.DownloadMnemoFile, { params: { blobId: mixer.id_blob } }).then(function (response) {
        const result = response.data;
        
        window.ShowLoad(false);
        if (result.success) {
            const url = window.moduleContext.DownloadFile +
                "?fileGuid=" + result.fileGuid +
                "&mimeType='" + result.mimeType + "'" +
                "&filename=" + result.fileName;

            /*if (result.viewType === 0) {
                window.location.href = url;
                window.open = oldOpen;
                win.close();
            } else {
                window.open(url);
            }*/
            window.location.href = url;
            window.open = oldOpen;
            win.close();

        } else {
            win.close();
            window.showNotification(result.message, "default");
        }
    }).catch((error) => {
        window.ShowLoad(false);
        window.open = oldOpen;
        win.close();
    });
}

function loadMnemoDetail() {
    return axios.get(window.moduleContext.GetMnemoDetail, { params: { "idMnemo": (mixerSelectBox.option("value") as any).id } }).then(function (response) {
        mnemoDetailArray = response.data;
    });
}

function loadMnemoData() {
    let data = "";
    if (mnemoDetailArray) {
        data = mnemoDetailArray.map(item => item.gage_id).join(',');
    }

    return axios.get(window.moduleContext.GetMnemoData, { params: { "parameters": data } }).then((response) => response.data);
}

function loadSchemas(data: any[]) {
    //console.log('data', data);
    /*let id: number = mixerSelectBox.option("value");
    if (id === -1) {
        console.log('загрузить', "все");
    } else {
            
        console.log('загрузиь', id);
    }*/

    /*var a = document.getElementById("svgMnemo") as HTMLObjectElement;
        
    // get the inner DOM of alpha.svg
    var svgDoc = a.contentDocument;*/
    //let maxDate = Math.max.apply(Math, data.map(function(item) { return item.date_stamp; }));

    //let container = document.querySelector(".scheme-container");
    //console.log('document.getElementById("gage928010511")', document.getElementById("gage928010511"));
    mnemoDetailArray.forEach((value, index, array) => {
            
        if (value.gage_id) {
            // get the inner element by id
            const id = "gage" + value.gage_id;
            const delta = document.getElementById(id);
            // add behaviour
            if (delta) {
                const find = data.find(x => x.gage_id === value.gage_id);
                //let gageValue = Number(find.value);
                //delta.style.fill = "transparent";

                delta.querySelectorAll("rect").forEach((rect) => {
                    if (rect && find) {
                        //rect.style.fill = "transparent";
                        rect.style.fill = find.color;
                        //rect.style.stroke = "green";
                    }
                });
                    
                const text = delta.querySelector("text");
                if (text && find) {
                    text.textContent = find.value;
                }
                    
                //delta.innerHTML = find.toString() + "&deg;";

                delta.style.cursor = "pointer";
                if (!delta.querySelector("title")) {
                    const title = document.createElementNS('http://www.w3.org/2000/svg', 'title');
                    title.textContent = "Показать историю";
                    delta.appendChild(title);
                }

                //console.log('delta.hasOwnProperty', delta.hasOwnProperty('onclick'));

                delta.onclick = (event) => {
                    event.stopPropagation();
                    gage = new Gage();
                    gage.id = value.gage_id;
                    gage.name = value.name;
                    showTechnology();
                }
            }
        }
    });
    /*var tableSource = mnemoDetailArray.filter((item: any) => item.id_mnemo === id);
    console.log('tableSource', tableSource);
    let rowMin = Math.min.apply(Math, tableSource.map(function(item) { return item.y; }));
    let rowMax = Math.max.apply(Math, tableSource.map(function(item) { return item.y; }));
    let colMin = Math.min.apply(Math, tableSource.map(function(item) { return item.x; }));
    let colMax = Math.max.apply(Math, tableSource.map(function(item) { return item.x; }));

    console.log('строк', rowMin, rowMax);
    console.log('колонок', colMin, colMax);
    //y - строка, x - столбец
        
    let container = document.querySelector(".scheme-container");
    container.innerHTML = null;

    var grid = document.createElement("div");
    grid.classList.add("mdl-grid"/*, "mdl-grid--no-spacing"#1#);

    for (var i = rowMin; i <= rowMax; i++) {
        for (var j = colMin; j <= colMax; j++) {
            let cell = document.createElement("div");
            cell.classList.add("mdl-cell", "mdl-cell--2-col");
                
            cell.innerHTML = "&nbsp;";
            cell.style.backgroundColor = "lightgray";

            cell.style.padding = "10px";
            cell.style.textAlign = "center";
            grid.append(cell);
        }
    }
        
    container.appendChild(grid);*/
}

function showSheme() {
    customGlobalRefresh();

    startTimer();

    document.querySelector(".scheme").classList.toggle("hide");
    document.querySelector(".technology-container").classList.toggle("hide");

    document.getElementById("subPanelScheme").classList.toggle("hide");
    document.getElementById("subPanelTechnology").classList.toggle("hide");

    Grid.clearTechnologyGrid();
    Chart.clearTechnologyChart();
}

function showTechnology() {
    stopTimer();

    document.querySelector(".scheme").classList.toggle("hide");
    document.querySelector(".technology-container").classList.toggle("hide");

    document.getElementById("subPanelScheme").classList.toggle("hide");
    document.getElementById("subPanelTechnology").classList.toggle("hide");

    let dateBegin = new Date();
    let dateEnd = new Date();
        
    const now = new Date();
    const nowHours = now.getHours();
        
    if (nowHours >= 0 && nowHours < 8) {
        dateBegin = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 0, 0);
        dateBegin.setDate(dateBegin.getDate() - 1);

        dateEnd = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 7, 0, 0);
    }else if (nowHours >= 8 && nowHours < 16) {
        dateBegin = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 7, 0, 0);
        dateEnd = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 15, 0, 0);
    }else if (nowHours >= 16) {
        dateBegin = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 15, 0, 0);
        dateEnd = new Date(now.getFullYear(), now.getMonth(), now.getDate(), 23, 0, 0);
    }

    const w: any = window;
    w.setDate(dateBegin, dateEnd);

    loadTechnology(dateBegin, dateEnd);
}

function loadTechnology(dateBegin: Date, dateEnd: Date) {
    const item = [{ id: gage.id, key: gage.id, text: gage.name }];
                    
    Chart.chartOptions.export.enabled = false;
    Chart.chartOptions.legend.visible = false;

    return Main.loadTechnology(item, dateBegin, dateEnd, sbStep.option('value'), false, true, 0);
}