import { Component, ChangeDetectorRef, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.html',
  styleUrl: './register.css'
})
export class RegisterComponent {
  fullName = '';
  email = '';
  password = '';
  gradeLevel = 'First Prep / الصف الأول الإعدادي';
  isLoading = signal(false);
  errorMessage = signal('');

  gradeOptions = [
    'First Prep / الصف الأول الإعدادي',
    'Second Prep / الصف الثاني الإعدادي',
    'Third Prep / الصف الثالث الإعدادي'
  ];

  private authService = inject(AuthService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  onSubmit(): void {
    if (!this.fullName || !this.email || !this.password) {
      this.errorMessage.set('Please complete all required fields.');
      return;
    }

    if (this.password.length < 8) {
      this.errorMessage.set('Password must be at least 8 characters long.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.authService.register({
      fullName: this.fullName,
      email: this.email,
      password: this.password,
      gradeLevel: this.gradeLevel
    }).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess) {
          this.router.navigate(['/chat']);
        } else {
          this.errorMessage.set(res.message || 'Registration failed.');
        }
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err.error?.errors && err.error.errors.length > 0) {
          this.errorMessage.set(err.error.errors.join(' '));
        } else {
          this.errorMessage.set(err.error?.message || 'Registration failed. Please check your information.');
        }
        this.cdr.markForCheck();
      }
    });
  }
}
