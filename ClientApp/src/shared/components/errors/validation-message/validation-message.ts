import { Component, input } from '@angular/core';

@Component({
  selector: 'app-validation-message',
  imports: [],
  templateUrl: './validation-message.html',
  styleUrl: './validation-message.css',
})
export class ValidationMessage {
  errorMessages = input<string[] | undefined>();
}
