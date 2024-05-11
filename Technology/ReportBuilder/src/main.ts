import axios from "_axios";

import * as Grid from "./grid";
import * as Chart from "./chart";

export let gridElement: HTMLElement;
export let chartElement: HTMLElement;

export function init(grid: HTMLElement, chart: HTMLElement) {
    gridElement = grid;
    chartElement = chart;
}

let jsonSource: any[];
let limitsJsonSource: any[];
let tableSummary: any = {};
let tableColumns: any[];

export function loadTechnology(checkedItems: any[], dateBegin: Date, dateEnd: Date, step: number, showChartSeparate: boolean, chartShowAllValues: boolean, valuesMode: number) {    
    return new Promise((resolve, reject) => {
        const diff = (dateEnd.getTime() - dateBegin.getTime()) / 3600000;
        if (step === 0 && diff > 8) {
            window.showSnackbar("Период больше 8-х часов, для минутных данных");
            resolve(null);
        } else {
            let find = checkedItems.some(v => v.type === 2);
            if (valuesMode === 0 && find === true) {
                window.showSnackbar("Данные по некоторым параметрам нельзя усреднять");
            }

            loadData(checkedItems, dateBegin, dateEnd, step, valuesMode).then((result: any) => {
                generateSource(checkedItems, result);
            }).then(() => {
                if (jsonSource) {
                    loadGrid();
                    loadChart(showChartSeparate, chartShowAllValues);
                }
                resolve(null);
            }).catch((error) => {
                reject(error);
            });
        }
    });

    
}

function loadData(checkedItems: any[], dateBegin: Date, dateEnd: Date, step: number, valuesMode: number): Promise<any> {
    return new Promise((resolve, reject) => {

        if (checkedItems.length > 0) {
            const checkedId: any[] = [];
            checkedItems.forEach((item, i) => {
                checkedId.push(item.id);
            });
            axios.post(`${window.location.pathname}/GetTechnologyCollection`, {
                array: checkedId.toString(),
                dateBegin: dateBegin.toISOString(),
                dateEnd: dateEnd.toISOString(),
                cStep: step,
                valuesMode: valuesMode
            }).then((response) => {
                resolve(response.data);
            }).catch((error) => {
                reject(error);
            });

        } else {
            let obj = Grid.clearTechnologyGrid;
            if (typeof obj === "function") {
                Grid.clearTechnologyGrid();
            }
            obj = Chart.clearTechnologyChart;
            if (typeof obj === "function") {
                Chart.clearTechnologyChart();
            }

            resolve(null);
        }
    });
}
    
function generateSource(checkedItems: any[], result: any) {
    jsonSource = [];
    tableSummary = {};
    limitsJsonSource = [];
    tableColumns = [];

    if (result) {
        var data = result;
            
        let jsonData: any[] = data.jsonData;
        jsonData.forEach((item, i) => {
            var model: any = {};

            model["date"] = item.date;

            Object.keys(item).forEach(key => {
                let value = item[key];
                if (key !== "date") {
                    let find = checkedItems.filter(row => {
                        if (row.id == key.replace("c_", "")) {
                            return row;
                        } else {
                            return null;
                        }

                    });
                    if (find && find.length > 0) {
                        if (find[0].type === 1 && value && value.toString().indexOf(",") !== -1) {
                            model[find[0].text] = Number(value.toString().replace(",", "."));
                        } else {
                            model[find[0].text] = value;
                        }
                    }
                }
            });
            jsonSource.push(model);
        });
        
        Object.keys(data.jsonSummary).forEach(key => {
            let value = data.jsonSummary[key];

            //console.log('key', key, key.substring(key.lastIndexOf("_") + 1, key.length));
            let keyFind = key.substring(key.lastIndexOf("_") + 1, key.length);
            let find = checkedItems.filter(row => {
                if (row.id == keyFind) {
                    return row;
                } else {
                    return null;
                }

            });
            if (find && find.length > 0) {
                let keyName = key.substring(0, key.lastIndexOf("_")) + "_" + find[0].text;

                tableSummary[keyName] = value;
            }
        });

        limitsJsonSource = data.jsonLimits;
        limitsJsonSource.forEach((item, i) => {
            Object.keys(item).forEach(key => {
                if (key !== "date") {
                    let find = checkedItems.filter(row => {
                        if (row.id == key) {
                            return row;
                        } else {
                            return null;
                        }

                    });
                    /*if (find && find.length > 0) {
                        model[find[0].text] = value;
                    }*/
                }
            });
            let find = checkedItems.filter(row => {
                if (row.id == item.ParamId) {
                    return row;
                } else {
                    return null;
                }

            });
            if (find && find.length > 0) {
                item.ParamName = find[0].text;
            }
            //limitsJsonSource.push(model);
        });

        tableColumns = checkedItems.map((item) => {
            return { dataField: item.text, dataType: (item.type === 1 ? "number" : "string") };
        })
    }
}

export function loadGrid() {
    Grid.loadTechnologyGrid(jsonSource, limitsJsonSource, tableSummary, tableColumns);
}
export function loadChart(showChartSeparate: boolean, chartShowAllValues: boolean) {
    Chart.loadTechnologyChart(jsonSource, limitsJsonSource, showChartSeparate, chartShowAllValues);
}