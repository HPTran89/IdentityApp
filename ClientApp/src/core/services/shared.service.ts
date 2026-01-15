import { inject, Injectable } from '@angular/core';
import { NgbModal, NgbModalOptions } from '@ng-bootstrap/ng-bootstrap';
import { ToastrService } from 'ngx-toastr';
import { ApiResponseModel } from '../../shared/models/apiResponse_m';
import { Notification } from '../../shared/components/modals/notification/notification';

@Injectable({
  providedIn: 'root',
})
export class SharedService {
  protected toastr = inject(ToastrService);
  private modalService = inject(NgbModal);

  showNotification(apiResponse: ApiResponseModel<any>, backdrop: boolean = false) {
    let isSuccess = false;

    if (apiResponse.showWithToaster) {
      if (isSuccess) {
        this.toastr.success(apiResponse.message, apiResponse.title);
      } else {
        this.toastr.error(apiResponse.message, apiResponse.title);
      }
    } else {
      const options: NgbModalOptions = {
        backdrop
      };
      const modalRef = this.modalService.open(Notification, options)
      modalRef.componentInstance.isSuccess = isSuccess;
      modalRef.componentInstance.title = apiResponse.title;
      modalRef.componentInstance.message = apiResponse.message;
      modalRef.componentInstance.isHtmlEnabled = apiResponse.isHtmlEnabled;
    }
  }
}
