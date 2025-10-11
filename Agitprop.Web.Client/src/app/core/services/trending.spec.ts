import { TestBed } from '@angular/core/testing';

import { Trending } from './trending';

describe('Trending', () => {
  let service: Trending;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(Trending);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
