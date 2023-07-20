import React from "react";
import ReactDOM from "react-dom";
import App from "./App";
import axios from "axios";

const root = document.getElementById("root") as HTMLElement;
const antiforgeryHeaderName: string = root.dataset.epiAntiforgeryHeaderName as string;
const antiforgeryFormFieldName: string = root.dataset.epiAntiforgeryFormFieldName as string;
const xsrfToken = document.getElementsByName(
    antiforgeryFormFieldName
)[0] as HTMLInputElement;
const xsrfHeader = {
    ...axios.defaults.headers,
    [antiforgeryHeaderName]: xsrfToken.value,
};
axios.defaults.headers = xsrfHeader;

ReactDOM.render(<App />, document.getElementById("root"));
