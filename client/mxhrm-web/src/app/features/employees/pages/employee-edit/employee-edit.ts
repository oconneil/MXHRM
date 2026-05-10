import { CommonModule } from '@angular/common';
import { Component, OnInit, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeeService } from '../../services/employee';
import { UpdateEmployeeRequest } from '../../models/employee';
import { ErrorCodes } from '../../../../core/models/error-codes';
import { ErrorService } from '../../../../core/services/error';

@Component({
  selector: 'app-employee-edit',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './employee-edit.html',
  styleUrl: './employee-edit.scss'
})
export class EmployeeEdit implements OnInit {
  loading = signal(false);
  saving = signal(false);
  errorMessage = signal('');

  companyID = '';
  employeeID = '';
  rowVersion = '';

  form: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly employeeService: EmployeeService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    protected readonly errorService: ErrorService
  ) {
    this.form = this.fb.nonNullable.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email, Validators.maxLength(150)]],
      hireDate: ['', [Validators.required]],
      salary: [0, [Validators.required, Validators.min(0)]],
      isActive: [true],
      modifiedBy: ['admin', [Validators.required, Validators.maxLength(100)]]
    });
  }

  ngOnInit(): void {
    this.companyID = this.route.snapshot.paramMap.get('companyID') ?? '';
    this.employeeID = this.route.snapshot.paramMap.get('employeeID') ?? '';

    if (!this.companyID || !this.employeeID) {
      this.errorMessage.set('Invalid employee route.');
      return;
    }

    this.loadEmployee();
  }

  loadEmployee(): void {
    this.loading.set(true);
    this.errorMessage.set('');
    this.errorService.clear();

    this.employeeService.getEmployeeById(this.companyID, this.employeeID).subscribe({
      next: (employee) => {
        this.rowVersion = employee.rowVersion;

        this.form.patchValue({
          firstName: employee.firstName,
          lastName: employee.lastName,
          email: employee.email,
          hireDate: employee.hireDate.substring(0, 10),
          salary: employee.salary,
          isActive: employee.isActive,
          modifiedBy: 'admin'
        });

        this.loading.set(false);
      },
      error: () => {
        this.loading.set(false);
        this.errorMessage.set('ไม่พบข้อมูลพนักงาน');
      }
    });
  }

  save(): void {
    this.errorMessage.set('');
    this.errorService.clear();

    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.saving.set(true);

    const request: UpdateEmployeeRequest = {
      ...this.form.getRawValue(),
      rowVersion: this.rowVersion
    };

    this.employeeService
      .updateEmployee(this.companyID, this.employeeID, request)
      .subscribe({
        next: () => {
          this.saving.set(false);
          this.router.navigate(['/employees']);
        },
        error: (err) => {
          this.saving.set(false);

          if (err.code === ErrorCodes.Conflict) {
            this.errorMessage.set(
              'ข้อมูลนี้ถูกแก้ไขโดยผู้ใช้อื่นแล้ว กรุณากลับไปโหลดข้อมูลใหม่อีกครั้ง'
            );
            return;
          }

          this.errorMessage.set(
            err?.message ?? 'ไม่สามารถแก้ไขข้อมูลพนักงานได้'
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

  fieldErrors(fieldName: string): string[] {
    return this.errorService.getFieldErrors(
      this.errorService.lastError(),
      fieldName
    );
  }
}
