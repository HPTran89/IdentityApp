import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../environments/environment.development';

@Injectable({
  providedIn: 'root',
})
export class AccountService {
  constructor(private http: HttpClient) { }

  login(model: any) {
    debugger;
    return this.http.post(`${environment.apiUrl}account/login`, model);
  }

  register(model: any) {
    return this.http.post(`${environment.apiUrl}account/register`, model);
  }
}
