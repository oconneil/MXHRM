import { CommonModule } from '@angular/common';
import { Component, signal } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { EmployeeService } from '../../services/employee';
import { CreateEmployeeRequest } from '../../models/employee';

@Component({
  selector: 'app-employee-create',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './employee-create.html',
  styleUrl: './employee-create.scss'
})
export class EmployeeCreate {
  saving = signal(false);
  errorMessage = signal('');
  form: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly employeeService: EmployeeService,
    private readonly router: Router
  ) {
    this.form = this.fb.nonNullable.group({
      companyID: ['JCORP', [Validators.required, Validators.maxLength(20)]],
      employeeID: ['', [Validators.required, Validators.maxLength(20)]],
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
      hireDate: ['', [Validators.required]],
      salary: [0, [Validators.required, Validators.min(0)]],
      createdBy: ['admin', [Validators.required, Validators.maxLength(100)]]
    });
  }

  save(): void {
    this.errorMessage.set('');

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);

    const request: CreateEmployeeRequest = this.form.getRawValue();

    this.employeeService.createEmployee(request).subscribe({
      next: () => {
        this.saving.set(false);
        this.router.navigate(['/employees']);
      },
      error: (err) => {
        this.saving.set(false);
        this.errorMessage.set(
          err?.error?.message ?? 'ไม่สามารถบันทึกข้อมูลพนักงานได้'
        );
      }
    });
  }

  cancel(): void {
    this.router.navigate(['/employees']);
  }

  hasError(controlName: keyof typeof this.form.controls): boolean {
    const control = this.form.controls[controlName];
    return control.invalid && (control.touched || control.dirty);
  }
}