import { Route } from "@angular/router"
import { Auth } from "./auth/auth"

export const accountRoutes: Route[] = [
    { path: "login", component: Auth },
    { path: "register", component: Auth },
]