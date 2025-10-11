import { ComponentFixture, TestBed } from '@angular/core/testing';

import { Trending } from './trending';

describe('Trending', () => {
  let component: Trending;
  let fixture: ComponentFixture<Trending>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [Trending]
    })
    .compileComponents();

    fixture = TestBed.createComponent(Trending);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
