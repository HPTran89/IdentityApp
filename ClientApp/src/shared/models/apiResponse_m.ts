export class ApiResponseModel<T> {
    constructor(statuscode: number,
    title: string,
    message: string,
    details: string,
    isHtmlEnabled: boolean,
    displayByDefault: boolean,
    showWithToaster: boolean,
    data: T,
    errors: string[]) {
        this.statuscode = statuscode;
        this.title = title;
        this.message = message;
        this.details = details;
        this.isHtmlEnabled = isHtmlEnabled = false;
        this.displayByDefault = displayByDefault = false;
        this.showWithToaster = showWithToaster = false;
        this.data = data;
        this.errors = errors;
    }

    statuscode: number;
    title: string;
    message: string;
    details: string;
    isHtmlEnabled: boolean;
    displayByDefault: boolean;
    showWithToaster: boolean;
    data: T;
    errors: string[];
}