import ExcelJS from 'exceljs';
import FileSaver from 'file-saver';

import { gridElement as GridElement } from "./main";

    
let dataGrid: DevExpress.ui.dxDataGrid;
let dataGridLimitsSource: any[];
let dataGridSummarySource: any[];

function createDataGrid() {
    if (!dataGrid && GridElement) {
        dataGrid = new DevExpress.ui.dxDataGrid(GridElement, {
            allowColumnReordering: true,
            allowColumnResizing: true,
            columnAutoWidth: true,
            rowAlternationEnabled: false,
            columnResizingMode: "widget",
            wordWrapEnabled: true,
            height: "100%",
            columnFixing: {
                enabled: true
            },
            sorting: {
                mode: "none"
            },
            export: {
                enabled: false
            },
            paging: {
                enabled: false
            },
            loadPanel: {
                enabled: false
            },
            scrolling: {
                //useNative: true,
                showScrollbar: "always",
                mode: "infinite" // "standard" || "virtual" || "infinite"
                //useNative: true
            },
            summary: {
                /*texts: [{ min: '{0}', max: '{0}', avg: '{0}' }],*/
                /*totalItems: total,*/
                calculateCustomSummary(options: any) {
                    if (options.summaryProcess === "finalize") {
                        options.totalValue = dataGridSummarySource[options.name];
                        if (options.name === "stdev" || options.name === "cnt_err" || options.name === "perc_err") {
                            options.totalValue = options.name;
                        }
                    }
                }
            },
            customizeColumns(columns) {
                if (columns[0]) {
                    columns[0].width = 110;
                    columns[0].fixed = true;
                    columns[0].caption = "Дата/время";
                    columns[0].alignment = "center";
                }

                const tableColumns = dataGrid.option("tableColumns") as any[];
                columns.forEach((item, i) => {
                    if (i > 0) {
                        item.headerCellTemplate = (header, info) => {
                            let div = document.createElement("div");
                            div.innerHTML = info.column.dataField.replace("\\n", "<br>").replace(/[_]/g, ".");
                            div.style.textAlign = "left";
                            header.append(div);
                        };

                        const find = tableColumns.find(v => v.dataField === item.dataField);
                        if (find) {
                            item.dataType = find.dataType;
                        }
                    }
                });
            },
            onCellPrepared(cellInfo) {
                if (cellInfo.column.dataField !== "date" && cellInfo.rowType !== "totalFooter" && (cellInfo.value || cellInfo.value === 0)) {

                    dataGridLimitsSource.forEach((item, i) => {
                        if (item.ParamName == cellInfo.column.dataField) {
                            if ((item.max_err || item.max_err === 0) &&
                                (item.min_err || item.min_err === 0) &&
                                (cellInfo.value > item.max_err || cellInfo.value < item.min_err)
                            ) {
                                cellInfo.cellElement.addClass("error");

                            } else if (
                                (item.max_war || item.max_war === 0) &&
                                (item.min_war || item.min_war === 0) &&
                                (cellInfo.value > item.max_war || cellInfo.value < item.min_war)
                            ) {
                                cellInfo.cellElement.addClass("warning");
                            }
                        }
                    });
                }
            },
            onRowPrepared(e) {
                if (e.values) {
                    const hasNull = e.values.every((element, index, array) => {
                        if (index === 0) {
                            return true;
                        } else {
                            return element == null;
                        }
                    });
                    if (hasNull === true) {
                        e.rowElement.find("td:first-child").addClass("error_no_data").attr("title", "Отсутствуют данные по некоторым полям");;
                    }
                }
            }
        });
    }
}

export function clearTechnologyGrid() {
    if (GridElement) {
        if (!GridElement.classList.contains("hide")) {
            GridElement.classList.add("hide");
        }
    }

    if (dataGrid) {
        dataGrid.option("dataSource", []);
    }
}

export function disposeTechnologyGrid() {
    if (dataGrid) {
        dataGrid.dispose();
        dataGrid = undefined;
    }
}

export function loadTechnologyGrid(jsonSource: any[], limitsSource: any[], summarySource: any[], tableColumns: any[]) {

    dataGridLimitsSource = limitsSource;
    dataGridSummarySource = summarySource;

    createDataGrid();

    if (GridElement) {
        GridElement.classList.remove("hide");
    }
    
        
    const dataSource = new DevExpress.data.DataSource({
        store: jsonSource
    });
        
    const total = [
        {
            column: "date",
            alignment: "right",
            displayFormat: "мин. знач."
        },
        {
            column: "date",
            alignment: "right",
            displayFormat: "макс. знач."
        },
        {
            column: "date",
            alignment: "right",
            displayFormat: "сред. знач."
        },
        {
            column: "date",
            alignment: "right",
            displayFormat: "стандарт.откл.",
            cssClass: "summary_selected",
            summaryType: "custom",
            name: "stdev"
        },
        {
            column: "date",
            alignment: "right",
            displayFormat: "кол-во знач."
        },
        {
            column: "date",
            alignment: "right",
            displayFormat: "откл по ТР <"
        },
        {
            column: "date",
            alignment: "right",
            displayFormat: "откл по ТР >"
        },
        {
            column: "date",
            alignment: "right",
            displayFormat: "Кол-во откл.",
            cssClass: "summary_selected",
            summaryType: "custom",
            name: "cnt_err"
        },
        {
            column: "date",
            alignment: "right",
            displayFormat: "% откл",
            cssClass: "summary_selected",
            summaryType: "custom",
            name: "perc_err"
        }
    ];

    if (jsonSource.length > 0) {
        let jsonItem = jsonSource[0];
        Object.keys(jsonItem).forEach(key => {
            //console.log(key, jsonItem[key]);
            if (key !== "date") {
                const name = key;

                let item: any = {};
                item["column"] = name; 
                item["name"] = `min_${name}`;
                item["summaryType"] = "custom";
                item["valueFormat"] = { type: "fixedPoint", precision: 2 };
                item["alignment"] = "right";
                total.push(item);

                item = {};
                item["column"] = name;
                item["name"] = `max_${name}`;
                item["summaryType"] = "custom";
                item["valueFormat"] = { type: "fixedPoint", precision: 2 };
                item["alignment"] = "right";
                total.push(item);

                item = {};
                item["column"] = name;
                item["name"] = `avg_${name}`;
                item["summaryType"] = "custom";
                item["valueFormat"] = { type: "fixedPoint", precision: 2 };
                item["alignment"] = "right";
                total.push(item);

                item = {};
                item["column"] = name;
                item["name"] = `stdev_${name}`;
                item["summaryType"] = "custom";
                item["valueFormat"] = { type: "fixedPoint", precision: 2 };
                item["alignment"] = "right";
                item["cssClass"] = "summary_selected";
                total.push(item);

                item = {};
                item["column"] = name;
                item["name"] = `cnt_${name}`;
                item["summaryType"] = "custom";
                //item["valueFormat"] = { type: 'fixedPoint', precision: 2 };
                item["alignment"] = "right";
                total.push(item);

                item = {};
                item["column"] = name;
                item["name"] = `cnt_min_err_${name}`;
                item["summaryType"] = "custom";
                //item["valueFormat"] = { type: 'fixedPoint', precision: 2 };
                item["alignment"] = "right";
                total.push(item);

                item = {};
                item["column"] = name;
                item["name"] = `cnt_max_err_${name}`;
                item["summaryType"] = "custom";
                //item["valueFormat"] = { type: 'fixedPoint', precision: 2 };
                item["alignment"] = "right";
                total.push(item);

                item = {};
                item["column"] = name;
                item["name"] = `cnt_err_${name}`;
                item["summaryType"] = "custom";
                //item["valueFormat"] = { type: 'fixedPoint', precision: 2 };
                item["alignment"] = "right";
                item["cssClass"] = "summary_selected";
                total.push(item);

                item = {};
                item["column"] = name;
                item["name"] = `perc_err_${name}`;
                item["summaryType"] = "custom";
                item["valueFormat"] = { type: "fixedPoint", precision: 1 };
                item["alignment"] = "right";
                item["cssClass"] = "summary_selected";
                total.push(item);
            }
        });
    }

    dataGrid.option({
        dataSource: dataSource,
        tableColumns: tableColumns,
        summary: {
            totalItems: total
        }
    } as any);
}

export function exportTechnologyGrid() {
    if (dataGrid) {        
        const workbook = new ExcelJS.Workbook(); 
        const worksheet = workbook.addWorksheet(); 

        DevExpress.excelExporter.exportDataGrid({ 
            worksheet: worksheet, 
            component: dataGrid,
            customizeCell: customizeExcelCell,
            autoFilterEnabled: true
        }).then(() => {
            workbook.xlsx.writeBuffer().then((buffer) => { 
                FileSaver.saveAs(new Blob([buffer], { type: 'application/octet-stream' }), `Технология.xlsx`); 
            }); 
        });
    }
}

function customizeExcelCell(options: any) {
    const { gridCell, excelCell } = options;
        
    if (gridCell.rowType === "header" && gridCell.column.caption) {
        excelCell.value = gridCell.column.caption.toString().replace("\\n", "\r\n").replace("&deg","\xB0");
        excelCell.alignment = { horizontal: "left" };
        excelCell.font = { bold: true };
                            
    }
    if (gridCell.rowType === "data" && gridCell.column.dataField !== "date" && gridCell.value !== null) {
        dataGridLimitsSource.forEach(function (item, i) {
            if (item.ParamName == gridCell.column.dataField && (gridCell.value || gridCell.value === 0)) {
                if ((item.max_err || item.max_err === 0) && (item.min_err || item.min_err === 0) && (gridCell.value > item.max_err || gridCell.value < item.min_err)) {
                    excelCell.fill = {
                        type: "pattern",
                        pattern:"solid",
                        fgColor: { argb: "F44336" }
                    };
                    excelCell.font = { color: {argb: "FFFFFF"} };
                } else if ((item.max_war || item.max_war === 0) && (item.min_war || item.min_war === 0) && (gridCell.value > item.max_war || gridCell.value < item.min_war)) {
                    excelCell.fill = {
                        type: "pattern",
                        pattern:"solid",
                        fgColor: { argb: "FBC02D" }
                    };
                }
            }

        });
    }
    if (gridCell.rowType === "totalFooter") {
        const name: string = gridCell.totalSummaryItemName;
        if (name && (name.startsWith("stdev") || name.startsWith("cnt_err") || name.startsWith("perc_err"))) {
            excelCell.fill = {
                type: "pattern",
                pattern:"solid",
                fgColor: { argb: "FFCC80" }
            };
        } else {
            excelCell.fill = {
                type: "pattern",
                pattern:"solid",
                fgColor: { argb: "F5F5F5" }
            };
        }

        excelCell.border = {
          top: {style:'thin', color: {argb:'BDBDBD'}},
          left: {style:'thin', color: {argb:'BDBDBD'}},
          bottom: {style:'thin', color: {argb:'BDBDBD'}},
          right: {style:'thin', color: {argb:'BDBDBD'}}
        }
    }
}