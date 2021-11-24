import React, { useEffect } from "react";
import { ContentArea, Workspace, Typography} from "@episerver/ui-framework";
import "@episerver/ui-framework/dist/main.css";
import "./App.scss";
import ABTestingSettings from "./ABTestingSettings";

function App() {
    return (
        <ContentArea>            
            <Workspace>                
                <ABTestingSettings/>
            </Workspace>
        </ContentArea>
    );
}

export default App;
