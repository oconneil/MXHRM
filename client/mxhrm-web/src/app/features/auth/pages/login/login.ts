import { CommonModule } from '@angular/common';
import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../../core/services/auth';
import { ErrorService } from '../../../../core/services/error';

@Component({
  selector: 'app-login',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './login.html',
  styleUrl: './login.scss'
})
export class Login {
  loading = signal(false);
  errorMessage = signal('');
  form: FormGroup;
  protected readonly errorService = inject(ErrorService);

  constructor(
    private readonly fb: FormBuilder,
    private readonly authService: AuthService,
    private readonly router: Router,
    private readonly route: ActivatedRoute
  ) {
    this.form = this.fb.nonNullable.group({
      companyID: ['', [Validators.required]],
      userName: ['', [Validators.required]],
      password: ['', [Validators.required]]
    });
  }

  login(): void {
    this.errorService.clear();
    this.errorMessage.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.loading.set(true);

    this.authService.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.loading.set(false);
        const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
        this.router.navigateByUrl(returnUrl || '/employees');
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('Company ID, Username หรือ Password ไม่ถูกต้อง');
      }
    });
  }

  protected fieldErrors(fieldName: string): string[] {
    return this.errorService.getFieldErrors(
      this.errorService.lastError(),
      fieldName
    );
  }

}
