import { Component, EventEmitter, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatNativeDateModule } from '@angular/material/core';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-date-range-picker',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDatepickerModule,
    MatFormFieldModule,
    MatInputModule,
    MatNativeDateModule,
    MatButtonModule
  ],
  templateUrl: './date-range-picker.html',
  styleUrls: ['./date-range-picker.scss']
})
export class DateRangePicker {
  fromDate: Date = new Date();
  toDate: Date = new Date();

  @Output() rangeChange = new EventEmitter<{ from: Date, to: Date }>();

  apply(): void {
    this.rangeChange.emit({ from: this.fromDate, to: this.toDate });
  }
}
