import { Component, inject, OnInit } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink, NavigationEnd } from '@angular/router';
import { AccountService } from '../services/account.service';
import { ToastrService } from 'ngx-toastr';
import { filter } from 'rxjs';

@Component({
    selector: 'app-auth',
    standalone: true,
    imports: [ReactiveFormsModule, RouterLink],
    templateUrl: './auth.html',
})
export class Auth implements OnInit {
    private fb = inject(FormBuilder);
    private accountService = inject(AccountService);
    private router = inject(Router);
    private toastr = inject(ToastrService);

    isRegisterMode = false;

    loginForm = this.fb.group({
        username: ['', Validators.required],
        password: ['', Validators.required],
    });

    registerForm = this.fb.group({
        username: ['', Validators.required],
        email: ['', [Validators.required, Validators.email]],
        password: ['', [Validators.required, Validators.minLength(6)]],
    });

    ngOnInit() {
        this.setMode();

        this.router.events.pipe(
            filter(event => event instanceof NavigationEnd)
        ).subscribe(() => {
            this.setMode();
        });
    }

    private setMode() {
        this.isRegisterMode = this.router.url.includes('register');
    }

    onSubmitLogin() {
        if (this.loginForm.valid) {
            this.accountService.login(this.loginForm.value).subscribe({
                next: () => {
                    this.router.navigateByUrl('/');
                },
                error: (error) => {
                    this.toastr.error(error.error);
                },
            });
        }
    }

    onSubmitRegister() {
        if (this.registerForm.valid) {
            this.accountService.register(this.registerForm.value).subscribe({
                next: () => {
                    this.toastr.success('Registration successful');
                    this.router.navigateByUrl('/account/login');
                },
                error: (error) => {
                    this.toastr.error(error.error);
                },
            });
        }
    }
}
