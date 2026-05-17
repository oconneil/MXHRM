import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmployeeResponse, PagedResponse } from '../../models/employee';
import { EmployeeService } from '../../services/employee';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth';
import { Permissions } from '../../../../core/models/permissions';
import { GridDataResult, KENDO_GRID, PageChangeEvent } from '@progress/kendo-angular-grid';

@Component({
  selector: 'app-employee-list',
  imports: [CommonModule, FormsModule, RouterLink, KENDO_GRID],
  templateUrl: './employee-list.html',
  styleUrl: './employee-list.scss'
})
export class EmployeeList implements OnInit {
  readonly employeeCreatePermission = Permissions.Employee.Create;
  readonly employeeUpdatePermission = Permissions.Employee.Update;
  readonly employeeDeletePermission = Permissions.Employee.Delete;

  employees = signal<EmployeeResponse[]>([]);
  gridData = computed<GridDataResult>(() => ({
    data: this.employees(),
    total: this.totalItems()
  }));
  loading = signal(false);
  errorMessage = signal('');

  page = signal(1);
  pageSize = signal(10);
  totalItems = signal(0);
  totalPages = signal(0);
  search = signal('');
  companyID = signal('');
  isActive = signal<boolean | null>(null);
  sortBy = signal('employeeId');
  sortDirection = signal<'asc' | 'desc'>('asc');

  readonly authService = inject(AuthService);

  constructor(private readonly employeeService: EmployeeService) { }

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    this.employeeService
      .getEmployees({
        search: this.search(),
        companyID: this.companyID(),
        isActive: this.isActive(),
        sortBy: this.sortBy(),
        sortDirection: this.sortDirection(),
        page: this.page(),
        pageSize: this.pageSize()
      })
      .subscribe({
        next: (res: PagedResponse<EmployeeResponse>) => {
          this.employees.set(res.items);
          this.totalItems.set(res.totalItems);
          this.totalPages.set(res.totalPages);
          this.loading.set(false);
        },
        error: () => {
          this.errorMessage.set('ไม่สามารถโหลดข้อมูลพนักงานได้');
          this.loading.set(false);
        }
      });
  }

  onSearch(): void {
    this.page.set(1);
    this.loadEmployees();
  }

  onIsActiveChange(value: string): void {
    if (value === '') {
      this.isActive.set(null);
      return;
    }

    this.isActive.set(value === 'true');
  }

  onSortDirectionChange(value: string): void {
    this.sortDirection.set(value === 'desc' ? 'desc' : 'asc');
  }

  applyFilters(): void {
    this.page.set(1);
    this.loadEmployees();
  }

  resetFilters(): void {
    this.search.set('');
    this.companyID.set('');
    this.isActive.set(null);
    this.sortBy.set('employeeId');
    this.sortDirection.set('asc');
    this.page.set(1);
    this.loadEmployees();
  }

  deleteEmployee(companyID: string, employeeID: string): void {
    const confirmed = confirm(`ต้องการลบพนักงาน ${employeeID} ใช่ไหม?`);

    if (!confirmed) {
      return;
    }

    this.loading.set(true);
    this.errorMessage.set('');

    this.employeeService.deleteEmployee(companyID, employeeID).subscribe({
      next: () => {
        this.loadEmployees();
      },
      error: (err) => {
        this.loading.set(false);
        this.errorMessage.set(
          err?.error?.message ?? 'ไม่สามารถลบข้อมูลพนักงานได้'
        );
      }
    });
  }

  onGridPageChange(event: PageChangeEvent): void {
    this.page.set(event.skip / event.take + 1);
    this.pageSize.set(event.take);
    this.loadEmployees();
  }
}
