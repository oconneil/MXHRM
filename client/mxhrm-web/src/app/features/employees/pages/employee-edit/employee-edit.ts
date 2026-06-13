import { CommonModule } from '@angular/common';
import { Component, OnInit, OnDestroy, signal } from '@angular/core';
import { DomSanitizer, SafeUrl } from '@angular/platform-browser';
import { FormBuilder, ReactiveFormsModule, Validators, FormGroup } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { EmployeeService } from '../../services/employee';
import { UpdateEmployeeRequest, EmployeeDocument } from '../../models/employee';
import { ErrorCodes } from '../../../../core/models/error-codes';
import { ErrorService } from '../../../../core/services/error';

@Component({
  selector: 'app-employee-edit',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './employee-edit.html',
  styleUrl: './employee-edit.scss'
})
export class EmployeeEdit implements OnInit, OnDestroy {
  loading = signal(false);
  saving = signal(false);
  errorMessage = signal('');

  companyID = '';
  employeeID = '';
  rowVersion = '';

  // Photo handling
  photoUrl = signal<SafeUrl | null>(null);
  uploadingPhoto = signal(false);
  photoMessage = signal('');
  private photoObjectUrl: string | null = null;

  // Document handling
  documents = signal<EmployeeDocument[]>([]);
  uploadingDoc = signal(false);
  docType = signal('Contract');
  docMessage = signal('');

  form: FormGroup;

  constructor(
    private readonly fb: FormBuilder,
    private readonly employeeService: EmployeeService,
    private readonly route: ActivatedRoute,
    private readonly router: Router,
    protected readonly errorService: ErrorService,
    private readonly sanitizer: DomSanitizer
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

  ngOnDestroy(): void {
    this.revokePhoto();
  }

  loadEmployee(): void {
    this.loading.set(true);
    this.errorMessage.set('');
    this.errorService.clear();

    this.employeeService.getEmployeeById(this.companyID, this.employeeID).subscribe({
      next: (employee) => {
        this.rowVersion = employee.rowVersion;
        this.loadPhoto();
        this.loadDocuments();

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

  loadPhoto(): void {
    this.employeeService.getPhotoBlob(this.companyID, this.employeeID).subscribe({
      next: (blob) => this.setPhotoFromBlob(blob),
      error: () => this.clearPhoto()   // 404 = ยังไม่มีรูป
    });
  }

  onPhotoSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    this.photoMessage.set('');

    this.uploadingPhoto.set(true);
    this.employeeService.uploadPhoto(this.companyID, this.employeeID, file).subscribe({
      next: () => {
        this.uploadingPhoto.set(false);
        input.value = '';
        this.loadPhoto();   // โหลดรูปใหม่มาแสดง
      },
      error: (err) => {
        this.uploadingPhoto.set(false);
        input.value = '';
        this.photoMessage.set(err?.message ?? 'อัปโหลดรูปไม่สำเร็จ');
      }
    });
  }

  removePhoto(): void {
    this.uploadingPhoto.set(true);
    this.employeeService.deletePhoto(this.companyID, this.employeeID).subscribe({
      next: () => {
        this.uploadingPhoto.set(false);
        this.clearPhoto();
      },
      error: () => this.uploadingPhoto.set(false)
    });
  }

  private setPhotoFromBlob(blob: Blob): void {
    this.revokePhoto();
    this.photoObjectUrl = URL.createObjectURL(blob);
    this.photoUrl.set(this.sanitizer.bypassSecurityTrustUrl(this.photoObjectUrl));
  }

  private clearPhoto(): void {
    this.revokePhoto();
    this.photoUrl.set(null);
  }

  private revokePhoto(): void {
    if (this.photoObjectUrl) {
      URL.revokeObjectURL(this.photoObjectUrl);   // คืน memory กัน leak
      this.photoObjectUrl = null;
    }
  }

  loadDocuments(): void {
    this.employeeService.listDocuments(this.companyID, this.employeeID).subscribe({
      next: (docs) => this.documents.set(docs),
      error: () => this.documents.set([])
    });
  }

  onDocumentSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) {
      return;
    }

    this.docMessage.set('');

    if (file.size > 10 * 1024 * 1024) {
      this.docMessage.set('ไฟล์ต้องไม่เกิน 10 MB');
      input.value = '';
      return;
    }

    this.uploadingDoc.set(true);
    this.employeeService
      .uploadDocument(this.companyID, this.employeeID, file, this.docType())
      .subscribe({
        next: () => {
          this.uploadingDoc.set(false);
          input.value = '';
          this.loadDocuments();
        },
        error: (err) => {
          this.uploadingDoc.set(false);
          input.value = '';
          this.docMessage.set(err?.message ?? 'อัปโหลดเอกสารไม่สำเร็จ');
        }
      });
  }

  downloadDocument(doc: EmployeeDocument): void {
    this.employeeService
      .downloadDocument(this.companyID, this.employeeID, doc.id)
      .subscribe({
        next: (blob) => this.saveBlob(blob, doc.fileName),
        error: () => this.docMessage.set('ดาวน์โหลดไม่สำเร็จ')
      });
  }

  deleteDocument(doc: EmployeeDocument): void {
    this.employeeService
      .deleteDocument(this.companyID, this.employeeID, doc.id)
      .subscribe({
        next: () => this.loadDocuments(),
        error: () => this.docMessage.set('ลบไม่สำเร็จ')
      });
  }

  private saveBlob(blob: Blob, fileName: string): void {
    // ดึง blob (ผ่าน HttpClient = มี token) แล้วสร้าง <a download> กดเอง
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(url);   // คืน memory ทันที
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
