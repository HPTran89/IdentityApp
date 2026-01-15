import { Login } from "./login/login"
import { Route } from "@angular/router"
import { Register } from "./register/register"

export const accountRoutes: Route[] = [
    {path: "login", component: Login},
    {path: "register", component: Register},
]