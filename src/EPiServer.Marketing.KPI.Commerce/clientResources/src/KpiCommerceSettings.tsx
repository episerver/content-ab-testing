import React, { useState, useEffect } from "react";
import "@rmwc/select/styles";
import { TextButton, Card, CardContentArea, ExposedDropdownMenu, Typography } from "@episerver/ui-framework";
import { KpiCommerceSettings } from "./models/KpiCommerceSettingsModel";
import axios from "axios";
import { Snackbar, SnackbarAction } from "@rmwc/snackbar/dist/snackbar";
import '@rmwc/snackbar/styles';

const KpiCommerceSettings : React.FC = () => {
    const [KpiCommerceSettings, setKpiCommerceSettings] = useState<KpiCommerceSettings>({} as KpiCommerceSettings);
    const [snackBarMessage, setSnackBarMessage] = useState({ message: "", isOpen: false });
    const root = document.getElementById("root");
    const moduleUrl = root?.dataset.moduleShellPath;

    useEffect(() => {
        axios.get<KpiCommerceSettings>(`${moduleUrl}Setting/Get`)
            .then(response => {
                setKpiCommerceSettings(response.data)
            });
    }, []);

    const changePreferredMarket = (value: string) => setKpiCommerceSettings(prevKpiCommerceSettings =>
        ({ ...prevKpiCommerceSettings, preferredMarket: value }));

    const save = () => {
        axios.post(`${moduleUrl}Setting/Save`, {
            preferredMarket: KpiCommerceSettings.preferredMarket
        }).then(response => {
            if (response.status === 200) {
                setSnackBarMessage({ message: response.data, isOpen: true });
            }
        });
    };

    return (
        <div className="kpi-commerce-container">
            <div className="header">
                <Typography tag="h1" use="headline3">{KpiCommerceSettings.kpiCommerceConfigTitle}</Typography>
            </div>
            <Snackbar
                open={snackBarMessage.isOpen}
                onClose={evt => setSnackBarMessage({ message: "", isOpen: false })}
                message={snackBarMessage.message}
                dismissesOnAction
                action={
                    <SnackbarAction
                        label="Dismiss"
                    />
                }
            />

            <Card header={KpiCommerceSettings.preferredMarketDescription}>
                <CardContentArea>
                    <ExposedDropdownMenu
                        value={KpiCommerceSettings.preferredMarket}
                        label={KpiCommerceSettings.preferredMarketLabel}
                        options={KpiCommerceSettings.marketList}
                        onValueChange={changePreferredMarket}
                    />
                </CardContentArea>

                <TextButton contained onClick={save}>{KpiCommerceSettings.kpiCommerceSaveButton}</TextButton>
            </Card>

        </div>
    );
};


export default KpiCommerceSettings;
