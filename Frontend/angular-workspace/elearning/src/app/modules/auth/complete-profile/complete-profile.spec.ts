import { ComponentFixture, TestBed } from '@angular/core/testing';
import { CompleteProfileComponent } from './complete-profile';
import { ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';
import { ToastService } from '../../../common-modules/ui/toast/toast-service';
import { GatewayUsersService } from 'api';
import { FormsModule } from '@angular/forms';

describe('CompleteProfileComponent', () => {
  let component: CompleteProfileComponent;
  let fixture: ComponentFixture<CompleteProfileComponent>;
  let mockRouter: any;
  let mockActivatedRoute: any;
  let mockToast: any;
  let mockUserApi: any;

  beforeEach(async () => {
    mockRouter = { navigate: jasmine.createSpy('navigate') };
    mockToast = { 
      showError: jasmine.createSpy('showError'), 
      showInfo: jasmine.createSpy('showInfo') 
    };
    mockUserApi = { 
      postApiUsersCompleteProfile: jasmine.createSpy('postApiUsersCompleteProfile').and.returnValue(of({})) 
    };

    mockActivatedRoute = {
      snapshot: {
        queryParamMap: {
          get: (key: string) => key === 'userId' ? 'user_999' : null
        }
      }
    };

    await TestBed.configureTestingModule({
      imports: [CompleteProfileComponent, FormsModule],
      providers: [
        { provide: Router, useValue: mockRouter },
        { provide: ActivatedRoute, useValue: mockActivatedRoute },
        { provide: ToastService, useValue: mockToast },
        { provide: GatewayUsersService, useValue: mockUserApi }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CompleteProfileComponent);
    component = fixture.componentInstance;
  });

  it('should initialize with userId from query params', () => {
    fixture.detectChanges();
    expect(component.userId).toBe('user_999');
  });

  describe('saveProfile()', () => {
    beforeEach(() => {
      fixture.detectChanges();
      component.name = 'John Doe';
      component.roleId = 3;
    });

    it('should show error if name is empty', () => {
      component.name = '   ';
      component.saveProfile();
      expect(mockToast.showError).toHaveBeenCalledWith('Please enter your name');
      expect(mockUserApi.postApiUsersCompleteProfile).not.toHaveBeenCalled();
    });

    it('should show error if roleId is invalid', () => {
      component.roleId = 99;
      component.saveProfile();
      expect(mockToast.showError).toHaveBeenCalledWith('Invalid role selected');
    });

    it('should call API with correct payload and navigate on success', () => {
      component.saveProfile();

      expect(component.loading).toBeTrue();
      expect(mockUserApi.postApiUsersCompleteProfile).toHaveBeenCalledWith({
        userId: 'user_999',
        name: 'John Doe',
        role: 'Student'
      });
      expect(mockToast.showInfo).toHaveBeenCalled();
      expect(mockRouter.navigate).toHaveBeenCalledWith(['/']);
    });

    it('should handle API error', () => {
      mockUserApi.postApiUsersCompleteProfile.and.returnValue(throwError(() => new Error('API Error')));
      
      component.saveProfile();

      expect(component.loading).toBeFalse();
      expect(mockToast.showError).toHaveBeenCalledWith('Something went wrong.');
    });
  });
});