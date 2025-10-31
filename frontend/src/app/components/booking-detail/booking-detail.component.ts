import {
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
  computed,
  inject,
  output,
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  BookingRead,
  BookingUpdateDto,
} from '../ResourceMenu/models/booking.model';
import { ButtonComponent } from '../../components/button/button.component';
import { BookingService } from '../ResourceMenu/Services/booking.service';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-booking-detail',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, ButtonComponent],
  templateUrl: './booking-detail.component.html',
})
export class BookingDetailComponent implements OnChanges, OnInit {
  @Input() bookingId: number | null = null;
  saved = output<void>();
  deleted = output<void>();

  private api = inject(BookingService);
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);

  loading = false;
  error = '';
  booking?: BookingRead;

  form = this.fb.group({
    // Temporarily, startTime and endTime are hard-coded, and
    // endDate is set to the same value as startDate in ngOnInit
    startDate: ['', Validators.required], // yyyy-MM-dd
    startTime: ['00:00', Validators.required], // HH:mm
    endDate: ['', Validators.required],
    endTime: ['23:59', Validators.required],
  });

  title = computed(() =>
    this.booking
      ? this.booking.resourceName
        ? this.booking.resourceName
        : `Bokning ${this.booking.id}`
      : 'Bokning'
  );

  currentDate: string = new Date().toISOString().split('T')[0];

  ngOnInit(): void {
    // Watch for changes on startDate
    this.form.get('startDate')?.valueChanges.subscribe((startDateValue) => {
      const endDateControl = this.form.get('endDate');

      if (startDateValue) {
        // Enable endDate and set it equal to startDate
        endDateControl?.enable({ emitEvent: false });
        endDateControl?.setValue(startDateValue, { emitEvent: false });
      } else {
        // Disable and clear if startDate is removed
        endDateControl?.disable({ emitEvent: false });
        endDateControl?.reset();
      }
    });
  }

  ngOnChanges(changes: SimpleChanges): void {
    if ('bookingId' in changes) this.load();
  }

  load() {
    this.error = '';

    if (!this.authService.isAdmin()) return console.error('Unauthorized');

    if (!this.bookingId) {
      this.booking = undefined;
      return;
    }
    this.loading = true;
    this.api.getById(this.bookingId).subscribe({
      next: (b) => {
        this.booking = b;

        const s = new Date(b.startTime);
        const e = new Date(b.endTime);

        // Format in Swedish local time
        const formatLocal = (date: Date) => {
          const year = date.getFullYear();
          const month = String(date.getMonth() + 1).padStart(2, '0');
          const day = String(date.getDate()).padStart(2, '0');
          const hours = String(date.getHours()).padStart(2, '0');
          const minutes = String(date.getMinutes()).padStart(2, '0');

          return {
            date: `${year}-${month}-${day}`,
            time: `${hours}:${minutes}`,
          };
        };

        const start = formatLocal(s);
        const end = formatLocal(e);

        this.form.setValue({
          // startDate: s.toISOString().slice(0, 10),
          // startTime: s.toISOString().slice(11, 16),
          // endDate: e.toISOString().slice(0, 10),
          // endTime: e.toISOString().slice(11, 16),
          startDate: start.date,
          startTime: start.time,
          endDate: end.date,
          endTime: end.time,
        });
      },
      error: (err) => {
        this.error = 'Kunde inte hämta bokningen';
        console.error(err);
      },
      complete: () => {
        this.loading = false;
      },
    });
  }

  private toIso(date: string, time: string) {
    const [y, m, d] = date.split('-').map(Number);
    const [hh, mm] = time.split(':').map(Number);
    return new Date(Date.UTC(y, m - 1, d, hh, mm, 0)).toISOString();
  }

  save() {
    if (!this.authService.isAdmin()) return console.error('Unauthorized');

    if (!this.booking) return;
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const startIso = this.toIso(
      this.form.value.startDate!,
      this.form.value.startTime!
    );
    const endIso = this.toIso(
      this.form.value.endDate!,
      this.form.value.endTime!
    );

    const dto: BookingUpdateDto = {
      userId: this.booking.userId,
      resourceId: this.booking.resourceId,
      startTime: startIso,
      endTime: endIso,
      status: this.booking.status || 'Confirmed',
    };

    this.loading = true;
    this.error = '';
    this.api.update(this.booking.id, dto).subscribe({
      next: () => this.saved.emit(),
      error: (err) => {
        const msg =
          (typeof err?.error === 'string' && err.error) ||
          err?.error?.message ||
          'Kunde inte spara';
        this.error = msg;
        console.error(err);
      },
      complete: () => {
        this.loading = false;
      },
    });
  }

  remove() {
    if (!this.authService.isAdmin()) return console.error('Unauthorized');

    if (!this.booking) return;
    if (!confirm('Radera bokningen?')) return;
    this.loading = true;
    this.error = '';

    this.api.delete(this.booking.id, this.booking.userId as any).subscribe({
      next: () => this.deleted.emit(),
      error: (err) => {
        this.error = 'Kunde inte radera';
        console.error(err);
      },
      complete: () => {
        this.loading = false;
      },
    });
  }
}
