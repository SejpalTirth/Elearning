import { ActivatedRoute, convertToParamMap } from '@angular/router';
import { of } from 'rxjs';
import { API_BASE_URL } from '@frontend/core';

export const provideCommonMocks = [
  {
    provide: ActivatedRoute,
    useValue: {
      snapshot: {
        paramMap: convertToParamMap({}),
        queryParamMap: convertToParamMap({})
      },
      params: of({}),
      queryParams: of({})
    }
  },
  {
    provide: API_BASE_URL,
    useValue: 'http://localhost:3000'
  }
];
