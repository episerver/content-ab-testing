import React from "react";
import { ContentArea, Workspace } from "@episerver/ui-framework";
import "@episerver/ui-framework/dist/main.css";
import "./App.scss";
import KpiCommerceSettingsView from "./KpiCommerceSettingsView";


function App() {
    return (
        <ContentArea>            
            <Workspace>                
                <KpiCommerceSettingsView/>
            </Workspace>
        </ContentArea>
    );
}

export default App;
