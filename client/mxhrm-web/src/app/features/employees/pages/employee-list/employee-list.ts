import { Component, computed, inject, OnInit, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EmployeeResponse } from '../../models/employee';
import { EmployeeService } from '../../services/employee';
import { RouterLink } from '@angular/router';
import { AuthService } from '../../../../core/services/auth';
import { Permissions } from '../../../../core/models/permissions';
import {
  DataStateChangeEvent,
  GridDataResult,
  KENDO_GRID
} from '@progress/kendo-angular-grid';
import { State } from '@progress/kendo-data-query';
import { KENDO_BUTTONS } from '@progress/kendo-angular-buttons';

@Component({
  selector: 'app-employee-list',
  imports: [CommonModule, RouterLink, KENDO_GRID, KENDO_BUTTONS],
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

    this.employeeService
      .getEmployeesGrid(this.gridState())
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

  resetFilters(): void {
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
    this.gridState.update(current => ({
      ...current,
      ...state,
      skip: state.skip ?? 0,
      take: state.take ?? current.take ?? 10
    }));
    this.loadEmployees();
  }

  refreshGrid(): void {
    this.loadEmployees();
  }

  clearGridState(): void {
    this.resetFilters();
  }
}
