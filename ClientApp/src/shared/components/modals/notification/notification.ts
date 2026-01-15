import { CommonModule, NgClass } from '@angular/common';
import { Component, inject, input } from '@angular/core';
import { NgbActiveModal } from '@ng-bootstrap/ng-bootstrap'

@Component({
  selector: 'app-notification',
  imports: [NgClass, CommonModule],
  templateUrl: './notification.html',
  styleUrl: './notification.css',
})
export class Notification {
  isSuccess = input<boolean>(true);
  title = input<string>('');
  message = input<string>('');
  isHtmlEnabled = input<boolean>(false);
  activeModal = inject(NgbActiveModal);

  constructor() {
    
  }
}
