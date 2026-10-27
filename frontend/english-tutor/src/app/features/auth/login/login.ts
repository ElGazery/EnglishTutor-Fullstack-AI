import { Component, ChangeDetectorRef, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.html',
  styleUrl: './login.css'
})
export class LoginComponent {
  email = '';
  password = '';
  isLoading = signal(false);
  errorMessage = signal('');

  private authService = inject(AuthService);
  private router = inject(Router);
  private cdr = inject(ChangeDetectorRef);

  onSubmit(): void {
    if (!this.email || !this.password) {
      this.errorMessage.set('Please provide both email and password.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.authService.login({ email: this.email, password: this.password }).subscribe({
      next: (res) => {
        this.isLoading.set(false);
        if (res.isSuccess) {
          this.router.navigate(['/chat']);
        } else {
          this.errorMessage.set(res.message || 'Login failed. Please check your credentials.');
        }
        this.cdr.markForCheck();
      },
      error: (err) => {
        this.isLoading.set(false);
        if (err.error?.errors && err.error.errors.length > 0) {
          this.errorMessage.set(err.error.errors.join(' '));
        } else {
          this.errorMessage.set(err.error?.message || 'Invalid email or password.');
        }
        this.cdr.markForCheck();
      }
    });
  }
}
