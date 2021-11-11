import React from "react";
import { ContentArea, Workspace } from "@episerver/ui-framework";
import "@episerver/ui-framework/dist/main.css";
import "./App.scss";
import KpiCommerceSettings from "./KpiCommerceSettings";


function App() {
    return (
        <ContentArea>            
            <Workspace>                
                <KpiCommerceSettings/>
            </Workspace>
        </ContentArea>
    );
}

export default App;
