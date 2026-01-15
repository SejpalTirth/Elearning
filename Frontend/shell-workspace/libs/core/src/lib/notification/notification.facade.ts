import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import {
  NotificationGatewayService,
  GatewayContractsNotificationEmailRequest,
  GatewayContractsNotificationTemplateRequest,
  GatewayContractsNotificationTriggerNotification,
  GatewayContractsNotificationUserIdRequest
} from '@frontend/api';

@Injectable({ providedIn: 'root' })
export class NotificationFacade {

  private readonly api = inject(NotificationGatewayService);

  // --------------------------------------------------
  // EMAIL / DIRECT NOTIFICATION
  // --------------------------------------------------

  sendEmail(
    payload: GatewayContractsNotificationEmailRequest
  ): Observable<void> {
    return this.api.postApiNotificationSend(payload, {withCredentials:true});
  }

  sendTemplate(
    payload: GatewayContractsNotificationTemplateRequest
  ): Observable<void> {
    return this.api.postApiNotificationTemplateSend(payload, {withCredentials:true});
  }

  // --------------------------------------------------
  // EVENT / TRIGGER BASED NOTIFICATION
  // --------------------------------------------------

  trigger(
    payload: GatewayContractsNotificationTriggerNotification
  ): Observable<void> {
    return this.api.postApiNotificationTrigger(payload, {withCredentials:true});
  }

  notifyUser(
    payload: GatewayContractsNotificationUserIdRequest
  ): Observable<void> {
    return this.api.postApiNotificationUser(payload, {withCredentials:true});
  }
}
