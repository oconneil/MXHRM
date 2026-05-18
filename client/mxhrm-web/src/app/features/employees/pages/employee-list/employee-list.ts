import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { EmployeeResponse, PagedResponse } from '../../models/employee';
import { EmployeeService } from '../../services/employee';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth';
import { Permissions } from '../../../../core/models/permissions';
import {
  DataStateChangeEvent,
  GridDataResult,
  KENDO_GRID
} from '@progress/kendo-angular-grid';
import {
  CompositeFilterDescriptor,
  FilterDescriptor,
  State
} from '@progress/kendo-data-query';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';

@Component({
  selector: 'app-employee-list',
  imports: [CommonModule, FormsModule, RouterLink, KENDO_GRID, KENDO_BUTTONS],
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

  gridState = signal<State>({
    skip: 0,
    take: 10,
    sort: [
      {
        field: 'employeeID',
        dir: 'asc'
      }
    ]
  });

  readonly authService = inject(AuthService);

  constructor(private readonly employeeService: EmployeeService) { }

  ngOnInit(): void {
    this.loadEmployees();
  }

  loadEmployees(): void {
    this.loading.set(true);
    this.errorMessage.set('');

    const state = this.gridState();
    const sort = state.sort?.[0];

    const take = state.take ?? this.pageSize();
    const skip = state.skip ?? 0;

    const pageSize = take;
    const page = Math.floor(skip / take) + 1;

    const sortBy = this.mapGridFieldToApiSort(sort?.field);
    const sortDirection = sort?.dir === 'desc' ? 'desc' : 'asc';

    this.employeeService
      .getEmployeesGrid(this.buildGridState())
      .subscribe({
        next: (res: GridDataResult) => {
          this.employees.set(res.data as EmployeeResponse[]);
          this.totalItems.set(res.total);

          const state = this.gridState();
          const take = state.take ?? 10;
          const skip = state.skip ?? 0;

          this.page.set(Math.floor(skip / take) + 1);
          this.pageSize.set(take);

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
    this.gridState.update(state => ({
      ...state,
      skip: 0
    }));

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

    this.gridState.set({
      skip: 0,
      take: 10,
      sort: [
        {
          field: 'employeeID',
          dir: 'asc'
        }
      ]
    });

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

  onGridStateChange(state: DataStateChangeEvent): void {
    this.gridState.set(state);
    this.loadEmployees();
  }

  private mapGridFieldToApiSort(field?: string): string {
    switch (field) {
      case 'employeeID':
        return 'employeeId';
      case 'firstName':
        return 'firstName';
      case 'lastName':
        return 'lastName';
      case 'email':
        return 'email';
      case 'hireDate':
        return 'hireDate';
      case 'salary':
        return 'salary';
      default:
        return 'employeeId';
    }
  }

  private buildGridState(): State {
    const filters: Array<FilterDescriptor | CompositeFilterDescriptor> = [];

    if (this.search().trim()) {
      const search = this.search().trim();

      filters.push({
        logic: 'or',
        filters: [
          { field: 'employeeID', operator: 'contains', value: search },
          { field: 'firstName', operator: 'contains', value: search },
          { field: 'lastName', operator: 'contains', value: search },
          { field: 'email', operator: 'contains', value: search }
        ]
      });
    }

    if (this.companyID().trim()) {
      filters.push({
        field: 'companyID',
        operator: 'eq',
        value: this.companyID().trim()
      });
    }

    if (this.isActive() !== null) {
      filters.push({
        field: 'isActive',
        operator: 'eq',
        value: this.isActive()
      });
    }

    return {
      ...this.gridState(),
      filter: filters.length
        ? {
          logic: 'and',
          filters
        }
        : undefined
    };
  }

  refreshGrid(): void {
    this.loadEmployees();
  }

  clearGridState(): void {
    this.resetFilters();
  }
}
