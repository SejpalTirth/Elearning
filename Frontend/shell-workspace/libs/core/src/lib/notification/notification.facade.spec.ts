import { TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { HttpResponse } from '@angular/common/http';

import { NotificationFacade } from './notification.facade';
import { NotificationGatewayService } from '@frontend/api';

describe('NotificationFacade', () => {
  let facade: NotificationFacade;
  let gatewaySpy: jasmine.SpyObj<NotificationGatewayService>;

  beforeEach(() => {
    gatewaySpy = jasmine.createSpyObj('NotificationGatewayService', [
      'postApiNotificationSend',
      'postApiNotificationTemplateSend',
      'postApiNotificationTrigger',
      'postApiNotificationUser'
    ]);

    TestBed.configureTestingModule({
      providers: [
        NotificationFacade,
        { provide: NotificationGatewayService, useValue: gatewaySpy }
      ]
    });

    facade = TestBed.inject(NotificationFacade);
  });

  it('should be created', () => {
    expect(facade).toBeTruthy();
  });

  // --------------------------------------------------
  // EMAIL / DIRECT NOTIFICATION
  // --------------------------------------------------

  it('should send email', () => {
    const payload = { to: 'test@test.com' } as any;

    gatewaySpy.postApiNotificationSend.and.returnValue(
      of(new HttpResponse({ body: null }))
    );

    facade.sendEmail(payload).subscribe();

    expect(gatewaySpy.postApiNotificationSend)
      .toHaveBeenCalledWith(
  payload,
  jasmine.objectContaining({ withCredentials: true })
);

  });

  it('should send template notification', () => {
    const payload = { templateId: 1 } as any;

    gatewaySpy.postApiNotificationTemplateSend.and.returnValue(
      of(new HttpResponse({ body: null }))
    );

    facade.sendTemplate(payload).subscribe();

    expect(gatewaySpy.postApiNotificationTemplateSend)
      .toHaveBeenCalledWith(
  payload,
  jasmine.objectContaining({ withCredentials: true })
);

  });

  // --------------------------------------------------
  // EVENT / TRIGGER BASED NOTIFICATION
  // --------------------------------------------------

  it('should trigger notification event', () => {
    const payload = { event: 'COURSE_PUBLISHED' } as any;

    gatewaySpy.postApiNotificationTrigger.and.returnValue(
      of(new HttpResponse({ body: null }))
    );

    facade.trigger(payload).subscribe();

    expect(gatewaySpy.postApiNotificationTrigger)
      .toHaveBeenCalledWith(
  payload,
  jasmine.objectContaining({ withCredentials: true })
);

  });

  it('should notify user', () => {
    const payload = { userId: '123' } as any;

    gatewaySpy.postApiNotificationUser.and.returnValue(
      of(new HttpResponse({ body: null }))
    );

    facade.notifyUser(payload).subscribe();

    expect(gatewaySpy.postApiNotificationUser)
      .toHaveBeenCalledWith(
  payload,
  jasmine.objectContaining({ withCredentials: true })
);

  });
});
