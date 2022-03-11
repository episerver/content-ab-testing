import React, { useState, useEffect } from "react";
import "@rmwc/select/styles";
import { Card, CardContentArea, TextButton, ExposedDropdownMenu, TextField, Typography, Checkbox } from "@episerver/ui-framework";
import { ABTestingSettingsModel } from "./models/ABTestingSettingsModel";
import axios from "axios";
import { Snackbar, SnackbarAction } from "@rmwc/snackbar/dist/snackbar";
import '@rmwc/snackbar/styles';

const ABTestingSettings = () => {
    const [aBTestingSettingsModel, setABTestingSettingsModel] = useState<ABTestingSettingsModel>({} as ABTestingSettingsModel);
    const [aBTestingSettingsModelOrg, setABTestingSettingsModelOrg] = useState<ABTestingSettingsModel>({} as ABTestingSettingsModel);
    const [snackBarMessage, setSnackBarMessage] = useState({ message: "", isOpen: false });
    const root = document.getElementById("root");
    const moduleUrl = root?.dataset.moduleShellPath;

    useEffect(() => {
         axios.get<ABTestingSettingsModel>(`${moduleUrl}Setting/Get`)
             .then(response => {
                setABTestingSettingsModel(response.data);
                setABTestingSettingsModelOrg(response.data)
             });
    }, []);

    const handleChange = (e: React.ChangeEvent<HTMLInputElement>)=>{
        let { name, value, checked, type } = e.target;
        let val = ["text", "number"].includes(type) ? value : checked;
        setABTestingSettingsModel({ ...aBTestingSettingsModel, [name]: val })
    }
    
    const changeAutoPublishWinner = (value: string) => setABTestingSettingsModel({ ...aBTestingSettingsModel, autoPublishWinner: value === "true" });

    const changeConfidenceLevel = (value: string) => setABTestingSettingsModel({ ...aBTestingSettingsModel, confidenceLevel: parseInt(value) });

    const isValidForm=()=>{
        if(aBTestingSettingsModel.participationPercent < 1 || aBTestingSettingsModel.participationPercent > 100){
            setSnackBarMessage({ message: aBTestingSettingsModel.participationError, isOpen: true });
            return false;
        }
        if(aBTestingSettingsModel.testDuration < 1 || aBTestingSettingsModel.testDuration > 365)
        {
            setSnackBarMessage({ message: aBTestingSettingsModel.durationError, isOpen: true });
            return false;
        }
        return true;
    }

    const save = () => {
        if(isValidForm()){
            axios.post(`${moduleUrl}Setting/Save`, {
                TestDuration: aBTestingSettingsModel.testDuration,
                ParticipationPercent: aBTestingSettingsModel.participationPercent,
                ConfidenceLevel: aBTestingSettingsModel.confidenceLevel,
                AutoPublishWinner: aBTestingSettingsModel.autoPublishWinner,
                IsEnabled: aBTestingSettingsModel.isEnabled
            }).then(response => {
                if (response.status === 200) {
                    setSnackBarMessage({ message: response.data, isOpen: true });                 
                }
            }).catch(e => {
                if(e.response?.status === 400) {
                    setSnackBarMessage({ message: e.response?.data, isOpen: true });                 
                }
            });
        }
    };

    const cancel = () =>{
        setABTestingSettingsModel(aBTestingSettingsModelOrg);
    }
    return (   
        <div className="abtesting-config-container">
            <div className="header">
                <Typography tag="h1" use="headline3">{aBTestingSettingsModel.abTestingConfigTitle}</Typography>
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
            <Typography use="subtitle2">{aBTestingSettingsModel.abTestingConfigDescription}</Typography>
            <Card>
                <CardContentArea>
                    <TextField
                        label={aBTestingSettingsModel.testDurationLabel}
                        value={aBTestingSettingsModel.testDuration?.toString()}
                        onChange={handleChange}
                        type="number"
                        name="testDuration"
                    />
                </CardContentArea>
                <CardContentArea>
                    <TextField
                        label={aBTestingSettingsModel.participationPercentLabel}
                        value={aBTestingSettingsModel.participationPercent?.toString()}
                        onChange={handleChange}
                        type="number"
                        name="participationPercent"
                    />
                </CardContentArea>
                <CardContentArea>
                    <ExposedDropdownMenu
                        label={aBTestingSettingsModel.autoPublishWinnerLabel}
                        value={aBTestingSettingsModel.autoPublishWinner?.toString()}
                        options={aBTestingSettingsModel.autoPublishWinners}
                        onValueChange={value => changeAutoPublishWinner(value)}
                    />
                </CardContentArea>
                <CardContentArea>
                    <ExposedDropdownMenu
                        label={aBTestingSettingsModel.confidenceLevelLabel}
                        value={aBTestingSettingsModel.confidenceLevel?.toString()}
                        options={aBTestingSettingsModel.confidenceLevels}
                        onValueChange={value => changeConfidenceLevel(value)}
                    />
                </CardContentArea>
                <CardContentArea>
                    <Checkbox
                        checked={aBTestingSettingsModel.isEnabled}
                        onChange={handleChange}
                        name="isEnabled"
                    >
                        {aBTestingSettingsModel.isEnabledLabel}
                    </Checkbox>
                </CardContentArea>
            </Card>
            <TextButton onClick={cancel} style={{ marginRight: "10px" }}>{aBTestingSettingsModel.cancelButton}</TextButton>
            <TextButton
                contained
                onClick={save}
                disabled={!isValidForm()}
            >
                {aBTestingSettingsModel.saveButton}
            </TextButton>
        </div>     
        
    );
};

export default ABTestingSettings;