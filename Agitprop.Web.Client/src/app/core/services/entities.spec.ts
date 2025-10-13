import { TestBed } from '@angular/core/testing';

import { Entities } from './entities';

describe('Entities', () => {
  let service: Entities;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Entities);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
