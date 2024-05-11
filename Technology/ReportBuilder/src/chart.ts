import { chartElement as ChartElement } from "./main";

export let chartOptions: DevExpress.viz.dxChartOptions = {

    /*palette: 'Default',*/
    /*palette: 'Soft Pastel',*/
    /*palette: 'Harmony Light',*/
    /*palette: 'Pastel',*/
    /*palette: 'Bright',*/
    /*palette: 'Soft',*/
    /*palette: 'Ocean',*/
    /*palette: 'Vintage',*/
    /*palette: 'Violet'*/

    /*palette: ['#7CBAB4', '#92C7E2', '#75B5D6', '#B78C9B', '#F2CA84', '#A7CA74'],*/
    /*palette: ['#377eb8', '#e41a1c', '#4daf4a', '#984ea3', '#ff7f00', '#ffff33', '#a65628', '#f781bf'],*/
    /*palette: ['#5cbae6', '#b6d957', '#fac364', '#8cd3ff', '#d998cb', '#f2d249', '#93b9c6', '#ccc5a8', '#52bacc', '#dbdb46', '#98aafb'],*/
    /*palette: ['#4D4D4D', '#5DA5DA', '#FAA43A', '#60BD68', '#F17CB0', '#B2912F', '#B276B2', '#DECF3F', '#F15854'],*/
    /*palette: ['#60a69f', '#78b6d9', '#6682bb', '#a37182', '#eeba69'],*/
    palette: ["#5b9bd5", "#ed7d31", "#a5a5a5", "#ffc000", "#4472c4", "#70ad47"], /*office*/


    commonSeriesSettings: {
        argumentField: "date",
        type: "line",
        point: {
            hoverStyle: {
                size: 6
            },
            size: 8
        }
    },
    argumentAxis: {
        /*label: {
            overlappingBehavior: 'rotate',
            rotationAngle: 270
        }*/
        
    },
    export: {
        enabled: true,
        fileName: "Технология (график)"
    },

    scrollBar: {
        visible: false,
        position: "bottom"
    },
    onLegendClick(e) {
        const series = e.target;
        series.isVisible() ? series.hide() : series.show();
    },
    tooltip: {
        enabled: true,
        customizeTooltip(arg) {
            return {
                text: arg.argumentText + "<br/> <br/>" + arg.valueText
            };
        }
    },
    legend: {
        visible: true
    }
}

export function clearTechnologyChart() {
    if (!ChartElement.classList.contains("hide")) {
        ChartElement.classList.add("hide");
    }
}

export function disposeTechnologyChart() {
    if (ChartElement) {
        ChartElement.querySelectorAll(".chart").forEach((element, key, parent) => {
            const widget = DevExpress.viz.dxChart.getInstance(element);
            if (widget) {
                widget.dispose();
                element.parentNode.removeChild(element);
            }
        });
    }
    
}
    
export function loadTechnologyChart(json: any[], limits: any[], showChartSeparate: boolean, chartShowAllValues: boolean) {

    if (ChartElement) {
        while (ChartElement.firstChild) {
            ChartElement.removeChild(ChartElement.firstChild);
        }
        ChartElement.classList.remove("hide");
    }
    
        
    if (json && json.length > 0) {
        /*для корректного отображения заголовков в графиках (заменяет \n на тире)*/
        const jsonString = JSON.stringify(json).replace(/\\\\n/g, " - ");
        json = JSON.parse(jsonString);
        if (limits) {
            const limitsString = JSON.stringify(limits).replace(/\\\\n/g, " - ");
            limits = JSON.parse(limitsString);
        }

        if (showChartSeparate) {
            loadMultiplyChart(json, limits, chartShowAllValues);
        } else {
            loadSingleChart(json, limits, chartShowAllValues);
        }
    }
}

function loadSingleChart(json: any[], limits: any, showAllPoint: boolean) {

    const tmpData: any[] = json;


    const series: any[] = [];

    Object.keys(tmpData[0]).forEach(key => {
        if (key !== "date") {
            const item: any = {
                valueField: key,
                name: key
            }
            series.push(item);
        }
    });

    const values: any[] = [];

    json.forEach((data, index) => {
        Object.keys(data).forEach(key => {
            const value = data[key];
            if (key !== "date") {
                values.push(value);
            }
        });
    });

    const valuesForMinMax = values.filter(val => val !== null);
    const maxSource = Math.max.apply(null, valuesForMinMax);
    const minSource = Math.min.apply(null, valuesForMinMax);

    let valueAxis: any = undefined;

    if (limits.length > 0) {
        const data = limits[0];
        const arrayItem = limits.filter((item: any) => {
            if (item["max_err"] !== data["max_err"] ||
                item["max_war"] !== data["max_war"] ||
                item["min_err"] !== data["min_err"] ||
                item["min_war"] !== data["min_war"])
                return item;
            return null;
        });

        if (arrayItem != null && arrayItem.length > 0) {
            //не рисуем линии если пределы разнятся
        } else {
            //пределы совпали

            const lines: any[] = [];
            let linesMax: any = undefined;
            let linesMin: any = undefined;
            let item: any;
            if (data.max_war && data.max_war != data.max_err) {
                item = {
                    label: {
                        text: `Max пред. ${data.max_war}`,
                        verticalAlignment: "bottom"
                    },
                    value: data.max_war,
                    width: 2,
                    dashStyle: "dash",
                    color: "#FBC02D"
                };
                lines.push(item);
                linesMax = data.max_war;
            }
            if (data.min_war && data.min_war != data.min_err) {
                item = {
                    label: { text: `Min пред. ${data.min_war}` },
                    value: data.min_war,
                    width: 2,
                    dashStyle: "dash",
                    color: "#FBC02D"
                };
                lines.push(item);
                linesMin = data.min_war;
                //linesMin = data.min_war - (data.min_war / 50);
            }
            if (data.max_err) {
                item = {
                    label: { text: `Max авар. ${data.max_err}`, paddingLeftRight: 0 },
                    value: data.max_err,
                    width: 2,
                    dashStyle: "dash",
                    color: "#F44336"
                };
                lines.push(item);
                linesMax = data.max_err;
                //linesMax = data.max_err + (data.max_err / 50);
            }
            if (data.min_err) {
                item = {
                    label: {
                        text: `Min авар. ${data.min_err}`,
                        paddingLeftRight: 0,
                        verticalAlignment: "bottom"
                    },
                    value: data.min_err,
                    width: 2,
                    dashStyle: "dash",
                    color: "#F44336"
                };
                lines.push(item);
                linesMin = data.min_err;
                //linesMin = data.min_err - (data.min_err / 50);
            }

            if (linesMax < maxSource) {
                linesMax = maxSource;
            }
            if (linesMin > minSource) {
                linesMin = minSource;
            }

            if (showAllPoint === false) {

                let delValue = 1;
                if (linesMin >= 1000) {
                    delValue = 50;
                } else {
                    delValue = 100;
                }

                linesMin = data.min_err - (data.min_err / delValue);
                linesMax = data.max_err + (data.max_err / delValue);

                if (linesMin == 0 && linesMax == 0) {
                    linesMin = minSource;
                    linesMax = maxSource;
                }
            } else {
                if (linesMax < maxSource) {
                    linesMax = maxSource;
                }
                if (linesMin > minSource) {
                    linesMin = minSource;
                }
            }

            valueAxis = {
                valueType: "numeric",
                valueMarginsEnabled: true,
                maxValueMargin: 0.1,
                visualRange: {
                    startValue: linesMax,
                    endValue: linesMin
                },
                //max: linesMax,
                //min: linesMin,
                constantLines: lines,
                grid: {
                    opacity: 0.5
                }
            };
        }

    }

    const dataSource = new DevExpress.data.DataSource({
        store: {
            type: "array",
            data: tmpData
        },
        paginate: false
    });

    if (series.length === 2) {
        series[0].axis = "first";
        series[1].axis = "second";

        valueAxis = [
            {
                name: "first",
                grid: { visible: true }
            }, {
                name: "second",
                position: "right",
                grid: { visible: true }
            }
        ];
    }


    chartOptions.series = series;
    chartOptions.valueAxis = valueAxis;
    chartOptions.dataSource = dataSource;
    chartOptions.zoomAndPan = {
        valueAxis: "both",
        argumentAxis: "none",
        dragToZoom: true,
        allowMouseWheel: true,
        panKey: "shift"
    };
    /*chartOptions.title = { text: "Технология - часовые данные" };*/

    chartOptions.tooltip = {
        enabled: true,
        customizeTooltip(arg: any) {
            return {
                text: `${arg.seriesName}<br/>&nbsp;<br/>${arg.argumentText}<br/>&nbsp;<br/>${arg.valueText}`
            }
        }
    };

    let width = 0;
    let height = 0;

    if (ChartElement) {
        width = ChartElement.clientWidth - 20;
        height = ChartElement.clientHeight - 20;
    }
        
    const chartContainer = document.createElement("div");
    chartContainer.classList.add("chart");
    chartContainer.style.width = width + "px";
    chartContainer.style.height = height + "px";

    if (ChartElement) {
        ChartElement.appendChild(chartContainer);
    }

    new DevExpress.viz.dxChart(chartContainer, chartOptions);
    
}

function loadMultiplyChart(dataSource: any[], limits: any[], showAllPoint: boolean) {
        
    const series: any[] = [];

    Object.keys(dataSource[0]).forEach(key => {
        if (key !== "date") {
            const item: any = {
                valueField: key,
                name: key
            }

            series.push(item);
        }
    });

    const magic: any[] = [];

    for (var i = 0; i < series.length; i++) {
        const param = series[i].name;
        const magicItem: any[] = [];

        let dateTmp: any;

        dataSource.forEach((row, index) => {
            Object.keys(row).forEach(key => {
                const value = row[key];
                if (key == "date") {
                    dateTmp = value;
                }
                if (key == param) {
                    const item: any = { };
                    item["date"] = dateTmp;
                    item[param] = value;

                    magicItem.push(item);
                }
            });
        });
        magic.push(magicItem);
    }


    let width = 0;
    let height = 0;

    if (ChartElement) {
        width = ChartElement.clientWidth - 20;
        height = ChartElement.clientHeight - 20;
    }

    magic.forEach((row: any[], index) => {
        
        const values: any[] = [];

        row.forEach((data, index) => {
            Object.keys(data).forEach(key => {
                let value = data[key];
                if (key !== "date") {
                    values.push(value);
                }
            });
        });

        const series: any[] = [];

        Object.keys(row[0]).forEach(key => {
            if (key !== "date") {
                const item: any = {
                    valueField: key,
                    name: key
                }

                series.push(item);
            }
        });

        /*console.log("------------------------------------");
        console.log('options', options);*/
        //console.log('values', values);
        const valuesForMinMax = values.filter(val => val !== null);
        //console.log('valuesForMinMax', valuesForMinMax);

        const maxSource = Math.max.apply(null, valuesForMinMax);
        const minSource = Math.min.apply(null, valuesForMinMax);
        /*console.log('maxSource', maxSource);
        console.log('minSource', minSource);*/

        const newOptions = window.completeAssign({}, chartOptions); // Deep copy
        newOptions["dataSource"] = row;
        newOptions["series"] = series;
        newOptions["legend"] = { visible: false };
        if (series[0] && series[0].name) {
            newOptions["title"] = { text: series[0].name };
        }

        newOptions["scrollBar"] = { visible: false };
        newOptions["zoomAndPan"] = {
            allowMouseWheel: false,
            allowTouchGestures: false
        };
        //newOptions["scrollingMode"] = "none";
        //newOptions["zoomingMode"] = "none";

        limits.forEach((data, index) => { 
            if (series[0] && series[0].name && data.ParamName == series[0].name) {

                const lines: any[] = [];
                let linesMax: any = undefined;
                let linesMin: any = undefined;
                let item: any;

                if (data.max_war && data.max_war != data.max_err) {
                    item = {
                        label: {
                            text: `Max пред. ${data.max_war}`,
                            verticalAlignment: "bottom"
                        },
                        value: data.max_war,
                        width: 2,
                        dashStyle: "dash",
                        color: "#FBC02D"
                    };
                    lines.push(item);
                    linesMax = data.max_war;
                    //linesMax = data.max_war + (data.max_war / 50);
                }
                if (data.min_war && data.min_war != data.min_err) {
                    item = {
                        label: { text: `Min пред. ${data.min_war}` },
                        value: data.min_war,
                        width: 2,
                        dashStyle: "dash",
                        color: "#FBC02D"
                    };
                    lines.push(item);
                    linesMin = data.min_war;
                    //linesMin = data.min_war - (data.min_war / 50);
                }
                if (data.max_err) {
                    item = {
                        label: { text: `Max авар. ${data.max_err}` },
                        value: data.max_err,
                        width: 2,
                        dashStyle: "dash",
                        color: "#F44336"
                    };
                    lines.push(item);
                    linesMax = data.max_err;
                    //linesMax = data.max_err + (data.max_err / 50);
                }
                if (data.min_err) {
                    item = {
                        label: {
                            text: `Min авар. ${data.min_err}`,
                            verticalAlignment: "bottom"
                        },
                        value: data.min_err,
                        width: 2,
                        dashStyle: "dash",
                        color: "#F44336"
                    };
                    lines.push(item);
                    linesMin = data.min_err;
                    //linesMin = data.min_err - (data.min_err / 50);
                }


                /*if (linesMin == undefined) {
                    linesMin = data.min_err - (data.min_err / 10);
                }
                if (linesMax == undefined) {
                    linesMax = data.max_err + (data.max_err / 10);
                }*/

                //console.log(showAllPoint);
                if (showAllPoint === false) {

                    let delValue = 1;
                    if (linesMin >= 1000) {
                        delValue = 50;
                    } else {
                        delValue = 100;
                    }

                    linesMin = data.min_err - (data.min_err / delValue);
                    linesMax = data.max_err + (data.max_err / delValue);

                    if (linesMin == 0 && linesMax == 0) {
                        linesMin = minSource;
                        linesMax = maxSource;
                    }
                } else {
                    if (linesMax < maxSource) {
                        linesMax = maxSource;
                    }
                    if (linesMin > minSource) {
                        linesMin = minSource;
                    }
                    /*linesMax = undefined;
                    linesMin = undefined;*/
                }

                newOptions["valueAxis"] = {
                    valueType: "numeric",
                    valueMarginsEnabled: true,
                    maxValueMargin: 0.1,
                    visualRange: {
                        startValue: linesMax,
                        endValue: linesMin
                    },
                    //max: linesMax,
                    //min: linesMin,
                    constantLines: lines,
                    grid: {
                        opacity: 0.5
                    }
                };

            }
        });
            

        //console.log('newOptions', newOptions);
        //console.log("------------------------------------");

        const color = "transparent";

        const chartContainer = document.createElement("div");
        chartContainer.classList.add("chart");
        chartContainer.style.width = width + "px";
        chartContainer.style.height = height + "px";
        chartContainer.style.marginBottom = 20 + "px";
        chartContainer.style.backgroundColor = color;

        new DevExpress.viz.dxChart(chartContainer, newOptions);

        if (ChartElement) {
            ChartElement.appendChild(chartContainer);
        }            
    });
}
